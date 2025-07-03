namespace NoviCode.Gateway.Exceptions;

public class CurrencyGatewayException : Exception
{
    public CurrencyGatewayException(string message, Exception? innerException = null)
        : base(message, innerException) { }
}