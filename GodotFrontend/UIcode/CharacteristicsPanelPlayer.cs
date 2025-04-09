using Core.Units;
using Godot;
using GodotFrontend.code.Input;
using System;

public partial class CharacteristicsPanelPlayer : Panel
{
	// Called when the node enters the scene tree for the first time.
	private Label nameLabel;
	private Label troopTypeLabel;
	private Label troopCountLabel;

	private Label movementLabel;
	private Label dexterityLabel;
	private Label shootLabel;    
	private Label strengthLabel;
	private Label resistanceLabel;
	private Label initiativeLabel;
	private Label attacksLabel;
	private Label leadershipLabel;
	InputManager inputManager;

	public override async void _Ready()
	{
		nameLabel = (Label)FindChild("Name");
		troopCountLabel = (Label)FindChild("TroopCount");
		troopTypeLabel = (Label)FindChild("TroopType");

		movementLabel = (Label)FindChild("Movement");
		dexterityLabel = (Label)FindChild("Dexterity");
		shootLabel = (Label)FindChild("Shoot");
		strengthLabel = (Label)FindChild("Strength");
		resistanceLabel = (Label)FindChild("Resistance");
		initiativeLabel = (Label)FindChild("Initiative");
		attacksLabel = (Label)FindChild("Attacks");
		leadershipLabel = (Label)FindChild("Leadership");
		//UpdateCharacteristics(UnitsClientManager.Instance.findUnitByName("Goblins"));

		inputManager = GetTree().CurrentScene.GetNode<Node3D>("Battlefield") as InputManager;
		await ToSignal(inputManager, SignalName.Ready);
		inputManager.OnHoverUnit += UpdateCharacteristics;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	public void UpdateCharacteristics(BaseUnit baseUnit)
	{
		// name
		nameLabel.Text = baseUnit.Name;
		troopCountLabel.Text = baseUnit.UnitCount.ToString();
		troopTypeLabel.Text = baseUnit.Type.ToString();
		// Update the characteristics of the player
		movementLabel.Text = baseUnit.Troop.Movement.ToString();
		dexterityLabel.Text = baseUnit.Troop.Dexterity.ToString();
		shootLabel.Text = baseUnit.Troop.Shooting.ToString();
		strengthLabel.Text = baseUnit.Troop.Strength.ToString();
		resistanceLabel.Text = baseUnit.Troop.Resistance.ToString();
		initiativeLabel.Text = baseUnit.Troop.Initiative.ToString();
		attacksLabel.Text = baseUnit.Troop.Attacks.ToString();
		leadershipLabel.Text = baseUnit.Troop.Leadership.ToString();
		

	}
}
