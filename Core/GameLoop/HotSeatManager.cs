using Core.List;
using Core.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.GameLoop
{
    public class HotSeatManager
    {

        private static readonly HotSeatManager instance = new HotSeatManager();
        public static HotSeatManager Instance
        {
            get
            {
                return instance;
            }
        }
        public PlayerInfo player1Info;
        public PlayerInfo player2Info;
        public PlayerSpotEnum currentPlayer;
        public bool isHotseat = false;
        public HotSeatManager()
        {
            init();
        }
        public void init()
        {
            player1Info = new PlayerInfo();
            player2Info = new PlayerInfo();
            player1Info.playerSpot = PlayerSpotEnum.PLAYER1;
            player2Info.playerSpot = PlayerSpotEnum.PLAYER2;
            player1Info.initBattleState();
            player2Info.initBattleState();

            currentPlayer = PlayerSpotEnum.PLAYER1;
        }
        public void changeCurrentPlayer(PlayerSpotEnum playerSpot)
        {
            currentPlayer = playerSpot;
        }
        public PlayerInfo getCurrentPlayer()
        {
            if (currentPlayer == PlayerSpotEnum.PLAYER1)
            {
                return player1Info;
            }
            else
            {
                return player2Info;
            }
        }
        public void populateBoard()
        {
            MockList mockList = new MockList(1);
            var unitsplay1 = mockList.unitManagerCore.getUnitsPlayer1();
            var unitsplay2 = mockList.unitManagerCore.getUnitsPlayer2();
            UnitsClientManager.Instance.addAllPlayerUnits(unitsplay1);
            UnitsClientManager.Instance.addAllEnemyUnits(unitsplay2);
            UnitsClientManager.Instance.unitsLoaded();
        }
    }
}
