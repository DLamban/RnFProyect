using Core.Units;
using Godot;
using GodotFrontend.code.Input;
using System;
using System.Collections.Generic;

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
	private TextureRect silhouetteIcon;
	private Dictionary<string, Texture2D> silhouettes;
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
		silhouetteIcon = (TextureRect)FindChild("SilhoutteIcon");
		silhouettes = loadSilhouttes();

		inputManager = GetTree().CurrentScene.GetNode<Node3D>("Battlefield") as InputManager;		
		await ToSignal(inputManager, SignalName.Ready);
		inputManager.OnHoverUnit += UpdateCharacteristics;
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	private Dictionary<string, Texture2D> loadSilhouttes()
	{
	
		string folderPath = "res://assets/UI/silhouttes/";

		Dictionary<string, Texture2D> silhouettes = new Dictionary<string, Texture2D>();
		DirAccess dir = DirAccess.Open(folderPath);

		if (dir == null)
		{
			GD.PrintErr("Failed to open directory: " + folderPath);
			return silhouettes;
		}
		dir.ListDirBegin();
		string fileName;
		while ((fileName = dir.GetNext()) != "")
		{
			if (fileName.EndsWith(".png"))
			{
				string name = fileName.Split('_', 2)[1].Split('.')[0];// removing common name beginning and extension
				string fullPath = folderPath + fileName;
				Texture2D texture = GD.Load<Texture2D>(fullPath);
				if (texture != null)
				{
					silhouettes[name] = texture;
				}
				else
				{
					GD.PrintErr("Failed to load texture: " + fullPath);
				}
			}
		}

		return silhouettes;
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
		updateSilhoutte(baseUnit);

	}
	private void updateSilhoutte(BaseUnit unit)
	{
		// MAGIC Strings, TODO: use enums. I'm pretty sure this is gonna last a long time
		
		switch (unit.Name.ToLower())
		{
			case "heavy orcs":
				silhouetteIcon.Texture = silhouettes["armored_orc"];
				break;
			case "gyrocopter":
				silhouetteIcon.Texture = silhouettes["gyrocopter"];
				break;
			case "goblins":
				silhouetteIcon.Texture = silhouettes["goblin"];
				break;
			case "dwarf warriors":
				silhouetteIcon.Texture = silhouettes["dwarf_warrior"];
				break;
			case "slayers":
				silhouetteIcon.Texture = silhouettes["slayer"];
				break;
			case "king dwarf on shield":
				silhouetteIcon.Texture = silhouettes["king"];
				break;
			case "elder dwarfs":
				silhouetteIcon.Texture = silhouettes["elder_dwarf"];
				break;
			case "boar riders":
				silhouetteIcon.Texture = silhouettes["orc_boar"];
				break;
			default:
				silhouetteIcon.Texture = silhouettes["missing"];
				break;
		}
	}
}
