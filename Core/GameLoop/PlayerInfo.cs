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
        PLAYER1  = 0b0000_0000,
        PLAYER2  = 0b1000_0000,
        HOT_SEAT = 0b0001_0000
    }
    
    public class PlayerInfo
    {                
        public string userId;
        public BattleStateManager battleStateManager;
        // azure webpubsub vars
        private string connectionString;        
        //public WebPubSubClient client;
        public bool connected = false;
        public ClientNetworkController clientNetworkController { get; set; }             
        public PlayerSpotEnum playerSpot { get; set; }
        
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
