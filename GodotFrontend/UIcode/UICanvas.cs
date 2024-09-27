using Core.Rules;
using Godot;
using GodotFrontend.UIcode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public partial class UICanvas : CanvasLayer
{
	List<Node> hBoxesValuesPlayer;
	List<Node> hBoxesValuesEnemy;
	Dictionary<string, Label> charValuesPlayer = new Dictionary<string, Label>();
	Dictionary<string, Label> charValuesEnemy = new Dictionary<string, Label>();
	public DiceThrower diceThrower { get; set; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		hBoxesValuesPlayer = new List<Node>();
		Panel charpanelplayer = GetNode<Panel>("CanvasGroup/CharacteristicsPanelPlayer");
		Panel charpanelenemy = GetNode<Panel>("CanvasGroup/CharacteristicsPanelEnemy");
		string pathhboxvalues = "CenterContainer/VBoxContainer";
		hBoxesValuesPlayer = charpanelplayer.GetNode<VBoxContainer>(pathhboxvalues).GetChildren().ToList().FindAll(node => node.GetType() == typeof(HBoxContainer));
		hBoxesValuesEnemy = charpanelenemy.GetNode<VBoxContainer>(pathhboxvalues).GetChildren().ToList().FindAll(node => node.GetType() == typeof(HBoxContainer));

		foreach (Node node in hBoxesValuesPlayer)
		{
			charValuesPlayer[node.GetChild(0).ToString()] = (Label)node.GetChild(1);
		}
		foreach (Node node in hBoxesValuesEnemy)
		{
			charValuesEnemy[node.GetChild(0).ToString()] = (Label)node.GetChild(1);
		}
		Node3D diceTray = GetNode<Node3D>("CanvasGroup/DicePanel/Panel/diceView/DiceViewport/DiceTray");
		diceThrower = new DiceThrower(diceTray);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
}
