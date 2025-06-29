using Microsoft.Extensions.Logging;
using NoviCode.Core.Abstractions;
using NoviCode.Core.Data;
using NoviCode.Core.Domain;

namespace NoviCode.Core.Services;

public class WalletService : IWalletService
{
    private readonly IWalletRepository _walletRepository;
    private readonly IAdjustFundsFactory _adjustFundsFactory;
    private readonly ICurrencyConverter _currencyConverter;
    private readonly ILogger<WalletService> _logger;

    public WalletService(IWalletRepository walletRepository,
                         IAdjustFundsFactory adjustFundsFactory, 
                         ICurrencyConverter currencyConverter,
                         ILogger<WalletService> logger)
    {
        _walletRepository = walletRepository;
        _adjustFundsFactory = adjustFundsFactory;
        _currencyConverter = currencyConverter;
        _logger = logger;
    }
    
    public async Task<WalletDto> GetWalletAsync(long walletId)
    {
        var wallet = await _walletRepository.GetByIdAsync(walletId);
        if (wallet is null) return null;
        return new WalletDto
        {
            Id = wallet.Id,
            Currency = wallet.Currency,
            Balance = wallet.Balance,
        };
    }

    public async Task<WalletDto> CreateWalletAsync(CreateWalletRequest request)
    {
        var wallet = new Wallet
        {
            Currency = request.Currency,
            Balance = request.StartingBalance
        };
        await _walletRepository.AddAsync(wallet);
        return new WalletDto
        {
            Id = wallet.Id,
            Currency = wallet.Currency,
            Balance = wallet.Balance
        };
    }

    public async Task<WalletDto?> AdjustBalanceAsync(AdjustBalanceRequest request)
    {
        //TODO: Validate request
        var wallet = await _walletRepository.GetByIdAsync(request.WalletId);
        decimal convertedAmount = await _currencyConverter.ConvertAsync(request.Amount, wallet.Currency, request.Currency);
        
        // pick and apply the strategy
        var strategy = _adjustFundsFactory.GetStrategy(request.Strategy);
        strategy.Apply(wallet, convertedAmount);
        
        // save the changes
        await _walletRepository.UpdateAsync(wallet);
        
        _logger.LogInformation("Adjusted balance for wallet {WalletId} by {Amount} {Currency} using strategy {Strategy}.",
            request.WalletId, convertedAmount, request.Currency, request.Strategy);

        return new WalletDto
        {
            Id = wallet.Id,
            Currency = wallet.Currency,
            Balance = wallet.Balance,
        };
    }
}