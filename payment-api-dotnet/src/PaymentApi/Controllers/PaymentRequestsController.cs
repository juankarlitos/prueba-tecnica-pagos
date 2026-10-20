using Microsoft.AspNetCore.Mvc;
using PaymentApi.Contracts;
using PaymentApi.Services;

namespace PaymentApi.Controllers;

[ApiController]
[Route("api/v1/payment-requests")]
[Produces("application/json")]
public sealed class PaymentRequestsController : ControllerBase
{
    private readonly IPaymentRequestService _paymentRequestService;

    public PaymentRequestsController(IPaymentRequestService paymentRequestService)
    {
        _paymentRequestService = paymentRequestService;
    }

    /// <summary>
    /// Registra una solicitud de pago. La comisión la calcula el servicio Java.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(PaymentRequestResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<PaymentRequestResponse>> Create(
        [FromBody] CreatePaymentRequest? request, CancellationToken cancellationToken)
    {
        var created = await _paymentRequestService.CreateAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, created);
    }

    /// <summary>
    /// Consulta las solicitudes registradas. Permite filtrar por canal (opcional).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PaymentRequestResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public ActionResult<IReadOnlyList<PaymentRequestResponse>> GetAll([FromQuery] string? channel)
    {
        return Ok(_paymentRequestService.GetAll(channel));
    }
}