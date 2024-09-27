using Azure.Core;
using Azure.Messaging.WebPubSub;
using Azure.Messaging.WebPubSub.Clients;
using Core.GameLoop;

using Core.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Text;
using System.Threading.Tasks;
using Server;

namespace ServerSocket
{


    public class ServerEventsHandler
    {              
        private WebPubSubServiceClient server;
        private string player1Id;
        private string player2Id;
        public ServerEventsHandler(WebPubSubServiceClient _server, string _playerId1, string _playerId2, ServerInstance serverInstance)
        {   
            server = _server;
            player1Id = _playerId1;
            player2Id = _playerId2;
            // setup events
            serverInstance.OnVec3NetSendServer += UpdatePos;            
        }
        // UPDATE THE POSITION OF THE UNIT, both in the server and in the client
        // REMEMBER, authority is in the server
        private void UpdatePos(SenderMessageEnum sender, Guid unitGuid, Vector3 vector3)
        {
            //UPDATE THE POSITION IN THE SERVER
            UpdatePosServer(unitGuid, vector3);
            //UPDATE THE POSITION IN THE CLIENT
            UpdatePosClient(sender, unitGuid, vector3);
        }



        private void UpdatePosServer(Guid unitguid, Vector3 pos)
        {
            // do nothing, for now
        }
        private void UpdatePosClient(SenderMessageEnum sender, Guid unitguid, Vector3 pos)
        {
            BinaryData binaryData = NetMappers.ConvertPositionInfoToBinary(SenderMessageEnum.SERVER ,pos, unitguid);
            // SEND THE POSITION TO THE CLIENT
            if (sender == SenderMessageEnum.PLAYER1)
            {
                server.SendToUserAsync(player2Id,binaryData, ContentType.ApplicationOctetStream);
            }
            else
            {
                server.SendToUserAsync(player1Id, binaryData, ContentType.ApplicationOctetStream);
            }
        }

        private void updateBattleState(SenderMessageEnum sender, BattleStates currentState)
        {
            BinaryData binaryData = NetMappers.ConvertBattleStateToBinary(sender,currentState);
            server.SendToAllAsync(binaryData, ContentType.ApplicationOctetStream);
        }
    }
}
