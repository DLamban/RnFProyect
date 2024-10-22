using Azure.Messaging.WebPubSub;
using Azure.Messaging.WebPubSub.Clients;
using Core.GameLoop;
using Core.Networking;
using ServerSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Server.PlayerInfoCopy;
namespace Server
{
    public class ServerInstance
    {
        // DELEGATES AND EVENTS, WE GOT A LOT BECAUSE IS NETWORKING
        public delegate void MsgNetSendDelegate(SenderMessageEnum sender, string message);
        public delegate void Vec2NetSendDelegate(SenderMessageEnum sender, Vector2 vector2);
        public delegate void Vec3NetSendDelegate(SenderMessageEnum sender, Guid unitGuid, Vector3 vector3);
        public delegate void Pos3DDistNetSendDelegate(SenderMessageEnum sender, Vector3 vector, float dist);
        public delegate void Pos2DDistNetSendDelegate(SenderMessageEnum sender, Vector3 vector, float dist);
        public delegate void BattleStateNetSendDelegate(SenderMessageEnum sender, BattleState battleState);

        public event MsgNetSendDelegate OnMsgNetSend;
        public event Vec2NetSendDelegate OnVec2NetSend;
        public event Vec3NetSendDelegate OnVec3NetSendServer;
        
        public event Pos3DDistNetSendDelegate OnPos3DDistNetSend;
        public event Pos2DDistNetSendDelegate OnPos2DDistNetSend;
        public event BattleStateNetSendDelegate OnBattleStateNetSend;
        
        public ServerInstance(WebPubSubServiceClient serverClient, WebPubSubClient client, string roomId)
        {            
            client.StartAsync().Wait();
            client.JoinGroupAsync("server_"+roomId).Wait();
            client.GroupMessageReceived += eventArgs =>
            {
                receivedData(eventArgs.Message.Data);                
                return Task.CompletedTask;
            };

            client.ServerMessageReceived += eventArgs =>
            {
                receivedData(eventArgs.Message.Data);                
                return Task.CompletedTask;
            };
            string player1Id = roomId + "_player1";
            string player2Id = roomId + "_player2";
            PlayerServerInfo playerServerInfo = new PlayerServerInfo(player1Id, player2Id);
            ServerEventsHandler serverEventsHandler = new ServerEventsHandler(serverClient, roomId+"_player1", roomId+"_player2", this);
        }
        
        
        /// <summary>
        /// We need to decode the binary date received, a bit complex
        /// </summary>
        /// <param name="binaryData"></param>
        public void receivedData(BinaryData binaryData)
        {
            using (var stream = binaryData.ToStream())
            using (var reader = new BinaryReader(stream))
            {
                Tuple<MSGType, SenderMessageEnum> result = NetMappers.decodeNetMsg(reader.ReadByte());

                switch (result.Item1)
                {
                    case MSGType.MSG:
                        break;
                    case MSGType.VEC2:
                        break;
                    case MSGType.VEC3UNIT:
                        Guid entityguid = NetMappers.BinaryToGuid(reader);
                        var vector3 = NetMappers.BinaryToVec3(reader);
                        OnVec3NetSendServer?.Invoke(result.Item2, entityguid, vector3);
                        //OnVec3NetSendPlayer?.Invoke(result.Item2, entityguid, vector3);
                        break;
                    case MSGType.POS2DDIST:
                        break;
                    case MSGType.POS3DDIST:
                        break;
                    case MSGType.BATTLESTATE:
                        Console.WriteLine("Received Battle State");
                        BattleState battleState = (BattleState)reader.ReadByte();
                        break;
                    default:
                        Console.WriteLine($"Received unknown message type: {result.Item1}");
                        throw new Exception("Unknown message type");
                        break;
                }
            }
        }
    }
}
