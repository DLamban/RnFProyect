using System;
using System.Collections.Generic;

namespace Core.DB.Models;

public partial class Character
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public int? RaceId { get; set; }

    public string? AssetFile { get; set; }

    public int? BaseSizeId { get; set; }

    public int? TroopTypeId { get; set; }

    public int? CategoryId { get; set; }

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

    public virtual BaseSize? BaseSize { get; set; }

    public virtual UnitCategory? Category { get; set; }

    public virtual Race? Race { get; set; }

    public virtual TroopType? TroopType { get; set; }

    public virtual ICollection<WeaponsCharacter> WeaponsCharacters { get; set; } = new List<WeaponsCharacter>();
}
