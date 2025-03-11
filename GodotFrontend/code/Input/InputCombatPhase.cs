using Core.GameLoop;
using Core.Units;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GodotFrontend.code.Input.InputManager;

namespace GodotFrontend.code.Input
{
    public class InputCombatPhase : ISubInputManager
    {
        private BattlefieldCursorPosDel battlefieldCursorPosDel;
        private UnitGodot selectedUnit;
        public Action<bool> OnSelectUnitToCombat;
        public InputCombatPhase(BattlefieldCursorPosDel _battlefieldCursorPosDel)
        {
            battlefieldCursorPosDel = _battlefieldCursorPosDel;
        }
        public void clickUnit(UnitGodot unitClicked)
        {
            selectedUnit = SelectOwnUnit(unitClicked);
            if (selectedUnit != null)
            {
                Debug.WriteLine("Unit selected");
                OnSelectUnitToCombat?.Invoke(true);
                //battlefieldCursorPosDel(selectedUnit.coreUnit.Position);
            }
            else
            {
                OnSelectUnitToCombat?.Invoke(false);
            }
        }
        public void executeCombat()
        {
            //check unit in combat
            BaseUnit coreunit = selectedUnit.coreUnit.temporalCombatVars.inCombatUnits.FirstOrDefault();
            Combat.singleCombat(selectedUnit.coreUnit, coreunit);
        }
        private UnitGodot? SelectOwnUnit(UnitGodot unitToSelect)
        {
            //bool isShooter = unitToSelect.coreUnit.
            if (UnitsClientManager.Instance.canSelectUnit(unitToSelect.coreUnit.Guid, true))
            {
                return unitToSelect;
            }
            return null;
        }
        public void CustomProcess(double delta)
        {
        }

        internal void finishCombatSubphase(UnitRenderCreator unitRenderCreator)
        {
            unitRenderCreator.enableTroops();
        }
    }
}
