using Microsoft.Extensions.Logging;
using NoviCode.Core.Abstractions;
using NoviCode.Core.Data;
using NoviCode.Core.Domain;

namespace NoviCode.Core.Services;

public class WalletService : IWalletService
{
    private readonly IWalletRepository _walletRepository;
    private readonly IAdjustFundsFactory _adjustFundsFactory;
    private readonly ILogger<WalletService> _logger;

    public WalletService(IWalletRepository walletRepository,
                         IAdjustFundsFactory adjustFundsFactory,
                         ILogger<WalletService> logger)
    {
        _walletRepository = walletRepository;
        _adjustFundsFactory = adjustFundsFactory;
        _logger = logger;
    }
    
    public async Task<Wallet?> GetWalletAsync(long walletId)
    {
        return await _walletRepository.GetByIdAsync(walletId);
    }

    public async Task<Wallet> CreateWalletAsync(CreateWalletRequest request)
    {
        var wallet = new Wallet
        {
            Currency = request.Currency,
            Balance = request.StartingBalance
        };
        await _walletRepository.AddAsync(wallet);
        return wallet;
    }

    public async Task AdjustBalanceAsync(Wallet wallet, decimal amount, Strategy strategy)
    {
        //TODO: Validate request
        var strategyInstance = _adjustFundsFactory.GetStrategy(strategy);
        strategyInstance.Apply(wallet, amount);
        
        // save the changes
        await _walletRepository.UpdateAsync(wallet);
        
        _logger.LogInformation("Adjusted balance for wallet {WalletId} by {Amount} using strategy {Strategy}.",
            wallet.Id, amount, strategy);
    }
}