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

namespace Core.Networking
{
    public class ClientNetworkController
    {        
        #region RECEIVE_DATA_SERVER
        private void OnUpdatePos(Guid unitGuid, Vector3 vector3)
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
