using FrasesRandomAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace FrasesRandomAPI.Data;

public class QuotesDbContext(DbContextOptions<QuotesDbContext> options) : DbContext(options)
{
    public DbSet<Quote> Quotes => Set<Quote>();

    public static void Initialize(QuotesDbContext context)
    {
        context.Database.EnsureCreated();

        if (context.Quotes.Any())
            return;

        var quotes = new[]
        {
            new Quote { Autor = "Donald Knuth", Texto = "La optimización prematura es la raíz de todo mal", Fecha = DateTime.Now },
            new Quote { Autor = "Steve Jobs", Texto = "El código es poesía escrita para máquinas", Fecha = DateTime.Now },
            new Quote { Autor = "Linus Torvalds", Texto = "La mayoría de los bugs buenos tienen historias largas", Fecha = DateTime.Now },
            new Quote { Autor = "Bill Gates", Texto = "La codificación es hoy más importante que nunca", Fecha = DateTime.Now },
            new Quote { Autor = "Ada Lovelace", Texto = "La imaginación es el primer paso en la creación de algo", Fecha = DateTime.Now },
            new Quote { Autor = "Alan Turing", Texto = "La máquina puede hacer todo lo que un humano pueda describir", Fecha = DateTime.Now },
            new Quote { Autor = "Grace Hopper", Texto = "El futuro pertenece a los que pueden imaginar más allá de lo visible", Fecha = DateTime.Now },
            new Quote { Autor = "Richard Stallman", Texto = "La libertad del software es fundamental", Fecha = DateTime.Now },
            new Quote { Autor = "Ken Thompson", Texto = "Cuando empieces a leer el código, abandona toda esperanza", Fecha = DateTime.Now },
            new Quote { Autor = "Margaret Hamilton", Texto = "El software debe estar hecho para durar", Fecha = DateTime.Now }
        };
        
        context.Quotes.AddRange(quotes);
        context.SaveChanges();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Quote>(entity =>
        {
            entity.ToTable("Quotes");
            entity.HasKey(q => q.Id);
            entity.Property(q => q.Autor)
                .IsRequired()
                .HasColumnType("varchar")
                .HasMaxLength(120);
            entity.Property(q => q.Texto)
                .IsRequired()
                .HasMaxLength(1000);
            entity.Property(q => q.Fecha)
                .IsRequired();
        });
    }
}
