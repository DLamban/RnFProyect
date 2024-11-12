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
        private Unidad unitSelected;
		private Unidad lastUnitSelected;
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
			PlayerInfoSingleton.Instance.battleStateManager.OnBattleStateChanged += OnBattleStateChanged;            
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
		private void OnBattleStateChanged(object sender, BattleState currentBattleState)
		{
			if (lastUnitSelected!=null) lastUnitSelected.inputEnabled = false;
			switch (currentBattleState)
			{
				case BattleState.move:
					// first time is the charge phase
					currentSubPhase = SubBattleStatePhase.charge;
                    setUpChargeInputSubPhase();
					break;
				case BattleState.strategic:
					// for now is only magic, remember that are more things
					setUpMagicInputPhase();
					break;
				default:
					Debug.WriteLine("state not implemented");
					break;
			}
		}
		private void setUpMagicInputPhase()
		{         
			magicStateProccess = inputMagic.CustomProcess;
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

		public void selectUnit(Unidad unitSelect)
		{
			if (lastUnitSelected != null)
			{
				lastUnitSelected.inputEnabled = false;
			}
			lastUnitSelected = unitSelect;
			
			switch (inputState)
			{
				case InputState.Empty:					

					SelectUnitToMove(unitSelect);
					break;
				case InputState.CastingSpell:					
					inputMagic.SelectUnitToTargetMagic(unitSelected,unitSelect);					
					break;
				default:
					throw new NotImplementedException();
					break;
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
		private Unidad? SelectOwnUnit(Unidad unitToSelect)
		{
			if (UnitsClientManager.Instance.canSelectUnit(unitToSelect.coreUnit.Guid, true))
			{
				return unitToSelect;
			}
			return null;
		}
        private void SelectUnitToMove(Unidad unitSelect)
		{

			if (SelectOwnUnit!=null)
			{
				unitSelect.inputEnabled = true;
				unitSelected = unitSelect;
				UnitsClientManager.Instance.unitSelected = unitSelect.coreUnit;
			}
		}
        private void SelectUnitToCharge(Unidad unitSelect)
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
