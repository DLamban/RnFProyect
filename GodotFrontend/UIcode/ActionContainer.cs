using Core.GameLoop;
using Core.Rules;
using Godot;
using GodotFrontend.code.Input;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
public class ActionGeneric
{
	public string name;
	public Action action;
	public ActionGeneric(string _name, Action _action)
	{
		name = _name;
		action = _action;
	}
}
public partial class ActionContainer : PanelContainer
{
	public List<ActionButton> actionButtons= new List<ActionButton>
	{
	};
	private HBoxContainer actionBtnContainer;

	List<ActionGeneric> strategicActions = new List<ActionGeneric>();
    List<ActionGeneric> chargeActions = new List<ActionGeneric>();

    List<ActionButton> strategicActionButtons = new List<ActionButton>();
    List<ActionButton> chargeActionButtons = new List<ActionButton>();
	List<List<ActionButton>> allActions = new List<List<ActionButton>>();

	#region nodestosendsignals
	PanelContainer spellContainer;
	InputCharge inputCharge;
	ReactiveInput reactiveInput;
    #endregion
    Button endSubPhaseButton;
	public override async void _Ready()
	{        
		actionBtnContainer = GetNode<HBoxContainer>("MainHBox/HBoxContainer");
		endSubPhaseButton = GetNode<Button>("MainHBox/MarginContainer/ActionButton");
		endSubPhaseButton.Pressed += endSubPhase;
        PlayerInfoSingleton.Instance.battleStateManager.OnSubPhaseChanged += OnSubPhaseChange;
		// we need the instances that we are send messages
		spellContainer = GetTree().CurrentScene.GetNode<PanelContainer>("Battlefield/UnitManager/HUD/CanvasGroup/AnchorProvider/SpellsContainer");
        InputManager inputManager = GetTree().CurrentScene.GetNode<Node3D>("Battlefield") as InputManager;

        await ToSignal(inputManager, SignalName.Ready);
        inputCharge = inputManager.inputCharge; 
		reactiveInput = inputManager.reactiveInput;
        populateActions();
		activateStrategicActions();


	}
	private async void endSubPhase()
	{
		switch (PlayerInfoSingleton.Instance.battleStateManager.currentSubPhase) {
			case SubBattleStatePhase.charge:
				await reactiveInput.ResolveCharges(inputCharge.charges); 
				break;
		}

        PlayerInfoSingleton.Instance.battleStateManager.passNextSubState();

    }
	public void OnSubPhaseChange(SubBattleStatePhase subBattleStatePhase)
	{
		disableActions();
		switch (subBattleStatePhase)
		{
			case SubBattleStatePhase.strategic:
				activateStrategicActions();
				break;
			case SubBattleStatePhase.charge:
				activateChargeActions();
				break;
		}
	}
	private void disableActions()
	{
		foreach ( var actionList in allActions)
		{
			foreach( var action in actionList)
			{
				action.Visible = false;
			}

		}
	}
	private void activateChargeActions()
	{
		foreach (var action in chargeActionButtons)
		{
			action.Visible = true;
		}
	}

	private void activateStrategicActions() {
		foreach (var action in strategicActionButtons) { 
			action.Visible = true;
		}
	}
	#region populateActions
	public void populateActions()
	{		
		strategicActionButtons =  createStrategicActions();
		allActions.Add(strategicActionButtons);
		chargeActionButtons = createChargeActions();
		allActions.Add(chargeActionButtons);
	}

	private List<ActionButton> createStrategicActions()
	{
		Action actionSpells = () =>
		{
			spellContainer.Visible = true;
		};
		strategicActions.Add(new ActionGeneric("Spells", actionSpells));
        strategicActions.Add(new ActionGeneric("Rally troops", () => { }));

        return createActions(strategicActions);

	}

	private List<ActionButton> createChargeActions()
	{
        Action actionSpells = () =>
        {
            spellContainer.Visible = true;
        };
        chargeActions.Add(new ActionGeneric("Spells", actionSpells));
        chargeActions.Add(new ActionGeneric("Charge", () => { }));
		return createActions(chargeActions);

	}
    private List<ActionButton> createActions(List<ActionGeneric> actions)
    {
        var list = new List<ActionButton>();

        PackedScene action_button_asset = GD.Load<PackedScene>("res://assets/UI/Actions_button.tscn");
        foreach (var action in actions)
        {
            ActionButton action_button = action_button_asset.Instantiate() as ActionButton;
            actionBtnContainer.AddChild(action_button);
            action_button.GetNode<Button>("ActionButton").Text = action.name;
			action_button.OnPressed += action.action;
            action_button.Visible = false;
            list.Add(action_button);
        }
        return list;
    }
    #endregion
}
