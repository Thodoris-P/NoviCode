namespace NoviCode.Core.Exceptions;

public class ConcurrencyException : Exception
{
    public ConcurrencyException(string message, Exception? inner = null)
        : base(message, inner)
    {
    }
}