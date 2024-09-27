using Core.GameLoop;
using Core.GeometricEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Core.Networking
{
    public class NetMappers
    {
        public static byte encodeNetMsg(MSGType mSGType, SenderMessageEnum senderMessageEnum)
        {
            return (byte)((byte)mSGType | (byte)senderMessageEnum);
        }
        public static Tuple<MSGType, SenderMessageEnum> decodeNetMsg(byte code)
        {
            MSGType mSGType = (MSGType)(code & 0b0000_1111);
            SenderMessageEnum senderMessageEnum = (SenderMessageEnum)(code & 0b1111_0000);
            return new Tuple<MSGType, SenderMessageEnum>(mSGType, senderMessageEnum);
        }


        #region OBJECT_TO_BINARY
        public static BinaryData ConvertPositionInfoToBinary(AffineTransformCore affine, Guid guid)
        {
            BinaryData binaryData;
            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream))
                {
                    byte msgType = encodeNetMsg(MSGType.VEC3UNIT, (SenderMessageEnum)PlayerInfoSingleton.Instance.playerSpot);

                    writer.Write(msgType);
                    writer.Write(guid.ToByteArray());

                    writer.Write((float)affine.offsetX);
                    writer.Write((float)affine.offsetY);
                    float currentAngle = (float)affine.currentAngle;
                    writer.Write((float)currentAngle);
                    binaryData = new BinaryData(stream.ToArray());
                }
            }
            return binaryData;
        }

        public static BinaryData ConvertPositionInfoToBinary(SenderMessageEnum sender, Vector3 vec3, Guid guid)
        {
            BinaryData binaryData;
            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream))
                {
                    byte msgType = encodeNetMsg(MSGType.VEC3UNIT, sender);
                    writer.Write(msgType);
                    writer.Write(guid.ToByteArray());
                    writer.Write(vec3.X);
                    writer.Write(vec3.Y);
                    writer.Write(vec3.Z);
                    binaryData = new BinaryData(stream.ToArray());
                }
            }
            return binaryData;
        }
        public static BinaryData ConvertBattleStateToBinary(SenderMessageEnum sender, BattleStates battleState)
        {
            BinaryData binaryData;
            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream))
                {
                    byte msgType = encodeNetMsg(MSGType.BATTLESTATE, sender);
                    writer.Write(msgType);
                    writer.Write((byte)battleState);
                    binaryData = new BinaryData(stream.ToArray());
                }
            }
            return binaryData;
        }
        #endregion

        #region BINARY_TO_OBJECT
        public static Guid BinaryToGuid(BinaryReader reader) {
            return new Guid(reader.ReadBytes(16));
        }
        //convert the binary data to a Vector3
        public static Vector3 BinaryToVec3(BinaryReader reader)
        {
            Vector3 vector3;
            float x1 = reader.ReadSingle();
            float y1 = reader.ReadSingle();
            float z1 = reader.ReadSingle();
            vector3 = new Vector3(x1, y1, z1);
            return vector3;
        }
        #endregion
    }
}