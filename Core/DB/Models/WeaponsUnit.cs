using System;
using System.Collections.Generic;

namespace Core.DB.Models;

public partial class WeaponsUnit
{
    public int Id { get; set; }

    public int? CharacterId { get; set; }

    public int? WeaponId { get; set; }

    public virtual Character? Character { get; set; }

    public virtual Weapon? Weapon { get; set; }
}
