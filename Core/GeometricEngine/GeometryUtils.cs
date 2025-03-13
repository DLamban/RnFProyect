using Core.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static Core.GeometricEngine.GeometricTypedef;

namespace Core.GeometricEngine
{
    public class GeometryUtils
    {
        public static bool checkSemisegmentUnitCross(Vector2 origin, Vector2 direction, UnitBorders unitBorders )
        {
            bool result = false;

            result = checkSemisegmentSegmentCross(origin, direction, unitBorders.frontLine);
            result = result || checkSemisegmentSegmentCross(origin, direction, unitBorders.leftLine);
            result = result || checkSemisegmentSegmentCross(origin, direction, unitBorders.rightLine);
            result = result || checkSemisegmentSegmentCross(origin, direction, unitBorders.backLine);

            return result;
        }
        public static bool checkSemisegmentSegmentCross(Vector2 rayOri, Vector2 rayDir, RectSegment segment)
        {
            ///https://ncase.me/sight-and-light/
            // check parallel
            Vector2 segmentDirection = segment.End - segment.Start;
            float determinant = Vector2.Dot(rayDir, Vector2.Normalize(segmentDirection));
            if (Math.Abs(determinant) > 0.9999f)
            {
                return false;
            }

            // check intersection

            // s = (r_dx*(s_py-r_py) + r_dy*(r_px-s_px))/(s_dx*r_dy - s_dy*r_dx)
            var s = ((rayDir.X * (segment.Start.Y - rayOri.Y)) +
                    (rayDir.Y * (rayOri.X - segment.Start.X))) /
                    (segmentDirection.X * rayDir.Y - segmentDirection.Y * rayDir.X);
            
            //t = (s_px + s_dx * T2 - r_px) / r_dx
            var t = (segment.Start.X + segmentDirection.X * s - rayOri.X)/rayDir.X;
            var intersection = new Vector2();
            if (s >= 0 && t >= 0 && t <= 1)
            {
                intersection.X = rayOri.X + rayDir.X * s;
                intersection.Y = rayOri.Y + rayDir.Y * s;
                return true;
            }

            return true;
        }
    }

}
