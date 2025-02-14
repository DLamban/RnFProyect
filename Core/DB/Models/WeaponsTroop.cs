using System;
using System.Collections.Generic;

namespace Core.DB.Models;

public partial class WeaponsTroop
{
    public int Id { get; set; }

    public int? TroopId { get; set; }

    public int? WeaponId { get; set; }

    public virtual TroopProfile? Troop { get; set; }

    public virtual Weapon? Weapon { get; set; }
}
