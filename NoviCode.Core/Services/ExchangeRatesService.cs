using Microsoft.Extensions.Logging;
using NoviCode.Core.Abstractions;

namespace NoviCode.Core.Services;

public class ExchangeRatesService : IExchangeRatesService
{
    private readonly IExchangeRatesRepository _exchangeRatesRepository;
    private readonly ILogger<ExchangeRatesService> _logger;
    private readonly IExchangeRateProvider _exchangeRateProvider;

    public ExchangeRatesService(IExchangeRatesRepository exchangeRatesRepository,
        ILogger<ExchangeRatesService> logger,
        IExchangeRateProvider exchangeRateProvider)
    {
        _exchangeRatesRepository = exchangeRatesRepository;
        _logger = logger;
        _exchangeRateProvider = exchangeRateProvider;
    }
    
    public async Task UpdateRatesAsync()
    {
        //get rates from gateway
        var rates = await _exchangeRateProvider.GetLatestRatesAsync();

        //update rates to db
        await _exchangeRatesRepository.UpdateRates(rates);
    }
}