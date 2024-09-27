using Core.GeometricEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Units
{
    public class UnitsClientManager
      
    {
        private Action finishLoadList;
        private static readonly UnitsClientManager instance =new UnitsClientManager();
        public Action<Guid> unitMovedNet;
        public static UnitsClientManager Instance
        {
            get
            {
                return instance;
            }
        }
        public Dictionary<string, BaseUnit> unitsPlayer {get; set;}
        public Dictionary<string, BaseUnit> unitsEnemy  {get; set;}
        
        public BaseUnit? unitSelected { get; set;}  
        
        public void setLoadedListEvent(Action loadedList)
        {
            finishLoadList = loadedList;
        }
        public void unitsLoaded()
        {
            finishLoadList();
        }

        public UnitsClientManager()
        {
            unitsPlayer = new Dictionary<string, BaseUnit>();
            unitsEnemy = new Dictionary<string, BaseUnit>();
        }
        public void networkMoveUnit(Guid unitGuid, AffineTransformCore newAffine)
        {
            BaseUnit unit = findUnit(unitGuid);
            unit.Transform.matrixTransform = newAffine.matrixTransform;
            unitMovedNet(unitGuid);
        }
        private BaseUnit findUnit(Guid guid)
        {
            if (unitsPlayer.TryGetValue(guid.ToString(), out BaseUnit unit))
            {
                return unit;
            }
            else if (unitsEnemy.TryGetValue(guid.ToString(), out BaseUnit unitenemy))
            {
                return unitenemy;
            }
            throw new Exception("Unit not found");
        }
        public void addAllPlayerUnits(List<BaseUnit> baseUnits)
        {
            foreach (BaseUnit unit in baseUnits)
            {
                addPlayerUnit(unit);
            }
        }

        public void addAllEnemyUnits(List<BaseUnit> baseUnits)
        {
            foreach (BaseUnit unit in baseUnits)
            {
                addEnemyUnit(unit);
            }
        }        

        public void addPlayerUnit(BaseUnit unit)
        {
            // We will use guid as unique key for unit
            unitsPlayer[unit.Guid.ToString()] = unit;            
        }        
        public void addEnemyUnit(BaseUnit unit)
        {
            // We will use guid as unique key for unit                        
            unitsEnemy[unit.Guid.ToString()] = unit;
        }
        
        public void removePlayerUnit(Guid unitGuid)
        {
            unitsPlayer.Remove(unitGuid.ToString());
        }
        public void removeEnemyUnit(Guid unitGuid)
        {
            unitsEnemy.Remove(unitGuid.ToString());
        }

        public bool canSelectUnit(Guid unitGuid, bool playerTurn = true)// maybe implement hotseat later
        {
            if(playerTurn)
            {
                if (unitsPlayer.ContainsKey(unitGuid.ToString()))
                {
                    return true;
                }
            }
            else
            {

            }
            return false;
        }
        public List<CollidedObject> checkRectangleCollision(RectangleBB collideRectangle)
        {
            bool result = false;
            List<CollidedObject> unitsCollided = new List<CollidedObject>();
            if (checkCollisionWorld(collideRectangle)) unitsCollided.Add(new CollidedObject(CollidedObject.CollidedType.world)); // probably return collision hit and object collided or something
            foreach (KeyValuePair<string, BaseUnit> unitToCheck in unitsPlayer)
            {
                if (checkCollisionBetweenUnits(collideRectangle, unitToCheck.Value.rectangleBB))
                {
                    result = true;
                    unitsCollided.Add(new CollidedObject(CollidedObject.CollidedType.unit, unitToCheck.Value, CollidedObject.Owner.player));
                }
            }
            foreach (KeyValuePair<string, BaseUnit> unitToCheck in unitsEnemy)
            {
                if (checkCollisionBetweenUnits(collideRectangle, unitToCheck.Value.rectangleBB))
                {
                    result = true;
                    unitsCollided.Add(new CollidedObject(CollidedObject.CollidedType.unit, unitToCheck.Value, CollidedObject.Owner.enemy));
                }
            }
            return unitsCollided;
        }
        // We start as a very basic collision test and improve performance as needed
        public bool checkGeneralCollision(BaseUnit unit)
        {
            bool result = false;
            if (checkCollisionWorld(unit.rectangleBB)) return true;// probably return collision hit and object collided or something
            foreach (var unitToCheck in unitsPlayer.Concat(unitsEnemy))
            {
                if (unitToCheck.Value.Equals(unit)) continue;

                result = result || checkCollisionBetweenUnits(unit.rectangleBB, unitToCheck.Value.rectangleBB);
            }
            return result;
        }
        // TODO: Check world collisions;
        private bool checkCollisionWorld(RectangleBB collideRectangle)
        {
            return false;
        }
        // develop a simple collision check and we try to filter the units to check
        private bool checkCollisionBetweenUnits(RectangleBB first, RectangleBB second)
        {
            return CollisionCheck.checkOverlap(first,second);            
        }
    }
}
