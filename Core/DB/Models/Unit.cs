using System;
using System.Collections.Generic;

namespace Core.DB.Models;

public partial class Unit
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public int? MinimumSize { get; set; }

    public int? RaceId { get; set; }

    public int? FormationId { get; set; }

    public virtual Formation? Formation { get; set; }

    public virtual Race? Race { get; set; }

    public virtual ICollection<TroopProfile> TroopProfiles { get; set; } = new List<TroopProfile>();
}
