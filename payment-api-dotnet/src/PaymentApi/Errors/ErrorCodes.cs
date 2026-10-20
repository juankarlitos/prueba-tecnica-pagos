namespace PaymentApi.Errors;

public static class ErrorCodes
{
    public const string ValidationError = "VALIDATION_ERROR";
    public const string MalformedRequest = "MALFORMED_REQUEST";
    public const string DuplicateReference = "DUPLICATE_REFERENCE";
    public const string CalculationServiceUnavailable = "CALCULATION_SERVICE_UNAVAILABLE";
    public const string NotFound = "NOT_FOUND";
    public const string MethodNotAllowed = "METHOD_NOT_ALLOWED";
    public const string UnsupportedMediaType = "UNSUPPORTED_MEDIA_TYPE";
    public const string InternalError = "INTERNAL_ERROR";
}