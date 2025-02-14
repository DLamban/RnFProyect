using System;
using System.Collections.Generic;
using Core.DB.Models;
using Microsoft.EntityFrameworkCore;

namespace Core.DB.Data;

public partial class GameDbContext : DbContext
{
    public GameDbContext()
    {
    }

    public GameDbContext(DbContextOptions<GameDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BaseSize> BaseSizes { get; set; }

    public virtual DbSet<Character> Characters { get; set; }

    public virtual DbSet<CharacterDetail> CharacterDetails { get; set; }

    public virtual DbSet<Formation> Formations { get; set; }

    public virtual DbSet<Race> Races { get; set; }

    public virtual DbSet<RaceTranslation> RaceTranslations { get; set; }

    public virtual DbSet<SpecialRule> SpecialRules { get; set; }

    public virtual DbSet<TroopProfile> TroopProfiles { get; set; }

    public virtual DbSet<TroopProfileDetail> TroopProfileDetails { get; set; }

    public virtual DbSet<TroopType> TroopTypes { get; set; }

    public virtual DbSet<Unit> Units { get; set; }

    public virtual DbSet<UnitCategory> UnitCategories { get; set; }

    public virtual DbSet<Weapon> Weapons { get; set; }

    public virtual DbSet<WeaponsCharacter> WeaponsCharacters { get; set; }

    public virtual DbSet<WeaponsTroop> WeaponsTroops { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlite("Data Source=C:\\\\\\\\dev\\\\\\\\games\\\\\\\\RnFProyect\\\\\\\\Core\\\\\\\\DB\\\\\\\\RnFDBSqlite.db");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Character>(entity =>
        {
            entity.HasOne(d => d.BaseSize).WithMany(p => p.Characters).HasForeignKey(d => d.BaseSizeId);

            entity.HasOne(d => d.Category).WithMany(p => p.Characters).HasForeignKey(d => d.CategoryId);

            entity.HasOne(d => d.Race).WithMany(p => p.Characters).HasForeignKey(d => d.RaceId);

            entity.HasOne(d => d.TroopType).WithMany(p => p.Characters).HasForeignKey(d => d.TroopTypeId);
        });

        modelBuilder.Entity<CharacterDetail>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("CharacterDetails");

            entity.Property(e => e.Race).HasColumnName("race");
        });

        modelBuilder.Entity<RaceTranslation>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.LanguageCode).HasColumnType("TEXT (2, 2)");

            entity.HasOne(d => d.Race).WithMany(p => p.RaceTranslations).HasForeignKey(d => d.RaceId);
        });

        modelBuilder.Entity<TroopProfile>(entity =>
        {
            entity.Property(e => e.IsMainProfile).HasColumnName("isMainProfile");

            entity.HasOne(d => d.BaseSize).WithMany(p => p.TroopProfiles)
                .HasForeignKey(d => d.BaseSizeId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Category).WithMany(p => p.TroopProfiles)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.TroopType).WithMany(p => p.TroopProfiles)
                .HasForeignKey(d => d.TroopTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Unit).WithMany(p => p.TroopProfiles)
                .HasForeignKey(d => d.UnitId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<TroopProfileDetail>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("TroopProfileDetails");
        });

        modelBuilder.Entity<Unit>(entity =>
        {
            entity.HasOne(d => d.Formation).WithMany(p => p.Units).HasForeignKey(d => d.FormationId);

            entity.HasOne(d => d.Race).WithMany(p => p.Units).HasForeignKey(d => d.RaceId);
        });

        modelBuilder.Entity<Weapon>(entity =>
        {
            entity.Property(e => e.Ap).HasColumnName("AP");
            entity.Property(e => e.IsStrengthFlat).HasColumnName("isStrengthFlat");
        });

        modelBuilder.Entity<WeaponsCharacter>(entity =>
        {
            entity.HasOne(d => d.Character).WithMany(p => p.WeaponsCharacters).HasForeignKey(d => d.CharacterId);

            entity.HasOne(d => d.Weapon).WithMany(p => p.WeaponsCharacters).HasForeignKey(d => d.WeaponId);
        });

        modelBuilder.Entity<WeaponsTroop>(entity =>
        {
            entity.HasOne(d => d.Troop).WithMany(p => p.WeaponsTroops).HasForeignKey(d => d.TroopId);

            entity.HasOne(d => d.Weapon).WithMany(p => p.WeaponsTroops).HasForeignKey(d => d.WeaponId);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
