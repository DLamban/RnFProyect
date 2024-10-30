using Core.List;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Magic
{
    
    public class SpellManager
    {

        private static readonly SpellManager _instance = new SpellManager();
        public static SpellManager Instance => _instance;
        public Dictionary<string, MagicSchool> magicSchools;
        public List<Spell> usedSpells = new List<Spell>();
        public event Action<Spell> OnSpellUsed;
        public SpellManager() {            
            magicSchools = JSONLoader.LoadSpellJSON();
        }
        
        public List<Spell> getSpellsBySchool(MagicSchool school)
        {
            return magicSchools[school.Name].Spells;
        }        
        public List<Spell> getSpellsByWizardLevelAndSchool( int level, MagicSchool school)
        {
            Random random = new Random();
            List<Spell> randomSpells = getSpellsBySchool(school)
                .OrderBy(x => random.Next())
                .Take(level)
                .ToList();          
            return randomSpells;
        }
        public void spellUsed(Spell spell) { 
            usedSpells.Add(spell);
            OnSpellUsed?.Invoke(spell);
        }
    }

}
