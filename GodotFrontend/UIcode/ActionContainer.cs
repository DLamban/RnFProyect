using Core.GameLoop;
using Core.Rules;
using Core.Units;
using Godot;
using GodotFrontend.code.Input;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
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
public partial class ActionContainer : Panel
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
    List<ActionGeneric> combatActions = new List<ActionGeneric>();

    List<ActionButton> strategicActionButtons = new List<ActionButton>();
	List<ActionButton> chargeActionButtons = new List<ActionButton>();
	List<ActionButton> moveActionButtons = new List<ActionButton>();
	List<ActionButton> shootingActionButtons = new List<ActionButton>();
	List<ActionButton> combatActionsButtons = new List<ActionButton>();

    List<List<ActionButton>> allActions = new List<List<ActionButton>>();

	#region nodestosendsignals
	PanelContainer spellContainer;
	InputManager inputManager;
	InputCharge inputCharge;
	InputResolveCharge inputResolveCharge;
	InputShootPhase inputShootPhase;
	ReactiveInput reactiveInput;
	InputCombatPhase inputCombatPhase;
    #endregion
    TextureButton endSubPhaseButton;
	// FEEDBACK vars
	Label inputPhaseLabel;
	Label subPhaseLabel;
	public override async void _Ready()
	{


        inputPhaseLabel = GetNode<Label>("MarginContainer/VBoxContainer/HBoxContainer/InputPhaseLabel");
		subPhaseLabel = GetNode<Label>("MarginContainer/VBoxContainer/HBoxContainer/SubPhaseLabel");
		actionBtnContainer = GetNode<HBoxContainer>("MarginContainer/VBoxContainer/MainHBox/HBoxContainer");
        

        endSubPhaseButton = GetNode<TextureButton>("EndPhaseButton");
		

		endSubPhaseButton.Pressed += endSubPhase;
		PlayerInfoSingletonHotSeat.Instance.battleStateManager.OnSubPhaseChanged += OnSubPhaseChange;
		// we need the instances that we are send messages
		spellContainer = GetTree().CurrentScene.GetNode<PanelContainer>("Battlefield/UnitManager/HUD/CanvasGroup/AnchorProvider/SpellsContainer");
		inputManager = GetTree().CurrentScene.GetNode<Node3D>("Battlefield") as InputManager;

		await ToSignal(inputManager, SignalName.Ready);
		
		inputCharge = inputManager.inputCharge; 
		reactiveInput = inputManager.reactiveInput;
		

        inputResolveCharge = inputManager.inputResolveCharge;
		inputResolveCharge.OnChargeSelectedToExecute += (enabled) => { toogleDisabled("Charge", !enabled); };
        
		inputShootPhase = inputManager.inputShootPhase;
        inputShootPhase.OnShootSelectedToExecute += (enabled) => { toogleDisabled("Shoot", !enabled); };// disable to true to enable the button
        
		inputCombatPhase = inputManager.inputCombatPhase;
        inputCombatPhase.OnSelectUnitToCombat += (enabled) => { toogleDisabled("Combat", !enabled); };// disable to true to enable the button

        populateActions();
		activateStrategicActions();
	}
	private void changeIcon()
	{
        string phaseString = PlayerInfoSingletonHotSeat.Instance.battleStateManager.currentState.ToString();
        string pathNormal = $"res://assets/UI/BubbleStatusButton/button_{phaseString}.png";
		string pathHover = $"res://assets/UI/BubbleStatusButton/button_{phaseString}_hover.png";
		string pathPressed = $"res://assets/UI/BubbleStatusButton/button_{phaseString}_pressed.png";
        endSubPhaseButton.TextureNormal = (Texture2D)GD.Load<Texture>(pathNormal);
		endSubPhaseButton.TextureHover = (Texture2D)GD.Load<Texture>(pathHover);
        endSubPhaseButton.TexturePressed = (Texture2D)GD.Load<Texture>(pathPressed);


    }
    private async void endSubPhase()
	{

		switch (PlayerInfoSingletonHotSeat.Instance.battleStateManager.currentSubPhase) {
			case SubBattleStatePhase.charge:
				if (inputState == InputState.ResolvingCharges)
				{
					PlayerInfoSingletonHotSeat.Instance.battleStateManager.passNextSubState();
					inputCharge.finishChargeSubphase();
				}
				else
				{
					if (inputCharge.charges.Count > 0) { 
                        await reactiveInput.ResolveCharges(inputCharge.charges);
                    
						// After the await we have all the charge reactions
						endSubPhaseButton.Disabled = true;
						endSubPhaseButton.TooltipText = "Resolve declared charges first";
						Action action = () => { endSubPhaseButton.Disabled = false; };
						inputManager.setUpResolveChargesInputphase(action);
                    }
					else
					{
                        PlayerInfoSingletonHotSeat.Instance.battleStateManager.passNextSubState();
                    }

                }                    
				break;
			case SubBattleStatePhase.combat:
				// END TURN
				HotSeatManager.Instance.endTurn();
                PlayerInfoSingletonHotSeat.Instance.battleStateManager.passNextSubState();
                break;
            default:
				PlayerInfoSingletonHotSeat.Instance.battleStateManager.passNextSubState();
				changeIcon();
                break;
		}        
	}

	private void toogleVisibility(string actionName, bool visibility)
	{
		if (visibility) showActionBtn(actionName);
		else hideActionBtn(actionName);
	}
	private void toogleDisabled(string actionName, bool disabled)
    {
        actionBtnContainer.GetNode<MarginContainer>(actionName).GetNode<Button>("ActionButton").Disabled = disabled;
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
			case SubBattleStatePhase.combat:
                enterCombatPhase();
                break;
        }
    }
	private void enterStrategicPhase()
	{
		inputPhaseLabel.Text = "Strategic Phase";
		subPhaseLabel.Text = "Strategic";
		endSubPhaseButton.TooltipText = "End strategic phase";
        activateStrategicActions();
	}
	private void enterChargePhase()
	{
        inputPhaseLabel.Text = "Charge subphase";
		subPhaseLabel.Text = "declare charges";
        endSubPhaseButton.TooltipText = "End charge phase";
        activateChargeActions();
	}
	private void enterMovePhase()
	{
		inputPhaseLabel.Text = "Move Phase";
		subPhaseLabel.Text = "Move troops";
        endSubPhaseButton.TooltipText = "End move phase";
        activateChargeActions();
	}
	private void enterShootingPhase()
	{
		inputPhaseLabel.Text = "Shooting Phase";
		subPhaseLabel.Text = "select shooter";
        endSubPhaseButton.TooltipText = "End shooting phase";
        activateShootingActions();
    }
    private void enterCombatPhase()
    {
        inputPhaseLabel.Text = "Combat Phase";
        subPhaseLabel.Text = "select unit";
        endSubPhaseButton.TooltipText = "Next turn";
        activateCombatActions();
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
			if (action.Name == "Charge") action.GetNode<Button>("ActionButton").Disabled = true;
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
			action.GetNode<Button>("ActionButton").Disabled = true;
        }
	}
    private void activateCombatActions()
    {
        foreach (var action in combatActionsButtons)
        {
            action.Visible = true;
            action.GetNode<Button>("ActionButton").Disabled = true;
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
        
		combatActionsButtons = createCombatActions();
        allActions.Add(combatActionsButtons);

    }
	private List<ActionButton> createShootingActions()
	{
		Action actionSpells = () =>
		{
			spellContainer.Visible = true;
		};
		strategicActions.Add(new ActionGeneric("Spells", actionSpells));
		Action actionShooting = () =>
        {
			inputShootPhase.executeShooting();
        };
        shootingActions.Add(new ActionGeneric("Shoot", actionShooting));
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
		strategicActions.Add(new ActionGeneric("Kill orcs(test)", () =>
		{
			UnitsClientManager.Instance.findUnitByName("Orcs").ApplyWoundUnit(1);
		}));
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
    private List<ActionButton> createCombatActions()
    {
        Action actionSpells = () =>
        {
            spellContainer.Visible = true;
        };
        combatActions.Add(new ActionGeneric("Spells", actionSpells));
        Action executeCombatAction = () =>
        {
            inputCombatPhase.executeCombat();
        };
        combatActions.Add(new ActionGeneric("Combat", executeCombatAction));
        return createActions(combatActions);
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
