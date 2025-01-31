using System.Text.Json.Serialization;
using Core.Units;

namespace Core.List
{
    public class BaseRace
    {
        public string Name { get; set; }

        public List<BaseUnit> Units { get; set; }
        public List<Character> Characters { get; set; }
        
        [JsonConstructor]
        public BaseRace(string name, List<Character> characters, List<BaseUnit> units)
        {
            Name = name;            
            Units = units;            
            if (characters == null)  characters = new List<Character>();
            Characters = characters;
        }        
    }
}
