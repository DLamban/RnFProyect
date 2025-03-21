using Core.DB.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DB.Data
{
    internal class DBSingleton
    {        
        private static GameDbContext _instance;
        public static GameDbContext Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameDbContext();
                }
                return _instance;
            }
        }
        public static Weapon getBasicWeapon()
        {
            return Instance.Weapons.FirstOrDefault(w => w.Code == "Hand Weapon");
        }
        public static Character GetCharacter(int id)
        {
            return Instance.Characters
                .Include(character => character.BaseSize)
                .Include(character => character.Race)                
                .Include(character => character.WeaponsCharacters)
                    .ThenInclude(wc => wc.Weapon)
                .FirstOrDefault(c => c.Id == id);
        }
        public static Character GetCharacter(CharacterEnum characterEnum)
        {
            return GetCharacter((int)characterEnum);
        }
        public static Unit GetUnit(UnitEnum unit)
        {
            Unit unitDetail = Instance.Units
                    .Include(u => u.Formation) // Carga la formación
                    .Include(u => u.Race)
                    .Include(u => u.TroopProfiles)
                        .ThenInclude(tp => tp.TroopType)
                    .Include(u => u.TroopProfiles)
                        .ThenInclude(tp => tp.BaseSize)
                    .Include(u => u.TroopProfiles)
                        .ThenInclude(tp => tp.Category)
                    .Include(u => u.TroopProfiles)
                        .ThenInclude(tp => tp.WeaponsTroops)
                            .ThenInclude(wt => wt.Weapon)                                        
                    .FirstOrDefault(u => u.Id == (int)unit);
            return unitDetail;
        }

        

    }
}
