using Core.GameLoop;
using Core.Rules;
using Godot;
using GodotFrontend.code.Input;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static GodotFrontend.code.Input.InputFSM;
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
	public List<ActionButton> actionButtons = new List<ActionButton>();
	private HBoxContainer actionBtnContainer;
    public InputState inputState
    {
        get { return InputFSM.currentState; }
        set
        {
            InputFSM.changeState(value);
        }
    }
    List<ActionGeneric> strategicActions = new List<ActionGeneric>();
    List<ActionGeneric> chargeActions = new List<ActionGeneric>();
    List<ActionGeneric> moveActions = new List<ActionGeneric>();
    List<ActionGeneric> shootingActions = new List<ActionGeneric>();

    List<ActionButton> strategicActionButtons = new List<ActionButton>();
    List<ActionButton> chargeActionButtons = new List<ActionButton>();
    List<ActionButton> moveActionButtons = new List<ActionButton>();
    List<ActionButton> shootingActionButtons = new List<ActionButton>();

    List<List<ActionButton>> allActions = new List<List<ActionButton>>();

	#region nodestosendsignals
	PanelContainer spellContainer;
    InputManager inputManager;
    InputCharge inputCharge;
    InputResolveCharge inputResolveCharge;
    ReactiveInput reactiveInput;
    #endregion
    Button endSubPhaseButton;
	// FEEDBACK vars
	Label inputPhaseLabel;
    Label subPhaseLabel;
    public override async void _Ready()
	{        
        inputPhaseLabel = GetNode<Label>("VBoxContainer/CenterContainer2/InputPhaseLabel");
        subPhaseLabel = GetNode<Label>("VBoxContainer/CenterContainer/SubPhaseLabel");
        actionBtnContainer = GetNode<HBoxContainer>("VBoxContainer/MainHBox/HBoxContainer");
		endSubPhaseButton = GetNode<Button>("VBoxContainer/MainHBox/MarginContainer/ActionButton");
		endSubPhaseButton.Pressed += endSubPhase;
        PlayerInfoSingleton.Instance.battleStateManager.OnSubPhaseChanged += OnSubPhaseChange;
		// we need the instances that we are send messages
		spellContainer = GetTree().CurrentScene.GetNode<PanelContainer>("Battlefield/UnitManager/HUD/CanvasGroup/AnchorProvider/SpellsContainer");
        inputManager = GetTree().CurrentScene.GetNode<Node3D>("Battlefield") as InputManager;

        await ToSignal(inputManager, SignalName.Ready);
		
		inputCharge = inputManager.inputCharge; 
		reactiveInput = inputManager.reactiveInput;
        
		inputResolveCharge = inputManager.inputResolveCharge;
		inputResolveCharge.OnChargeSelectedToExecute += (visibility) => { toogleVisibility("Charge", visibility); };
        populateActions();
		activateStrategicActions();
	}
	private async void endSubPhase()
	{
		switch (PlayerInfoSingleton.Instance.battleStateManager.currentSubPhase) {
			case SubBattleStatePhase.charge:
				if (inputState == InputState.ResolvingCharges)
				{
                    PlayerInfoSingleton.Instance.battleStateManager.passNextSubState();
					inputCharge.finishChargeSubphase();
                }
				else
				{
                    await reactiveInput.ResolveCharges(inputCharge.charges);
                    // After the await we have all the charge reactions
					endSubPhaseButton.Disabled = true;
					endSubPhaseButton.TooltipText = "Resolve declared charges first";
                    Action action = () => { endSubPhaseButton.Disabled = false; };
                    inputManager.setUpResolveChargesInputphase(action);

                }                    
                break;
			default:
                PlayerInfoSingleton.Instance.battleStateManager.passNextSubState();
                break;
        }        
    }

    private void toogleVisibility(string actionName, bool visibility)
	{
		if (visibility) showActionBtn(actionName);
		else hideActionBtn(actionName);
	}

    private void showActionBtn(string actionName)
    {
		actionBtnContainer.GetNode<MarginContainer>(actionName).Visible = true;
    }
    private void hideActionBtn(string actionName)
    {
        actionBtnContainer.GetNode<MarginContainer>(actionName).Visible = false;
    }
    public void OnSubPhaseChange(SubBattleStatePhase subBattleStatePhase)
	{
		disableActions();
		switch (subBattleStatePhase)
		{
			case SubBattleStatePhase.strategic:
				enterStrategicPhase();				
				break;
			case SubBattleStatePhase.charge:
				enterChargePhase();                
				break;
            case SubBattleStatePhase.move:
                enterMovePhase();
                break;

            case SubBattleStatePhase.shoot:
                enterShootingPhase();
                break;
        }
	}
    private void enterStrategicPhase()
    {
        inputPhaseLabel.Text = "Strategic Phase";
        subPhaseLabel.Text = "Strategic";
        activateStrategicActions();
    }
    private void enterChargePhase()
    {
        inputPhaseLabel.Text = "Move Phase";
        subPhaseLabel.Text = "charge";
		activateChargeActions();
    }
    private void enterMovePhase()
    {
        inputPhaseLabel.Text = "Move Phase";
        subPhaseLabel.Text = "Move";
        activateChargeActions();
    }
	private void enterShootingPhase()
	{
        inputPhaseLabel.Text = "Shooting Phase";
        subPhaseLabel.Text = "charge";
		activateShootingActions();
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
			if (action.Name == "Charge") break;
			action.Visible = true;
		}
	}

	private void activateStrategicActions() {
		foreach (var action in strategicActionButtons) { 
			action.Visible = true;
		}
	}
    private void activateShootingActions()
    {
        foreach (var action in shootingActionButtons)
        {
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

        shootingActionButtons = createShootingActions();
        allActions.Add(shootingActionButtons);
    }
    private List<ActionButton> createShootingActions()
    {
        Action actionSpells = () =>
        {
            spellContainer.Visible = true;
        };
        strategicActions.Add(new ActionGeneric("Spells", actionSpells));
        shootingActions.Add(new ActionGeneric("Shoot", () => { }));
        return createActions(shootingActions);
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
		Action actionCharge = () =>
		{
			inputResolveCharge.executeCharge();
		};
        chargeActions.Add(new ActionGeneric("Charge", actionCharge));
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
            action_button.Name = action.name;
            action_button.GetNode<Button>("ActionButton").Text = action.name;
			action_button.OnPressed += action.action;
            action_button.Visible = false;
            list.Add(action_button);
        }
        return list;
    }
    #endregion
}
