using FluentResults;
using NoviCode.Core.Abstractions;
using NoviCode.Core.Domain;
using NoviCode.Core.Errors;
using NoviCode.Core.Utils;

namespace NoviCode.Core.Services;

public class EcbExchangeRateProvider : IExchangeRateProvider
{
    private readonly ICurrencyGateway _currencyGateway;

    public EcbExchangeRateProvider(ICurrencyGateway currencyGateway)
    {
        _currencyGateway = currencyGateway;
    }
    
    public async Task<Result<IEnumerable<ExchangeRate>>> GetLatestRatesAsync()
    {
        try
        {
            var envelope = await _currencyGateway.GetLatestRatesAsync();
            var result = EcbRatesToExchangeRatesMapper.Map(envelope);
            return new Result<IEnumerable<ExchangeRate>>().WithValue(result);
        }
        catch (Exception e)
        {
            return Result.Fail<IEnumerable<ExchangeRate>>(
                new ExchangeProviderError("Failed to fetch exchange rates from ECB").CausedBy(e));
        }
    }
}