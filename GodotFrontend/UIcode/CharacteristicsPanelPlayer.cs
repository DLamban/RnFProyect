
using Core.Units;
using Godot;
using GodotFrontend.code.Input;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public partial class CharacteristicsPanelPlayer : Panel
{
	// Called when the node enters the scene tree for the first time.
	private Label nameLabel;
	private Label troopTypeLabel;
	private Label troopCountLabel;
	private CenterContainer unitContainer;
	private Label movementLabel;
	private Label dexterityLabel;
	private Label shootLabel;    
	private Label strengthLabel;
	private Label resistanceLabel;
	private Label initiativeLabel;
	private Label attacksLabel;
	private Label leadershipLabel;
	private TextureRect silhouetteIcon;
	// COMMANDER STATS
	private Label commandLabelName;
	private Label commandWounds;

	private Panel commandPanel;
	private VBoxContainer commanderContainer;
	private Panel commanderPanel;
	private TextureRect commanderIcon;
	private Dictionary<string, Texture2D> silhouettes;
	InputManager inputManager;

	public override async void _Ready()
	{

		/// UNIT STATS
		unitContainer = (CenterContainer)FindChild("UnitContainer");
		nameLabel = (Label)unitContainer.FindChild("Name");
		troopCountLabel = (Label)unitContainer.FindChild("TroopCount");
		troopTypeLabel = (Label)unitContainer.FindChild("TroopType");
		movementLabel = (Label)unitContainer.FindChild("Movement");
		dexterityLabel = (Label)unitContainer.FindChild("Dexterity");
		shootLabel = (Label)unitContainer.FindChild("Shoot");
		strengthLabel = (Label)unitContainer.FindChild("Strength");
		resistanceLabel = (Label)unitContainer.FindChild("Resistance");
		initiativeLabel = (Label)unitContainer.FindChild("Initiative");
		attacksLabel = (Label)unitContainer.FindChild("Attacks");
		leadershipLabel = (Label)unitContainer.FindChild("Leadership");
		//UpdateCharacteristics(UnitsClientManager.Instance.findUnitByName("Goblins"));
		silhouetteIcon = (TextureRect)unitContainer.FindChild("SilhoutteIcon");
		silhouettes = loadSilhouttes();
		// COMMANDER STATS
		commandPanel = (Panel)FindChild("CommandPanel");
		commanderContainer = (VBoxContainer)FindChild("CommanderContainer");
		commanderPanel = (Panel)FindChild("CommanderPanel");
		commandLabelName = (Label)commanderContainer.FindChild("Name");
		commanderIcon = (TextureRect)commanderContainer.FindChild("CommandSilhoutteIcon");
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
		updateCommandGroup(baseUnit);
		updateSilhoutte(baseUnit, silhouetteIcon);

	}
	private void updateCommandGroup(BaseUnit unit)
	{
		
		if (unit.Troops.FindAll(troop => troop is Character).Count > 0 && unit.Troops.Count>1) {
			commandPanel.Visible = true;
			Character testChar = (Character)unit.Troops.Find(troop => troop is Character);

			commandLabelName.Text = testChar.Name;
            updateSilhoutte(testChar.Name, commanderIcon);
        }
		else
		{
			commandPanel.Visible = false;
		}
	}
	private void addCharacterToCommandGroup()
	{

	}
	

    private void updateSilhoutte(BaseUnit unit, TextureRect iconToChange)
	{
		updateSilhoutte(unit.Name, iconToChange);
    }
    private void updateSilhoutte(string name, TextureRect iconToChange)
    {
        // MAGIC Strings, TODO: use enums. I'm pretty sure this is gonna last a long time
        
        switch (name.ToLower())
        {
            case "heavy orcs":
                iconToChange.Texture = silhouettes["armored_orc"];
                break;
            case "gyrocopter":
                iconToChange.Texture = silhouettes["gyrocopter"];
                break;
            case "goblins":
                iconToChange.Texture = silhouettes["goblin"];
                break;
            case "dwarf warriors":
                iconToChange.Texture = silhouettes["dwarf_warrior"];
                break;
            case "slayers":
                iconToChange.Texture = silhouettes["slayer"];
                break;
            case "king dwarf on shield":
                iconToChange.Texture = silhouettes["king"];
                break;
			case "king dwarf":
                iconToChange.Texture = silhouettes["king"];
                break;
            case "elder dwarfs":
                iconToChange.Texture = silhouettes["elder_dwarf"];
                break;
            case "boar riders":
                iconToChange.Texture = silhouettes["orc_boar"];
                break;
			case "warlord black orc":
                iconToChange.Texture = silhouettes["orcboss"];
                break;
			case "goblin wizard":
                iconToChange.Texture = silhouettes["goblin_wizard"];
                break;
            default:
                iconToChange.Texture = silhouettes["missing"];
                break;
        }
    }
}
