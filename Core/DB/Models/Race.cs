using System;
using System.Collections.Generic;

namespace Core.DB.Models;

public partial class Race
{
    public int Id { get; set; }

    public string? Code { get; set; }

    public virtual ICollection<Character> Characters { get; set; } = new List<Character>();

    public virtual ICollection<RaceTranslation> RaceTranslations { get; set; } = new List<RaceTranslation>();

    public virtual ICollection<Unit> Units { get; set; } = new List<Unit>();
}
