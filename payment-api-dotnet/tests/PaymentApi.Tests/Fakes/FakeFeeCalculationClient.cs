using PaymentApi.Errors;
using PaymentApi.Integration;

namespace PaymentApi.Tests.Fakes;

/// <summary>
/// Simula el servicio Java para no necesitar levantarlo en las pruebas.
/// </summary>
public sealed class FakeFeeCalculationClient : IFeeCalculationClient
{
    private readonly Func<decimal, string, FeeCalculationResult> _behavior;

    private FakeFeeCalculationClient(Func<decimal, string, FeeCalculationResult> behavior)
    {
        _behavior = behavior;
    }

    public int CallCount { get; private set; }

    public static FakeFeeCalculationClient Returning(decimal fee) =>
        new((amount, _) => new FeeCalculationResult(fee, amount + fee));

    public static FakeFeeCalculationClient Failing() =>
        new((_, _) => throw new FeeServiceUnavailableException("Servicio de cálculo no disponible (simulado)."));

    public Task<FeeCalculationResult> CalculateAsync(
        decimal amount, string channel, CancellationToken cancellationToken = default)
    {
        CallCount++;
        return Task.FromResult(_behavior(amount, channel));
    }
}