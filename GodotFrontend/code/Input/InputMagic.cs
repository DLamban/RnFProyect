using Core.Magic;
using Core.Units;
using Godot;
using GodotFrontend.UIcode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GodotFrontend.code.Input.InputFSM;
using static GodotFrontend.code.Input.InputManager;

namespace GodotFrontend.code.Input
{
    
    public class InputMagic:ISubInputManager
    {
        public event Action OnOpenConfirmMenu;
        private SpellTarget currentSpellTarget;
        private Spell spellSelected;
        private BattlefieldCursorPosDel battlefieldCursorPosDel;
        private MeshInstance3D cursorEffect;
        private Unidad unitSelected;
        public InputState inputState
        {
            get { return InputFSM.currentState; }
            set
            {
                InputFSM.changeState(value);
            }
        }

        public InputMagic(BattlefieldCursorPosDel _battlefieldCursorPosDel, MeshInstance3D _cursorEffect)
        {
            battlefieldCursorPosDel = _battlefieldCursorPosDel;
            cursorEffect = _cursorEffect;
        }
        public void SelectUnitToTargetMagic(Unidad _unitSelected, Unidad unitSelection)
        {
            unitSelected = _unitSelected;
            if (currentSpellTarget == SpellTarget.OwnTroops)
            {
                if (UnitsClientManager.Instance.canSelectUnit(unitSelection.coreUnit.Guid, true))
                {
                    unitSelected = unitSelection;
                    unitSelected.magicSelectionFX.Visible = true;
                }
            }
            else if (currentSpellTarget == SpellTarget.EnemyTroops)
            {
                if (UnitsClientManager.Instance.canSelectUnit(unitSelection.coreUnit.Guid, false))
                {
                    unitSelected = unitSelection;
                    unitSelected.magicSelectionFX.Visible = true;
                }
            }
            if (unitSelected != null)
            {
                OpenConfirmMenu();
            }
        }
        private void OpenConfirmMenu()
        {
            inputState = InputState.Magic;
            OnOpenConfirmMenu?.Invoke();
        }
        public async void executeSpell()
        {
            if (spellSelected == null) throw new InvalidOperationException("Spell error, not selected");
            if (spellSelected.Type == SpellType.Hex || spellSelected.Type == SpellType.Buff)
            {
                unitSelected.coreUnit.spellsAffecting.Add(spellSelected);
            }
            else
            {
                unitSelected.magicSelectionFX.Visible = false;
                // FIREBALL STATS, JUST TESTING
                int hits = await DiceThrower.Instance.ThrowDicesSum(2, "fireball hits");
                List<int> result = await DiceThrower.Instance.ThrowDices(hits,"wounding");
                unitSelected.coreUnit.woundUnit(result, 4, null);
                
            }
            SpellManager.Instance.spellUsed(spellSelected);
            cursorEffect.Visible = false;
            inputState = InputState.Magic;
        }
        
        public void SpellSelection(SpellTarget spellTarget, Spell spell)
        {
            spellSelected = spell;
            currentSpellTarget = spellTarget;
            inputState = InputState.CastingSpell;
        }
        private Vector3? getBattlefieldCursorPos()
        {
            return battlefieldCursorPosDel();
        }
        public void cancelSpell()
        {
            if (unitSelected != null)
            {
                unitSelected.magicSelectionFX.Visible = false;
                unitSelected = null;
                cursorEffect.Visible = false;
            }
        }
        public void CustomProcess(double delta) {
            if (Godot.Input.IsMouseButtonPressed(MouseButton.Right))
            {
               cancelSpell();
            }
            if (inputState == InputState.CastingSpell)
            {
                // add selection spell fx
                cursorEffect.Visible = true;
                cursorEffect.Position = getBattlefieldCursorPos().Value;
            }
        }
    }
}
