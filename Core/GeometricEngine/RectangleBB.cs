using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Core.GeometricEngine
{
    public class RectangleBB
    {
        private float width;
        private float height;
        private float centerX { get { return width / 2; } }
        private float centerY { get { return height / 2; } }
        private AffineTransformCore transform;
        public Vector2 centerCoord {
            get { return transform.localToGlobalTransforms(centerX, centerY); }
        }
        // we use forward vec because is a normalized Vector
        public Vector2 YVector
        {
            get { return transform.ForwardVec; }
        }
        public Vector2 XVector
        {
            get { return new Vector2(YVector.Y * -1, YVector.X); }
        }

        public RectangleBB(float width, float height, AffineTransformCore afftrans) {
            this.width = width;
            this.height = -height;
            transform = afftrans;
        }

        public float projectionLenght(Vector2 axisRect)
        {
            return projectionXOverAxis(axisRect) + projectionYOverAxis(axisRect);
        }
        private float projectionXOverAxis(Vector2 axisRect)
        {
            Vector2 halfWidthVec = XVector * (width / 2);
            return Math.Abs(Vector2.Dot(halfWidthVec, axisRect));            
        }
        private float projectionYOverAxis(Vector2 axisRect)
        {
            Vector2 halfHeightVec = YVector * (height / 2);
            return Math.Abs(Vector2.Dot(halfHeightVec, axisRect));
        }
    }
}
