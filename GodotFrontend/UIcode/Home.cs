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
        NakamaService.Instance.OnMatchFound += HandleMatchFound;
        // Connect signals using async lambdas
        
        login1.Pressed += async () => await AttemptLogin("Player1_" + deviceId);
        login2.Pressed += async () => await AttemptLogin("Player2_" + deviceId);

        _btnFindMatch = GetNode<Button>("%BtnFindMatch");
        _btnFindMatch.Pressed += async () =>
        {
            _btnFindMatch.Disabled = true;
            _statusLabel.Text = "Searching for opponent...";            
            await NakamaService.Instance.FindAuthoritativeMatch();            
        };
        string baseDir = AppContext.BaseDirectory;
        NakamaService.Instance.OnReceiveMatchState += OnReceiveMatchState;
        PLayerInfoNetcode.Instance.playerSpot = PlayerSpotEnum.PLAYER2;
        loadUnits();
        PackedScene combatScene = (PackedScene)ResourceLoader.Load("res://battlefield.tscn");
        GetTree().ChangeSceneToPacked(combatScene);
    }
    
    private void HandleMatchFound()
    {
        GD.Print("Match found! Transitioning to battlefield...");
        NakamaService.Instance.OnMatchFound -= HandleMatchFound;
        // subscribe to match state updates
        NakamaService.Instance.OnReceiveMatchState += OnReceiveMatchState;
    }
    
    private void OnReceiveMatchState(ServerInit data)
    {
        if (data.PlayerNumber == 1)
        {
            PLayerInfoNetcode.Instance.playerSpot = PlayerSpotEnum.PLAYER1;
        }
        else if (data.PlayerNumber == 2)
        {
            PLayerInfoNetcode.Instance.playerSpot = PlayerSpotEnum.PLAYER2;
        }
        // Load the lists
        loadUnits();
        PackedScene combatScene = (PackedScene)ResourceLoader.Load("res://battlefield.tscn");
        GetTree().ChangeSceneToPacked(combatScene);        
    }
    private void loadUnits()
    {
        
        MockList MockList = new (2);        
        UnitsClientManager.Instance.addAllPlayerUnits(MockList.unitManagerCore.getUnitsPlayer());
        
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