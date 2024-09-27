using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
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
        public int Cost { get; set; }
        public Size Size { get; set; }
        public bool isDying { get; set; }
        public List<Modifiers> Mods { get; set; }
        [JsonConstructor]
        public BaseTroop(string name,string assetFile, int movement, int Dexterity, int Shooting, int strength, int Resistance, int wounds, int initiative, int attacks, int leadership, int armour, int wardSave, int regeneration, int cost, Size size, List<Modifiers> mods)
        {
            Name = name;
            AssetFile = assetFile;
            Movement = movement;
            Dexterity = Dexterity;
            Shooting = Shooting;
            Strength = strength;
            Resistance = Resistance;
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
