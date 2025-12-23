using Core.GameLoop;
using Core.Magic;
using Core.Units;
using Godot;
using GodotFrontend.UIcode;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GodotFrontend.code.Input.InputFSM;


namespace GodotFrontend.code.Input
{
	
	public partial class InputManager : Node3D
	{
		private delegate void StateProcess(double delta);
		private StateProcess currentStateProccess = (delta)=> { };
        private StateProcess magicStateProccess = (delta) => { };
        private UnitGodot unitSelected;
		private UnitGodot lastUnitSelected;
		// SubInputManagers
		private InputMovePhase inputMovePhase;
		public InputMagic inputMagic;
		public InputCharge inputCharge;
        public InputShootPhase inputShootPhase;
        public InputResolveCharge inputResolveCharge; 
        public ReactiveInput reactiveInput;
		public InputCombatPhase inputCombatPhase;


        private SpellTarget? currentSpellTarget;
		private Camera3D mainCamera;
		private Viewport viewport;
		private PhysicsDirectSpaceState3D spaceState;
		private SubBattleStatePhase prevSubState;
		public InputState inputState {
			get { return InputFSM.currentState; }
			set { 
				InputFSM.changeState(value);               
			}
		}
		private BattleState battleState
		{
			get { return PlayerInfoSingletonHotSeat.Instance.battleStateManager.currentState; }
		}
        private SubBattleStatePhase currentSubPhase
        {
            get { return PlayerInfoSingletonHotSeat.Instance.battleStateManager.currentSubPhase;}
			set { PlayerInfoSingletonHotSeat.Instance.battleStateManager.currentSubPhase = value;}
        }
		// DELEGATES
        public delegate Vector3? BattlefieldCursorPosDel();
		BattlefieldCursorPosDel getBattlefieldCursorPosDel;
		UnitRenderCreator unitRenderCreator;
        public event Action<BaseUnit,string> OnHoverUnit;// string defines the owner of the unit, player or enemy
        public override void _Ready()
		{
        //    PlayerInfoSingleton.Instance.battleStateManager.OnBattleStateChanged += OnBattleStateChanged;
            PlayerInfoSingletonHotSeat.Instance.battleStateManager.OnSubPhaseChanged += OnSubPhaseChanged;
            getBattlefieldCursorPosDel = getBattlefieldCursorPos;
			// we get the viewport and spacesate to pass as parameter so we can raycast the battlefield
			viewport = GetViewport();
			mainCamera = viewport.GetCamera3D() as Camera3D;
			spaceState = GetWorld3D().DirectSpaceState;			
			BuildSubManagers();
			Combat.vinculateDiceThrower(DiceThrower.Instance.diceThrowerCombatTaskDelegate);
        }
		private void BuildSubManagers()
		{
			// needed for effects on the units
			unitRenderCreator= GetNode<UnitRenderCreator>("UnitManager");

            // get the node of cursor effects
            MeshInstance3D cursorEffect = GetNode<MeshInstance3D>("CursorEffect") as MeshInstance3D;
            // load scene fireball fx
            Node3D FireballFX = GD.Load<PackedScene>("res://Spells/fireball.tscn").Instantiate() as Node3D;
            AddChild(FireballFX);
			FireballFX.Position = new Vector3(0, 0, 0);
			FireballFX.Visible = false;
            // create the subinput child managers
            inputMovePhase = new InputMovePhase(getBattlefieldCursorPosDel);
            inputMagic = new InputMagic(getBattlefieldCursorPosDel, cursorEffect, FireballFX);
            inputShootPhase = new InputShootPhase(getBattlefieldCursorPosDel);
            inputCharge = new InputCharge(getBattlefieldCursorPosDel);
			inputCombatPhase = new InputCombatPhase(getBattlefieldCursorPosDel);

            Panel blockPanel = GetNode<Panel>("UnitManager/HUD/CanvasGroup/AnchorProvider/BlockGamePanel");
            reactiveInput = new ReactiveInput(blockPanel);
			inputResolveCharge = new InputResolveCharge();
            // magic runs in all phases
            magicStateProccess = inputMagic.CustomProcess;
        }
		public void SpellSelection(SpellTarget spellTarget, Spell spell)
		{
			inputMagic.SpellSelection(spellTarget, spell);
		}
		private void OnSubPhaseChanged(SubBattleStatePhase currentSubPhase)
		{
            // leaving previous subphase
            switch (prevSubState)
			{
				case SubBattleStatePhase.charge:
                    inputCharge.finishChargeSubphase();
                    break;
                case SubBattleStatePhase.move:
                    inputMovePhase.finishMoveSubphase(unitRenderCreator);
                    break;
                case SubBattleStatePhase.shoot:
                    inputShootPhase.finishShootSubphase(unitRenderCreator);
                    break;
                case SubBattleStatePhase.combat:
                    inputCombatPhase.finishCombatSubphase(unitRenderCreator);
                    break;
                default:
                    break;
            }
            // entering new subphase
            switch (currentSubPhase)
            {
                case SubBattleStatePhase.charge:
                    // first time is the charge phase
                    //currentSubPhase = SubBattleStatePhase.charge;
                    setUpChargeInputSubPhase();
                    break;
				case SubBattleStatePhase.move:
					setUpMovementInputPhase();
					break;
				case SubBattleStatePhase.shoot:
					setUpShootInputSubPhase();
					break;
                case SubBattleStatePhase.combat:
                    setUpCombatPhase();
                    break;
                default:
                    Debug.WriteLine("state not implemented");
                    break;
            }
			prevSubState = currentSubPhase;
        }
		// Breaking the patterns, sorry
        public void setUpResolveChargesInputphase(Action OnfinishResolvingCharges)
        {
            inputState = InputState.ResolvingCharges;
            currentStateProccess = inputResolveCharge.CustomProcess;
            inputResolveCharge.setChargesToResolve(inputCharge.charges,OnfinishResolvingCharges);            
        }
		private void setUpShootInputSubPhase()
		{
            inputState = InputState.Empty;
			unitRenderCreator.disableNoShootingTroops();
            currentStateProccess = inputShootPhase.CustomProcess;
        }
        private void setUpChargeInputSubPhase()
		{
            inputState = InputState.Empty;
            currentStateProccess = inputCharge.CustomProcess;
        }
		private void setUpMovementInputPhase()
		{
			inputState = InputState.Empty;
			currentStateProccess = inputMovePhase.CustomProcess;
		}
		private void setUpCombatPhase()
        {
            inputState = InputState.Empty;
			unitRenderCreator.disableOutOfCombatTroops();
            currentStateProccess = inputCombatPhase.CustomProcess;
        }
		
        public void clickUnit(UnitGodot unitSelect)
		{
            //if (lastUnitSelected != null)
            //{
            //    lastUnitSelected.inputEnabled = false;
            //}
            //lastUnitSelected = unitSelect;

            if (inputState == InputState.CastingSpell) {
                inputMagic.SelectUnitToTargetMagic(unitSelected, unitSelect);
            }
			else
			{
                switch (currentSubPhase)
                {
                    
                    case SubBattleStatePhase.charge:// mixing subphase and inputphase, not good
                        if (inputState == InputState.ResolvingCharges)
						{
							inputResolveCharge.selectCharge(unitSelect);
                        }
						else
						{
                            inputCharge.selectUnit(unitSelect);
                        }						
                        break;
                    case SubBattleStatePhase.move:
                        SelectUnitToMove(unitSelect);
                        break;
					case SubBattleStatePhase.shoot:
                        inputShootPhase.clickUnit(unitSelect);
						break;
                    case SubBattleStatePhase.combat:
                        inputCombatPhase.clickUnit(unitSelect);
                        break;
                    default:
						throw new NotImplementedException();
						break;                        
                }

            }
        }
		public void HoveringUnit(UnitGodot unitGodot)
		{
            //check if is player or enemy unit
            string unitowner = UnitsClientManager.Instance.isCurrentPlayerUnit(unitGodot.coreUnit.UnitGuid)?"Player":"Enemy";
            OnHoverUnit?.Invoke(unitGodot.coreUnit,unitowner);
            Debug.WriteLine("hovering unit");
        }
		private Vector3? getBattlefieldCursorPos()
		{
			Vector3 from = mainCamera.ProjectRayOrigin(viewport.GetMousePosition());
			Vector3 to = from + mainCamera.ProjectRayNormal(viewport.GetMousePosition()) * 1000;
			// Raycast
			PhysicsRayQueryParameters3D paramsRaycast = new PhysicsRayQueryParameters3D();
			paramsRaycast.From = from;
			paramsRaycast.To = to;
			paramsRaycast.CollideWithAreas = true;
			paramsRaycast.CollisionMask = 2;
			var result = spaceState.IntersectRay(paramsRaycast);
			if (result.Count > 0)
			{
				return (Vector3)result["position"];
			}
			else
			{
				return null;
			}
		}
		public  override void _Process(double delta)
		{
			magicStateProccess(delta); // running in parallel, watch for headaches
			currentStateProccess(delta);
		}
		#region MOVEMENT
		public void onArrowClick(Node camera, InputEvent @event, Vector3 position, Vector3 normal, long shapeIdx, Node collider)
		{
			if (currentSubPhase == SubBattleStatePhase.charge)
			{
				inputCharge.onArrowClick(camera, @event, position, normal, shapeIdx, collider);
            }else
			{
                inputMovePhase.onArrowClick(camera, @event, position, normal, shapeIdx, collider);
            }
					
		}	
		private UnitGodot? SelectOwnUnit(UnitGodot unitToSelect)
		{
			if (UnitsClientManager.Instance.canSelectUnit(unitToSelect.coreUnit.UnitGuid, true))
			{
				return unitToSelect;
			}
			return null;
		}
        private void deselectLastUnit()
        {
            if (unitSelected != null)
            {
                unitSelected.inputEnabled = false;
				unitSelected = null;
            }
        }
        private void SelectUnitToMove(UnitGodot unitSelect)
		{
			deselectLastUnit();
			// can be selected, and can be moved(not charging this turn, not in combat )
			if (SelectOwnUnit(unitSelect)!=null && !unitSelect.coreUnit.temporalCombatVars.isCharging && !unitSelect.coreUnit.temporalCombatVars.isInCombatRange)
			{
				unitSelect.inputEnabled = true;
				unitSelected = unitSelect;
				UnitsClientManager.Instance.unitSelected = unitSelect.coreUnit;
				
			}
		}
        private void SelectUnitToCharge(UnitGodot unitSelect)
        {

            if (SelectOwnUnit != null)
            {
                unitSelect.inputEnabled = true;
                unitSelected = unitSelect;
                UnitsClientManager.Instance.unitSelected = unitSelect.coreUnit;
            }
        }
        #endregion

    }
}
