using Core.GameLoop;
using Core.GeometricEngine;
using Core.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Core.Networking
{
    /// <summary>
    /// OLD SCHOOL: table of events
    /// 0 - Msg
    /// 1 - Vec2
    /// 2 - Vec3
    /// 3 - Pos3DDist
    /// 4 - Pos2DDist
    /// 5 - BattleState
    /// </summary>
    public enum MSGType :byte
    {
        MSG = 0,        
        VEC2 = 1,
        VEC3UNIT = 2,// usually used to send the position of the unit as X = X, Y = Y, Z = rotation in radians
        POS3DDIST = 3,
        POS2DDIST = 4,
        BATTLESTATE = 5
    }
    public enum SenderMessageEnum:byte
    {
        PLAYER1 = 0b0000_0000,
        PLAYER2 = 0b1000_0000,
        SERVER  = 0b0100_0000
    }
    public static class ClientNetEvents
    {
        // DELEGATES AND EVENTS, WE GOT A LOT BECAUSE IS NETWORKING
        public delegate void MsgNetSendDelegate(SenderMessageEnum sender, string message);
        public delegate void Vec2NetSendDelegate(SenderMessageEnum sender, Vector2 vector2);
        public delegate void Vec3NetSendDelegate(Guid unitGuid, Vector3 vector3);
        public delegate void Pos3DDistNetSendDelegate(Vector3 vector,float dist);
        public delegate void Pos2DDistNetSendDelegate(SenderMessageEnum sender,Vector3 vector, float dist);
        public delegate void BattleStateNetSendDelegate(BattleState battleState);

        public static event MsgNetSendDelegate OnMsgNetSend;
        public static event Vec2NetSendDelegate OnVec2NetSend;        
        public static event Vec3NetSendDelegate OnVec3NetSendPlayer;
        public static event Pos3DDistNetSendDelegate OnPos3DDistNetSend;
        public static event Pos2DDistNetSendDelegate OnPos2DDistNetSend;
        public static event BattleStateNetSendDelegate OnBattleStateNetSend;

        //private static WebPubSubClient client;
        static ClientNetEvents()
        {        
        }
        



    
    }
}
