using System;
using System.Collections.Generic;

namespace Core.DB.Models;

public partial class TroopProfileDetail
{
    public int? Id { get; set; }

    public string? Name { get; set; }

    public string? TroopType { get; set; }

    public string? Category { get; set; }

    public string? AssetFile { get; set; }

    public int? Width { get; set; }

    public int? Height { get; set; }

    public int? Movement { get; set; }

    public int? Dexterity { get; set; }

    public int? Shooting { get; set; }

    public int? Strength { get; set; }

    public int? Resistance { get; set; }

    public int? Wounds { get; set; }

    public int? Initiative { get; set; }

    public int? Attacks { get; set; }

    public int? Leadership { get; set; }

    public int? Armour { get; set; }

    public int? Cost { get; set; }
}
