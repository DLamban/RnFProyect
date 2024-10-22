using Azure.Messaging.WebPubSub.Clients;
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

        private static WebPubSubClient client;
        static ClientNetEvents()
        {        
        }
        
        public static void setEventHandlers(WebPubSubClient _client)
        {
            client = _client;
            client.GroupMessageReceived += eventArgs =>
            {
                groupMessageReceived(eventArgs.Message);
                return Task.CompletedTask;
            };
            client.ServerMessageReceived += eventArgs =>
            {
                serverMessageReceived(eventArgs.Message);
                return Task.CompletedTask;
            };
        }
        // we try to use the same method for both group and server messages
        private static void groupMessageReceived(GroupDataMessage msg)
        {
            if (msg.DataType == WebPubSubDataType.Binary)
            {
                receivedData(msg.Data);
            } else if (msg.DataType == WebPubSubDataType.Json)
            {
                receivedData(msg.Data.ToString());
            }
        }
        private static void serverMessageReceived(ServerDataMessage msg)
        {

            if (msg.DataType == WebPubSubDataType.Binary)
            {
                receivedData(msg.Data);
            }
            else if (msg.DataType == WebPubSubDataType.Json)
            {
                receivedData(msg.Data.ToString());
            }

        }


        /// <summary>
        /// Read data, parse and act acordingly
        /// </summary>
        /// <param name="json"></param>
        public static void receivedData(string json)
        {

            Dictionary<string, object>? data = JsonSerializer.Deserialize<Dictionary<string, object>>(json);                       
            if (data.ContainsKey("player1_units"))
            {
                loadUnits(data);
            }                             
        }
        private static void loadUnits(Dictionary<string, object>? data)
        {
            UnitsServerManager unitManager = new UnitsServerManager();
            List<BaseUnit> player1units = unitManager.createUnits(JsonSerializer.Deserialize<List<MinimumUnitTransferInfo>>(data["player1_units"].ToString()));
            List<BaseUnit> player2units = unitManager.createUnits(JsonSerializer.Deserialize<List<MinimumUnitTransferInfo>>(data["player2_units"].ToString()));

            if (PlayerSpotEnum.PLAYER1 == PlayerInfoSingleton.Instance.playerSpot)
            {
                UnitsClientManager.Instance.addAllPlayerUnits(player1units);
                UnitsClientManager.Instance.addAllEnemyUnits(player2units);
            }
            else
            {
                UnitsClientManager.Instance.addAllPlayerUnits(player2units);
                UnitsClientManager.Instance.addAllEnemyUnits(player1units);
            }
            UnitsClientManager.Instance.unitsLoaded();
        }
        /// <summary>
        /// We need to decode the binary date received, a bit complex
        /// </summary>
        /// <param name="binaryData"></param>
        public static void receivedData(BinaryData binaryData)
        {            
            using (var stream = binaryData.ToStream())
            using (var reader = new BinaryReader(stream))
            {

                
                Tuple<MSGType,SenderMessageEnum> result =  NetMappers.decodeNetMsg(reader.ReadByte());

                switch (result.Item1) { 
                    //TODO: AI CODE, RECHECK
                    case MSGType.MSG:
                        string message = reader.ReadString();
                        Console.WriteLine($"Received message: {message}");
                        break;
                    case MSGType.VEC2:
                        float x = reader.ReadSingle();
                        float y = reader.ReadSingle();
                        var vector2 = new Vector2(x, y);
                        Console.WriteLine($"Received Vector2: X={vector2.X}, Y={vector2.Y}");
                        break;
                    case MSGType.VEC3UNIT:
                        
                        Guid entityguid = NetMappers.BinaryToGuid(reader);
                        var vector3 = NetMappers.BinaryToVec3(reader);                        
                        OnVec3NetSendPlayer?.Invoke( entityguid, vector3);
                        //NECESITAMOS EL PLAYER, VIENEN EN EL MENSAJE, mas info
                        break;
                    case MSGType.POS2DDIST:
                        float x2 = reader.ReadSingle();
                        float y2 = reader.ReadSingle();
                        float z2 = reader.ReadSingle();
                        float dist = reader.ReadSingle();
                        var vector3Dist = new Vector3(x2, y2, z2);
                        Console.WriteLine($"Received Vector3: X={vector3Dist.X}, Y={vector3Dist.Y}, Z={vector3Dist.Z} Distance: {dist}");
                        break;
                    case MSGType.POS3DDIST:
                        float x3 = reader.ReadSingle();
                        float y3 = reader.ReadSingle();
                        float z3 = reader.ReadSingle();
                        float dist2 = reader.ReadSingle();
                        var vector3Dist2 = new Vector3(x3, y3, z3);
                       // Console.WriteLine($"Received Vector3: X={vector3Dist2.X}, Y={vector3Dist2.Y}, Z={vector3Dist2.Z} Distance: {dist2}");
                        break;
                    case MSGType.BATTLESTATE:
                        //Console.WriteLine("Received Battle State");
                        BattleState battleState = (BattleState)reader.ReadByte();
                        break;
                    default:
                        Console.WriteLine($"Received unknown message type: {result.Item1}");
                        throw new Exception("Unknown message type");
                        break;
                }               

                //Console.WriteLine($"Received Vector3: X={vector3.X}, Y={vector3.Y}, Z={vector3.Z}");
            }
        }        
    }
}
