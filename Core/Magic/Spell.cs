using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Core.Magic
{
    public enum SpellTarget
    {
        [EnumMember(Value = "OwnTroops")]
        OwnTroops,
        [EnumMember(Value = "EnemyTroops")]
        EnemyTroops,
        [EnumMember(Value = "Self")]
        Self,
        [EnumMember(Value = "Range")]
        Range
    }
    public class Spell
    {
        public string Name { get; set; }   
        public string Image { get; set; }
        public string Description { get; set; }
        public int Difficulty { get; set; }
        public int Range { get; set; }
        public string Type { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public SpellTarget Target { get; set; }

        
        [JsonConstructor]
        public Spell(string name, string image,string description, int difficulty,int range,string type, SpellTarget target) { 
            Name = name;
            Image = image;
            Description = description;
            Difficulty = difficulty;
            Range = range;
            Type = type;
            Target = target;

        }
    }
}
