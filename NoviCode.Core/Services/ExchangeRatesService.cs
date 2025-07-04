using Microsoft.Extensions.Logging;
using NoviCode.Core.Abstractions;
using NoviCode.Core.Domain;

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
        var rates = await _exchangeRateProvider.GetLatestRatesAsync(CancellationToken.None);
        
        if (rates.IsFailed)
        {
            _logger.LogError("Failed to fetch exchange rates: {Errors}", rates.Errors);
            throw new InvalidOperationException("Failed to fetch exchange rates");
        }
        
        await _exchangeRatesRepository.UpdateRates(rates.Value);
    }
    
    public async Task<ExchangeRate?> GetExchangeRate(string currencyCode)
    {
        return await _exchangeRatesRepository.GetExchangeRate(currencyCode);
    }
}