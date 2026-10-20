using Microsoft.Extensions.Logging.Abstractions;
using PaymentApi.Contracts;
using PaymentApi.Domain;
using PaymentApi.Errors;
using PaymentApi.Persistence;
using PaymentApi.Services;
using PaymentApi.Tests.Fakes;

namespace PaymentApi.Tests.Services;

public class PaymentRequestServiceTests
{
    private readonly InMemoryPaymentRequestRepository _repository = new();

    private PaymentRequestService CreateService(FakeFeeCalculationClient feeClient) =>
        new(_repository, feeClient, NullLogger<PaymentRequestService>.Instance);

    private static CreatePaymentRequest Request(string? reference, decimal? amount, string? channel) =>
        new() { ExternalReference = reference, Amount = amount, Channel = channel };

    // Prueba obligatoria: si Java falla, no se guarda ninguna solicitud.
    [Fact]
    public async Task CreateAsync_WhenFeeServiceIsUnavailable_DoesNotStoreAnyRequest()
    {
        var feeClient = FakeFeeCalculationClient.Failing();
        var service = CreateService(feeClient);

        await Assert.ThrowsAsync<FeeServiceUnavailableException>(
            () => service.CreateAsync(Request("PAY-2026-0001", 1000.00m, "WEB")));

        Assert.Equal(1, feeClient.CallCount);
        Assert.Empty(_repository.GetAll());
    }

    [Fact]
    public async Task CreateAsync_WithValidData_StoresPendingRequestWithTrimmedReference()
    {
        var service = CreateService(FakeFeeCalculationClient.Returning(10.00m));

        var created = await service.CreateAsync(Request("  PAY-2026-0001  ", 1000.00m, "WEB"));

        Assert.NotEqual(Guid.Empty, created.Id);
        Assert.Equal("PAY-2026-0001", created.ExternalReference);
        Assert.Equal(1000.00m, created.Amount);
        Assert.Equal("WEB", created.Channel);
        Assert.Equal(10.00m, created.Fee);
        Assert.Equal(1010.00m, created.Total);
        Assert.Equal(PaymentStatus.Pending, created.Status);
        Assert.Single(_repository.GetAll());
    }

    // Prueba extra: una referencia duplicada produce conflicto y no agrega otro registro.
    [Fact]
    public async Task CreateAsync_WithDuplicatedReference_ThrowsConflictAndKeepsSingleRecord()
    {
        var feeClient = FakeFeeCalculationClient.Returning(10.00m);
        var service = CreateService(feeClient);
        await service.CreateAsync(Request("PAY-2026-0001", 1000.00m, "WEB"));

        await Assert.ThrowsAsync<DuplicateReferenceException>(
            () => service.CreateAsync(Request("PAY-2026-0001", 500.00m, "MOBILE")));

        Assert.Single(_repository.GetAll());
        Assert.Equal(1, feeClient.CallCount); // No se llamó a Java para el duplicado.
    }

    [Fact]
    public async Task CreateAsync_ReferenceComparisonIsCaseSensitive()
    {
        var service = CreateService(FakeFeeCalculationClient.Returning(1.00m));

        await service.CreateAsync(Request("PAY-2026-0001", 100.00m, "WEB"));
        await service.CreateAsync(Request("pay-2026-0001", 100.00m, "WEB"));

        Assert.Equal(2, _repository.GetAll().Count);
    }

    public static TheoryData<string?, decimal?, string?> InvalidRequests => new()
    {
        { null, 100.00m, "WEB" },                 // referencia ausente
        { "   ", 100.00m, "WEB" },                // referencia solo con espacios
        { new string('A', 51), 100.00m, "WEB" },  // referencia de más de 50 caracteres
        { "PAY-1", null, "WEB" },                 // monto ausente
        { "PAY-1", 0m, "WEB" },                   // monto igual a cero
        { "PAY-1", -5.00m, "WEB" },               // monto negativo
        { "PAY-1", 10.123m, "WEB" },              // más de dos decimales
        { "PAY-1", 100.00m, null },               // canal ausente
        { "PAY-1", 100.00m, "ATM" },              // canal inexistente
        { "PAY-1", 100.00m, "web" },              // canal en minúsculas
    };

    [Theory]
    [MemberData(nameof(InvalidRequests))]
    public async Task CreateAsync_WithInvalidData_ThrowsValidationAndDoesNotCallFeeService(
        string? reference, decimal? amount, string? channel)
    {
        var feeClient = FakeFeeCalculationClient.Returning(1.00m);
        var service = CreateService(feeClient);

        await Assert.ThrowsAsync<RequestValidationException>(
            () => service.CreateAsync(Request(reference, amount, channel)));

        Assert.Equal(0, feeClient.CallCount);
        Assert.Empty(_repository.GetAll());
    }
}