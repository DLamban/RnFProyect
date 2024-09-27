using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Core.GeometricEngine
{   
    public static class CollisionCheck
    {
        public static bool checkOverlap(RectangleBB rect1, RectangleBB rect2)
        {
            return !checkSATRectangles(rect1, rect2);
        }
        /// <summary>
        /// Separating Axis Theorem for checking collisions with rectangle, maybe later with polygons
        /// https://jkh.me/files/tutorials/Separating%20Axis%20Theorem%20for%20Oriented%20Bounding%20Boxes.pdf
        /// </summary>
        /// <returns></returns>
        private static bool checkSATRectangles(RectangleBB rect1, RectangleBB rect2)
        {
            bool result = false;
            // check pdf, but on construction
            // check the 4 axes,2 in rect1, 2 in rect2
            // if all are false, there's none separator axis and so are colliding            
            result = checkAxis(rect1.XVector, rect1, rect2);
            result = result || checkAxis(rect1.YVector, rect1, rect2);
            result = result || checkAxis(rect2.XVector, rect2, rect1);
            result = result || checkAxis(rect2.YVector, rect2, rect1);
            
            return result;
        }
        private static bool checkAxis(Vector2 axisRect, RectangleBB rect1, RectangleBB rect2) { 
            float projectiontotal = rect1.projectionLenght(axisRect) + rect2.projectionLenght(axisRect);
            Vector2 vectorSeparation = new Vector2(rect2.centerCoord.X - rect1.centerCoord.X, rect2.centerCoord.Y - rect1.centerCoord.Y);
            float projectionVectorSep = Math.Abs(Vector2.Dot(vectorSeparation,axisRect));
            return projectionVectorSep > projectiontotal;            
        }
    }
}
