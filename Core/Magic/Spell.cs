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
    public enum SpellType
    {
        [EnumMember(Value = "Hex")]
        Hex,
        [EnumMember(Value = "Buff")]
        Buff,
        [EnumMember(Value = "ThrowingMagic")]
        ThrowingMagic
    }

    public class Spell
    {
        public string Name { get; set; }   
        public string Image { get; set; }
        public string Description { get; set; }
        public int Difficulty { get; set; }
        public int Range { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]

        public SpellType Type { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public SpellTarget Target { get; set; }
        
        public bool used;
        
        [JsonConstructor]
        public Spell(string name, string image,string description, int difficulty,int range,SpellType type, SpellTarget target) { 
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
