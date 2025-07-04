using FluentResults;
using NoviCode.Core.Domain;

namespace NoviCode.Core.Abstractions;

public interface IExchangeRateProvider
{
    Task<Result<IEnumerable<ExchangeRate>>> GetLatestRatesAsync(CancellationToken cancellationToken = default);
}