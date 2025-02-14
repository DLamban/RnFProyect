using System.Linq;
using Core.DB.Data;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

[TestFixture]
public class CharacterDatabaseTests
{
    private GameDbContext _context;

    [SetUp]
    public void Setup()
    {
        // Usa directamente el contexto que ya tiene la conexión
        
        var options = new DbContextOptionsBuilder<GameDbContext>()
            .UseSqlite("Data Source=C:\\dev\\games\\RnFProyect\\Core\\DB\\RnFDBSqlite.db")
           .Options;
        _context = new GameDbContext(options);


    }

    [Test]
    public void Should_LoadCharacterById_FromDatabase()
    {
        int testCharacterId = 1;  // Un ID que ya exista en la BD

        Core.DB.Models.Character? loadedCharacter = _context.Characters
                                      .Include(c => c.Category)
                                      .FirstOrDefault(c => c.Id == testCharacterId);
        Console.WriteLine(loadedCharacter);
        // assert that we loaded a character
        Assert.That(loadedCharacter, Is.Not.Null);
    }

    [Test]
    public void loadCharFromView()
    {
        int testCharacterId = 1;  // Un ID que ya exista en la BD

        Core.DB.Models.CharacterDetail? loadedCharacter = _context.CharacterDetails                                      
                                      .FirstOrDefault(c => c.Id == testCharacterId);
        Console.WriteLine(loadedCharacter);
        // assert that we loaded a character
        Assert.That(loadedCharacter, Is.Not.Null);
    }
    [Test]
    public void loadUnit()
    {
        int testUnitId = 1;  // Un ID que ya exista en la BD
        Core.DB.Models.Unit? unit = _context.Units
                                      .FirstOrDefault(c => c.Id == testUnitId);
        Console.WriteLine(unit);
        // assert that we loaded a character
        Assert.That(unit, Is.Not.Null);
    }
    [Test]
    public void loadUnitFromDetail()
    {
        int testUnitId = 1;  // Un ID que ya exista en la BD
        Core.DB.Models.UnitDetail? unit = _context.UnitDetails
                                      .FirstOrDefault(c => c.Id == testUnitId);
        Console.WriteLine(unit);
        // assert that we loaded a character
        Assert.That(unit, Is.Not.Null);
    }
}
