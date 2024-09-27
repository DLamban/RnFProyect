using Core.Units;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Text;
using System.Threading.Tasks;

namespace Core.GeometricEngine
{

    using RectSegment = Tuple<Vector2, Vector2>;
    public class CollidedObject
    {
        public enum CollidedType
        {
            none,
            world,
            unit
        }
        public enum Owner
        {
            world,
            player,
            enemy
        }
        private CollidedType type;
        public Owner owner;
        public BaseUnit? unit;

        public CollidedObject(CollidedType type, BaseUnit? baseUnit=null, Owner owner=Owner.world) 
        {
            this.type = type;
            this.owner = owner;
            unit = baseUnit;
        }
        public List<Vector2> collisionPoligon()
        {            
            if (unit != null)
            {
                return unit.polygonPointsWorld;
            }
            throw new NotImplementedException();
            return null;
        }
        public Tuple<Vector2,float> checkClosestPoint(Tuple<Vector2,Vector2> frontlineSegment)
        {
            
            List<Vector2> polygonPoints = new List<Vector2>();
            polygonPoints = collisionPoligon();
            float minDist = float.PositiveInfinity;
            Vector2 closestPoint = new Vector2();
            for (int i =0;i<polygonPoints.Count;i++)
            {
                RectSegment polygonSegment;
                //last point, close poligon with first
                if ( i!= polygonPoints.Count - 1)
                {
                    polygonSegment = new RectSegment(polygonPoints[i], polygonPoints[i+1]);
                }
                else
                {
                    polygonSegment = new RectSegment(polygonPoints[i], polygonPoints[1]);
                }
                Tuple<Vector2, float> pointAndDist = calculateDistance(polygonSegment, frontlineSegment);
                if ( pointAndDist.Item2 < minDist)
                {
                    minDist = pointAndDist.Item2;
                    closestPoint = pointAndDist.Item1;
                }    
            }
            return new Tuple<Vector2, float> (closestPoint,minDist);
        }
       
        private Tuple<Vector2,float> calculateDistance(RectSegment poligonRect, RectSegment rectSegment)
        {

            Vector2 pointprojection1 = calculatePointProjection(poligonRect.Item1, rectSegment);
            Vector2 pointprojection2 = calculatePointProjection(poligonRect.Item2, rectSegment);
            Vector3 rect = convertSegmentToRect(rectSegment);
            Vector2 crossingPoint1 = pointCrossing(poligonRect, pointprojection1,rect);
            Vector2 crossingPoint2 = pointCrossing(poligonRect, pointprojection2,rect);

            float distance1 = calcDistancePointRect(crossingPoint1, rect);
            float distance2 = calcDistancePointRect(crossingPoint2, rect);
            if (distance1 < distance2)
            {
                return new Tuple<Vector2, float>(crossingPoint1, distance1);
            }else
            {
                return new Tuple<Vector2, float>(crossingPoint2, distance2);
            }

        }
        private float calcDistancePointRect(Vector2 point, Vector3 rectEq)
        {
            float distance = float.PositiveInfinity;
            float numerator = Math.Abs(rectEq.X * point.X + rectEq.Y * point.Y + rectEq.Z);
            double denominator = Math.Sqrt(rectEq.X * rectEq.X + rectEq.Y * rectEq.Y);
            distance = (float)(numerator / denominator);
            return distance;
        }
        private Vector2 pointCrossing(RectSegment poligonSideSegment, Vector2 projectedPoint, Vector3 rectEq)
        {
            // get the perpendicular rect of the unit front passing from projectedpoint
            // use point slope eq

            float slopePerpendicular = 0;// 1 as default so we can avoid the problem when x is 0, or horizontal lines
            float A, B, C;
            if (rectEq.X != 0)
            {
                slopePerpendicular = rectEq.Y / rectEq.X;
                A = slopePerpendicular * -1;
                B = 1;
                C = (slopePerpendicular * projectedPoint.X) - projectedPoint.Y;

            }
            else // perpendicular should be a vertical line
            {
                A = 1;
                B = 0;
                C = -projectedPoint.X;
            }


            Vector3 perpendicularEq = new Vector3(A, B, C);
 
            Vector3 poligonSideRect = convertSegmentToRect(poligonSideSegment); 
            // solve equations, recheck that is right, not right
            float Xnumerator = poligonSideRect.Y*perpendicularEq.Z - perpendicularEq.Y* poligonSideRect.Z;
            float Xdenominator = poligonSideRect.X * perpendicularEq.Y - perpendicularEq.X * poligonSideRect.Y;
            float X = Xnumerator / Xdenominator;
            // falta YYYYYYY
            float Y = (-1* poligonSideRect.X*X - poligonSideRect.Z)/ poligonSideRect.Y;
            Vector2 result  = new Vector2(X, Y);
            bool insideSegment = checkPointInsideSegment(result, poligonSideSegment);
            if (!insideSegment)
            {
                result = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
            }
            return result;
        }
        private bool checkPointInsideSegment(Vector2 point, RectSegment rectSegment)
        {
            bool xinside = Math.Min(rectSegment.Item2.X, rectSegment.Item1.X)<= point.X && point.X <= Math.Max(rectSegment.Item2.X,rectSegment.Item1.X);
            bool yinside = Math.Min(rectSegment.Item2.Y, rectSegment.Item1.Y) <= point.Y && point.Y <= Math.Max(rectSegment.Item2.Y, rectSegment.Item1.Y);

            return xinside && yinside; 

        }
        // got problems finding the points in the 
        private Vector2 calculatePointProjection(Vector2 point, RectSegment rectSegment)
        {
            Vector2 frontlineDirector = new Vector2(
                rectSegment.Item2.X - rectSegment.Item1.X, 
                rectSegment.Item2.Y - rectSegment.Item1.Y
                );
            Vector2 pointToLineVector = new Vector2(point.X - rectSegment.Item1.X, point.Y - rectSegment.Item1.Y);
            float dotproduct = Vector2.Dot(pointToLineVector, frontlineDirector);
            double magnitude = Math.Pow(frontlineDirector.X, 2) + Math.Pow(frontlineDirector.Y, 2);
            float resultProjectionScalar = (float)(dotproduct / magnitude);

            resultProjectionScalar = Math.Clamp(resultProjectionScalar, 0, 1);    
            Vector2 pointProjection = resultProjectionScalar * frontlineDirector;
            pointProjection.X = pointProjection.X + rectSegment.Item1.X;
            pointProjection.Y = pointProjection.Y + rectSegment.Item1.Y;
            

            return pointProjection;
        }
        // we check the projection over the frontline rect segment of the unit,
        // so we know we're to check
        // then check minimum distance in the rect segments
        public Vector2 checkProjectionOverRectSegment(Vector2 ori, Vector2 end)
        {
            List<Vector2> polygonPoints = new List<Vector2>();
            polygonPoints = collisionPoligon();
            // start with the first segment rect of polygon
            
            RectSegment poligonSegment = new RectSegment(polygonPoints[0], polygonPoints[1]);
            RectSegment frontlineRectSegment = new RectSegment(ori,end);

            Vector2 projection = vectorProjection(poligonSegment, frontlineRectSegment);

            return projection;
            }
        private Vector3 convertSegmentToRect(RectSegment rectSegment)
        {
            float x2subx1 = rectSegment.Item2.X - rectSegment.Item1.X;
            float y2suby1 = rectSegment.Item2.Y - rectSegment.Item1.Y;

            float A = y2suby1;
            float B = x2subx1 * -1;
            float C = rectSegment.Item1.Y * x2subx1 - rectSegment.Item1.X * y2suby1;
            return new Vector3(A, B, C);
        }
        private Vector2 vectorProjection(RectSegment projected, RectSegment overProject)
        {
            Vector2 director1 = new Vector2(projected.Item2.X - projected.Item1.X, projected.Item2.Y - projected.Item1.Y);
            Vector2 director2 = new Vector2(overProject.Item2.X - overProject.Item1.X, overProject.Item2.Y - overProject.Item1.Y);
            
            float dotProduct = Vector2.Dot(director1, director2);
            double normVectorProjected = Math.Pow(director2.X,2) + Math.Pow(director2.Y,2);
            float projectionMagnitude = (float)(dotProduct / normVectorProjected);
            Vector2 result = new Vector2(projectionMagnitude * director2.X, projectionMagnitude * director2.Y);
            return result;
        }   
    }
}
