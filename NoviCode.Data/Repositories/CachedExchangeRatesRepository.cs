using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
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
    
    public Task UpdateRates(IEnumerable<ExchangeRate> rates)
    {
        return _inner.UpdateRates(rates);
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

        try
        {
            var serialized = JsonSerializer.Serialize(result);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _cacheDuration
            };
            await _cache.SetStringAsync(currency, serialized, options);
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Failed to write to cache. key={Key}", currency);    
        }
        
        return result;
    }
}