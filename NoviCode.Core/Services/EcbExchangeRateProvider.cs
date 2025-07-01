using NoviCode.Core.Abstractions;
using NoviCode.Core.Domain;
using NoviCode.Gateway.Utils;

namespace NoviCode.Core.Services;

public class EcbExchangeRateProvider : IExchangeRateProvider
{
    private readonly ICurrencyGateway _currencyGateway;

    public EcbExchangeRateProvider(ICurrencyGateway currencyGateway)
    {
        _currencyGateway = currencyGateway;
    }
    
    public async Task<IEnumerable<ExchangeRate>> GetLatestRatesAsync()
    {
        var envelope = await _currencyGateway.GetLatestRatesAsync();
        return EcbRatesToExchangeRatesMapper.Map(envelope);
    }
}