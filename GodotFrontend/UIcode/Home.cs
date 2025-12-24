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
        login1.Pressed += async () => {
            PlayerInfoNetcode.Instance.playerSpot = PlayerSpotEnum.PLAYER1;
            await AttemptLogin(player1id);            
        };
        login2.Pressed += async () => {
            PlayerInfoNetcode.Instance.playerSpot = PlayerSpotEnum.PLAYER2;
            await AttemptLogin(player2id);
        };

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
        PlayerInfoNetcode.Instance.initNetPlayer(deviceId);
        _clientNetworkController = PlayerInfoNetcode.Instance.networkController;

    }

    private void OpponentListReceived(List<UnitSpawnDTO> list)
    {
        DebugOverlay.Instance.Log("Opponent list received");
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
        DebugOverlay.Instance.Log("Match start");
        NakamaService.Instance.OnMatchFound -= HandleMatchFound;
        // Load the lists
        var units = loadUnits();
        _clientNetworkController.sendListtToOpponent(units);
    }
    
    private void OnReceiveMatchState(ServerInit data)
    {
        DebugOverlay.Instance.Log("Received match state");
        if (data.PlayerNumber == 1)
        {
            PlayerInfoNetcode.Instance.playerSpot = PlayerSpotEnum.PLAYER1;
        }
        else if (data.PlayerNumber == 2)
        {
            PlayerInfoNetcode.Instance.playerSpot = PlayerSpotEnum.PLAYER2;
        }
        DebugOverlay.Instance.Log($"Assigned as Player {data.PlayerNumber}");
        DebugOverlay.Instance.changeCurrentPlayerLabel("Player" + data.PlayerNumber.ToString());

    }
    private List<UnitSpawnDTO> loadUnits()
    {
        
        MockList MockList = new MockList(2);        
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
            DebugOverlay.Instance.Log("Connection refused");
            GD.PrintErr($"UI Login Error: {ex.Message}");
            _statusLabel.Text = "Error: Connection Refused";
            _statusLabel.Modulate = new Color(1, 0.3f, 0.3f);
        }
    }
}