using Core.GeometricEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Core.Networking
{
    public  class SerializableAffineTrans
    {
        public float m11 { get; set; }
        public float m12 { get; set; }
        public float m21 { get; set; }
        public float m22 { get; set; }
        public float offsetX { get; set; }
        public float offsetY { get; set; }
        
        public SerializableAffineTrans()
        {
        }
        public SerializableAffineTrans(float m11, float m12, float m21,float m22, float offsetX, float offsetY)
        {
            this.m11 = m11;
            this.m12 = m12;
            this.m21 = m21;
            this.m22 = m22;
            this.offsetX = offsetX;
            this.offsetY = offsetY;
        }
        public SerializableAffineTrans(AffineTransformCore affTrans)
        {
            m11 = (float)affTrans.m11;
            m12 = (float)affTrans.m12;
            m21 = (float)affTrans.m21;
            m22 = (float)affTrans.m22;
            offsetX = (float)affTrans.offsetX;
            offsetY = (float)affTrans.offsetY;

        }
    }
}
