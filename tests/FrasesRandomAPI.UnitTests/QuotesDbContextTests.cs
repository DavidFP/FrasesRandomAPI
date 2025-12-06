using System.IO;
using FrasesRandomAPI.Data;
using FrasesRandomAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace FrasesRandomAPI.UnitTests;

public class QuotesDbContextTests : IDisposable
{
    private readonly string _databasePath;
    private readonly DbContextOptions<QuotesDbContext> _options;

    public QuotesDbContextTests()
    {
        _databasePath = Path.Combine(Path.GetTempPath(), $"test-quotes-{Guid.NewGuid():N}.db");
        _options = new DbContextOptionsBuilder<QuotesDbContext>()
            .UseSqlite($"Data Source={_databasePath}")
            .Options;
    }

    [Fact]
    public void Initialize_WithEmptyDatabase_SeedsQuotes()
    {
        using var context = new QuotesDbContext(_options);
        context.Database.EnsureCreated();

        QuotesDbContext.Initialize(context);

        Assert.NotEmpty(context.Quotes);
        Assert.Equal(10, context.Quotes.Count());
        
        var donald = context.Quotes.FirstOrDefault(q => q.Autor == "Donald Knuth");
        Assert.NotNull(donald);
        Assert.Contains("optimización prematura", donald!.Texto);
    }

    [Fact]
    public void Initialize_WithExistingData_DoesNotDuplicate()
    {
        using var context = new QuotesDbContext(_options);
        context.Database.EnsureCreated();
        
        // First initialization
        QuotesDbContext.Initialize(context);
        var firstCount = context.Quotes.Count();

        // Second initialization
        QuotesDbContext.Initialize(context);
        var secondCount = context.Quotes.Count();

        Assert.Equal(firstCount, secondCount);
    }

    [Fact]
    public void DbContext_CanInsertAndRetrieveQuotes()
    {
        using var context = new QuotesDbContext(_options);
        context.Database.EnsureCreated();

        var quote = new Quote
        {
            Autor = "Test Author",
            Texto = "Test quote text",
            Fecha = DateTime.UtcNow.Date
        };

        context.Quotes.Add(quote);
        context.SaveChanges();

        using var contextRead = new QuotesDbContext(_options);
        var retrievedQuote = contextRead.Quotes.FirstOrDefault(q => q.Autor == "Test Author");

        Assert.NotNull(retrievedQuote);
        Assert.Equal("Test quote text", retrievedQuote!.Texto);
    }

    [Fact]
    public void Initialize_WithExistingData_SkipsAddition()
    {
        using var context = new QuotesDbContext(_options);
        context.Database.EnsureCreated();
        
        // Add a quote manually
        context.Quotes.Add(new Quote { Autor = "Manual", Texto = "Manual add", Fecha = DateTime.UtcNow.Date });
        context.SaveChanges();

        // Initialize should not add more quotes
        var countBefore = context.Quotes.Count();
        QuotesDbContext.Initialize(context);
        var countAfter = context.Quotes.Count();

        // Should not have added the standard quotes
        Assert.Equal(countBefore, countAfter);
        Assert.Single(context.Quotes);
    }

    public void Dispose()
    {
        if (File.Exists(_databasePath))
        {
            File.Delete(_databasePath);
        }
    }
}
