using Core.GameLoop;
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
	public Panel gameStatePanel;
	private Sprite2D cursorEffect;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		cursorEffect = GetNode<Sprite2D>("CanvasGroup/CursorEffect");
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


		// DICE TRAY
		Node3D diceTray = GetNode<Node3D>("CanvasGroup/DicePanel/Panel/diceView/DiceViewport/DiceTray");		
		diceThrower = new DiceThrower(diceTray);
		// BATTLESTATE MANAGER
		gameStatePanel = GetNode<Panel>("CanvasGroup/GameStatusContainer/GameStateStatus");
		
		SetEventsGameStatePanel(gameStatePanel);
		SetEventSpellsSelector();
	}
	private void SetEventSpellsSelector(){
		Button spellsSelector = GetNode<Button>("CanvasGroup/ActionContainer/Panel/CenterContainer/HBoxContainer/MarginContainer/SpellButton");
		spellsSelector.Pressed += ()=> {
			PanelContainer spellPanel = GetNode<PanelContainer>("CanvasGroup/SpellsContainer");
			spellPanel.Visible = true;
		};

	}
	private void SetEventsGameStatePanel(Panel _gameStatePanel)
	{
		Button nextBattleStateBtn = _gameStatePanel.GetNode<Button>("NextPhase");
		nextBattleStateBtn.Pressed += ()=> {
				NextBattleState(_gameStatePanel, nextBattleStateBtn);
			};
	}
	private void NextBattleState(Panel _gameStatePanel, Button nextBattleStateBtn)
	{
		BattleStateManager battleManager = PlayerInfoSingleton.Instance.battleStateManager;        
		battleManager.passNextState();
		_gameStatePanel.GetNode<Label>("CurrentPhase").Text = battleManager.currentState.ToString();
		TextureRect phaseIcon = _gameStatePanel.GetNode<TextureRect>("PhaseIcon");
		Tween tween = CreateTween();
		float targetRotation = phaseIcon.RotationDegrees + 90;
		tween.TweenProperty(phaseIcon, "rotation_degrees", targetRotation, 1).SetTrans(Tween.TransitionType.Sine);
		
		// phaseIcon.RotationDegrees += 90;
		if (battleManager.currentState == BattleState.combat)
		{
			nextBattleStateBtn.Text = "End Turn";
		} else if (battleManager.currentState == BattleState.outofturn)
		{
			nextBattleStateBtn.Disabled = true;
		}

	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		cursorEffect.Position = GetViewport().GetMousePosition();

	}
	
}
