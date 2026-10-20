using PaymentApi.Domain;

namespace PaymentApi.Contracts;

public sealed record PaymentRequestResponse(
    Guid Id,
    string ExternalReference,
    decimal Amount,
    string Channel,
    decimal Fee,
    decimal Total,
    string Status)
{
    public static PaymentRequestResponse From(PaymentRequest request) => new(
        request.Id,
        request.ExternalReference,
        request.Amount,
        request.Channel,
        request.Fee,
        request.Total,
        request.Status);
}