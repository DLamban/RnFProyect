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
namespace Core.Units
{
    using Hit = Tuple<Vector2, float>;
    public class BaseUnit
    {
        
        // Characteristics of the unit
        public string Name { get; set; }
        public Guid Guid { get; set; }
        public int UnitCount { get; set; }
        public int Points { get; set; }
        /// <summary>
        /// The width in troops count
        /// </summary>
        public int TroopsWidth { get; set; }
        public string Type { get; set; }
        public string Category { get; set; }
        public int Minimum { get; set; }
        public List<string> Weapons { get; set; }
        public Size sizeEnclosedRectangle { get; set; }
        /// <summary>
        /// Rectangle but in dm size and float because of the precision
        /// </summary>
        public Vector2 sizeEnclosedRectangledm { get
            {
                return new Vector2((float)sizeEnclosedRectangle.Width / 100, (float)sizeEnclosedRectangle.Height / 100);
            }            
        }
        public RectangleBB rectangleBB { get { return new RectangleBB(sizeEnclosedRectangledm.X, sizeEnclosedRectangledm.Y, Transform);}
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

        // Combat temporal variables
        public List<Spell> spellsAffecting = new List<Spell>();
        public CombatSide CombatSide { get; set; }
        public bool isCharging { get; set; }
        public bool isCharged { get; set; }
        public float distanceRemaining { get;set; }
        // This constructor is for information purposes, creating a unit with a single troop
        // as a dummy unit to get da information
        [JsonConstructor]
        public BaseUnit(string name, string type, string category, List<string> weapons, int minimum,List<string> specialRules, BaseTroop troop) { 
            Name = name;
            Type = type;
            Category = category;
            Troop = troop;
            SpecialRules = specialRules;
            Minimum = minimum;
            Weapons = weapons;
        }

        // Unit constructor
        public BaseUnit(string name,int troopsWidth, Formation_type formation_Type, List<string> specialRules, List<BaseTroop> troops)
        {
            UnitCount = troops.Count;
            Name = name;
            TroopsWidth = troopsWidth;
            Formation_Type = formation_Type;
            SpecialRules = specialRules;
            Troops = troops;
            // take the last because first is the characters
            Troop = troops[UnitCount-1];
            if (formation_Type == Formation_type.CLOSE_ORDER)
            {
                //Build close order unit
                CloseOrder unitForm = new CloseOrder(TroopsWidth, troops);
                enclosedPolygonPointsdm = unitForm.calculateEnclosedPolygondm(troopsWidth,troops);
                //polygonPoints = unitForm.calculateEnclosedPolygondm;
                
            }
            // calculate the size of the rectangle, with characters will be more complicated
            sizeEnclosedRectangle = new Size(TroopsWidth * Troop.Size.Width, (int)Math.Ceiling((float)troops.Count / TroopsWidth) * Troop.Size.Height);
            Transform = new AffineTransformCore(1, 0, 0, 1, 0, 0);            
            restartCombatState();
        }
        public void restartCombatState()
        {
            distanceRemaining = Troop.MovementDm;
        }
        // to check the charge we're gonna create a rectangle and check the overlapping with other rectangles, 
        // and then, we see, maybe increase accuracy and/or check only the closest hit
        public List<HitCollider> checkCharge()
        {
            // first we need the width of the unit
            // the height will be the distance remaining+charge distance
            // and the transform the unit transform
            List<HitCollider> hittedColliders = new List<HitCollider>();
            float maximumCharge = distanceRemaining + MaximumChargedm;
            RectangleBB rectangleBB = new RectangleBB(sizeEnclosedRectangledm.X, -1*maximumCharge, Transform);

            List<CollidedObject> collisions =  UnitsClientManager.Instance.checkRectangleCollision(rectangleBB).FindAll(collided=>collided.unit !=this);// remove himself from the list
            // we check every corner of the rectangles collided, so we found the closest corner to the unit
            Tuple<Vector2, Vector2> rectSegment = new Tuple<Vector2, Vector2>(Transform.localToGlobalTransforms(0,0),Transform.localToGlobalTransforms(sizeEnclosedRectangledm.X,0));

            foreach(CollidedObject collision in collisions)
            {
                Tuple<Vector2, float> collisionPointAndDist = collision.checkClosestPoint(rectSegment);
                HitCollider hit = new HitCollider(collision,collisionPointAndDist.Item1,collisionPointAndDist.Item2);
                hittedColliders.Add(hit);
                //collision.checkProjectionOverRectSegment(rectSegment.Item1,rectSegment.Item2);
            }
            return hittedColliders;
        }
        
        public void WoundUnit(int wounds, List<string> specialRules = null)
        {
            for (int i = 0; i < wounds; i++)
            {
                // eat dat wound
                // wound the first rank
                BaseTroop? firstTroop = Troops.FirstOrDefault(troop=>troop.Wounds>0);
                // is is null, we finish da troops :)
                if (firstTroop!=null) firstTroop.Wounds--;
                
            }            
        }
    }
}
