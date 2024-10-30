using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Core.Magic;
using Core.Units;

namespace Core.List
{
    

    public static class JSONLoader
    {
        // why automatize something that is already simple
        public static Dictionary<string, BaseRace> races = new Dictionary<string, BaseRace>();
        public static Dictionary<string, MagicSchool> magicSchools = new Dictionary<string,MagicSchool>();
        public static Dictionary<string, MagicSchool> LoadSpellJSON()
        {
            string resourceName = "Core.List.Spells.BattleMagic.json";
            LoadMagicResource(resourceName);
            return magicSchools;

        }
        private static void LoadMagicResource(string resourceName)
        {
            string magicSchool = resourceName.Split('.')[3];
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    // Leer el archivo JSON
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string json = reader.ReadToEnd();
                        // Hacer algo con el JSON
                        MagicSchool magicSchoolObj = JsonSerializer.Deserialize<MagicSchool>(json);
                        magicSchools.Add(magicSchoolObj.Name, magicSchoolObj);
                    }
                }
            }
        }
        public static Dictionary<string, BaseRace> LoadJSON()
        {
            // REMEMBER, WE ARE LOADING FROM EMBEDDED RESOURCES
            // SO REMEMBER TO EMBED THE JSON FILES
            // TODO: read every json file in the folder races
            // and load it into the dictionary
            string resourceName = "Core.List.races.Dwarfs.json";
            // Cargar el recurso incrustado
            LoadResource(resourceName);
            resourceName = "Core.List.races.Ratkin.json";

            // Cargar el recurso incrustado
            LoadResource(resourceName);
            resourceName = "Core.List.races.Orcs.json";
            LoadResource(resourceName);
            return races;
        }
        private static void LoadResource(string resourceName)
        {
            string race = resourceName.Split('.')[3];
            using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    // Leer el archivo JSON
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string json = reader.ReadToEnd();
                        // Hacer algo con el JSON
                        BaseRace baseRace =JsonSerializer.Deserialize<BaseRace>(json);
                        races.Add(race, baseRace);
                    }
                }
            }
        }
        public static BaseRace GetRace(string raceString)
        {
            return races[raceString];
        }
        public static string getpath()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }
    }
}
