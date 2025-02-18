using System;
using System.Collections.Generic;

namespace Core.DB.Models;

public partial class UnitCategory
{
    public int Id { get; set; }

    public string? Code { get; set; }

    public virtual ICollection<Character> Characters { get; set; } = new List<Character>();

    public virtual ICollection<TroopProfile> TroopProfiles { get; set; } = new List<TroopProfile>();
}
