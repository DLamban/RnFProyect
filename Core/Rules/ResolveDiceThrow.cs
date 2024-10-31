using Core.Units;
using Core.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Rules
{
    public static class ResolveDiceThrow
    {
        #region TOHIT
        private static int toHitTableResult(int WsAtt, int WsDef)
        {
            int tohitResult = 0;
            if (WsDef > WsAtt * 2)
            {
                tohitResult = 5;
            }
            else if (WsAtt > WsDef * 2)
            {
                tohitResult = 2;
            }
            else if (WsAtt > WsDef)
            {
                tohitResult = 3;
            }
            else
            {
                tohitResult = 4;
            }
            return tohitResult;
        }
        public static int resolveToHit(List<int> diceValues, int dexAttacker, int dexDefender, IReadOnlyList<BaseRule> specialRules = null)
        {
            int hits = 0;
            // put an empty list, so we avoid null checks
            if (specialRules == null) specialRules = new List<BaseRule>();
            int toHitVal = toHitTableResult(dexAttacker, dexDefender);
            foreach (int val in diceValues)
            {
                // TODO: remember to apply especial rules 
                if (val >= toHitVal)
                {
                    hits++;
                }
            }
            return hits;
        }
        #endregion
        #region TOWOUND
        private static int woundTableResult(int strength, int resistance)
        {
            int woundResult = 0;

            // not the fastest, but easier to remember, from top to bottom
            if (resistance > strength + 5) //impossible
            {
                woundResult = 7;
            }
            else if (resistance > strength + 1)
            {
                woundResult = 6;
            }
            else if (resistance > strength)
            {
                woundResult = 5;
            }
            else if (resistance == strength)
            {
                woundResult = 4;
            }
            else if (resistance < strength - 1)
            {
                woundResult = 2;
            }
            else if (resistance < strength)
            {
                woundResult = 3;
            }
            return (woundResult);
        }
        public static int resolveToWound(List<int> diceValues, int strenght, int resistance, IReadOnlyList<BaseRule> specialRules = null)
        {
            int wounds = 0;
            // put an empty list, so we avoid null checks
            if (specialRules == null) specialRules = new List<BaseRule>();
            int toWoundVal = woundTableResult(strenght, resistance);
            foreach (int val in diceValues)
            {
                // TODO: remember to apply especial rules 
                if (val >= toWoundVal)
                {
                    wounds++;
                }
            }
            return wounds;
        }
        #endregion
        #region SAVE
        // We take he wounds, so is inversed.
        // it must be less to wound, not less or equal
        // and we start at 7 because no armour = no save
        public static int saveTable(int armour, int ap = 0)
        {
            int save = 7 - Math.Clamp(armour - ap, 0, 5);
            return save;
        }
        public static int armourSave(int wounds, List<int> savingDices,int ap, int armour, IReadOnlyList<BaseRule> specialRules = null)
        {
            int confirmedwounds = 0;
            // armmourasve = 6 - (armourvalue - ap) // clamped to 0-5 the armour value
            int armoursave = saveTable(armour, ap);
            if (armoursave > 6) // automatic wound
            {               
                return wounds;
            }
            foreach(int val in savingDices)
            {
                if (val < armoursave)
                {
                    confirmedwounds++;
                }
            }
            return confirmedwounds;
        }
        #endregion
    }
}
