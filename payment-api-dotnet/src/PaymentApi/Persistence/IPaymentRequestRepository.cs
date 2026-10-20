using PaymentApi.Domain;

namespace PaymentApi.Persistence;

public interface IPaymentRequestRepository
{
    bool ExistsByReference(string externalReference);

    /// <summary>
    /// Agrega la solicitud solo si la referencia no existe (operación atómica).
    /// Devuelve false si ya existía.
    /// </summary>
    bool TryAdd(PaymentRequest request);

    IReadOnlyList<PaymentRequest> GetAll(string? channel = null);
}