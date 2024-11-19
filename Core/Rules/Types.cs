using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Rules
{
    
    public enum UnitType
    {
        INFANTRY,
        CAVALRY,
        CHARIOT,
        MONSTER,
        WAR_MACHINE

    }
    public enum UnitCategory
    {
        CORE,
        SPECIAL,
        RARE,
        CHARACTER
    }
    public enum InfantryType
    {
        REGULAR,
        HEAVY,
        MONSTRUOUS,
        SWARM
    }
    public enum CavalryType
    {
        LIGHT,
        HEAVY,
        MONSTRUOUS,
        WAR_BEAST
    }
    public enum ChariotType
    {
        LIGHT,
        HEAVY
    }
    public enum MonsterType
    {
        REGULAR,
        BEHEMOTH
    }
    public enum Attribute_Affected
    {
        MOVEMENT,
        WEAPON_SKILL,
        BALLISTIC_SKILL,
        STRENGTH,
        Resistance,
        WOUNDS,
        INITIATIVE,
        ATTACKS,
        LEADERSHIP,
        ARMOUR,
        WARD_SAVE,
        REGENERATION,
    }
    public enum Formation_type
    {
        CLOSE_ORDER,
        OPEN_ORDER,
        SKIRMISH
    }
    public enum ChargeResponse
    {
        HOLD,
        STANDANDSHOOT,
        FLEE
    }
}
