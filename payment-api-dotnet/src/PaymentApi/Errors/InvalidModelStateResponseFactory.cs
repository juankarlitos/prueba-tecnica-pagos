using Microsoft.AspNetCore.Mvc;
using PaymentApi.Contracts;

namespace PaymentApi.Errors;

/// <summary>
/// Reemplaza la respuesta 400 automática de ASP.NET (ProblemDetails) por nuestra
/// estructura de error, sin exponer mensajes internos del deserializador.
/// </summary>
public static class InvalidModelStateResponseFactory
{
    public static IActionResult Create(ActionContext context)
    {
        var invalidFields = context.ModelState
            .Where(entry => entry.Value is { Errors.Count: > 0 } && entry.Key.StartsWith('$'))
            .Select(entry => entry.Key.TrimStart('$', '.'))
            .Where(field => field.Length > 0)
            .Distinct()
            .Select(field => $"{field}: valor con formato o tipo inválido")
            .ToList();

        var error = new ErrorResponse(
            ErrorCodes.MalformedRequest,
            "El cuerpo de la solicitud no es un JSON válido o contiene tipos incorrectos.",
            invalidFields);

        return new BadRequestObjectResult(error);
    }
}