using System.Net;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using PaymentApi.Errors;
using PaymentApi.Integration;

namespace PaymentApi.Tests.Integration;

public class HttpFeeCalculationClientTests
{
    private static HttpFeeCalculationClient CreateClient(Func<HttpRequestMessage, HttpResponseMessage> handler)
    {
        var httpClient = new HttpClient(new StubHttpMessageHandler(handler))
        {
            BaseAddress = new Uri("http://fee-service.test/")
        };

        return new HttpFeeCalculationClient(httpClient, NullLogger<HttpFeeCalculationClient>.Instance);
    }

    private static HttpResponseMessage Json(HttpStatusCode status, string body) => new(status)
    {
        Content = new StringContent(body, Encoding.UTF8, "application/json")
    };

    [Fact]
    public async Task CalculateAsync_WithValidResponse_ReturnsFeeAndTotal()
    {
        HttpRequestMessage? sentRequest = null;
        var client = CreateClient(request =>
        {
            sentRequest = request;
            return Json(HttpStatusCode.OK, "{\"fee\":10.00,\"total\":1010.00}");
        });

        var result = await client.CalculateAsync(1000.00m, "WEB");

        Assert.Equal(10.00m, result.Fee);
        Assert.Equal(1010.00m, result.Total);
        Assert.Equal(HttpMethod.Post, sentRequest!.Method);
        Assert.Equal("/api/v1/fees/calculate", sentRequest.RequestUri!.AbsolutePath);
    }

    [Theory]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    public async Task CalculateAsync_WhenServiceReturnsNonOkStatus_ThrowsUnavailable(HttpStatusCode status)
    {
        var client = CreateClient(_ => Json(status, "{\"code\":\"ERROR\",\"message\":\"x\"}"));

        await Assert.ThrowsAsync<FeeServiceUnavailableException>(() => client.CalculateAsync(1000.00m, "WEB"));
    }

    [Theory]
    [InlineData("{\"fee\":10.00,\"total\":9999.00}")]   // total no coincide con monto + comisión
    [InlineData("{\"fee\":-1.00,\"total\":999.00}")]    // comisión negativa
    [InlineData("{\"total\":1010.00}")]                 // falta la comisión
    [InlineData("{\"fee\":\"abc\",\"total\":1010.00}")] // comisión no numérica
    [InlineData("esto no es json")]                     // respuesta con formato inválido
    public async Task CalculateAsync_WithInvalidResult_ThrowsUnavailable(string body)
    {
        var client = CreateClient(_ => Json(HttpStatusCode.OK, body));

        await Assert.ThrowsAsync<FeeServiceUnavailableException>(() => client.CalculateAsync(1000.00m, "WEB"));
    }

    [Fact]
    public async Task CalculateAsync_WhenServiceIsUnreachable_ThrowsUnavailable()
    {
        var client = CreateClient(_ => throw new HttpRequestException("Conexión rechazada"));

        await Assert.ThrowsAsync<FeeServiceUnavailableException>(() => client.CalculateAsync(1000.00m, "WEB"));
    }

    [Fact]
    public async Task CalculateAsync_WhenTimeoutExpires_ThrowsUnavailable()
    {
        var client = CreateClient(_ => throw new TaskCanceledException("Timeout"));

        await Assert.ThrowsAsync<FeeServiceUnavailableException>(() => client.CalculateAsync(1000.00m, "WEB"));
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

        public StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
        {
            _handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(_handler(request));
    }
}