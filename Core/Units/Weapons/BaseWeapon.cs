using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Units.Weapons
{
    public class BaseWeapon
    {
        public string Name { get; set; }
        public int Range { get; set; }
        public int Strength { get; set; }
        public int ArmourPiercing { get; set; }
        public int Cost { get; set; }
        public bool ExtremellyCommon { get; set; }
        public List<string> SpecialRules { get; set; }
        public BaseWeapon(string name, int range, int strenght, int armourPiercing,int cost, bool extremellyCommon,List<string> specialRules)
        {
            Name = name;
            Range = range;
            Strength = strenght;
            ArmourPiercing = armourPiercing;
            Cost = cost;
            ExtremellyCommon = extremellyCommon;
            SpecialRules = specialRules;

        }
    }
}
