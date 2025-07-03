using FluentResults;
using Microsoft.Extensions.Logging;
using NoviCode.Core.Abstractions;
using NoviCode.Core.Data;
using NoviCode.Core.Errors;
using NoviCode.Core.Utils;

namespace NoviCode.Core.Services;

public class WalletFacade : IWalletFacade
{
    private readonly IWalletService _walletService;
    private readonly ICurrencyConverter _currencyConverter;
    private readonly ILogger<WalletFacade> _logger;

    public WalletFacade(IWalletService walletService, ICurrencyConverter currencyConverter, ILogger<WalletFacade> logger)
    {
        _walletService = walletService;
        _currencyConverter = currencyConverter;
        _logger = logger;
    }
    
    public async Task<Result<WalletDto>> GetWalletAsync(long walletId, string? requestedCurrency)
    {
        var wallet = await _walletService.GetWalletAsync(walletId);

        if (wallet is null)
        {
            _logger.LogWarning("Wallet with ID {WalletId} not found.", walletId);
            return Result.Fail(new NotFoundError("Wallet not found"));
        }

        var walletDto = wallet.ToDto();

        if (!string.IsNullOrWhiteSpace(requestedCurrency))
        {
            _logger.LogInformation("Converting wallet balance from {OriginalCurrency} to {RequestedCurrency}.", wallet.Currency, requestedCurrency);
            walletDto.Balance = await _currencyConverter.ConvertAsync(wallet.Balance, wallet.Currency, requestedCurrency);
            walletDto.Currency = requestedCurrency;
        }
        
        return walletDto;
    }

    public async Task<Result<WalletDto>> AdjustBalanceAsync(AdjustBalanceRequest request)
    {
        var wallet = await _walletService.GetWalletAsync(request.WalletId);
        if (wallet is null)
        {
            _logger.LogWarning("Wallet with ID {WalletId} not found.", request.WalletId);
            return Result.Fail(new NotFoundError("Wallet not found"));
        }

        decimal convertedAmount;
        try
        {
            convertedAmount = await _currencyConverter.ConvertAsync(request.Amount, wallet.Currency, request.Currency);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to convert amount {Amount} from {FromCurrency} to {ToCurrency}.",
                request.Amount, wallet.Currency, request.Currency);
            return Result.Fail(new Error("Failed to convert amount").CausedBy(e));
        }

        try
        {
            await _walletService.AdjustBalanceAsync(wallet, convertedAmount, request.Strategy);
        }
        catch (Exception e)
        {
            _logger.LogInformation("Adjusted balance for wallet {WalletId} by {Amount} using strategy {Strategy}.",
                wallet.Id, convertedAmount, request.Strategy);
            return Result.Fail(new Error("Failed to adjust wallet balance").CausedBy(e));
        }
        

        return wallet.ToDto();
    }

    public async Task<Result<WalletDto>> CreateWalletAsync(CreateWalletRequest request)
    {
        var wallet = await _walletService.CreateWalletAsync(request);
        
        if (wallet is null)
        {
            _logger.LogError("Failed to create wallet with request: {Request}", request);
            return Result.Fail(new Error("Failed to create wallet"));
        }

        return wallet.ToDto();
    }
}