using System.Collections.Generic;
using FrasesRandomAPI.Models;

namespace FrasesRandomAPI.Services;

public static class QuoteRequestValidator
{
    public static bool TryValidate(QuoteRequest request, out Dictionary<string, string[]> errors)
    {
        errors = [];

        if (string.IsNullOrWhiteSpace(request.Autor))
        {
            errors["Autor"] = ["El autor es obligatorio."];
        }

        if (string.IsNullOrWhiteSpace(request.Texto))
        {
            errors["Texto"] = ["El texto es obligatorio."];
        }

        if (request.Texto?.Length > 1000)
        {
            errors["Texto"] = ["El texto no puede superar los 1000 caracteres."];
        }

        if (request.Autor?.Length > 120)
        {
            errors["Autor"] = ["El autor no puede superar los 120 caracteres."];
        }

        if (request.Fecha == default)
        {
            errors["Fecha"] = ["La fecha debe ser válida."];
        }

        return errors.Count == 0;
    }
}
