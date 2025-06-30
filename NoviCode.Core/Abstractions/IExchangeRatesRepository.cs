using NoviCode.Core.Domain;

namespace NoviCode.Core.Abstractions;

public interface IExchangeRatesRepository
{
    Task UpdateRates(IEnumerable<ExchangeRate> rates);
    Task<ExchangeRate?> GetExchangeRate(string currency);
}