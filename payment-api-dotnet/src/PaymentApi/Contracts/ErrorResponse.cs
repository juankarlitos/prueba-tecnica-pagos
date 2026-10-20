namespace PaymentApi.Contracts;

/// <summary>
/// Estructura única de error, igual a la del servicio Java: { code, message, details }.
/// </summary>
public sealed record ErrorResponse(string Code, string Message, IReadOnlyList<string> Details)
{
    public static ErrorResponse Of(string code, string message) =>
        new(code, message, Array.Empty<string>());
}