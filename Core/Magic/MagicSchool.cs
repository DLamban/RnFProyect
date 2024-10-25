using Core.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Core.Magic
{
    public class MagicSchool
    {
        public string Name { get; set; }

        public List<Spell> Spells { get; set; }
        [JsonConstructor]
        public MagicSchool(string name, List<Spell> spells)
        {
            Name = name;
            Spells = spells;

        }
    }
}
