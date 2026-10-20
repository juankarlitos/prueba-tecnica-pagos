using PaymentApi.Contracts;

namespace PaymentApi.Errors;

public static class ErrorHandlingExtensions
{
    public static WebApplication UseApiErrorHandling(this WebApplication app)
    {
        // Último recurso para excepciones que ocurran fuera de los controladores.
        app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(ErrorResponse.Of(
                ErrorCodes.InternalError,
                "Ocurrió un error inesperado. Intente nuevamente."));
        }));

        // Respuestas de error sin cuerpo (ruta inexistente, método o Content-Type no soportado).
        app.UseStatusCodePages(async statusContext =>
        {
            var response = statusContext.HttpContext.Response;

            var error = response.StatusCode switch
            {
                StatusCodes.Status404NotFound =>
                    ErrorResponse.Of(ErrorCodes.NotFound, "El recurso solicitado no existe."),
                StatusCodes.Status405MethodNotAllowed =>
                    ErrorResponse.Of(ErrorCodes.MethodNotAllowed, "Método HTTP no permitido para este recurso."),
                StatusCodes.Status415UnsupportedMediaType =>
                    ErrorResponse.Of(ErrorCodes.UnsupportedMediaType, "El Content-Type debe ser application/json."),
                _ =>
                    ErrorResponse.Of($"HTTP_{response.StatusCode}", "La solicitud no pudo ser procesada.")
            };

            await response.WriteAsJsonAsync(error);
        });

        return app;
    }
}