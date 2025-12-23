using Aranfee;
using Core.GameLoop;
using Core.GeometricEngine;
using Core.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static Core.GeometricEngine.GeometryUtils;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Core.Networking
{
    public class ClientNetworkController
    {
        private string networkId;
        public ClientNetworkController()
        {
            NakamaService.Instance.OnReceiveUnitPosition += OnUpdatePos;
        }
        public void setNetId(string netId)
        {
            networkId = netId;
        }
        #region RECEIVE_DATA_SERVER
        private void OnUpdatePos(UnitPosition unitpos)
        {
            Vector2 position = new Vector2(unitpos.Position.X, unitpos.Position.Y);
            Vector2 director = new Vector2(unitpos.Director.X, unitpos.Director.Y);
            AffineTransformCore newAffine = new AffineTransformCore(position,director);
            if (Guid.TryParse(unitpos.Guid, out Guid unitGuid))
            {

                UnitsClientManager.Instance.networkMoveUnit(unitGuid, newAffine);
            }
            else
            {
                Console.WriteLine("Invalid GUID format: " + unitpos.Guid);
            }
                
        }
        private void OnUpdateBattleState(BattleState battleState)
        {
            PlayerInfoSingletonHotSeat.Instance.battleStateManager.currentState = battleState;
        }
        #endregion


        #region SEND_DATA_SERVER 
        public void updateUnitTransform(BaseUnit unit)
        {            
            NakamaService.Instance.sendUpdatedUnitPosition(unit);
        }
        public void updateBattleState(BattleState currentState)
        {
           
        }
        #endregion
    }
}
