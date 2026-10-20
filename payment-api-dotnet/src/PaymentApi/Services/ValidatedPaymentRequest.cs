namespace PaymentApi.Services;

/// <summary>
/// Datos ya validados y normalizados (referencia sin espacios al inicio ni al final).
/// </summary>
public sealed record ValidatedPaymentRequest(string ExternalReference, decimal Amount, string Channel);