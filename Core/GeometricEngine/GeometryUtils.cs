using Core.Units;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static Core.GeometricEngine.GeometricTypedef;

namespace Core.GeometricEngine
{
    public class GeometryUtils
    {
        public struct PolygonsResult
        {
            public List<PointF> leftPolygon;
            public List<PointF> rightPolygon;
        }
        public struct PointTCross
        {
            public Vector2 point;
            public float t;
        }
        /// <summary>
        /// Get the area of a polygon
        /// by determinants(gauss)
        /// </summary>
        /// <param name="crossingPoints"></param>        
        public static float getAreaPolygon (List<PointF> Points)
        {
            float res = 0;
            for(int i = 0; i < Points.Count; i++)
            {
                int index = (i + 1) % Points.Count;
                res += Points[i].X * Points[index].Y - Points[i].Y * Points[index].X;
            }
            return Math.Abs(res) / 2;
        }
        public static (float,bool) isLeftSideBiggerWithArea(Vector2 rayOri, Vector2 rayDir, UnitBorders unitBorders)
        {
            PolygonsResult polygonsResult = getSubPolygons(rayOri, rayDir, unitBorders);
            float area = getAreaPolygon(polygonsResult.leftPolygon);
            float area2 = getAreaPolygon(polygonsResult.rightPolygon);
            float totalArea = Vector2.Distance(unitBorders.frontLine.Start, unitBorders.frontLine.End) * Vector2.Distance(unitBorders.firstRankLeftLine.Start, unitBorders.firstRankLeftLine.End);
            if (Math.Abs(area + area2 - totalArea) > 0.01)
            {
                throw new Exception("Area calculation failed");
            }
            return (Math.Max(area,area2),  area > area2);
        }
        /// <summary>
        ///  To divde the polygon and still have the correcto order
        ///  we iterate clockwise and return left and right polygons
        ///  we're actually doing a implicit graph traversal
        ///  because we always charge frontal, it's always a point in the front
        ///  lot of assumptions here
        /// </summary>
        /// <param name="rayOri"></param>
        /// <param name="rayDir"></param>
        /// <param name="unitBorders"></param>
        /// <returns></returns>
        public static PolygonsResult getSubPolygons(Vector2 rayOri, Vector2 rayDir, UnitBorders unitBorders)
        {

            List<PointF> leftPolygon = new List<PointF>();
            List<PointF> rightPolygon = new List<PointF>();

            PointTCross? frontcross = getPointAndTSegmentCrossing(rayOri, rayDir, unitBorders.frontLine);
            if (frontcross == null)
            {
                throw new Exception("Front line should always be crossed");
            }

            leftPolygon.Add(new PointF(unitBorders.frontLine.Start.X, unitBorders.frontLine.Start.Y));
            leftPolygon.Add(new PointF(frontcross.Value.point.X, frontcross.Value.point.Y));
            rightPolygon.Add(new PointF(frontcross.Value.point.X, frontcross.Value.point.Y));
            rightPolygon.Add(new PointF(unitBorders.frontLine.End.X, unitBorders.frontLine.End.Y));

            PointTCross? rightcross = getPointAndTSegmentCrossing(rayOri, rayDir, unitBorders.firstRankRightLine);
            if (rightcross != null)
            {
                leftPolygon.Add(new PointF(rightcross.Value.point.X, rightcross.Value.point.Y));
                leftPolygon.Add(new PointF(unitBorders.firstRankRightLine.End.X, unitBorders.firstRankRightLine.End.Y));
                leftPolygon.Add(new PointF(unitBorders.firstRankBackLine.End.X, unitBorders.firstRankBackLine.End.Y));
                rightPolygon.Add(new PointF(rightcross.Value.point.X, rightcross.Value.point.Y));
            }

            PointTCross? backcross = getPointAndTSegmentCrossing(rayOri, rayDir, unitBorders.firstRankBackLine);
            if (backcross != null)
            {
                rightPolygon.Add(new PointF(unitBorders.firstRankBackLine.Start.X, unitBorders.firstRankBackLine.Start.Y));
                rightPolygon.Add(new PointF(backcross.Value.point.X, backcross.Value.point.Y));

                leftPolygon.Add(new PointF(backcross.Value.point.X, backcross.Value.point.Y));
                leftPolygon.Add(new PointF(unitBorders.firstRankBackLine.End.X, unitBorders.firstRankBackLine.End.Y));


            }

            PointTCross? leftcross = getPointAndTSegmentCrossing(rayOri, rayDir, unitBorders.firstRankLeftLine);
            if (leftcross != null)
            {
                rightPolygon.Add(new PointF(unitBorders.firstRankRightLine.End.X, unitBorders.firstRankRightLine.End.Y));
                rightPolygon.Add(new PointF(unitBorders.firstRankBackLine.Start.X, unitBorders.firstRankBackLine.Start.Y));
                rightPolygon.Add(new PointF(leftcross.Value.point.X, leftcross.Value.point.Y));
                leftPolygon.Add(new PointF(leftcross.Value.point.X, leftcross.Value.point.Y));

            }


            PolygonsResult result = new PolygonsResult();
            result.leftPolygon = leftPolygon;
            result.rightPolygon = rightPolygon;

            return result;

        }
        public static bool checkSemisegmentUnitCross(Vector2 origin, Vector2 direction, UnitBorders unitBorders)
        {
            bool result = false;

            result = getPointSemisegmentSegmentCross(origin, direction, unitBorders.frontLine).Item1;
            var result2 = getPointSemisegmentSegmentCross(origin, direction, unitBorders.firstRankRightLine).Item1;
            var result3 = getPointSemisegmentSegmentCross(origin, direction, unitBorders.firstRankBackLine).Item1;
            var result4 = getPointSemisegmentSegmentCross(origin, direction, unitBorders.firstRankLeftLine).Item1;

            return result || result2 || result3 || result4;
        }
        /// <summary>
        ///  Return the T parameter of equation if there is an intersection between the ray and the segment
        /// </summary>
        /// <param name="rayOri"></param>
        /// <param name="rayDir"></param>
        /// <param name="segment"></param>
        /// <returns>T of parametic equation</returns>
        public static PointTCross? getPointAndTSegmentCrossing(Vector2 rayOri, Vector2 rayDir, RectSegment segment)
        {
            ///https://ncase.me/sight-and-light/

            Vector2 segmentDirection = segment.End - segment.Start;
            // check parallel cases
            float determinant = Vector2.Dot(rayDir, Vector2.Normalize(segmentDirection));
            if (Math.Abs(determinant) > 0.9999f)
            {
                return null;
            }

            // check intersection

            // s = (r_dx*(s_py-r_py) + r_dy*(r_px-s_px))/(s_dx*r_dy - s_dy*r_dx)
            var t = ((rayDir.X * (segment.Start.Y - rayOri.Y)) +
                    (rayDir.Y * (rayOri.X - segment.Start.X))) /
                    (segmentDirection.X * rayDir.Y - segmentDirection.Y * rayDir.X);

            //t = (s_px + s_dx * T2 - r_px) / r_dx
            var s = (segment.Start.X + segmentDirection.X * t - rayOri.X) / rayDir.X;
            var intersection = new Vector2();
            if (s >= 0 && t >= 0 && t <= 1)
            {
                intersection.X = rayOri.X + rayDir.X * s;
                intersection.Y = rayOri.Y + rayDir.Y * s;
                PointTCross pointTCross = new PointTCross();
                pointTCross.point = intersection;
                pointTCross.t = t;
                return pointTCross;
            }

            return null;
        }
        public static (bool, Vector2) getPointSemisegmentSegmentCross(Vector2 rayOri, Vector2 rayDir, RectSegment segment)
        {
            ///https://ncase.me/sight-and-light/

            Vector2 segmentDirection = segment.End - segment.Start;
            // check parallel cases
            float determinant = Vector2.Dot(rayDir, Vector2.Normalize(segmentDirection));
            if (Math.Abs(determinant) > 0.9999f)
            {
                return (false, new Vector2());
            }

            // check intersection

            // s = (r_dx*(s_py-r_py) + r_dy*(r_px-s_px))/(s_dx*r_dy - s_dy*r_dx)
            var t = ((rayDir.X * (segment.Start.Y - rayOri.Y)) +
                    (rayDir.Y * (rayOri.X - segment.Start.X))) /
                    (segmentDirection.X * rayDir.Y - segmentDirection.Y * rayDir.X);

            //t = (s_px + s_dx * T2 - r_px) / r_dx
            var s = (segment.Start.X + segmentDirection.X * t - rayOri.X) / rayDir.X;
            var intersection = new Vector2();
            if (s >= 0 && t >= 0 && t <= 1)
            {
                intersection.X = rayOri.X + rayDir.X * s;
                intersection.Y = rayOri.Y + rayDir.Y * s;
                return (true, intersection);
            }

            return (false, intersection);
        }
    }

}
