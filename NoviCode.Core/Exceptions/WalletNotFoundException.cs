namespace NoviCode.Core.Exceptions;

public class WalletNotFoundException: Exception
{
    public WalletNotFoundException(string message, Exception? inner = null)
        : base(message, inner)
    {
    }
}