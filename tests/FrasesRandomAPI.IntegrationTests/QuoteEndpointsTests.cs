using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.IO;
using FrasesRandomAPI.Data;
using FrasesRandomAPI.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FrasesRandomAPI.IntegrationTests;

public class QuoteEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public QuoteEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetQuotes_ReturnsSeededQuotes()
    {
        var response = await _client.GetAsync("/api/quotes");

        response.EnsureSuccessStatusCode();
        var quotes = await response.Content.ReadFromJsonAsync<IList<Quote>>();

        Assert.NotNull(quotes);
        Assert.NotEmpty(quotes);
    }

    [Fact]
    public async Task PostQuote_WithValidPayload_ReturnsCreated()
    {
        var payload = new
        {
            Autor = "Integration Tester",
            Texto = "Esta es una frase de prueba",
            Fecha = DateTime.UtcNow
        };

        var response = await _client.PostAsJsonAsync("/api/quotes", payload);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var createdQuote = await response.Content.ReadFromJsonAsync<Quote>();

        Assert.NotNull(createdQuote);
        Assert.Equal(payload.Autor, createdQuote!.Autor);
        Assert.Equal(payload.Texto, createdQuote.Texto);
    }

    [Fact]
    public async Task PostQuote_WithInvalidPayload_ReturnsBadRequest()
    {
        var payload = new
        {
            Autor = "",
            Texto = "",
            Fecha = DateTime.MinValue
        };

        var response = await _client.PostAsJsonAsync("/api/quotes", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databasePath = Path.Combine(
        Path.GetTempPath(), $"quotes-tests-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            var overrides = new Dictionary<string, string?>
            {
                ["ConnectionStrings:QuotesDatabase"] = $"Data Source={_databasePath}"
            };

            config.AddInMemoryCollection(overrides!);
        });

        builder.ConfigureServices(services =>
        {
            var serviceProvider = services.BuildServiceProvider();

            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<QuotesDbContext>();
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            if (!context.Quotes.Any())
            {
                context.Quotes.Add(new Quote
                {
                    Autor = "Seed Author",
                    Texto = "Seed quote",
                    Fecha = DateTime.UtcNow
                });
                context.SaveChanges();
            }
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing && File.Exists(_databasePath))
        {
            File.Delete(_databasePath);
        }
    }
}
