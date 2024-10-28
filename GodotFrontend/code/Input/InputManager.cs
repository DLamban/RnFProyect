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
        private SpellTarget? currentSpellTarget;
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
        public override void _Ready()
        {
            // we get the viewport and spacesate to pass as parameter so we can raycast the battlefield
            Viewport viewport = GetViewport();
            PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;
            inputMovePhase = new InputMovePhase( viewport, spaceState);
            PlayerInfoSingleton.Instance.battleStateManager.OnBattleStateChanged += OnBattleStateChanged;
        }
        public void SpellSelection(SpellTarget spellTarget)
        {
            currentSpellTarget = spellTarget;
            inputState = InputState.CastingSpell;
        }
        private void OnBattleStateChanged(object sender, BattleState currentBattleState)
        {
            if (lastUnitSelected!=null) lastUnitSelected.inputEnabled = false;
            switch (currentBattleState)
            {
                case BattleState.move:
                    setUpMovementInputPhase();
                    break;
                default:
                    Debug.WriteLine("state not implemented");
                    break;
            }
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

            switch (battleState)
            {
                case BattleState.move:
                    lastUnitSelected = unitSelect;
                    SelectUnitToMove(unitSelect);
                    break;
                case BattleState.strategic:
                    if (inputState == InputState.CastingSpell)
                    {
                        SelectUnitToTargetMagic(unitSelect, currentSpellTarget.Value);
                    }                    
                    break;
                default:
                    throw new NotImplementedException();
                    break;
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
        private void SelectUnitToTargetMagic(Unidad unitSelect, SpellTarget spellTarget)
        {
            if (spellTarget == SpellTarget.OwnTroops)
            {
                if (UnitsClientManager.Instance.canSelectUnit(unitSelect.coreUnit.Guid, true))
                {
                    unitSelected = unitSelect;
                }
            } else if ( spellTarget == SpellTarget.EnemyTroops)
            {
                if (UnitsClientManager.Instance.canSelectUnit(unitSelect.coreUnit.Guid, false))
                {
                    unitSelected = unitSelect;
                }
            }
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
