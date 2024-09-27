using Core.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Core.GeometricEngine
{
    // where a unit has hit, keep the info of hit position, distance from unit,
    // and target unit/environment hitted
    public class HitCollider
    {
        public CollidedObject HitObject { get; set; }
        public Vector2 hitpoint {  get; set; }
        public float distance { get; set; }
        
        public HitCollider(CollidedObject collidedObject,Vector2 hitpoint, float distance) { 
        
            HitObject = collidedObject;
            this.hitpoint = hitpoint;
            this.distance = distance;
        }
    }
}
