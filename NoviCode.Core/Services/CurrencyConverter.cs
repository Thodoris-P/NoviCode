using NoviCode.Core.Abstractions;

namespace NoviCode.Core.Services;

public class CurrencyConverter : ICurrencyConverter
{
    public Task<decimal> ConvertAsync(decimal amount, string fromCurrency, string toCurrency)
    {
        // Simulate an asynchronous operation for currency conversion.
        // In a real application, this would call an external API or service.
        return Task.FromResult(amount); // For now, just return the same amount.
    }
}