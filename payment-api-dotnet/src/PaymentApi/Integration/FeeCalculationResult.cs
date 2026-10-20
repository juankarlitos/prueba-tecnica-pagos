namespace PaymentApi.Integration;

/// <summary>
/// Resultado validado del cálculo realizado por el servicio Java.
/// </summary>
public sealed record FeeCalculationResult(decimal Fee, decimal Total);