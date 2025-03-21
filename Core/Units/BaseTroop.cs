using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Core.DB.Data;
using Core.DB.Models;
using Core.Rules;
using Core.Units.Weapons;

namespace Core.Units
{
    public class BaseTroop
    {
        public string Name { get; set; }
        public string AssetFile { get; set; }
        private int _movement;
        public int Movement
        {
            get { return applyModifiers(Attribute_Affected.MOVEMENT, _movement);}
            set { _movement = value; }
        }
        // We are using Dm because reasons
        public float MovementDm
        {
            get { return (float)((Movement * 2.54f) / 10); }
        }
        //Rebuild the class to use the Status class
        private int _Dexterity;
        public int Dexterity
        {
            get { return applyModifiers(Attribute_Affected.WEAPON_SKILL, _Dexterity); }
            set { _Dexterity = value; }
        }
        private int _Shooting;
        public int Shooting
        {
            get { return applyModifiers(Attribute_Affected.BALLISTIC_SKILL, _Shooting); }
            set { _Shooting = value; }
        }
        private int _strength;
        public int Strength
        {
            get { return applyModifiers(Attribute_Affected.STRENGTH, _strength); }
            set { _strength = value; }
        }
        private int _Resistance;
        public int Resistance
        {
            get { return applyModifiers(Attribute_Affected.Resistance, _Resistance); }
            set { _Resistance = value; }
        }
        private int _wounds;
        public int Wounds
        {
            get { return applyModifiers(Attribute_Affected.WOUNDS, _wounds); }
            set { _wounds = value; }
        }
        private int _initiative;
        public int Initiative
        {
            get { return applyModifiers(Attribute_Affected.INITIATIVE, _initiative); }
            set { _initiative = value; }
        }
        private int _attacks;
        public int Attacks
        {
            get { return applyModifiers(Attribute_Affected.ATTACKS, _attacks); }
            set { _attacks = value; }
        }
        private int _leadership;
        public int Leadership
        {
            get { return applyModifiers(Attribute_Affected.LEADERSHIP, _leadership); }
            set { _leadership = value; }
        }
        private int _armour;
        public int Armour
        {
            get { return applyModifiers(Attribute_Affected.ARMOUR, _armour); }
            set { _armour = value; }
        }
        private int _wardSave;
        public int WardSave
        {
            get { return applyModifiers(Attribute_Affected.WARD_SAVE, _wardSave); }
            set { _wardSave = value; }
        }
        private int _regeneration;
        public int Regeneration
        {
            get { return applyModifiers(Attribute_Affected.REGENERATION, _regeneration); }
            set { _regeneration = value; }
        }
        public BaseWeapon Weapon { get; set; }
        [JsonIgnore]
        public List<Weapon> Weapons { get; set; }
        public bool canShoot { get; set;}
        public int Cost { get; set; }
        public Size Size { get; set; }
        public float Heightdm{ get { return (Size.Height / 100f); } }
        public float Widthdm { get { return (Size.Width / 100f); } }
        public bool isDying { get; set; }
        
        public List<Modifiers> Mods { get; set; }
        [JsonConstructor]
        public BaseTroop(string name,string assetFile, int movement, int dexterity, int shooting, int strength, int resistance, int wounds, int initiative, int attacks, int leadership, int armour, int wardSave, int regeneration, int cost, Size size, List<Modifiers> mods)
        {
            Name = name;
            AssetFile = assetFile;
            Movement = movement;
            Dexterity = dexterity;
            Shooting = shooting;
            Strength = strength;
            Resistance = resistance;
            Wounds = wounds;
            Initiative = initiative;
            Attacks = attacks;
            Leadership = leadership;
            Armour = armour;
            WardSave = wardSave;
            Regeneration = regeneration;
            Cost = cost;
            Size = size;
            isDying = false;
            //Weapon = new BaseWeapon();
            Mods = new List<Modifiers>();
        }
        public BaseTroop(BaseTroop baseTroop) { 
            Name = baseTroop.Name;
            AssetFile = baseTroop.AssetFile;
            Movement = baseTroop.Movement;
            Dexterity = baseTroop.Dexterity;
            Shooting = baseTroop.Shooting;
            Strength = baseTroop.Strength;
            Resistance = baseTroop.Resistance;
            Wounds = baseTroop.Wounds;
            Initiative = baseTroop.Initiative;
            Attacks = baseTroop.Attacks;
            Leadership = baseTroop.Leadership;
            Armour = baseTroop.Armour;
            WardSave = baseTroop.WardSave;
            Regeneration = baseTroop.Regeneration;
            Cost = baseTroop.Cost;
            Size = baseTroop.Size;
            isDying = false;
            //Weapon = new BaseWeapon();
            Mods = new List<Modifiers>();
        }
        public BaseTroop(TroopProfile troopProfile)
        {
            Mods = new List<Modifiers>();
            Name = troopProfile.Name;
            AssetFile = troopProfile.AssetFile;
            Movement = troopProfile.Movement;
            Dexterity = troopProfile.Dexterity;
            Shooting = troopProfile.Shooting;
            Strength = troopProfile.Strength;
            Resistance = troopProfile.Resistance;
            Wounds = troopProfile.Wounds;
            Initiative = troopProfile.Initiative;
            Attacks = troopProfile.Attacks;
            Leadership = troopProfile.Leadership;
            Armour = troopProfile.Armour;
            Cost = troopProfile.Cost;
            Size = new Size(troopProfile.BaseSize.Width, troopProfile.BaseSize.Height);
            var weapons = troopProfile.WeaponsTroops.Select(wt => wt.Weapon).ToList();
            // set basic hand weapon if no weapon is found
            if (weapons.FindAll((w) => w.Range == 0).Count == 0)
            {
                weapons.Add(DBSingleton.getBasicWeapon());
            }
            Weapons = weapons;
            foreach (var weapon in weapons)
            {
                if (weapon.Range > 0)
                {
                    canShoot = true;
                }
            }


        }

        public BaseTroop(DB.Models.Character character)
        {
            Mods = new List<Modifiers>();
            Name = character.Name;
            AssetFile = character.AssetFile;
            Movement = character.Movement;
            Dexterity = character.Dexterity;
            Shooting = character.Shooting;
            Strength = character.Strength;
            Resistance = character.Resistance;
            Wounds = character.Wounds;
            Initiative = character.Initiative;
            Attacks = character.Attacks;
            Leadership = character.Leadership;
            Armour = character.Armour;
            Cost = character.Cost;
            Size = new Size(character.BaseSize.Width, character.BaseSize.Height);
            List<Weapon?> weapons = character.WeaponsCharacters.Select(wt => wt.Weapon).ToList();
            // set basic hand weapon if no weapon is found
            if (weapons.FindAll((w) => w.Range == 0).Count == 0)
            {
                weapons.Add(DBSingleton.getBasicWeapon());
            }
            Weapons = weapons;

        }

        private int applyModifiers(Attribute_Affected attr,int value)
        {
            // el foreach no me entusiasma, pero es posible tener multiples status por atributo
            // asi que descartamos el diccionario :(
            foreach (Modifiers mods in Mods)
            {
                if (mods.AttributeAffected == attr)
                {
                    value += mods.Value;
                }
            }
            return value;
        }


    }
}
