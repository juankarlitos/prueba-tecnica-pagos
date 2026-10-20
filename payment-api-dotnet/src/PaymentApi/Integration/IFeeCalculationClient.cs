namespace PaymentApi.Integration;

public interface IFeeCalculationClient
{
    /// <summary>
    /// Solicita al servicio Java la comisión y el total.
    /// Lanza FeeServiceUnavailableException si el servicio no responde,
    /// vence el timeout o el resultado no es válido.
    /// </summary>
    Task<FeeCalculationResult> CalculateAsync(
        decimal amount, string channel, CancellationToken cancellationToken = default);
}