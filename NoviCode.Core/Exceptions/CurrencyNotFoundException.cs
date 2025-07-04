namespace NoviCode.Core.Exceptions;

public class CurrencyNotFoundException : Exception
{
    public CurrencyNotFoundException(string message) : base(message)
    {
    }

    public CurrencyNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }
}