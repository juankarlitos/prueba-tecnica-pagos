using PaymentApi.Contracts;
using PaymentApi.Domain;
using PaymentApi.Errors;
using PaymentApi.Integration;
using PaymentApi.Persistence;

namespace PaymentApi.Services;

public sealed class PaymentRequestService : IPaymentRequestService
{
    private readonly IPaymentRequestRepository _repository;
    private readonly IFeeCalculationClient _feeCalculationClient;
    private readonly ILogger<PaymentRequestService> _logger;

    public PaymentRequestService(
        IPaymentRequestRepository repository,
        IFeeCalculationClient feeCalculationClient,
        ILogger<PaymentRequestService> logger)
    {
        _repository = repository;
        _feeCalculationClient = feeCalculationClient;
        _logger = logger;
    }

    public async Task<PaymentRequestResponse> CreateAsync(
        CreatePaymentRequest? request, CancellationToken cancellationToken = default)
    {
        // 1. Validar antes de llamar a Java.
        var valid = PaymentRequestValidator.Validate(request);

        // 2. Evitar una llamada innecesaria a Java si la referencia ya existe.
        if (_repository.ExistsByReference(valid.ExternalReference))
        {
            throw new DuplicateReferenceException(valid.ExternalReference);
        }

        // 3. Calcular en Java. Si falla, se lanza FeeServiceUnavailableException y no se guarda nada.
        var calculation = await _feeCalculationClient.CalculateAsync(valid.Amount, valid.Channel, cancellationToken);

        // 4. Guardar solo si el cálculo terminó bien. TryAdd es atómico y cubre
        //    el caso de dos solicitudes concurrentes con la misma referencia.
        var paymentRequest = PaymentRequest.CreatePending(
            valid.ExternalReference, valid.Amount, valid.Channel, calculation.Fee, calculation.Total);

        if (!_repository.TryAdd(paymentRequest))
        {
            throw new DuplicateReferenceException(valid.ExternalReference);
        }

        _logger.LogInformation(
            "Solicitud de pago registrada. Id: {Id}, Referencia: {Reference}",
            paymentRequest.Id, paymentRequest.ExternalReference);

        return PaymentRequestResponse.From(paymentRequest);
    }

    public IReadOnlyList<PaymentRequestResponse> GetAll(string? channel = null)
    {
        var validChannel = PaymentRequestValidator.ValidateChannelFilter(channel);

        return _repository.GetAll(validChannel)
            .Select(PaymentRequestResponse.From)
            .ToList();
    }
}