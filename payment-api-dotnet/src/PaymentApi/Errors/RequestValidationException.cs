namespace PaymentApi.Errors;

public sealed class RequestValidationException : Exception
{
    public RequestValidationException(IReadOnlyList<string> errors)
        : base("La solicitud contiene datos inválidos.")
    {
        Errors = errors;
    }

    public IReadOnlyList<string> Errors { get; }
}