using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PaymentApi.Contracts;

namespace PaymentApi.Errors;

/// <summary>
/// Traduce las excepciones de negocio a respuestas HTTP con la estructura { code, message, details }.
/// Nunca expone trazas internas al usuario.
/// </summary>
public sealed class ApiExceptionFilter : IExceptionFilter
{
    private const string CalculationUnavailableMessage = "No fue posible calcular la comisión. Intente nuevamente.";
    private const string UnexpectedErrorMessage = "Ocurrió un error inesperado. Intente nuevamente.";

    private readonly ILogger<ApiExceptionFilter> _logger;

    public ApiExceptionFilter(ILogger<ApiExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        var (statusCode, error) = context.Exception switch
        {
            RequestValidationException ex => (
                StatusCodes.Status400BadRequest,
                new ErrorResponse(ErrorCodes.ValidationError, ex.Message, ex.Errors)),

            DuplicateReferenceException ex => (
                StatusCodes.Status409Conflict,
                ErrorResponse.Of(ErrorCodes.DuplicateReference, ex.Message)),

            FeeServiceUnavailableException => (
                StatusCodes.Status503ServiceUnavailable,
                ErrorResponse.Of(ErrorCodes.CalculationServiceUnavailable, CalculationUnavailableMessage)),

            _ => (
                StatusCodes.Status500InternalServerError,
                ErrorResponse.Of(ErrorCodes.InternalError, UnexpectedErrorMessage))
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(context.Exception, "Error inesperado al procesar {Path}", context.HttpContext.Request.Path);
        }

        context.Result = new ObjectResult(error) { StatusCode = statusCode };
        context.ExceptionHandled = true;
    }
}