using NoviCode.Core.Domain;

namespace NoviCode.Core.Abstractions;

public interface ICurrencyGateway
{
    Task<IEnumerable<ExchangeRate>> GetLatestRatesAsync(CancellationToken cancellationToken = default);
}