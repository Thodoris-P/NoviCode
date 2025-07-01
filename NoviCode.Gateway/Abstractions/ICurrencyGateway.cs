using NoviCode.Gateway.Models;

namespace NoviCode.Core.Abstractions;

public interface ICurrencyGateway
{
    Task<Envelope> GetLatestRatesAsync(CancellationToken cancellationToken = default);
}