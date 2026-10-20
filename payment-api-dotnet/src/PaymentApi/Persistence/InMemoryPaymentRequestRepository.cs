using PaymentApi.Domain;

namespace PaymentApi.Persistence;

/// <summary>
/// Almacenamiento en memoria, seguro ante solicitudes concurrentes.
/// La comparación de referencias distingue mayúsculas y minúsculas (Ordinal).
/// </summary>
public sealed class InMemoryPaymentRequestRepository : IPaymentRequestRepository
{
    private readonly object _sync = new();
    private readonly Dictionary<string, PaymentRequest> _byReference = new(StringComparer.Ordinal);
    private readonly List<PaymentRequest> _inInsertionOrder = new();

    public bool ExistsByReference(string externalReference)
    {
        lock (_sync)
        {
            return _byReference.ContainsKey(externalReference);
        }
    }

    public bool TryAdd(PaymentRequest request)
    {
        lock (_sync)
        {
            if (!_byReference.TryAdd(request.ExternalReference, request))
            {
                return false;
            }

            _inInsertionOrder.Add(request);
            return true;
        }
    }

    public IReadOnlyList<PaymentRequest> GetAll(string? channel = null)
    {
        lock (_sync)
        {
            return channel is null
                ? _inInsertionOrder.ToList()
                : _inInsertionOrder.Where(r => r.Channel == channel).ToList();
        }
    }
}