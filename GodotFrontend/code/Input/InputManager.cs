using Core.GameLoop;
using Core.Magic;
using Core.Units;
using Godot;
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

        private SpellTarget? currentSpellTarget;
		private Camera3D mainCamera;
		private Viewport viewport;
		private PhysicsDirectSpaceState3D spaceState;
		public InputState inputState {
			get { return InputFSM.currentState; }
			set { 
				InputFSM.changeState(value);               
			}
		}
		private BattleState battleState
		{
			get { return PlayerInfoSingleton.Instance.battleStateManager.currentState; }
		}
        private SubBattleStatePhase currentSubPhase
        {
            get { return PlayerInfoSingleton.Instance.battleStateManager.currentSubPhase;}
			set { PlayerInfoSingleton.Instance.battleStateManager.currentSubPhase = value;}
        }
        public delegate Vector3? BattlefieldCursorPosDel();
		BattlefieldCursorPosDel getBattlefieldCursorPosDel;
		public override void _Ready()
		{
        //    PlayerInfoSingleton.Instance.battleStateManager.OnBattleStateChanged += OnBattleStateChanged;
            PlayerInfoSingleton.Instance.battleStateManager.OnSubPhaseChanged += OnSubPhaseChanged;
            getBattlefieldCursorPosDel = getBattlefieldCursorPos;
			// we get the viewport and spacesate to pass as parameter so we can raycast the battlefield
			viewport = GetViewport();
			mainCamera = viewport.GetCamera3D() as Camera3D;
			spaceState = GetWorld3D().DirectSpaceState;			
			BuildSubManagers();
        }
		private void BuildSubManagers()
		{
            // get the node of cursor effects
            MeshInstance3D cursorEffect = GetNode<MeshInstance3D>("CursorEffect") as MeshInstance3D;
            // create the subinput child managers
            inputMovePhase = new InputMovePhase(getBattlefieldCursorPosDel);
            inputMagic = new InputMagic(getBattlefieldCursorPosDel, cursorEffect);
			inputCharge = new InputCharge(getBattlefieldCursorPosDel);

            // magic runs in all phases
            magicStateProccess = inputMagic.CustomProcess;
        }
		public void SpellSelection(SpellTarget spellTarget, Spell spell)
		{
			inputMagic.SpellSelection(spellTarget, spell);
		}
		private void OnSubPhaseChanged(SubBattleStatePhase currentSubPhase)
		{
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
                default:
                    Debug.WriteLine("state not implemented");
                    break;
            }
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
                    case SubBattleStatePhase.charge:
						inputCharge.selectUnit(unitSelect);
                        break;
                    case SubBattleStatePhase.move:
                        SelectUnitToMove(unitSelect);
                        break;
                    default:
						throw new NotImplementedException();
						break;                        
                }

            }



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
			if (UnitsClientManager.Instance.canSelectUnit(unitToSelect.coreUnit.Guid, true))
			{
				return unitToSelect;
			}
			return null;
		}
        private void SelectUnitToMove(UnitGodot unitSelect)
		{

			if (SelectOwnUnit!=null)
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
