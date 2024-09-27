using Core.GameLoop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.PlayerInfoCopy
{
    // keep a copy of the info in the server
    internal class PlayerServerInfo
    {
        //public static PlayerInfo[] playerInfos;
        private PlayerInfo player1Info;
        private PlayerInfo player2Info;
        public enum TurnEnum : byte        
        {
            TURNPLAYER1,
            TURNPLAYER2
        }
        private TurnEnum currentTurn;
        public PlayerServerInfo(string player1Id, string player2Id)
        {
            player1Info = new PlayerInfo();
            player2Info = new PlayerInfo();
            player1Info.userId = player1Id;
            player2Info.userId = player2Id;
        }
        public void startGame()
        {
            
            player1Info.battleStateManager.currentState = BattleStates.strategic;
            
            player2Info.battleStateManager.currentState = BattleStates.outofturn;            
            
            currentTurn = TurnEnum.TURNPLAYER1;
        }

        public void nextPhase()
        {
            if (currentTurn == TurnEnum.TURNPLAYER1)
            {
                player1Info.battleStateManager.passNextState();
                if (player1Info.battleStateManager.currentState == BattleStates.outofturn)
                {
                    endTurn(currentTurn);
                }
            }
            else
            {
                player2Info.battleStateManager.passNextState();
                if (player2Info.battleStateManager.currentState == BattleStates.outofturn)
                {
                    endTurn(currentTurn);
                }
            }
        }
        public void endTurn(TurnEnum endingTurn) 
        { 
            if(endingTurn == TurnEnum.TURNPLAYER1)
            {
                player1Info.battleStateManager.currentState = BattleStates.outofturn;
                player2Info.battleStateManager.currentState = BattleStates.strategic;
                currentTurn = TurnEnum.TURNPLAYER2;
            }
            else
            {
                player2Info.battleStateManager.currentState = BattleStates.outofturn;
                player1Info.battleStateManager.currentState = BattleStates.strategic;
                currentTurn = TurnEnum.TURNPLAYER1;
            }
        }
        public void endGame()
        {
            player1Info = null;
            player2Info = null;
        }
    }
}
