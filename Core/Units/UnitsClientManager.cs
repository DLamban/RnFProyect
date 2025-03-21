using Core.GeometricEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
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
        public BaseUnit findUnitByName(string name)
        {
            foreach (KeyValuePair<string, BaseUnit> unit in unitsPlayer.Concat(unitsEnemy))
            {
                if (unit.Value.Name == name) return unit.Value;
            }
            return null;
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
        /// <summary>
        /// Beware of hotseat
        /// </summary>
        /// <returns></returns>
        public bool isPlayerUnit(Guid unitGuid)
        {
            if (unitsPlayer.ContainsKey(unitGuid.ToString()))
            {
                return true;
            }
            return false;
        }
        public bool canSelectUnit(Guid unitGuid, bool ownedTroops)
        {
            if (ownedTroops)
            {
                if (unitsPlayer.ContainsKey(unitGuid.ToString()))
                {
                    return true;
                }
            }
            else
            {
                if (unitsEnemy.ContainsKey(unitGuid.ToString()))
                {
                    return true;
                }
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
            foreach (KeyValuePair<string, BaseUnit> unitToCheck in unitsPlayer.Concat(unitsEnemy))
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
        // When pivot, we need to check whats units are in the bounding circle
        // bounded by the rotating unit, defined by the pivot point and the rotating point
        private List<BaseUnit> findUnitsBoundingCircle(BaseUnit unit, Vector2 pivotPoint, Vector2 rotatingPoint)
        {
            float radius = Vector2.Distance(pivotPoint, rotatingPoint);
            List<BaseUnit> units = new List<BaseUnit>();
            foreach (KeyValuePair<string, BaseUnit> unitToCheck in unitsPlayer.Concat(unitsEnemy))
            {
                if (unitToCheck.Value.Equals(unit)) continue;
                if (CollisionCheck.checkCirclePolygonCollision(pivotPoint,radius,unitToCheck.Value.polygonPointsWorld)) units.Add(unitToCheck.Value);
            }
            return units;
        }
        
        public BaseUnit getCloseUnitInRotation(BaseUnit unit, bool isAnchorPointLeft)
        {
            Vector2 pivotPoint;
            Vector2 rotatingPoint;
            // WATCH OUT FOR THE NORMAL OF THE SEGMENT, WE CAN'T CHANGE THE ORDER OF THE POINTS
            if (isAnchorPointLeft)
            {
                pivotPoint = unit.worldFrontLinePoints.Start;
                rotatingPoint = unit.worldFrontLinePoints.End;
            }
            else
            {
                rotatingPoint = unit.worldFrontLinePoints.Start;
                pivotPoint = unit.worldFrontLinePoints.End;                
            }

            List<BaseUnit> units = findUnitsBoundingCircle(unit, pivotPoint, rotatingPoint);
            foreach (BaseUnit unitToCheck in units)
            {
                CollisionCheck.getAngleRotatingCollision(pivotPoint, rotatingPoint, unitToCheck.polygonPointsWorld);
            }
            throw new NotImplementedException();
            return null;
        }
    }
}
