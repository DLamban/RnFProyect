using System;
using System.Collections.Generic;

namespace Core.DB.Models;

public partial class RaceTranslation
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public int? RaceId { get; set; }

    public string? Description { get; set; }

    public string? LanguageCode { get; set; }

    public virtual Race? Race { get; set; }
}
