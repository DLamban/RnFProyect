using Core.GameLoop;
using Core.Magic;
using Core.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Units
{
    public class TemporalCombatVars
    {
        // Combat temporal variables, restart every turn
        public List<Spell> spellsAffecting = new List<Spell>();
        public CombatSide CombatSide { get; set; }
        public bool isCharging { get; set; }
        public bool isCharged { get; set; }
        public bool isInCombatRange { get; set; } = false;
        public List<BaseUnit> inCombatUnits { get; set; } = new List<BaseUnit>();
        public Dictionary<CombatSide, bool> occupiedCombatSides { get; set; } = new Dictionary<CombatSide, bool>();
        public ChargeResponse chargeResponse { get; set; }
        public float distanceRemaining { get; set; }
        public TemporalCombatVars()
        {
            foreach(var combatSide in Enum.GetValues(typeof(CombatSide)))
            {
                occupiedCombatSides.Add((CombatSide)combatSide, false);
            }
        }


    }
}
