using System.Text.Json.Serialization;
using Core.Units;

namespace Core.List
{
    public class BaseRace
    {
        public string Name { get; set; }

        public List<BaseUnit> Units { get; set; }
        [JsonConstructor]
        public BaseRace(string name, List<BaseUnit> units)
        {
            Name = name;            
            Units = units;

        }        
    }
}
