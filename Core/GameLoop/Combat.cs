using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Units;
using Core.utils;

namespace Core.GameLoop
{
    public enum CombatSide
    {
        FRONT,
        LEFTFLANK,
        RIGHTFLANK,
        REAR
    }
    public static class Combat
    {
        private static int toHitResult(int WsAtt, int WsDef, int modifier=0)
        {
            int tohitResult = 0;
            if (WsDef > WsAtt * 2)
            {
                tohitResult = 5;
            }
            else if ( WsAtt > WsDef * 2 )
            {
                tohitResult = 2;
            } else if ( WsAtt > WsDef)
            {
                tohitResult = 3;
            } else {
                tohitResult = 4;
            }
            return tohitResult + modifier;
        }
        private static int woundResult(int strength, int Resistance, int modifier = 0)
        {
            int woundResult = 0;

            // not the fastest, but easier to remember, from top to bottom
            if (Resistance > strength + 5) //impossible
            {
                woundResult = 7;
            }
            else if (Resistance > strength + 1)
            {
                woundResult = 6;
            }else if(Resistance > strength)
            {
                woundResult = 5;
            }else if (Resistance == strength)
            {
                woundResult = 4;
            }else if (Resistance < strength - 1)
            {
                woundResult = 2;
            }else if ( Resistance < strength)
            {
                woundResult = 3;
            }
            return (woundResult + modifier);            
        }
        // need to check the units that are touching
        // and the units that are in the front line
        public static void combatUnit(BaseUnit attacker, BaseUnit defender)
        {
            //calculate the combat width
            int combatWidth = Math.Min(attacker.TroopsWidth, defender.TroopsWidth);
            // take the front line


        }

        public static void combat(List<BaseUnit> units, List<BaseUnit> enemyUnits)
        { 
            foreach (BaseUnit unit in units)
            {
                // find frontline in combat
            }
        }

        public static void singleCombat(BaseUnit unitA, BaseUnit unitB)
        {
            executeCombatRound(unitA, unitB);   
        }
        /// <summary>
        /// All the troops should be in place for the combat round,
        /// Characters at the front, readdress the flanks and center reasigned
        /// </summary>
        /// <param name="unitA"></param>
        /// <param name="unitB"></param>
        private static void executeCombatRound(BaseUnit unitA,BaseUnit unitB)
        {
            // forget about duel


            // take troops form the front width
            // for now we are going to assume that combat is front to front

            List<BaseTroop> troopsAFrontRank = unitA.Troops.GetRange(0, unitA.TroopsWidth);
            List<BaseTroop> troopsBFrontRank = unitB.Troops.GetRange(0, unitB.TroopsWidth);
            //find the highest initiative

            int maxInitiative = Math.Max(troopsAFrontRank.Max(troop => troop.Initiative), troopsBFrontRank.Max(troop => troop.Initiative));
            while(maxInitiative>0)
            {
                int aptest = 0;
                List<BaseTroop> troopsAAttack = troopsAFrontRank.FindAll(troop => troop.Initiative == maxInitiative && troop.Wounds >0);
                List<BaseTroop> troopsBAttack = troopsBFrontRank.FindAll(troop => troop.Initiative == maxInitiative && troop.Wounds >0);


                if (troopsAAttack.Count() > 0) {
                    Console.Write("linea A ");
                    Console.WriteLine(troopsAAttack.Count());
                    int hitsAtoB = executeAttack(troopsAAttack, troopsBFrontRank);
                    int wounds = ConfirmWoundsTroop(hitsAtoB, aptest, unitB);
                    unitB.ApplyWoundUnit(wounds);
                    Console.Write("hits from A:");
                    Console.WriteLine(hitsAtoB);
                    Console.Write("wounds form A:");
                    Console.WriteLine(wounds);
                }
                if (troopsBAttack.Count() > 0)
                {

                    Console.Write("linea B ");
                    Console.WriteLine(troopsBAttack.Count());
                    int hitsBtoA = executeAttack(troopsBAttack, troopsAFrontRank);
                    int wounds = ConfirmWoundsTroop(hitsBtoA, aptest, unitA);
                    unitA.ApplyWoundUnit(wounds);
                    Console.Write("hits from B:");
                    Console.WriteLine(hitsBtoA);
                    Console.Write("wounds form B:");
                    Console.WriteLine(wounds);
                }
                maxInitiative--;
            }
        }
       
        private static int executeAttack(List<BaseTroop> baseTroopsAtt, List<BaseTroop> defenders)
        {
            int successAtt = 0;
            // take the first, this will change with champions and characters etc etc
            BaseTroop defenderExample = defenders.First();
            // we need the weapon skill table

            foreach (BaseTroop attacker in baseTroopsAtt)
            {
                int attacks = attacker.Attacks;
                int toHit = toHitResult(attacker.Dexterity, defenderExample.Dexterity);
                for (int i = 0;i < attacks; i++)
                {
                    if (DiceRoller.rolld6() >= toHit)
                    {
                        successAtt++;
                    }
                }
            }
            int successWounds = 0;
            // again, let's test with example troops
            int wound = woundResult(baseTroopsAtt.First().Strength, defenderExample.Resistance);

            for (int i = 0;i<successAtt;i++)
            {
                if (DiceRoller.rolld6() >= wound)
                {
                    successWounds++;
                }                    
            }
            return successWounds;
        }
        // We take he wounds, so is inversed.
        // it must be less to wound, not less or equal
        // and we start at 7 because no armour = no save
        // MISSING WARD SAVE AND REGENERATION
        public static int ConfirmWoundsTroop(int hits, int ap, BaseUnit defenderUnit)
        {
            int wounds = 0;
            BaseTroop defenderExample = defenderUnit.Troop;
            // armmourasve = 6 - (armourvalue - ap) // clamped to 0-5 the armour value
            int armoursave = 7 - Math.Clamp(defenderExample.Armour - ap, 0,5);
            if (armoursave > 6) // automatic wound
            {
                wounds = hits;
                return wounds;
            }
            for (int i = 0; i < hits; i++)
            {
                if (DiceRoller.rolld6() < armoursave)
                {
                    wounds++;
                }
            }
            return wounds;
        }
    }
}
