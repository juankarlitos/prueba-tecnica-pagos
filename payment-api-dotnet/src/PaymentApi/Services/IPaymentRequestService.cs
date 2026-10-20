using PaymentApi.Contracts;

namespace PaymentApi.Services;

public interface IPaymentRequestService
{
    Task<PaymentRequestResponse> CreateAsync(CreatePaymentRequest? request, CancellationToken cancellationToken = default);

    IReadOnlyList<PaymentRequestResponse> GetAll(string? channel = null);
}