using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using PaymentApi.Errors;

namespace PaymentApi.Integration;

/// <summary>
/// Cliente HTTP del servicio Java de cálculo. La URL base y el timeout
/// se configuran en appsettings.json (sección FeeService).
/// </summary>
public sealed class HttpFeeCalculationClient : IFeeCalculationClient
{
    private const string CalculatePath = "api/v1/fees/calculate";

    private readonly HttpClient _httpClient;
    private readonly ILogger<HttpFeeCalculationClient> _logger;

    public HttpFeeCalculationClient(HttpClient httpClient, ILogger<HttpFeeCalculationClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<FeeCalculationResult> CalculateAsync(
        decimal amount, string channel, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.PostAsJsonAsync(
                CalculatePath, new FeeCalculationRequestPayload(amount, channel), cancellationToken);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw Unavailable($"El servicio de cálculo respondió con estado {(int)response.StatusCode}.");
            }

            var payload = await response.Content.ReadFromJsonAsync<FeeCalculationResponsePayload>(cancellationToken);
            return ValidateResult(amount, payload);
        }
        catch (FeeServiceUnavailableException)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            throw Unavailable("No fue posible conectar con el servicio de cálculo.", ex);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw Unavailable("Venció el tiempo de espera del servicio de cálculo.", ex);
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException)
        {
            throw Unavailable("El servicio de cálculo devolvió una respuesta con formato inválido.", ex);
        }
    }

    /// <summary>
    /// Un resultado es válido si fee y total son numéricos, no negativos
    /// y total = amount + fee.
    /// </summary>
    private FeeCalculationResult ValidateResult(decimal amount, FeeCalculationResponsePayload? payload)
    {
        if (payload?.Fee is not decimal fee || payload.Total is not decimal total)
        {
            throw Unavailable("El servicio de cálculo no devolvió fee y total.");
        }

        if (fee < 0 || total < 0)
        {
            throw Unavailable("El servicio de cálculo devolvió valores negativos.");
        }

        if (total != amount + fee)
        {
            throw Unavailable("El total devuelto no coincide con monto + comisión.");
        }

        return new FeeCalculationResult(fee, total);
    }

    private FeeServiceUnavailableException Unavailable(string reason, Exception? inner = null)
    {
        _logger.LogWarning(inner, "Fallo en la integración con el servicio de cálculo: {Reason}", reason);
        return new FeeServiceUnavailableException(reason, inner);
    }
}

internal sealed record FeeCalculationRequestPayload(decimal Amount, string Channel);

internal sealed record FeeCalculationResponsePayload(decimal? Fee, decimal? Total);