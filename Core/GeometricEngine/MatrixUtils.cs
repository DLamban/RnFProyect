using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;

using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Reflection.Metadata;
using Core.Networking;
using static Core.GeometricEngine.GeometryUtils;

namespace Core.GeometricEngine
{
    public enum InterpolateType
    {
        cuadratic,
        linear,
        cubic
    }

    public class AffineTransformCore
    {
        
        public MatrixAffine matrixTransform { get; set; }
        // forward vector is -90 degress cuz the troops are looking up, shrug 
        public Vector2 ForwardVec
        {
            get { return normalizeVec(new Vector2((float)m12, (float)m11)); }
            set { }
        }
        /// <summary>
        ///  This is the rect that lies in the frontline, useful for a lot of things
        /// it's in implicit form so in the vector3 is A=x, B=y, C=z, the natural order
        /// we use an horizontal line an apply current transform a=0 b=1 c=0,
        /// so we only need the central values of the matrix, magic!
        /// </summary>
        public Vector3 frontLineRect 
        { 
            get {
                return new Vector3((float)m12, (float)m22, (float)offsetY);
            } set { } 
        }                
        public double currentAngle
        {
            get { return Math.Atan2(m21, m11); }
            set { }
        }
        public float currentAngleDegrees
        {
            get { return (float) (Math.Atan2(m21, m11) * (360/Math.Tau)); }
            set { }
        }
        public double m11
        {
            get { return matrixTransform[0, 0]; }
            set { matrixTransform[0, 0] = value; }
        }
        public double m12
        {
            get { return matrixTransform[0, 1]; }
            set { matrixTransform[0, 1] = value; }
        }
        public double m21
        {
            get { return matrixTransform[1, 0]; }
            set { matrixTransform[1, 0] = value; }
        }
        public double m22
        {
            get { return matrixTransform[1, 1]; }
            set { matrixTransform[1, 1] = value; }
        }
        public double offsetX
        {
            get { return matrixTransform[0, 2]; }
            set { matrixTransform[0, 2] = value; }
        }
        public double offsetY
        {
            get { return matrixTransform[1, 2]; }
            set { matrixTransform[1, 2] = value; }
        }
        public SimplePositionRotation toSimplePosRot()
        {
            return new SimplePositionRotation(new Vector2((float)offsetX, (float)offsetY), getVectorDirector());
        }
        /// <summary>
        /// Default affinetransform 
        /// </summary>
        public AffineTransformCore() {
            new AffineTransformCore(1, 0, 0, 1, 0, 0);        
        }
        /// <summary>
        /// Affine transformation from position and director vector
        /// </summary>
        public AffineTransformCore(Vector2 pos, Vector2 director )
        {
            //normalize director
            Vector2 normDirector = normalizeVec(director);
            matrixTransform = new MatrixAffine(normDirector.Y * -1, normDirector.X, pos.X,
                                        normDirector.X, normDirector.Y, pos.Y,
                                        0, 0, 1);
        }
        public AffineTransformCore(double m11, double m12,
                                double m21, double m22,
                                double offsetX, double offsetY)
        {
            matrixTransform = new MatrixAffine(m11, m12, offsetX,
                                        m21, m22, offsetY,
                                        0, 0, 1);
        }
        /// <summary>
        /// Build an affine transformation from a vector3
        /// X = offsetx
        /// Y = offsety
        /// Z = angle in radians
        /// </summary>
        /// <param name="vector">Vector3 as X=xoffset,Y=yoffset,Z=angleoffset </param>
        public AffineTransformCore(Vector3 vector) { 
            float cos = (float)Math.Cos(vector.Z);
            float sin = (float)Math.Sin(vector.Z);
            matrixTransform = new MatrixAffine(cos, -sin, vector.X,
                                        sin, cos, vector.Y,
                                        0, 0, 1);
        }
        
        /// <summary>
        ///  for net code, create a new affinetransform from a minimum serialized one
        /// </summary>
        /// <param name="ser"></param>
        public AffineTransformCore(SerializableAffineTrans ser)
        {
            matrixTransform = new MatrixAffine(ser.m11,ser.m12,ser.offsetX,
                                        ser.m21, ser.m22, ser.offsetY,
                                        0, 0, 1);                                
        }
        /// <summary>
        ///  This is the rect that lies in the frontline, useful for a lot of things
        /// it's in implicit form so in the vector3 is A=x, B=y, C=z, the natural order.
        /// We use an horizontal line an apply current transform a=0 b=1 c=0,
        /// so we only need the central values of the matrix, magic!
        /// </summary>
        public Vector3 getFrontLineRectImplicitEq()
        {
            return frontLineRect;
        }

        private Vector2 normalizeVec(Vector2 vec)
        {
            double length = Math.Sqrt(vec.X * vec.X + vec.Y * vec.Y);
            return new Vector2((float)(vec.X / length), (float)(vec.Y / length));
        }
        public void resetTransform()
        {
            m11 = 1; m12 = 0;
            m21 = 0;m22=1; 
            offsetX = 0;
            offsetY = 0;
        }

        /// <summary>
        /// Calculate angle to between vectors
        /// </summary>
        /// <param name="X">X component</param>
        /// <param name="Y">Y component</param>
        /// <returns>angle in RADIANS</returns>
        public float calculateAngle(double X1, double Y1, double X2, double Y2,  bool full360 = true)
        {
            Vector2 vector1 = new Vector2((float)X1, (float)Y1);
            Vector2 vector2 = new Vector2((float)X2, (float)Y2);
            // Normalize vector for cleaner code, so we can get the angle without the lenght division
            Vector2 normalizedVector1 = normalizeVec(vector1);
            Vector2 normalizedVector2 = normalizeVec(vector2);
            double dotProduct = Vector2.Dot(normalizedVector1, normalizedVector2);
            dotProduct = Math.Clamp(dotProduct, -1.0f, 1.0f);


            //full 360 degreess, slower, but sometimes we need it 
            if (full360)
            {
                double crossProduct = normalizedVector1.X * normalizedVector2.Y - normalizedVector1.Y * normalizedVector2.X;
                return (float)Math.Atan2(crossProduct, dotProduct);
            }
            else//will be 0-180 or [-pi,pi]
            {
                return (float)Math.Acos(dotProduct);
            }
        }
        /// <summary>
        /// Calculate angle to the frontline of the unit
        /// </summary>
        /// <param name="X">X component</param>
        /// <param name="Y">Y component</param>
        /// <returns>angle in RADIANS</returns>
        public float calculateAngleToTransform(double X, double Y, bool full360=true)
        {
            Vector2 vector = new Vector2((float)X, (float)Y);
            Vector2 director = getVectorDirector();
            // Normalize vector for cleaner code, so we can get the angle without the lenght division
            Vector2 normalizedVector = normalizeVec(vector);
            Vector2 normalizedDirector = normalizeVec(director);
            double dotProduct = Vector2.Dot(normalizedVector, normalizedDirector);
            dotProduct = Math.Clamp(dotProduct, -1.0f, 1.0f);
            
            //full 360 degreess, slower, but sometimes we need it 
            if (full360)
            {
                double crossProduct = normalizedVector.X * normalizedDirector.Y - normalizedVector.Y * normalizedDirector.X;
                return (float)Math.Atan2(crossProduct, dotProduct);
            }
            else
            {
                return (float)Math.Acos(dotProduct);
            }
        }

        public Vector2 getVectorDirector()
        {
            return new Vector2((float)(m22 * -1), (float)m21);
        }
        public Vector3 localEqPointDirToWorld(Vector2 point, Vector2 dir)
        {
            var A = (float)(dir.X * m11 + dir.Y * m12);
            var B = (float)(dir.X * m21 + dir.Y * m22);
            var pointworld = localToGlobalTransforms(point);
            //C = vx​y−vy​x0
            var C = -(float)(pointworld.X * A + pointworld.Y * B);
            return new Vector3(A, B, C);
        }
        /// <summary>
        ///  Vector transformations ignore the translation part of the matrix
        /// </summary>
        /// <param name="vec"></param>
        /// <returns>Vector transformed</returns>
        public Vector2 localVectorToWorldTransform(Vector2 vec)
        {
            Vector2 result = new Vector2();
            result.X = (float)(vec.X * m11 + vec.Y * m12);
            result.Y = (float)(vec.X * m21 + vec.Y * m22);
            return result;
        }        
        public Vector2 localToGlobalTransforms(Vector2 vec)
        {
            return localToGlobalTransforms(vec.X, vec.Y);
        }
        public Vector2 localToGlobalTransforms(double X, double Y)
        {
            Vector2 result = new Vector2();
            result.X = (float)((float)( X *   m11 +  Y *  m12) +   offsetX);
            result.Y = (float)( X *   m21 + Y *   m22 +   offsetY);
            return result;
        }
        public Vector2 GlobalToLocalTransforms(Vector2 vec)
        {
            return GlobalToLocalTransforms(vec.X, vec.Y);
        }
        public Vector2 GlobalToLocalTransforms(double X, double Y)
        {
            Vector2 result = new Vector2();
            MatrixAffine invert = invertAffineMatrix(matrixTransform);
            result.X = (float)((invert[0, 0] * X) + (invert[0, 1] * Y) + (invert[0, 2]));
            result.Y = (float)((invert[1, 0] * X) + (invert[1, 1] * Y) + (invert[1, 2]));
            return result;
        }
        /// <summary>
        /// This formula only works with affine transforms, because it has 0,0,1 in the last row
        /// </summary>
        /// <returns></returns>
        private MatrixAffine invertAffineMatrix(MatrixAffine matrix)
        {
            MatrixAffine inverted = new MatrixAffine();
            float det = 1/ (float)((matrix[0, 0] * matrix[1, 1]) - (matrix[0, 1] * matrix[1, 0]));
            inverted = new MatrixAffine(det * matrix[1, 1], det * -matrix[0, 1], det * (matrix[0, 1] * matrix[1, 2] - matrix[0, 2] * matrix[1, 1]),
                                  det * -matrix[1, 0], det * matrix[0, 0], det * (matrix[0, 2] * matrix[1, 0] - matrix[0, 0] * matrix[1, 2]),
                                  0, 0, 1);
            return inverted;
        }

        /// <summary>
        /// Because sometimes we need a copy of the transform matrix
        /// </summary>
        /// <returns>a new copy of the transform matrix</returns>
        public MatrixAffine copyMatrixTransformValues()
        {
            return new MatrixAffine(m11, m12, offsetX,
                              m21, m22, offsetY,
                              0, 0, 1);
        }

        public float distanceToFrontLine(float X, float Y)
        {
            float distance = 0;
            // remember A =x, B=y, C=z in the vector
            // we use the formulae of distance between point and rect d = |Ax + By + C| / sqrt(A^2 + B^2)
            float numerator = (frontLineRect.X * X) + (frontLineRect.Y * Y) + (frontLineRect.Z);
            //numerator = Math.Abs(numerator);
            float denominator = (float)Math.Sqrt((frontLineRect.X * frontLineRect.X) + (frontLineRect.Y * frontLineRect.Y));
            distance = numerator / denominator;
            return distance;
        }
        public static MatrixAffine InterpolateMatrix(MatrixAffine original, MatrixAffine target, float t, InterpolateType interpolateType=InterpolateType.linear)
        {
            return new MatrixAffine(t * target[0, 0] + (1 - t) * original[0, 0],
                              t * target[0, 1] + (1 - t) * original[0, 1],
                              t * target[0, 2] + (1 - t) * original[0, 2],
                              t * target[1, 0] + (1 - t) * original[1, 0],
                              t * target[1, 1] + (1 - t) * original[1, 1],
                              t * target[1, 2] + (1 - t) * original[1, 2],
                              0, 0, 1);
        }
        /// <summary>
        /// Rotate the current transform by a degrees angle, around the pivot point
        /// </summary>
        /// <param name="degrees">angle in degrees</param>
        /// <param name="offsetCenterX">pivot point X</param>
        /// <param name="offsetCenterY">pivot point y</param>
        public void rotate(double degrees, double offsetCenterX, double offsetCenterY)
        {
            // we need to take the center of the unit, and rotate around it
            MatrixAffine rotateTransform = rotateTransformDeg(degrees);
            AffineTransformCore affineTransformoffset = new AffineTransformCore(1, 0, 0, 1, offsetCenterX, offsetCenterY);
            AffineTransformCore affineTransformoffsetinverso = new AffineTransformCore(1, 0, 0, 1, -offsetCenterX, -offsetCenterY);

            matrixTransform = multiplyMatrix(matrixTransform, affineTransformoffset.matrixTransform);
            matrixTransform = multiplyMatrix(matrixTransform, rotateTransform);
            matrixTransform = multiplyMatrix(matrixTransform, affineTransformoffsetinverso.matrixTransform);
        }
        public string afftransformString()
        {
            string afftransstr = "\n";
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    afftransstr += matrixTransform[i, j].ToString() + ";"; 
                }
                afftransstr += "\n";
            }
            return afftransstr;
        }
        
        private MatrixAffine multiplyMatrix(MatrixAffine matrixTransform, MatrixAffine rotateTransform)
        {
            //create matrix multiplication
            double[,] result = new double[3, 3];
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    result[i, j] = 0;
                    for (int k = 0; k < 3; k++)
                    {
                        result[i, j] += matrixTransform[i, k] * rotateTransform[k, j];
                    }
                }
            }
            return new MatrixAffine(result);
        }
        //public void move
        private MatrixAffine rotateTransformDeg(double degrees)
        {
            degrees = degrees % 360;

            double radians = degrees * Math.Tau / 360;
            double cos = 0;
            double sin = 0;

            if (degrees == 90 || degrees == -270)
            {
                cos = 0;
                sin = 1;
            }
            else if (Math.Abs(degrees) == 180)
            {
                cos = -1;
                sin = 0;
            }
            else if (degrees == 270 || degrees == -90)
            {
                cos = 0;
                sin = -1;
            }
            else
            {
                cos = Math.Cos(radians);
                sin = Math.Sin(radians);
            }
            return new MatrixAffine(
                               cos, -sin, 0,
                               sin, cos, 0,
                               0, 0, 1);
        }
    }
    public class MatrixAffine
    {
        public double m11 { get { return matrix[0, 0]; } }
        public double m12 { get { return matrix[0, 1]; } }
        public double offsetX { get { return matrix[0, 2]; } }
        public double m21 { get { return matrix[1, 0]; } }
        public double m22 { get { return matrix[1, 1]; } }
        public double offsetY { get { return matrix[1, 2]; } }
        private double[,] matrix { get; set; }
        public double this[int row, int col]
        {
            get { return matrix[row, col]; }
            set { matrix[row, col] = value; }
        }
        public MatrixAffine()
        {
            matrix = new double[3, 3];
        }
        public MatrixAffine(double m11, double m12, double m21,double m22, double offsetX, double offsetY)
        {
            matrix = new double[3, 3];
            matrix[0, 0] = m11;
            matrix[0, 1] = m12;
            matrix[0, 2] = offsetX;
            matrix[1, 0] = m21;
            matrix[1, 1] = m22;
            matrix[1, 2] = offsetY;
            matrix[2, 0] = 0;
            matrix[2, 1] = 0;
            matrix[2, 2] = 1;
        }
        public MatrixAffine(double[,] values)
        {
            if (values.GetLength(0) != 3 || values.GetLength(1) != 3)
            {
                throw new ArgumentException("Matrix dimensions must be 3x3.");
            }
            matrix = new double[3, 3];
            matrix = values;
        }
        public MatrixAffine(double m11, double m12, double m13,
                      double m21, double m22, double m23,
                      double m31, double m32, double m33)
        {
            matrix = new double[3, 3];
            matrix[0, 0] = m11;
            matrix[0, 1] = m12;
            matrix[0, 2] = m13;
            matrix[1, 0] = m21;
            matrix[1, 1] = m22;
            matrix[1, 2] = m23;
            matrix[2, 0] = m31;
            matrix[2, 1] = m32;
            matrix[2, 2] = m33;
        }
        public MatrixAffine cloneMatrix()
        {
            return new MatrixAffine(matrix[0, 0], matrix[0, 1], matrix[0, 2], 
                              matrix[1, 0], matrix[1, 1], matrix[1, 2], 
                              matrix[2, 0], matrix[2, 1], matrix[2,2]
                              );
        }
    }
}
