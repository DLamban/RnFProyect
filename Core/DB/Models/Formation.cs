using System;
using System.Collections.Generic;

namespace Core.DB.Models;

public partial class Formation
{
    public int Id { get; set; }

    public string? Code { get; set; }

    public virtual ICollection<Unit> Units { get; set; } = new List<Unit>();
}
