using Azure.Messaging.WebPubSub.Clients;
using Core.GameLoop;
using Core.GeometricEngine;
using Core.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Core.Networking
{
    public class ClientNetworkController
    {
        public WebPubSubClient client { get; set; }
        public string serverRoomId;
        public ClientNetworkController(WebPubSubClient _client, string _serverRoomId)
        {
            client = _client;

            serverRoomId = _serverRoomId;

            ClientNetEvents.OnVec3NetSendPlayer += OnUpdatePos;
            ClientNetEvents.OnBattleStateNetSend += OnUpdateBattleState;
        }

        #region RECEIVE_DATA_SERVER
        private void OnUpdatePos( Guid unitGuid, Vector3 vector3)
        {
            AffineTransformCore affineTransformCore = new AffineTransformCore(vector3);
            UnitsClientManager.Instance.networkMoveUnit(unitGuid, affineTransformCore);
        }
        private void OnUpdateBattleState(BattleState battleState)
        {            
            PlayerInfoSingleton.Instance.battleStateManager.currentState = battleState;
        }
        #endregion
        

        #region SEND_DATA_SERVER 
        public void updateUnitTransform(AffineTransformCore affine, Guid guid)
        {            
            BinaryData binaryData = NetMappers.ConvertPositionInfoToBinary(affine, guid);
            client.SendToGroupAsync(serverRoomId, new BinaryData(binaryData), WebPubSubDataType.Binary);
        }
        public void updateBattleState(BattleState currentState)
        {
           // BinaryData binaryData = NetMappers.ConvertPositionInfoToBinary(affine, guid);
            //client.SendToGroupAsync(serverRoomId, new BinaryData(binaryData), WebPubSubDataType.Binary);
        }
        #endregion
    }
}
