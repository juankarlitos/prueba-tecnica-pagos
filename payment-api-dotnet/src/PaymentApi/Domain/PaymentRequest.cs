namespace PaymentApi.Domain;

public sealed class PaymentRequest
{
    public required Guid Id { get; init; }
    public required string ExternalReference { get; init; }
    public required decimal Amount { get; init; }
    public required string Channel { get; init; }
    public required decimal Fee { get; init; }
    public required decimal Total { get; init; }
    public required string Status { get; init; }

    /// <summary>
    /// Crea una solicitud nueva. El identificador y el estado los asigna el backend.
    /// </summary>
    public static PaymentRequest CreatePending(
        string externalReference, decimal amount, string channel, decimal fee, decimal total) => new()
    {
        Id = Guid.NewGuid(),
        ExternalReference = externalReference,
        Amount = amount,
        Channel = channel,
        Fee = fee,
        Total = total,
        Status = PaymentStatus.Pending
    };
}