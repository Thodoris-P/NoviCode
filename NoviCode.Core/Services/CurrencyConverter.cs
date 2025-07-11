using NoviCode.Core.Abstractions;
using NoviCode.Core.Exceptions;

namespace NoviCode.Core.Services;

public class CurrencyConverter : ICurrencyConverter
{
    private readonly IExchangeRatesRepository _exchangeRatesRepository;

    public CurrencyConverter(IExchangeRatesRepository exchangeRatesRepository)
    {
        _exchangeRatesRepository = exchangeRatesRepository;
    }
    
    public async Task<decimal> ConvertAsync(decimal amount, string fromCurrency, string toCurrency)
    {
        if (fromCurrency == toCurrency)
        {
            return amount;
        }
        
        var from = await _exchangeRatesRepository.GetExchangeRate(fromCurrency);
        var to = await _exchangeRatesRepository.GetExchangeRate(toCurrency);
        
        if (from == null || to == null)
            throw new CurrencyNotFoundException($"Exchange rate not found for {fromCurrency} or {toCurrency}");
        
        // Convert from A → EUR → B
        var amountInEur    = amount / from.Rate;
        var amountInTarget = amountInEur * to.Rate;

        return amountInTarget;
    }
}