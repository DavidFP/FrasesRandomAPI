using System.Diagnostics.CodeAnalysis;
using FrasesRandomAPI.Data;
using FrasesRandomAPI.Models;
using FrasesRandomAPI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<QuotesDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("QuotesDatabase")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<QuotesDbContext>();
    if (db.Database.IsRelational())
    {
        db.Database.Migrate();
    }
    else
    {
        db.Database.EnsureCreated();
    }
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.MapGet("/api/quotes", async (QuotesDbContext db) =>
        await db.Quotes.AsNoTracking().ToListAsync())
    .WithName("GetQuotes")
    .WithSummary("Obtiene todas las frases registradas.")
    .WithDescription("Devuelve la colección completa de frases almacenadas en la base de datos.")
    .WithTags("Quotes");

app.MapGet("/api/quotes/{id:int}", async (int id, QuotesDbContext db) =>
    await db.Quotes.AsNoTracking().FirstOrDefaultAsync(q => q.Id == id)
        is Quote quote
            ? Results.Ok(quote)
            : Results.NotFound())
    .WithName("GetQuoteById")
    .WithSummary("Obtiene una frase específica por su identificador.")
    .WithDescription("Cuando la frase no existe devuelve un estado 404.")
    .WithTags("Quotes");

app.MapPost("/api/quotes", async (QuoteRequest request, QuotesDbContext db) =>
    {
        if (!QuoteRequestValidator.TryValidate(request, out var errors))
        {
            return Results.ValidationProblem(errors);
        }

        var quote = new Quote
        {
            Autor = request.Autor.Trim(),
            Texto = request.Texto.Trim(),
            Fecha = request.Fecha.Date
        };

        db.Quotes.Add(quote);
        await db.SaveChangesAsync();

        return Results.Created($"/api/quotes/{quote.Id}", quote);
    })
    .WithName("CreateQuote")
    .WithSummary("Crea una nueva frase.")
    .WithDescription("Recibe autor, texto y fecha en el cuerpo de la petición.")
    .WithTags("Quotes");

app.MapPut("/api/quotes/{id:int}", async (int id, QuoteRequest request, QuotesDbContext db) =>
    {
        if (!QuoteRequestValidator.TryValidate(request, out var errors))
        {
            return Results.ValidationProblem(errors);
        }

        var quote = await db.Quotes.FindAsync(id);
        if (quote is null)
        {
            return Results.NotFound();
        }

        quote.Autor = request.Autor.Trim();
        quote.Texto = request.Texto.Trim();
        quote.Fecha = request.Fecha.Date;

        await db.SaveChangesAsync();

        return Results.NoContent();
    })
    .WithName("UpdateQuote")
    .WithSummary("Actualiza una frase existente.")
    .WithDescription("Si la frase no existe devuelve un estado 404.")
    .WithTags("Quotes");

app.MapDelete("/api/quotes/{id:int}", async (int id, QuotesDbContext db) =>
    {
        var quote = await db.Quotes.FindAsync(id);
        if (quote is null)
        {
            return Results.NotFound();
        }

        db.Quotes.Remove(quote);
        await db.SaveChangesAsync();

        return Results.NoContent();
    })
    .WithName("DeleteQuote")
    .WithSummary("Elimina una frase.")
    .WithDescription("Elimina de forma permanente la frase indicada por el identificador.")
    .WithTags("Quotes");

app.Run();

[ExcludeFromCodeCoverage]
public partial class Program { }
