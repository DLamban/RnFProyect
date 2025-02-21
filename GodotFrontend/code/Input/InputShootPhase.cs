using Core.DB.Models;
using Core.Units;
using Godot;
using GodotFrontend.code.UIOverlay;
using GodotFrontend.UIcode;
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
        private enum ShootRange
        {
            Short,
            Long,
            OutOfRange
        }
        private UnitGodot selectedShooter;
        private UnitGodot selectedTarget;
        protected BattlefieldCursorPosDel battlefieldCursorPosDel;
        // EVENTS
        public event Action<bool> OnShootSelectedToExecute;
        private ShootRange shootRange;
        public InputShootPhase(BattlefieldCursorPosDel _battlefieldCursorPosDel)
        {
            battlefieldCursorPosDel = _battlefieldCursorPosDel;
        }        
        public void clickUnit(UnitGodot unitClicked)
        {
            
            
            if (selectedTarget == null && unitClicked.coreUnit.canShoot)
            {
                selectedShooter = SelectOwnUnit(unitClicked);
                drawShootLine(selectedShooter);
            }
            else if (selectedShooter != null && selectedTarget == null)
            {
                selectedTarget = SelectEnemyUnit(unitClicked);
                if (selectedTarget != null)
                {
                    shootRange = checkRangeEnemyUnit(selectedShooter, selectedTarget);
                    if (shootRange != ShootRange.OutOfRange)
                    {
                        OnShootSelectedToExecute?.Invoke(true);
                    }else
                    {
                        selectedTarget = null;
                    }
                    
                }
            }
            else
            {
                
            }
        }

        private  ShootRange checkRangeEnemyUnit(UnitGodot shooter, UnitGodot target)
        {
            // cehck centers for fast implementation
            float distance = shooter.Position.DistanceTo(target.Position);
            Weapon rangedWeapon = shooter.coreUnit.Troop.Weapons.FirstOrDefault(w => w.Range >0);
            float inch = 0.254f;
            float rangeindm = (float)(rangedWeapon.Range * inch);
            drawDebugLine(shooter, shooter.Position, target.Position, Color.Color8(255, 0, 0, 255));
            if (distance < rangeindm)
            {
                if (distance <rangeindm/2)
                {
                    return ShootRange.Short;
                }
                else
                {
                    return ShootRange.Long;
                }
            }
            return ShootRange.OutOfRange;
        }

        private void drawShootLine(UnitGodot selectedUnit)
        {
            BattlefieldOverlay.Instance.drawShootLine(selectedUnit);
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

        public async void executeShooting()
        {
            int dicesToThrow = 0;   
            var troopsThatCanShoot = selectedShooter.coreUnit.Troops.FindAll(t => t.Weapons.FirstOrDefault(w => w.Range > 0) != null);
            var weapon = troopsThatCanShoot.First().Weapons.FirstOrDefault(w => w.Range > 0);
            int weaponStrenght = (int)(weapon.IsStrengthFlat !=0 ? weapon.Strength : troopsThatCanShoot.First().Strength + weapon.Strength);
            dicesToThrow = troopsThatCanShoot.Count();
            int shootingSkill = 7 - troopsThatCanShoot.First().Shooting;
            //dicesToThrow = selectedShooter.coreUnit.Troops.Count(t => t.Weapons.FirstOrDefault(w => w.Range > 0));
            if (shootRange == ShootRange.Long)
            {
                shootingSkill = shootingSkill - 1;
            }

            List<int> resHits = await DiceThrower.Instance.ThrowDicesThreshold(10, $"Shooting at {shootingSkill}", shootingSkill);
            List<int> resToWound = await DiceThrower.Instance.ThrowDices(resHits.Count, $"Wounding with strenght {weaponStrenght}");

            selectedTarget.coreUnit.woundUnit(resToWound, weaponStrenght, new List<Core.Rules.BaseRule>());


        }
        public void CustomProcess(double delta)
        {
            
        }

        
        private void drawDebugLine(UnitGodot parent, Vector3 origin, Vector3 end, Color color)
        {
            origin = parent.ToLocal(origin);
            end = parent.ToLocal(end);
            origin.Z += 0.1f;
            end.Z += 0.1f;
            var immediateMesh = new ImmediateMesh();
            immediateMesh.SurfaceBegin(Mesh.PrimitiveType.Lines);

            immediateMesh.SurfaceAddVertex(origin);
            immediateMesh.SurfaceAddVertex(end);

            immediateMesh.SurfaceEnd();

            var meshInstance = new MeshInstance3D();
            meshInstance.Mesh = immediateMesh;

            var material = new StandardMaterial3D();
            material.AlbedoColor = color;
            meshInstance.MaterialOverride = material;
            //  TEMPORAL ERASED
            parent.AddChild(meshInstance);
        }
    }
}
