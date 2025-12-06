using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.IO;
using FrasesRandomAPI;
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

    [Fact]
    public async Task GetQuoteById_WithValidId_ReturnsQuote()
    {
        // First get all quotes to find a valid ID
        var allResponse = await _client.GetAsync("/api/quotes");
        allResponse.EnsureSuccessStatusCode();
        var allQuotes = await allResponse.Content.ReadFromJsonAsync<IList<Quote>>();
        
        Assert.NotNull(allQuotes);
        Assert.NotEmpty(allQuotes);
        
        var firstQuote = allQuotes!.First();

        // Now get that specific quote
        var response = await _client.GetAsync($"/api/quotes/{firstQuote.Id}");

        response.EnsureSuccessStatusCode();
        var quote = await response.Content.ReadFromJsonAsync<Quote>();

        Assert.NotNull(quote);
        Assert.Equal(firstQuote.Id, quote!.Id);
        Assert.Equal(firstQuote.Autor, quote.Autor);
        Assert.Equal(firstQuote.Texto, quote.Texto);
    }

    [Fact]
    public async Task GetQuoteById_WithInvalidId_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/quotes/99999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PutQuote_WithValidId_ReturnsNoContent()
    {
        // Get a valid quote ID
        var allResponse = await _client.GetAsync("/api/quotes");
        var allQuotes = await allResponse.Content.ReadFromJsonAsync<IList<Quote>>();
        var quoteId = allQuotes!.First().Id;

        var payload = new
        {
            Autor = "Updated Author",
            Texto = "Updated quote text for testing",
            Fecha = DateTime.UtcNow
        };

        var response = await _client.PutAsJsonAsync($"/api/quotes/{quoteId}", payload);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify the update
        var getResponse = await _client.GetAsync($"/api/quotes/{quoteId}");
        getResponse.EnsureSuccessStatusCode();
        var updatedQuote = await getResponse.Content.ReadFromJsonAsync<Quote>();

        Assert.NotNull(updatedQuote);
        Assert.Equal("Updated Author", updatedQuote!.Autor);
        Assert.Equal("Updated quote text for testing", updatedQuote.Texto);
    }

    [Fact]
    public async Task PutQuote_WithInvalidId_ReturnsNotFound()
    {
        var payload = new
        {
            Autor = "Updated Author",
            Texto = "Updated text",
            Fecha = DateTime.UtcNow
        };

        var response = await _client.PutAsJsonAsync("/api/quotes/99999", payload);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PutQuote_WithInvalidPayload_ReturnsBadRequest()
    {
        // Get a valid quote ID
        var allResponse = await _client.GetAsync("/api/quotes");
        var allQuotes = await allResponse.Content.ReadFromJsonAsync<IList<Quote>>();
        var quoteId = allQuotes!.First().Id;

        var payload = new
        {
            Autor = "",
            Texto = "",
            Fecha = DateTime.MinValue
        };

        var response = await _client.PutAsJsonAsync($"/api/quotes/{quoteId}", payload);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DeleteQuote_WithValidId_ReturnsNoContent()
    {
        // First create a quote to delete
        var createPayload = new
        {
            Autor = "To Delete",
            Texto = "This quote will be deleted",
            Fecha = DateTime.UtcNow
        };

        var createResponse = await _client.PostAsJsonAsync("/api/quotes", createPayload);
        createResponse.EnsureSuccessStatusCode();
        var createdQuote = await createResponse.Content.ReadFromJsonAsync<Quote>();

        // Delete the quote
        var deleteResponse = await _client.DeleteAsync($"/api/quotes/{createdQuote!.Id}");

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        // Verify the deletion
        var getResponse = await _client.GetAsync($"/api/quotes/{createdQuote.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteQuote_WithInvalidId_ReturnsNotFound()
    {
        var response = await _client.DeleteAsync("/api/quotes/99999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}

public sealed class CustomWebApplicationFactory : WebApplicationFactory<AssemblyMarker>
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

            QuotesDbContext.Initialize(context);
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
