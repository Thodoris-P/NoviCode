using Microsoft.Extensions.Logging;
using NoviCode.Core.Abstractions;

namespace NoviCode.Core.Services;

public class ExchangeRatesService : IExchangeRatesService
{
    private readonly IExchangeRatesRepository _exchangeRatesRepository;
    private readonly ILogger<ExchangeRatesService> _logger;
    private readonly ICurrencyGateway _currencyGateway;

    public ExchangeRatesService(IExchangeRatesRepository exchangeRatesRepository, ILogger<ExchangeRatesService> logger, ICurrencyGateway currencyGateway)
    {
        _exchangeRatesRepository = exchangeRatesRepository;
        _logger = logger;
        _currencyGateway = currencyGateway;
    }
    
    public async Task UpdateRatesAsync()
    {
        //get rates from gateway
        var rates = await _currencyGateway.GetLatestRatesAsync();

        //update rates to db
        await _exchangeRatesRepository.UpdateRates(rates);
    }
}