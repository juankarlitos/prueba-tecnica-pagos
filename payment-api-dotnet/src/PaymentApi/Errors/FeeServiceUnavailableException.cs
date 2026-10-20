namespace PaymentApi.Errors;

/// <summary>
/// El servicio Java no respondió, venció el timeout o devolvió un resultado inválido.
/// El mensaje interno se registra en logs; al usuario se le muestra un mensaje genérico.
/// </summary>
public sealed class FeeServiceUnavailableException : Exception
{
    public FeeServiceUnavailableException(string internalReason, Exception? innerException = null)
        : base(internalReason, innerException)
    {
    }
}