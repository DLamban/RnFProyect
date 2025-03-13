using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using System.Text;
using System.Threading.Tasks;
using Core.Rules;
using System.Text.Json.Serialization;
using Core.GeometricEngine;
using Core.GameLoop;
using System.Numerics;
using Core.Magic;
using Core.List;
using System.Diagnostics;
namespace Core.Units
{
    using static Core.GeometricEngine.GeometricTypedef;
    using Hit = Tuple<Vector2, float>;
    public struct UnitBorders
    {
        public RectSegment frontLine;
        public RectSegment backLine;
        public RectSegment leftLine;
        public RectSegment rightLine;
    }
    public struct ArcSeparatorStruct
    {
        public Vector2 dir;
        public Vector2 origin;
        public Vector3 line;
    }
    public enum ArcSeparatorName
    {
        point00,
        point01,
        point11,
        point10
    }
    public class BaseUnit
    {
        
        // Characteristics of the unit
        public string Race { get; set; }
        public string Name { get; set; }
        public Guid Guid { get; set; }
        public int UnitCount { get { return  Troops.Count; } }
        public int Points { get; set; }
        /// <summary>
        /// The width in troops count
        /// </summary>
        public int TroopsWidth { get; set; }
        public Formation_type formationType { get; set; }
        public string Type { get; set; }
        public string Category { get; set; }
        public int Minimum { get; set; }
        public List<string> Weapons { get; set; }
        public Vector2 centerTroop { get
            {
                return Transform.localToGlobalTransforms(sizeEnclosedRectangledm.X / 2, -(sizeEnclosedRectangledm.Y / 2));
            }
        }    
        public Size sizeEnclosedRectangle { get; set; }
        /// <summary>
        /// Rectangle but in dm size and float because of the precision
        /// </summary>
        public Vector2 sizeEnclosedRectangledm { get
            {
                return new Vector2((float)sizeEnclosedRectangle.Width / 100, (float)sizeEnclosedRectangle.Height / 100);
            }            
        }
        public RectangleBB rectangleBB { get { return new RectangleBB(sizeEnclosedRectangledm.X, sizeEnclosedRectangledm.Y, Transform);}}
        public Dictionary<ArcSeparatorName,ArcSeparatorStruct> ArcSeparators {
            get
            {
                Dictionary<ArcSeparatorName, ArcSeparatorStruct> _arcSeparators = new Dictionary<ArcSeparatorName, ArcSeparatorStruct>();
                // ********************************************
                // WATCH  OUT!   in the line equation:
                //       C = dy*x - dx*y
                // *******************************************
                // FOLLOW CONVENTIONS

                
                // *****************************************
                // Point 0,0 135 degrees
                // *****************************************
                Vector2 vec135 = Vector2.Normalize(new Vector2(-1, 1));
                Vector2 ori135 = new Vector2(0, 0);
                _arcSeparators.Add(ArcSeparatorName.point00, new ArcSeparatorStruct()
                {
                    dir = Transform.localVectorToWorldTransform(new Vector2(vec135.X, vec135.Y)),
                    origin = Transform.localToGlobalTransforms(ori135.X, ori135.Y),
                    line = Transform.localEqPointDirToWorld(ori135, new Vector2(-vec135.Y, vec135.X))
                });

                // *****************************************
                // 0,1  45 degrees
                // *****************************************                
                Vector2 vec45 = Vector2.Normalize(new Vector2(1, 1));
                Vector2 ori45 = new Vector2(sizeEnclosedRectangledm.X, 0);
                _arcSeparators.Add(ArcSeparatorName.point01, new ArcSeparatorStruct()
                {
                    dir = Transform.localVectorToWorldTransform(new Vector2(vec45.X, vec45.Y)),
                    origin = Transform.localToGlobalTransforms(ori45.X, ori45.Y),
                    line = Transform.localEqPointDirToWorld(ori45, new Vector2(-vec45.Y, vec45.X))
                });

                // *****************************************
                // 1,1  315 degrees
                // *****************************************                
                Vector2 vec315 = Vector2.Normalize(new Vector2(1, -1));
                Vector2 ori315 = new Vector2(sizeEnclosedRectangledm.X, -sizeEnclosedRectangledm.Y);
                _arcSeparators.Add(ArcSeparatorName.point11, new ArcSeparatorStruct()
                {
                    dir = Transform.localVectorToWorldTransform(new Vector2(vec315.X, vec315.Y)),
                    origin = Transform.localToGlobalTransforms(ori315.X, ori315.Y),
                    line = Transform.localEqPointDirToWorld(ori315, new Vector2(-vec315.Y, vec315.X))
                });

                // *****************************************
                // 1,0  225 degrees
                // *****************************************                
                
                Vector2 vec225 = Vector2.Normalize(new Vector2(-1, -1));
                Vector2 ori225 = new Vector2(0, -sizeEnclosedRectangledm.Y);
                _arcSeparators.Add(ArcSeparatorName.point10, new ArcSeparatorStruct()
                {
                    dir = Transform.localVectorToWorldTransform(new Vector2(vec225.X, vec225.Y)),
                    origin = Transform.localToGlobalTransforms(ori315.X, ori315.Y),
                    line = Transform.localEqPointDirToWorld(ori225, new Vector2(-vec225.Y, vec225.X))
                });
                return _arcSeparators;
            }
        }   

        public List<Point> polygonPoints { get; set; }
        public List<Vector2> enclosedPolygonPointsdm { get; set; }
        public List<Vector2> polygonPointsWorld { 
            get {
                List<Vector2> _polygonpointsworld = new List<Vector2>();
                foreach ( Vector2 polygonPoint in enclosedPolygonPointsdm)
                {
                    _polygonpointsworld.Add(Transform.localToGlobalTransforms(polygonPoint.X, polygonPoint.Y));
                }
                return _polygonpointsworld;
            }
        }
        public UnitBorders unitBorders {
            get
            {
                // We made the points clockwise
                UnitBorders _unitBorders = new UnitBorders();
                _unitBorders.frontLine = frontLinePoints;
                _unitBorders.rightLine = new RectSegment(new Vector2(sizeEnclosedRectangledm.X, 0), new Vector2(sizeEnclosedRectangledm.X, sizeEnclosedRectangledm.Y));
                _unitBorders.backLine = new RectSegment(new Vector2(sizeEnclosedRectangledm.X, sizeEnclosedRectangledm.Y), new Vector2(0, sizeEnclosedRectangledm.Y));
                _unitBorders.leftLine = new RectSegment(new Vector2(0, sizeEnclosedRectangledm.Y), new Vector2(0, 0));
                
                return _unitBorders;
            }                
        }
        public UnitBorders unitBordersWorld
        {
            get
            {
                UnitBorders _unitBorders = new UnitBorders();
                _unitBorders.frontLine = new RectSegment(Transform.localToGlobalTransforms(unitBorders.frontLine.Start), Transform.localToGlobalTransforms(unitBorders.frontLine.End));
                _unitBorders.backLine = new RectSegment(Transform.localToGlobalTransforms(unitBorders.backLine.Start), Transform.localToGlobalTransforms(unitBorders.backLine.End));
                _unitBorders.leftLine = new RectSegment(Transform.localToGlobalTransforms(unitBorders.leftLine.Start), Transform.localToGlobalTransforms(unitBorders.leftLine.End));
                _unitBorders.rightLine = new RectSegment(Transform.localToGlobalTransforms(unitBorders.rightLine.Start), Transform.localToGlobalTransforms(unitBorders.rightLine.End));
                return _unitBorders;
            }
        }
        public RectSegment frontLinePoints { 
            get { 
                Vector2 left = new Vector2(0,0);
                Vector2 right = new Vector2(sizeEnclosedRectangle.Width/100f, 0);
                return new RectSegment(left,right);
            } 
        }
        public RectSegment worldFrontLinePoints
        {
            get
            {
                return new RectSegment(Transform.localToGlobalTransforms(frontLinePoints.Start.X,frontLinePoints.Start.Y), Transform.localToGlobalTransforms(frontLinePoints.End.X,frontLinePoints.End.Y));
            }
        }
        public AffineTransformCore Transform { get; set; }
        public Formation_type Formation_Type { get; set; }
        public float MaximumChargedm {// TODO:implement swiftstride 
            get {
                return (6 * 2.54f) / 10;
            }
        }
        public BaseTroop Troop { get; set; }
        public List<BaseTroop> Troops { get; set; }
        public List<string> SpecialRules { get; set; }
        public bool canShoot
        {
            get
            {
                foreach (BaseTroop baseTroop in Troops)
                {
                    if (baseTroop.canShoot) { return true; }
                }
                return false;
            }
        }
        #region EVENTS&DELEGATES
        // EVENTS
        public event Action<BaseUnit,int> OnDeathTroops;
        //delegates
        public delegate Task<List<int>> DiceThrowerTaskDelegate(int numberdices, string dicePhase, int dicetype=6);
        private DiceThrowerTaskDelegate DiceThrowerTaskDel;

        #endregion
        #region TEMPORALCOMBAT 
        public TemporalCombatVars temporalCombatVars { get; set; }
        #endregion
        // This constructor is for information purposes, creating a unit with a single troop
        // as a dummy unit to get da information
        [JsonConstructor]
        public BaseUnit(string race, string name, string type, string category, List<string> weapons, int minimum,List<string> specialRules, BaseTroop troop) {
            Race = race;
            Name = name;
            Type = type;
            Category = category;
            Troop = troop;
            SpecialRules = specialRules;
            Minimum = minimum;
            Weapons = weapons;
        }

        // Unit constructor
        public BaseUnit(string race, string name,int troopsWidth, Formation_type formation_Type, List<string> specialRules, List<BaseTroop> troops)
        {
            Race = race;
            
            Name = name;
            TroopsWidth = troopsWidth;
            Formation_Type = formation_Type;
            SpecialRules = specialRules;
            Troops = troops;
            // take the last because first is the characters
            Troop = troops[UnitCount-1];
            formationType = formation_Type;
            reformTroops();
            restartCombatState();
        }
        // Single Char unit
        public BaseUnit(string race, string name, Character character) { 
            Race = race;
            Name = name;
            TroopsWidth = 1;
            Troop = character;
            Troops = new List<BaseTroop>();
            Troops.Add(character);
            reformTroops();
            restartCombatState();
            
        }
        public void reformTroops()
        {
            if (formationType == Formation_type.CLOSE_ORDER)
            {
                //Build close order unit
                CloseOrder unitForm = new CloseOrder(TroopsWidth, Troops);
                enclosedPolygonPointsdm = unitForm.calculateEnclosedPolygondm(TroopsWidth, Troops);
                //polygonPoints = unitForm.calculateEnclosedPolygondm;

            }
            // calculate the size of the rectangle, with characters will be more complicated
            sizeEnclosedRectangle = new Size(TroopsWidth * Troop.Size.Width, (int)Math.Ceiling((float)Troops.Count / TroopsWidth) * Troop.Size.Height);
            Transform = new AffineTransformCore(1, 0, 0, 1, 0, 0);
        }
        public void AddCharacter(Character character)
        {
            Troops.Insert(0,character);

            reformTroops();
        }
        public void vinculateDiceThrower(DiceThrowerTaskDelegate _diceThrowDel)
        {
            DiceThrowerTaskDel = _diceThrowDel;
        }
        public void restartCombatState()
        {
            temporalCombatVars = new TemporalCombatVars();            
            temporalCombatVars.distanceRemaining = Troop.Movement;
        }
        // to check the charge we're gonna create a rectangle and check the overlapping with other rectangles, 
        // and then, we see, maybe increase accuracy and/or check only the closest hit
        public List<HitCollider> checkCharge()
        {
            // first we need the width of the unit
            // the height will be the distance remaining+charge distance
            // and the transform the unit transform
            List<HitCollider> hittedColliders = new List<HitCollider>();
            float maximumCharge = temporalCombatVars.distanceRemaining + MaximumChargedm;
            RectangleBB rectangleBB = new RectangleBB(sizeEnclosedRectangledm.X, -1*maximumCharge, Transform);

            List<CollidedObject> collisions =  UnitsClientManager.Instance.checkRectangleCollision(rectangleBB).FindAll(collided=>collided.unit !=this);// remove himself from the list
            // we check every corner of the rectangles collided, so we found the closest corner to the unit
            RectSegment rectSegment = new RectSegment(Transform.localToGlobalTransforms(0,0),Transform.localToGlobalTransforms(sizeEnclosedRectangledm.X,0));

            foreach(CollidedObject collision in collisions)
            {
                Tuple<Vector2, float> collisionPointAndDist = collision.checkClosestPoint(rectSegment);
                HitCollider hit = new HitCollider(collision,collisionPointAndDist.Item1,collisionPointAndDist.Item2);
                hittedColliders.Add(hit);
                //collision.checkProjectionOverRectSegment(rectSegment.Start,rectSegment.End);
            }
            return hittedColliders;
        }
      
        /// <summary>
        /// This method is to get the troops that are in direct combat
        /// only touching troops
        /// WE NEED TO CALCULATE PRECISE POSITIONS oof
        /// </summary>
        /// <param name="widthFrontLine"></param>
        /// <returns></returns>
        private List<BaseTroop> getInDirectCombatTroops(CombatSide combatSide, BaseUnit enemyUnit)
        {
            
            List<BaseTroop> inDirectCombatTroops = new List<BaseTroop>();

            // we do calculations in local space of the main unit, lot easier
            RectSegment lineEnemy;
            switch (combatSide)
            {
                case CombatSide.FRONT:
                    lineEnemy = new RectSegment(Transform.GlobalToLocalTransforms(enemyUnit.unitBordersWorld.frontLine.Start), Transform.GlobalToLocalTransforms(enemyUnit.unitBordersWorld.frontLine.End));
                    break;
            }
                      
            
            return null;


        }
        /// <summary>
        /// this is not as evident as it seems, because they are special rules
        /// for example, spears weapon allow second rank to fight
        /// also, it deppends on the orientation of the fight
        /// and the if is flanked, reared ....
        /// </summary>
        /// <returns>All the troops able to attack, and maybe supporting attacks</returns>
        public List<BaseTroop> getAttackingTroops(List<BaseUnit> unitsInContact)
        {
            List<BaseTroop> attackingTroops = new List<BaseTroop>();
            foreach(BaseUnit unit in unitsInContact)
            {
                var combatSide = Combat.calcCombatSide(this, unit);

                attackingTroops = getInDirectCombatTroops(combatSide,unit);

            }
            return attackingTroops;
        }
        public int hitUnit(List<int> diceValues, int dexAttacker, List<BaseRule> specialRules)
        {
            int hits = ResolveDiceThrow.resolveToHit(diceValues, dexAttacker, this.Troop.Dexterity, specialRules);
            return hits;
        }
        public void woundUnit(List<int> diceValues, int strenght, List<BaseRule> specialRules)
        {
            int wounds = ResolveDiceThrow.resolveToWound(diceValues, strenght, Troop.Resistance);
            if (wounds > 0) {
                confirmWounds(wounds, null, 0);
            }
            
            
        }
        public async void confirmWounds(int wounds, List<BaseRule> specialRules, int ap)
        {
            List<int> savingThrow = await DiceThrowerTaskDel(wounds, "armour save");
            int confirmedWounds = ResolveDiceThrow.armourSave(wounds, savingThrow, ap, Troop.Armour);
            ApplyWoundUnit(confirmedWounds, specialRules);
        }
        public void DeleteDeathUnits()
        {
            Troops.RemoveAll(troop => troop.Wounds == 0);
        }
        public void ApplyWoundUnit(int wounds, List<BaseRule> specialRules = null)
        {
            int deathunits = 0;
            for (int i = 0; i < wounds; i++)
            {
                // eat dat wound
                // wound the first rank
                // avoid characters
                Func<BaseTroop, bool> hasWounds = troop => troop.Wounds > 0;
                Func<BaseTroop, bool> isNotCharacter = troop => troop.GetType() != typeof(Character);


                Func<BaseTroop, bool> condition = troop => hasWounds(troop);
                
                if (Troops.Count > 1)
                {
                    condition = troop => hasWounds(troop) && isNotCharacter(troop);
                }                
                BaseTroop? troopToWound = Troops.FirstOrDefault(condition);

                if (troopToWound == null)
                {
                    // probably we should destroy the unit!
                    Debug.WriteLine("ERROR getting troop from unit");
                    throw new Exception("ERROR getting troop from unit");
                }


                troopToWound.Wounds--;
                if (troopToWound.Wounds == 0)
                {
                    deathunits++;
                }
            }          
            OnDeathTroops(this,deathunits);
        }
    }
}
