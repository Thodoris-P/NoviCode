using NoviCode.Core.Abstractions;
using NoviCode.Core.Data;

namespace NoviCode.Core.Services;

public class WalletFacade : IWalletFacade
{
    private readonly IWalletService _walletService;
    private readonly ICurrencyConverter _currencyConverter;

    public WalletFacade(IWalletService walletService, ICurrencyConverter currencyConverter)
    {
        _walletService = walletService;
        _currencyConverter = currencyConverter;
    }
    
    public async Task<WalletDto?> GetWalletAsync(long walletId, string? requestedCurrency)
    {
        var wallet = await _walletService.GetWalletAsync(walletId);
        if (wallet is null)
            return null;

        var walletDto = new WalletDto()
        {
            Id = wallet.Id,
            Currency = wallet.Currency,
            Balance = wallet.Balance,
        };

        if (!string.IsNullOrWhiteSpace(requestedCurrency))
        {
            walletDto.Balance = await _currencyConverter.ConvertAsync(wallet.Balance, wallet.Currency, requestedCurrency);
            walletDto.Currency = requestedCurrency;
        }
        return walletDto;
    }

    public async Task<WalletDto> AdjustBalanceAsync(AdjustBalanceRequest request)
    {
        var wallet = await _walletService.GetWalletAsync(request.WalletId);
        
        var convertedAmount = await _currencyConverter.ConvertAsync(request.Amount, wallet.Currency, request.Currency);
        
        await _walletService.AdjustBalanceAsync(wallet, convertedAmount, request.Strategy);
        
        return new WalletDto
        {
            Id = wallet.Id,
            Currency = wallet.Currency,
            Balance = wallet.Balance,
        };
    }

    public async Task<WalletDto> CreateWalletAsync(CreateWalletRequest request)
    {
        var wallet = await _walletService.CreateWalletAsync(request);
        return new WalletDto
        {
            Id = wallet.Id,
            Currency = wallet.Currency,
            Balance = wallet.Balance,
        };
    }
}