﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.GameLoop
{
    public enum BattleState:byte
    {
        strategic,
        //magic,
        //charge,
        move,
        shoot,
        combat,
        outofturn
    }
    
    public class BattleStateManager
    {
        private List<BattleState> states;
        private BattleState _currentState;

        public BattleState currentState
        {
            get { return _currentState; }
            set { _currentState = value; }
        }
        
        public BattleStateManager() {

            states = Enum.GetValues(typeof(BattleState)).Cast<BattleState>().ToList();
        }        
        public void passNextState()
        {
            int index = states.FindIndex(x => x == currentState);
            if (index == states.Count()-1)
            {
                currentState = states[0];
            } else { 
                currentState = states[index+1];
            }
        }
    }
}
