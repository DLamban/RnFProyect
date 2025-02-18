using System;
using System.Collections.Generic;

namespace Core.DB.Models;

public partial class Weapon
{
    public int Id { get; set; }

    public string? Code { get; set; }

    public int? Range { get; set; }

    public int? Strength { get; set; }

    public int? IsStrengthFlat { get; set; }

    public int? Ap { get; set; }

    public virtual ICollection<WeaponsCharacter> WeaponsCharacters { get; set; } = new List<WeaponsCharacter>();

    public virtual ICollection<WeaponsTroop> WeaponsTroops { get; set; } = new List<WeaponsTroop>();
}
