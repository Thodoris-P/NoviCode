using FluentResults;
using NoviCode.Core.Abstractions;
using NoviCode.Core.Domain;
using NoviCode.Core.Errors;
using NoviCode.Core.Utils;

namespace NoviCode.Core.Services;

public class EcbExchangeRateProvider : IExchangeRateProvider
{
    private readonly ICurrencyGateway _currencyGateway;
    private static readonly ExchangeRate EuroExchangeRate = new(){ Rate = EuroRate, Currency = EuroCode };
    private const decimal EuroRate = 1m;
    private const string EuroCode = "EUR";
    
    public EcbExchangeRateProvider(ICurrencyGateway currencyGateway)
    {
        _currencyGateway = currencyGateway;
    }
    
    public async Task<Result<IEnumerable<ExchangeRate>>> GetLatestRatesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var envelope = await _currencyGateway.GetLatestRatesAsync(cancellationToken);
            var result = EcbRatesToExchangeRatesMapper.Map(envelope);
            // Add the Euro rate to the result, because ECB always provides rates relative to Euro
            result = result.Concat([EuroExchangeRate]);
            return new Result<IEnumerable<ExchangeRate>>().WithValue(result);
        }
        catch (Exception e)
        {
            return Result.Fail<IEnumerable<ExchangeRate>>(
                new ExchangeProviderError("Failed to fetch exchange rates from ECB").CausedBy(e));
        }
    }
}