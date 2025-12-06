using FrasesRandomAPI.Models;
using FrasesRandomAPI.Services;

namespace FrasesRandomAPI.UnitTests;

public class QuoteRequestValidatorTests
{
    [Fact]
    public void ValidRequest_DoesNotProduceErrors()
    {
        var request = new QuoteRequest("Autor", "Texto válido", DateTime.UtcNow);

        var result = QuoteRequestValidator.TryValidate(request, out var errors);

        Assert.True(result);
        Assert.Empty(errors);
    }

    [Fact]
    public void MissingAutor_GeneratesAutorError()
    {
        var request = new QuoteRequest("   ", "Texto válido", DateTime.UtcNow);

        var result = QuoteRequestValidator.TryValidate(request, out var errors);

        Assert.False(result);
        Assert.Contains("Autor", errors);
        Assert.Contains("El autor es obligatorio.", errors["Autor"]);
    }

    [Fact]
    public void TextoTooLong_GeneratesTextoError()
    {
        var longText = new string('x', 1001);
        var request = new QuoteRequest("Autor", longText, DateTime.UtcNow);

        var result = QuoteRequestValidator.TryValidate(request, out var errors);

        Assert.False(result);
        Assert.Contains("Texto", errors);
        Assert.Contains("El texto no puede superar los 1000 caracteres.", errors["Texto"]);
    }

    [Fact]
    public void InvalidFecha_GeneratesFechaError()
    {
        var request = new QuoteRequest("Autor", "Texto válido", default);

        var result = QuoteRequestValidator.TryValidate(request, out var errors);

        Assert.False(result);
        Assert.Contains("Fecha", errors);
        Assert.Contains("La fecha debe ser válida.", errors["Fecha"]);
    }
}
