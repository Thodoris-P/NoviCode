using Microsoft.Extensions.Logging;
using NoviCode.Core.Abstractions;
using NoviCode.Core.Data;
using NoviCode.Core.Domain;
using NoviCode.Core.Exceptions;
using NoviCode.Core.Utils;
using Polly;

namespace NoviCode.Core.Services;

public class WalletService : IWalletService
{
    private readonly IWalletRepository _walletRepository;
    private readonly IAdjustFundsFactory _adjustFundsFactory;
    private readonly IAsyncPolicy _retryPolicy;
    private readonly ILogger<WalletService> _logger;

    public WalletService(IWalletRepository walletRepository,
                         IAdjustFundsFactory adjustFundsFactory,
                         ILogger<WalletService> logger)
    {
        _walletRepository = walletRepository;
        _adjustFundsFactory = adjustFundsFactory;
        _logger = logger;
        _retryPolicy = Policy
            .Handle<ConcurrencyException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(50 * attempt),
                onRetry: (retryCount, context) =>
                {
                    _logger.LogWarning("Concurrency failure, retry {RetryCount}", retryCount);
                }
            );
    }
    
    public async Task<Wallet?> GetWalletAsync(long walletId)
    {
        return await _walletRepository.GetByIdAsync(walletId);
    }

    public async Task<Wallet?> CreateWalletAsync(CreateWalletRequest request)
    {
        var wallet = request.ToDomain();
        await _walletRepository.AddAsync(wallet);
        return wallet;
    }

    public async Task<Wallet> AdjustBalanceAsync(long walletId, decimal amount, Strategy strategy)
    {
        var strategyInstance = _adjustFundsFactory.GetStrategy(strategy);
        
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            var wallet = await _walletRepository.GetByIdAsync(walletId);
            if (wallet is null)
                throw new WalletNotFoundException($"Wallet with ID {walletId} not found.");
            strategyInstance.Apply(wallet, amount);
            await _walletRepository.UpdateAsync(wallet);
            _logger.LogInformation("Adjusted balance for wallet {WalletId} by {Amount} using strategy {Strategy}.",
                wallet.Id, amount, strategy);
            return wallet;
        });
    }
}