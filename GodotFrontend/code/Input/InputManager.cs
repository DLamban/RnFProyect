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
        private Unidad unitSelected;
        private Unidad lastUnitSelected;
        private InputMovePhase inputMovePhase;
        public InputMagic inputMagic;
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
            get { 
                return PlayerInfoSingleton.Instance.battleStateManager.currentState; 
            }
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
            // get the node of cursor effects
            MeshInstance3D cursorEffect = GetNode<MeshInstance3D>("CursorEffect") as MeshInstance3D;

            // create the subinput child managers
            inputMovePhase = new InputMovePhase(getBattlefieldCursorPosDel);
            inputMagic = new InputMagic(getBattlefieldCursorPosDel, cursorEffect);     
            
            // because is the first time, remember to init the state
            setUpMagicInputPhase();
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
                    setUpMovementInputPhase();
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
            currentStateProccess = inputMagic.CustomProcess;
        }
        private void setUpMovementInputPhase()
        {
            inputState = InputState.Movement;
            currentStateProccess = inputMovePhase.CustomProcess;
        }
        public void selectUnit(Unidad unitSelect)
        {
            if (lastUnitSelected != null)
            {
                lastUnitSelected.inputEnabled = false;
            }
            lastUnitSelected = unitSelect;
            switch (battleState)
            {
                case BattleState.move:
                    
                    SelectUnitToMove(unitSelect);
                    break;
                case BattleState.strategic:
                    if (inputState == InputState.CastingSpell)
                    {
                        inputMagic.SelectUnitToTargetMagic(unitSelected,unitSelect);
                    }                    
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
            currentStateProccess(delta);
        }
        #region MOVEMENT
        public void onArrowClick(Node camera, InputEvent @event, Vector3 position, Vector3 normal, long shapeIdx, Node collider)
        {
            inputMovePhase.onArrowClick(camera,@event,position,normal,shapeIdx,collider);
        }

        private void SelectUnitToMove(Unidad unitSelect)
        {
            if (inputState == InputState.Movement)
            {
                if (UnitsClientManager.Instance.canSelectUnit(unitSelect.coreUnit.Guid,true))
                {
                    unitSelect.inputEnabled = true;
                    unitSelected = unitSelect;
                    UnitsClientManager.Instance.unitSelected = unitSelect.coreUnit;
                }
            }
        }
        #endregion

    }
}
