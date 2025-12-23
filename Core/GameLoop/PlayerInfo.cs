using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Core.Networking;
namespace Core.GameLoop
{
    public enum PlayerSpotEnum :byte
    {
        PLAYER1,
        PLAYER2        
    }
    
    public class PlayerInfo
    {                
        public string userId;
        public BattleStateManager battleStateManager;
        public bool connected = false;
        public ClientNetworkController networkController;
        public PlayerSpotEnum playerSpot { get; set; }
        public void initNetPlayer(string netId)
        {

            networkController = new ClientNetworkController();
        }
        public void initBattleState()
        {
            battleStateManager = new BattleStateManager();
            if (playerSpot == PlayerSpotEnum.PLAYER1)
            {
                battleStateManager.currentState = BattleState.strategic;
            }
            else
            {
                battleStateManager.currentState = BattleState.outofturn;
            }
        }

        public void setPlayerSpot(PlayerSpotEnum player)
        {
            playerSpot = player;
        }
    }
}
