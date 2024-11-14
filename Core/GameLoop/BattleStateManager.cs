using System;
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
    public enum SubBattleStatePhase { 
        strategic,
        charge,
        move,
        compulsory_move,
        shoot,
        combat
    }
   
    public class BattleStateManager
    {
        public EventHandler<BattleState> OnBattleStateChanged;
        public event Action<SubBattleStatePhase> OnSubPhaseChanged;
        private List<BattleState> states;
        private List<SubBattleStatePhase> subPhaseStates;
        
        private BattleState _currentState;
        public BattleState currentState
        {
            get { return _currentState; }
            set { 
                _currentState = value;
                stateChanged();
            }        
        }
        
        private SubBattleStatePhase _currentSubPhase;
        public SubBattleStatePhase currentSubPhase
        {
            get { return _currentSubPhase; }
            set
            {
                _currentSubPhase = value;
                subStateChanged();
            }
        }
        public void stateChanged()
        {
            OnBattleStateChanged?.Invoke(this, currentState);
        }
        public void subStateChanged()
        {
            OnSubPhaseChanged?.Invoke(currentSubPhase);
        }
        public BattleStateManager() {

            states = Enum.GetValues(typeof(BattleState)).Cast<BattleState>().ToList();
            subPhaseStates = Enum.GetValues(typeof(SubBattleStatePhase)).Cast<SubBattleStatePhase>().ToList();
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
        public void passNextSubState()
        {
            int index = subPhaseStates.FindIndex(x => x == currentSubPhase);
            if (index == subPhaseStates.Count() - 1)
            {
                currentSubPhase = subPhaseStates[0];
            }
            else
            {
                currentSubPhase = subPhaseStates[index + 1];
            }
        }
    }
}
