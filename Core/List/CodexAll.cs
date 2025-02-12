using Core.Units;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.List
{
    public class CodexAll
    {
        public static Dictionary<string, BaseRace> races = new Dictionary<string, BaseRace>();
        private static readonly CodexAll instance = new CodexAll();        
        public static CodexAll Instance
        {
            get
            {
                return instance;
            }
        }
        public CodexAll()
        {
            races = JSONLoader.LoadRacesJSON();
        }
        public BaseRace getRaceCodex(string race)
        {
            return races[race];
        }
        /// <summary>
        /// We do a search in all races, it will hurt performance, but I think reduces complexity, it's a tradeoff
        /// </summary>
        /// <param name="unitname"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public BaseUnit getUnitCodex( string unitname)
        {
            foreach (BaseRace race in races.Values)
            {
                BaseUnit? unit = race.Units.Find(unit => unit.Name == unitname);
                if (unit != null) return unit;  
            }
            throw new KeyNotFoundException($"Unit '{unitname}' not found");
        }
        public Character getCharCodex(string charname)
        {
            foreach (BaseRace race in races.Values)
            {
                Character? character = race.Characters.Find(character => character.Name == charname);
                if (character != null) return character;                
            }
            throw new KeyNotFoundException($"Unit '{charname}' not found");
        }
    }
}
