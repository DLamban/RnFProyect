using Core.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GodotFrontend.code.Input.InputManager;

namespace GodotFrontend.code.Input
{
    public class InputShootPhase:ISubInputManager
    {
        private UnitGodot selectedShooter;
        private UnitGodot selectedTarget;
        protected BattlefieldCursorPosDel battlefieldCursorPosDel;
        public InputShootPhase(BattlefieldCursorPosDel _battlefieldCursorPosDel)
        {
            battlefieldCursorPosDel = _battlefieldCursorPosDel;
        }        
        public void clickUnit(UnitGodot unitClicked)
        {
            
            
            if (selectedTarget == null && unitClicked.coreUnit.canShoot)
            {
                selectedShooter = SelectOwnUnit(unitClicked);

            }
            else
            {
                
            }
        }
        private void drawShootLine(UnitGodot selectedUnit)
        {
            
        }
        private UnitGodot? SelectOwnUnit(UnitGodot unitToSelect)
        {
            //bool isShooter = unitToSelect.coreUnit.
            if (UnitsClientManager.Instance.canSelectUnit(unitToSelect.coreUnit.Guid, true) )
            {
                return unitToSelect;
            }
            return null;
        }
        private UnitGodot? SelectEnemyUnit(UnitGodot unitToSelect)
        {
            if (UnitsClientManager.Instance.canSelectUnit(unitToSelect.coreUnit.Guid, false))
            {
                return unitToSelect;
            }
            return null;
        }
        public void CustomProcess(double delta)
        {
            
        }
    }
}
