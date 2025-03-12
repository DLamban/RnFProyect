using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Core.GeometricEngine
{
    public class GeometricTypedef
    {
        public struct RectSegment
        {
            public Vector2 Start { get; }
            public Vector2 End { get; }

            public RectSegment(Vector2 start, Vector2 end)
            {
                Start = start;
                End = end;
            }
        }
    }
}
