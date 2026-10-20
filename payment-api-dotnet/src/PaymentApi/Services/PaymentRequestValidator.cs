using PaymentApi.Contracts;
using PaymentApi.Domain;
using PaymentApi.Errors;

namespace PaymentApi.Services;

public static class PaymentRequestValidator
{
    public const int MaxReferenceLength = 50;

    // Mismo límite que el servicio Java (15 dígitos enteros y 2 decimales),
    // para que un monto aceptado aquí nunca sea rechazado por Java.
    public const decimal MaxAmount = 999_999_999_999_999.99m;

    private const string InvalidChannelMessage = "channel: debe ser WEB, MOBILE o BRANCH";

    public static ValidatedPaymentRequest Validate(CreatePaymentRequest? request)
    {
        if (request is null)
        {
            throw new RequestValidationException(new[] { "body: el cuerpo de la solicitud es obligatorio" });
        }

        var errors = new List<string>();

        var reference = request.ExternalReference?.Trim();
        if (string.IsNullOrEmpty(reference))
        {
            errors.Add("externalReference: es obligatoria");
        }
        else if (reference.Length > MaxReferenceLength)
        {
            errors.Add($"externalReference: admite como máximo {MaxReferenceLength} caracteres");
        }

        if (request.Amount is not decimal amount)
        {
            errors.Add("amount: es obligatorio");
        }
        else
        {
            if (amount <= 0)
            {
                errors.Add("amount: debe ser mayor que cero");
            }

            if (decimal.Round(amount, 2) != amount)
            {
                errors.Add("amount: admite como máximo dos decimales");
            }

            if (amount > MaxAmount)
            {
                errors.Add("amount: excede el monto máximo permitido");
            }
        }

        if (!PaymentChannels.IsValid(request.Channel))
        {
            errors.Add(InvalidChannelMessage);
        }

        if (errors.Count > 0)
        {
            throw new RequestValidationException(errors);
        }

        return new ValidatedPaymentRequest(reference!, request.Amount!.Value, request.Channel!);
    }

    /// <summary>
    /// Valida el filtro opcional por canal de la consulta.
    /// </summary>
    public static string? ValidateChannelFilter(string? channel)
    {
        if (channel is null)
        {
            return null;
        }

        if (!PaymentChannels.IsValid(channel))
        {
            throw new RequestValidationException(new[] { InvalidChannelMessage });
        }

        return channel;
    }
}