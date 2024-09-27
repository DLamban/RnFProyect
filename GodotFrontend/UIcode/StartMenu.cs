using Godot;
using Core.GameLoop;
using Core.Networking;

public partial class StartMenu : Control
{
	private delegate void ButtonClickDelegate();
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		Button connPlayer1btn = GetNode<Button>("CenterContainer/VBoxContainer/ConnPlayer1");
        Button connPlayer2btn = GetNode<Button>("CenterContainer/VBoxContainer/ConnPlayer2");

		connPlayer1btn.Pressed += connectPlayer1;
        connPlayer2btn.Pressed += connectPlayer2;
    }
	private void ConnectPlayer(string player)
	{
        ConnectToServer.Connect(player);
        PackedScene combatScene = (PackedScene)ResourceLoader.Load("res://battlefield.tscn");
        GetTree().ChangeSceneToPacked(combatScene);
    }
    private void connectPlayer1()
    {
        ConnectPlayer("player1");
    }
    private void connectPlayer2()
    {
        ConnectPlayer("player2");
    }
    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{

	}
}
