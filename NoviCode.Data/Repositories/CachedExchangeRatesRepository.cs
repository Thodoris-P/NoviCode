using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using NoviCode.Core.Abstractions;
using NoviCode.Core.Domain;

namespace NoviCode.Core.Services;

public class CachedExchangeRatesRepository : IExchangeRatesRepository
{
    private readonly ILogger<CachedExchangeRatesRepository> _logger;
    private readonly IExchangeRatesRepository _inner;
    private readonly IDistributedCache _cache;
    private readonly TimeSpan _cacheDuration =  TimeSpan.FromMinutes(10);

    public CachedExchangeRatesRepository(ILogger<CachedExchangeRatesRepository> logger,  IExchangeRatesRepository inner, IDistributedCache cache)
    {
        _logger = logger;
        _inner = inner;
        _cache = cache;
    }
    
    public async Task UpdateRates(IEnumerable<ExchangeRate> rates)
    {
        await _inner.UpdateRates(rates);
        
        await CacheRates(rates);
    }

    private async Task CacheRates(IEnumerable<ExchangeRate> rates)
    {
        foreach (var rate in rates)
        {
            await CacheRateSingle(rate);
        }
    }

    private async Task CacheRateSingle(ExchangeRate rate)
    {
        try
        {
            var serialized = JsonSerializer.Serialize(rate);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _cacheDuration
            };
            await _cache.SetStringAsync(rate.Currency, serialized, options);
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Failed to write to cache. key={Key}", rate.Currency);    
        }
    }

    public async Task<ExchangeRate?> GetExchangeRate(string currency)
    {
        try
        {
            var cached = await _cache.GetStringAsync(currency);
            if (!string.IsNullOrWhiteSpace(cached))
            {
                _logger.LogDebug("Cache hit for {currency}", currency);
                return JsonSerializer.Deserialize<ExchangeRate>(cached);
            }
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Failed to retrieve data from cache. key {Key}", currency);
        }
        
        _logger.LogDebug("Cache miss for {Currency}",  currency);
        
        var result = await _inner.GetExchangeRate(currency);

        if (result is not null)
        {
            await CacheRateSingle(result);
        }
        
        return result;
    }
}