using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Rules;
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
    //It's gonna be more complex
    public class WoundingAttacks
    {
        public BaseTroop troop;
        public int numAttacks;
        public int AP;
        public List<BaseRule> specialRules = new List<BaseRule>();
        public WoundingAttacks(BaseTroop troop,int numAttacks, int AP, List<BaseRule> rules)
        {
            this.troop = troop;
            this.numAttacks = numAttacks;
            this.AP = AP;
            this.specialRules = rules;
        }
       
    }
    public static class Combat
    {
        public delegate Task<List<int>> DiceThrowerCombatTaskDelegate(int numberdices, string dicePhase, int dicetype = 6);
        private static DiceThrowerCombatTaskDelegate DiceThrowerTaskDel;        
        public static void vinculateDiceThrower(DiceThrowerCombatTaskDelegate diceThrowerTaskDelegate)
        {
            DiceThrowerTaskDel = diceThrowerTaskDelegate;
        }
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
        /// <summary>
        /// We put this calculation in combat class
        /// and as static, make it fast    
        /// inside is two Atan2, so it is not that fast
        /// </summary>
        /// <param name="unitA"></param>
        /// <param name="unitB"></param>
        /// <returns>the combat side that is A over B</returns>
        public static CombatSide calcCombatSide(BaseUnit unitA, BaseUnit unitB) {
            var angleA = unitA.Transform.currentAngleDegrees;
            var angleB = unitB.Transform.currentAngleDegrees;
            // watch put for orientation
            var angle = angleA - angleB;
            angle -= 180;

            if (angle < 0)
            {
                angle = 360 + angle;
            }
            if (angle > 315 || angle < 45)
            {
                return CombatSide.FRONT;
            }
            if (angle > 45 && angle < 135)
            {
                return CombatSide.LEFTFLANK;
            }
            if (angle > 135 && angle < 225)
            {
                return CombatSide.REAR;
            }
            if (angle > 225 && angle < 315)
            {
                return CombatSide.RIGHTFLANK;
            }
            return CombatSide.FRONT;
        }
        // need to check the units that are touching
        // and the units that are in the front line
        // for now only front to front
        public static async void combatUnit(BaseUnit attacker, BaseUnit defender)
        {
            //calculate the combat width
            int combatWidth = Math.Min(attacker.TroopsWidth, defender.TroopsWidth);
            // get front line troops            
            EngagedTroops troopsAtt = attacker.getEngagedTroops(defender);                        
            EngagedTroops troopsDefender = defender.getEngagedTroops(attacker);
            await combatRound(troopsAtt, troopsDefender, attacker, defender, 3);
        }
        
        public static async Task combatRound(EngagedTroops attackerTroops, EngagedTroops defenderTroops,BaseUnit attackUnit, BaseUnit defendUnit, int attackerChargeInitiativeBonus)
        {
            // find max initiative

            int maxInitiative = attackerTroops.directCombatTroops.DefaultIfEmpty().Max(troop => troop?.Initiative + attackerChargeInitiativeBonus ?? 0 );
            maxInitiative = Math.Max(maxInitiative, attackerTroops.supportingTroops.DefaultIfEmpty().Max(troop => troop?.Initiative + attackerChargeInitiativeBonus ?? 0 ));
            maxInitiative = Math.Max(maxInitiative, defenderTroops.directCombatTroops.DefaultIfEmpty().Max(troop => troop?.Initiative  ?? 0));
            maxInitiative = Math.Max(maxInitiative, defenderTroops.supportingTroops.DefaultIfEmpty().Max(troop => troop?.Initiative ?? 0));

            while (maxInitiative > 0)
            {
                // find the troops with that initiative
                List<BaseTroop> troopsDirectAttack = attackerTroops.directCombatTroops.FindAll(
                        troop => troop.Initiative + attackerChargeInitiativeBonus == maxInitiative && troop.Wounds > 0);
                List<BaseTroop> troopsSupportAttack = attackerTroops.supportingTroops.FindAll(
                        troop => troop.Initiative + attackerChargeInitiativeBonus == maxInitiative && troop.Wounds > 0);
                List<BaseTroop> troopsDirectDefend = defenderTroops.directCombatTroops.FindAll(
                        troop => troop.Initiative == maxInitiative && troop.Wounds > 0);
                List<BaseTroop> troopsSupportDefend =    defenderTroops.supportingTroops.FindAll(
                        troop => troop.Initiative == maxInitiative && troop.Wounds > 0);
                if (troopsDirectAttack.Count + troopsSupportAttack.Count + troopsDirectDefend.Count + troopsSupportDefend.Count == 0)
                {
                    maxInitiative--;
                    continue;
                }
                
                await executeCombatByInitiative(troopsDirectAttack, troopsSupportAttack, troopsDirectDefend, troopsSupportDefend, attackUnit, defendUnit);
                maxInitiative--;
            }

            

        }
        // calculate the wounds
        public static async Task<WoundingAttacks> calcWounds(BaseTroop troop, BaseUnit defUnit, int numAttacks)
        {
            DB.Models.Weapon meleeweapon = troop.Weapons.FirstOrDefault(weapon=>weapon.Range==0);
            int strength = (meleeweapon.IsStrengthFlat == 0 ? troop.Strength + meleeweapon.Strength : meleeweapon.Strength).Value;

            int toHit = toHitResult(troop.Dexterity, defUnit.Troop.Dexterity);
            List<int> tohitResult = await DiceThrowerTaskDel.Invoke(numAttacks, "Attacking with " + troop.Name);
            int tohitDicesSuccess = tohitResult.FindAll(res => res >= toHit).Count;
            if (tohitDicesSuccess == 0)
            {
                return new WoundingAttacks(troop,0, 0, new List<BaseRule>());
            }
            int toWound = woundResult(strength, defUnit.Troop.Resistance);
            List<int> towoundResult = await DiceThrowerTaskDel.Invoke(tohitDicesSuccess, "Wounding with " + troop.Name);
            int hittedAttacksToDefender = towoundResult.FindAll(res => res >= toWound).Count;
            if (hittedAttacksToDefender==0)
            {
                return new WoundingAttacks(troop, 0, 0, new List<BaseRule>());
            }
            return new WoundingAttacks(troop,hittedAttacksToDefender, meleeweapon.Ap.Value, new List<BaseRule>());
        }
        public static async Task executeCombatByInitiative(List<BaseTroop> attackerDirectTroops, List<BaseTroop> attackerSupportTroops, 
                                                     List<BaseTroop> defenderDirectTroops, List<BaseTroop> defenderSupportTroops,
                                                     BaseUnit attackUnit, BaseUnit defendUnit   )
        {
            int hitsAtttoDefend = 0;
            int hitsDefendtoAtt = 0;
            // first characters and champions
            // then the rest
            List<Character> charactersAttDirect = attackerDirectTroops.FindAll(troop => troop is Character).ConvertAll(troop => (Character)troop);
            List<Character> charactersAttSupport = attackerSupportTroops.FindAll(troop => troop is Character).ConvertAll(troop => (Character)troop);
            List<Character> charactersDefend = defenderDirectTroops.FindAll(troop => troop is Character).ConvertAll(troop => (Character)troop);
            List<Character> charactersDefendSupport = defenderSupportTroops.FindAll(troop => troop is Character).ConvertAll(troop => (Character)troop);

            List<BaseTroop> troopsAttDirect = attackerDirectTroops.FindAll(troop => !(troop is Character));
            List<BaseTroop> troopsAttSupport = attackerSupportTroops.FindAll(troop => !(troop is Character));
            List<BaseTroop> troopsDefend = defenderDirectTroops.FindAll(troop => !(troop is Character));
            List<BaseTroop> troopsDefendSupport = defenderSupportTroops.FindAll(troop => !(troop is Character));
            // ugly as fuck 

            //*********************************************************
            // NOT DICT, SPECIAL STRUCT WITH ALL THE SCPECIAL RULEs
            // **************************************************
            List<WoundingAttacks> woundingAttacksList = new List<WoundingAttacks>();
            List<WoundingAttacks> woundingDefenderList = new List<WoundingAttacks>();

            //ATACKER
            foreach (Character character in charactersAttDirect)
            {
                var woundindAttacks = await calcWounds(character, defendUnit, character.Attacks);
                woundingAttacksList.Add(woundindAttacks);
            }

            foreach (Character character in charactersAttSupport)
            {
                var woundindAttacks = await calcWounds(character, defendUnit, 1);
                woundingAttacksList.Add(woundindAttacks);

            }
            int troopsAttacks = 0;
            
            troopsAttacks += troopsAttDirect.Count * (troopsAttDirect.FirstOrDefault()?.Attacks ?? 0);
            troopsAttacks += troopsAttSupport.Count;

            if (troopsAttacks > 0) {
                var woundindAttacks = await calcWounds(attackUnit.Troop, defendUnit, troopsAttacks);
                woundingAttacksList.Add(woundindAttacks);
            }

            // DEFENDER TIME
            foreach (Character character in charactersDefend)
            {
                var woundindAttacks = await calcWounds(character, attackUnit, character.Attacks);
                woundingDefenderList.Add(woundindAttacks);
            }
            foreach (Character character in charactersDefendSupport)
            {
                var woundindAttacks = await calcWounds(character, attackUnit, 1);
                woundingDefenderList.Add(woundindAttacks);
            }

            int troopsDefendAttacks = 0;
            troopsDefendAttacks += troopsDefend.Count * (troopsDefend.FirstOrDefault()?.Attacks ?? 0);
            troopsDefendAttacks += troopsDefendSupport.Count;

            if (troopsDefendAttacks > 0)
            {
                var woundindAttacks = await calcWounds(defendUnit.Troop, attackUnit, troopsDefendAttacks);
                woundingDefenderList.Add(woundindAttacks);
            }
            // apply wounds// DAMMN AP irregular
            foreach (WoundingAttacks woundingAttacks in woundingAttacksList)
            {
                if (woundingAttacks.numAttacks == 0) return;
                await defendUnit.confirmWounds(woundingAttacks.numAttacks,woundingAttacks.specialRules, woundingAttacks.AP);
            }
            foreach (WoundingAttacks woundingAttacks in woundingDefenderList)
            {
                if (woundingAttacks.numAttacks == 0) return;
                await attackUnit.confirmWounds(woundingAttacks.numAttacks, woundingAttacks.specialRules, woundingAttacks.AP);
            }



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
        /// DEPRECATED
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
