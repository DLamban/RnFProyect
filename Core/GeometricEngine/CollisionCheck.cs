using Core.Units;
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
        private static Tuple<Vector2, Vector2> getSegment(Vector2 start, Vector2 end)
        {
            return new Tuple<Vector2, Vector2>(start, end);
        }
        private static List<Tuple<Vector2, Vector2>> buildSegmentFromPoints(List<Vector2> points)
        {
            List<Tuple<Vector2, Vector2>> segments = new List<Tuple<Vector2, Vector2>>();
            for (int i = 0; i < points.Count; i++)
            {
                if (i == points.Count - 1)
                {
                    segments.Add(getSegment(points[i], points[0]));
                }
                else
                {
                    segments.Add(getSegment(points[i], points[i + 1]));
                }
            }
            return segments;
        }
        public static bool checkCirclePolygonCollision(Vector2 circleCenter, float radius, List<Vector2> polygonPoints)
        {         
            List<Tuple<Vector2, Vector2>> segments = buildSegmentFromPoints(polygonPoints);
            foreach (var segment in segments)
            {
                if (checkCircleSegmentCollision(circleCenter, radius, segment.Item1, segment.Item2))
                {
                    return true;
                } 
            }           
            return false;
        }
        private static bool checkCircleSegmentCollision(Vector2 circleCenter, float radius, Vector2 segmentStart, Vector2 segmentEnd)
        {
            var A = Math.Pow((segmentStart.X - segmentEnd.X),2) + Math.Pow((segmentStart.Y - segmentEnd.Y), 2);
            var B = 2 * (
                (segmentStart.X - segmentEnd.X) * (segmentEnd.X - circleCenter.X) 
                + 
                (segmentStart.Y - segmentEnd.Y) * (segmentEnd.Y - circleCenter.Y)
                );
            var C = Math.Pow((segmentEnd.X - circleCenter.X), 2) + Math.Pow((segmentEnd.Y - circleCenter.Y),2) - (radius*radius);

            var delta = B * B - 4 * A * C;
            if (delta < 0)
            {
                return false;
            }
            // we have intersection, but we need to check if it's in the segment
            var t1 = (-B + Math.Sqrt(delta)) / (2 * A);
            var t2 = (-B - Math.Sqrt(delta)) / (2 * A);
            if (t1 >= 0 && t1 <= 1)
            {
                return true;
            }
            if (t2 >= 0 && t2 <= 1)
            {
                return true;
            }
            return false;
        }
        public static float getAngleRotatingCollision(Vector2 pivotPoint,Vector2 rotationPoint, List<Vector2> polygonPoints)
        {
            List<Tuple<Vector2, Vector2>> segments = buildSegmentFromPoints(polygonPoints);
            foreach (var segment in segments)
            {
                float result = getIntersectionRotationSegment(pivotPoint, rotationPoint, segment.Item1, segment.Item2);
                if (result != 0)
                {
                    return result;
                }
            }
            return 0;
        }
        private static float getIntersectionRotationSegment(Vector2 circleCenter, Vector2 rotationPoint, Vector2 segmentStart, Vector2 segmentEnd)
        {
            // two options
            // 1 the unit is forwarded, called en passant
            // 2 the unit is behind
            // vector director
            Vector2 vectorDirector = new Vector2(segmentEnd.X - segmentStart.X, segmentEnd.Y - segmentStart.Y);
            Vector2 vectorCenter = new Vector2(circleCenter.X - segmentStart.X, circleCenter.Y - segmentStart.Y);
            float crossProduct = vectorDirector.X * vectorCenter.Y - vectorDirector.Y * vectorCenter.X;
            if (crossProduct < 0) // negative means the center is at right?!
            {
                return -1;
            }
            return 1;




        }

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
