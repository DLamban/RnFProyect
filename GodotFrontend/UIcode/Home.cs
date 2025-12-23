using Aranfee;
using Core.GameLoop;
using Core.List;
using Core.Networking;
using Core.Units;
using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

public partial class Home : Control
{
    private VBoxContainer _vboxConnected;
    private VBoxContainer _vboxDisconnected;
    private Label _statusLabel;
    private Button _btnFindMatch;
    private ClientNetworkController _clientNetworkController;
    public override void _Ready()
    {
        Button login1 = GetNode<Button>("%Login1btn");
        Button login2 = GetNode<Button>("%Login2btn");
        Button testMsg = GetNode<Button>("%TestMsg");
        _vboxConnected = GetNode<VBoxContainer>("%Connected");
        _vboxDisconnected = GetNode<VBoxContainer>("%Disconnected");
        _statusLabel = GetNode<Label>("%StatusLabel");

        string deviceId = OS.GetUniqueId();
        // The handler when a match is found

        // Connect signals using async lambdas
        string player1id = "Player1_" + deviceId;
        string player2id = "Player2_" + deviceId;
        login1.Pressed += async () => await AttemptLogin(player1id);
        login2.Pressed += async () => await AttemptLogin(player2id);

        _btnFindMatch = GetNode<Button>("%BtnFindMatch");
        _btnFindMatch.Pressed += async () =>
        {
            _btnFindMatch.Disabled = true;
            _statusLabel.Text = "Searching for opponent...";            
            await NakamaService.Instance.FindAuthoritativeMatch();            
        };
        string baseDir = AppContext.BaseDirectory;
        NakamaService.Instance.OnReceiveMatchState += OnReceiveMatchState;
        NakamaService.Instance.OnMatchFound += HandleMatchFound;
        NakamaService.Instance.OnReceiveOpponentUnitSpawnList += OpponentListReceived;
        PlayerInfoNetcode.Instance.playerSpot = PlayerSpotEnum.PLAYER1;
        
        PlayerInfoNetcode.Instance.initNetPlayer(PlayerInfoNetcode.Instance.playerSpot == PlayerSpotEnum.PLAYER1? player1id:player2id);
        _clientNetworkController = PlayerInfoNetcode.Instance.networkController;
        loadUnits();
    }

    private void OpponentListReceived(List<UnitSpawnDTO> list)
    {
        foreach (var unitParam in list)
        {
            BaseUnit unit = UnitsServerManager.CreateNewUnit(unitParam);
            UnitsServerManager.addEnemyUnit(unit);
        }
        var enemyunits = UnitsServerManager.getUnitsEnemy();
        UnitsClientManager.Instance.addAllEnemyUnits(enemyunits);
        PackedScene combatScene = (PackedScene)ResourceLoader.Load("res://battlefield.tscn");
        GetTree().ChangeSceneToPacked(combatScene);
    }

    private void HandleMatchFound()
    {
        GD.Print("Match found! Transitioning to battlefield...");
        NakamaService.Instance.OnMatchFound -= HandleMatchFound;
        // Load the lists
        var units = loadUnits();
        _clientNetworkController.sendListtToOpponent(units);
        
       
        
        // subscribe to match state updates
        //NakamaService.Instance.OnReceiveMatchState += OnReceiveMatchState;
    }
    
    private void OnReceiveMatchState(ServerInit data)
    {
        
        if (data.PlayerNumber == 1)
        {
            PlayerInfoNetcode.Instance.playerSpot = PlayerSpotEnum.PLAYER1;
        }
        else if (data.PlayerNumber == 2)
        {
            PlayerInfoNetcode.Instance.playerSpot = PlayerSpotEnum.PLAYER2;
        }
        

    }
    private List<UnitSpawnDTO> loadUnits()
    {
        
        MockList MockList = new (2);        
        UnitsClientManager.Instance.addAllPlayerUnits(UnitsServerManager.getUnitsPlayer());
        return MockList.playerunitsParamsToCreateandSpawn;
    }

    private async Task AttemptLogin(string userId)
    {
        try
        {
            _statusLabel.Text = "Connecting to Server...";
            _statusLabel.Modulate = new Color(1, 1, 1);

            await NakamaService.Instance.LoginUser(userId);

            _vboxConnected.Visible = true;
            _vboxDisconnected.Visible = false;
        }
        catch (Exception ex)
        {
            GD.PrintErr($"UI Login Error: {ex.Message}");
            _statusLabel.Text = "Error: Connection Refused";
            _statusLabel.Modulate = new Color(1, 0.3f, 0.3f);
        }
    }
}