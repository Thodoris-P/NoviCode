using NoviCode.Core.Domain;

namespace NoviCode.Core.Abstractions;

public interface IExchangeRatesService
{
    Task UpdateRatesAsync();
    Task<ExchangeRate?> GetExchangeRate(string currencyCode);
}