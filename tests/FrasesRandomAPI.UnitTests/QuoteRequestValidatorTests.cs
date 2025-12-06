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

    [Fact]
    public void MissingTexto_GeneratesTextoError()
    {
        var request = new QuoteRequest("Autor", "   ", DateTime.UtcNow);

        var result = QuoteRequestValidator.TryValidate(request, out var errors);

        Assert.False(result);
        Assert.Contains("Texto", errors);
        Assert.Contains("El texto es obligatorio.", errors["Texto"]);
    }

    [Fact]
    public void AutorTooLong_GeneratesAutorError()
    {
        var longAutor = new string('x', 121);
        var request = new QuoteRequest(longAutor, "Texto válido", DateTime.UtcNow);

        var result = QuoteRequestValidator.TryValidate(request, out var errors);

        Assert.False(result);
        Assert.Contains("Autor", errors);
        Assert.Contains("El autor no puede superar los 120 caracteres.", errors["Autor"]);
    }

    [Fact]
    public void MultipleErrors_AllReported()
    {
        var longText = new string('x', 1001);
        var longAutor = new string('y', 121);
        var request = new QuoteRequest(longAutor, longText, default);

        var result = QuoteRequestValidator.TryValidate(request, out var errors);

        Assert.False(result);
        Assert.True(errors.Count >= 2);
        Assert.Contains("Texto", errors);
        Assert.Contains("Fecha", errors);
    }

    [Fact]
    public void ValidRequest_MaxLengths_DoesNotProduceErrors()
    {
        var maxAutor = new string('a', 120);
        var maxTexto = new string('b', 1000);
        var request = new QuoteRequest(maxAutor, maxTexto, DateTime.UtcNow);

        var result = QuoteRequestValidator.TryValidate(request, out var errors);

        Assert.True(result);
        Assert.Empty(errors);
    }

    [Fact]
    public void EmptyAutorAndTexto_GeneratesBothErrors()
    {
        var request = new QuoteRequest("", "", DateTime.UtcNow);

        var result = QuoteRequestValidator.TryValidate(request, out var errors);

        Assert.False(result);
        Assert.Contains("Autor", errors);
        Assert.Contains("Texto", errors);
    }

    [Fact]
    public void ValidRequest_WithWhitespaceAroundValues_IsValid()
    {
        var request = new QuoteRequest("  Autor  ", "  Texto válido  ", DateTime.UtcNow);

        var result = QuoteRequestValidator.TryValidate(request, out var errors);

        Assert.True(result);
        Assert.Empty(errors);
    }
}
