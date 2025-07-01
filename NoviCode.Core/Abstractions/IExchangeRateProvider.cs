using NoviCode.Core.Domain;

namespace NoviCode.Core.Abstractions;

public interface IExchangeRateProvider
{
    Task<IEnumerable<ExchangeRate>> GetLatestRatesAsync();
}