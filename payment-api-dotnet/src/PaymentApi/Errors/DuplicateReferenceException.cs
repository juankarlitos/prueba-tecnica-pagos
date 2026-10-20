namespace PaymentApi.Errors;

public sealed class DuplicateReferenceException : Exception
{
    public DuplicateReferenceException(string externalReference)
        : base($"Ya existe una solicitud con la referencia '{externalReference}'.")
    {
        ExternalReference = externalReference;
    }

    public string ExternalReference { get; }
}