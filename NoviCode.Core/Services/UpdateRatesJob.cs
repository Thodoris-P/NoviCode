using Microsoft.Extensions.Logging;
using NoviCode.Core.Abstractions;
using Quartz;

namespace NoviCode.Core.Services;

public class UpdateRatesJob : IJob
{
    private readonly IExchangeRatesService _exchangeRatesService;
    private readonly ILogger<UpdateRatesJob> _logger;

    public UpdateRatesJob(IExchangeRatesService exchangeRatesService, ILogger<UpdateRatesJob> logger)
    {
        _exchangeRatesService = exchangeRatesService ?? throw new ArgumentNullException(nameof(exchangeRatesService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Running Quartz Job for updating currency rates");
        try
        {
            await _exchangeRatesService.UpdateRatesAsync();
            _logger.LogInformation("Quartz job finished updating currency rates");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Quartz - Error updating currency rates");
            throw;
        }
    }
}