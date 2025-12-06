using System.IO;
using System.Net.Http.Json;
using BenchmarkDotNet.Attributes;
using FrasesRandomAPI;
using FrasesRandomAPI.Data;
using FrasesRandomAPI.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FrasesRandomAPI.Benchmarks;
[MemoryDiagnoser]
[MinColumn, MaxColumn, MeanColumn]
public class QuoteEndpointsBenchmark : IDisposable
{
    private CustomWebApplicationFactory? _factory;
    private HttpClient? _client;

    [GlobalSetup]
    public void Setup()
    {
        _factory = new CustomWebApplicationFactory();
        _client = _factory.CreateClient();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _client?.Dispose();
        _factory?.Dispose();
    }

    [Benchmark(Description = "GET /api/quotes")]
    public async Task GetQuotesAsync()
    {
        var response = await _client!.GetAsync("/api/quotes");
        response.EnsureSuccessStatusCode();
    }

    [Benchmark(Description = "POST /api/quotes")]
    public async Task PostQuoteAsync()
    {
        var payload = new
        {
            Autor = $"Benchmark Author {Guid.NewGuid():N}",
            Texto = "Benchmark quote payload para medir rendimiento",
            Fecha = DateTime.UtcNow
        };

        var response = await _client!.PostAsJsonAsync("/api/quotes", payload);
        response.EnsureSuccessStatusCode();
    }

    public void Dispose()
    {
        Cleanup();
    }
}

public sealed class CustomWebApplicationFactory : WebApplicationFactory<AssemblyMarker>
{
    private readonly string _databasePath = Path.Combine(
        Path.GetTempPath(), $"quotes-bench-{Guid.NewGuid():N}.db");

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
