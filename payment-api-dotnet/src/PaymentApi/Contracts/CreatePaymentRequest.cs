namespace PaymentApi.Contracts;

/// <summary>
/// Datos de entrada para registrar una solicitud de pago.
/// Las propiedades son anulables para que la validación pueda informar
/// claramente qué campo falta en lugar de fallar en la deserialización.
/// </summary>
public sealed class CreatePaymentRequest
{
    public string? ExternalReference { get; init; }

    public decimal? Amount { get; init; }

    public string? Channel { get; init; }
}