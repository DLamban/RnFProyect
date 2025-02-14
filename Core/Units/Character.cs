using Core.List;
using Core.Rules;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Core.Units
{
    public class Character : BaseTroop
    {
        [JsonConstructor]
        public Character(string name, string assetFile, int movement, int dexterity, int shooting, int strength, int resistance, int wounds, int initiative, int attacks, int leadership, int armour, int wardSave, int regeneration, int cost, Size size, List<Modifiers> mods) : base(name, assetFile, movement, dexterity, shooting, strength, resistance, wounds, initiative, attacks, leadership, armour, wardSave, regeneration, cost, size, mods)
        {
        }
        public Character(DB.Models.Character character) : base(character)
        {
        }
    }
}
