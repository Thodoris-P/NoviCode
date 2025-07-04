using FluentResults;
using Microsoft.Extensions.Logging;
using NoviCode.Core.Abstractions;
using NoviCode.Core.Data;
using NoviCode.Core.Errors;
using NoviCode.Core.Exceptions;
using NoviCode.Core.Utils;

namespace NoviCode.Core.Services;

public class WalletFacade : IWalletFacade
{
    private readonly IWalletService _walletService;
    private readonly ICurrencyConverter _currencyConverter;
    private readonly IExchangeRatesService _exchangeRatesService;
    private readonly ILogger<WalletFacade> _logger;

    public WalletFacade(IWalletService walletService,
        ICurrencyConverter currencyConverter,
        ILogger<WalletFacade> logger,
        IExchangeRatesService exchangeRatesService)
    {
        _walletService = walletService;
        _currencyConverter = currencyConverter;
        _logger = logger;
        _exchangeRatesService = exchangeRatesService;
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
            try
            {
                _logger.LogInformation("Converting wallet balance from {OriginalCurrency} to {RequestedCurrency}.", wallet.Currency, requestedCurrency);
                walletDto.Balance = await _currencyConverter.ConvertAsync(wallet.Balance, wallet.Currency, requestedCurrency);
                walletDto.Currency = requestedCurrency;
            }
            catch (CurrencyNotFoundException e)
            {
                _logger.LogError(e, "Currency not found: {Message}", e.Message);
                return Result.Fail(new ValidationError($"Not a valid currency: {requestedCurrency}").CausedBy(e));
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to convert wallet balance: {Wallet}, {Balance}, {Currency}", wallet.Id, wallet.Balance, requestedCurrency);
                return Result.Fail(new Error("Failed to convert wallet balance").CausedBy(e));
            }
        }
        
        return walletDto;
    }

    public async Task<Result<WalletDto>> AdjustBalanceAsync(AdjustBalanceRequest request)
    {
        var wallet = await _walletService.GetWalletAsync(request.WalletId);
        if (wallet is null)
        {
            _logger.LogWarning("Wallet with ID {WalletId} not found.", request.WalletId);
            return Result.Fail(new NotFoundError($"Wallet '{request.WalletId}' not found"));
        }

        decimal convertedAmount;
        try
        {
            convertedAmount = await _currencyConverter.ConvertAsync(request.Amount, wallet.Currency, request.Currency);
        }
        catch (CurrencyNotFoundException e)
        {
            _logger.LogError(e, "Currency not found: {Message}", e.Message);
            return Result.Fail(new ValidationError($"Currency not found: {request.Currency}").CausedBy(e));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to convert amount {Amount} from {FromCurrency} to {ToCurrency}.",
                request.Amount, wallet.Currency, request.Currency);
            return Result.Fail(new Error("Failed to convert amount").CausedBy(e));
        }

        try
        {
            await _walletService.AdjustBalanceAsync(wallet.Id, convertedAmount, request.Strategy);
        }
        catch (InsufficientFundsException e)
        {
            _logger.LogWarning(e, "No available funds for wallet {WalletId} to adjust by {Amount}.", wallet.Id, convertedAmount);
            return Result.Fail(new BusinessError("Insufficient funds").CausedBy(e));
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
        var temp = await _exchangeRatesService.GetExchangeRate(request.Currency);
        if (temp is null)
        {
            _logger.LogWarning("Invalid currency: {Currency}", request.Currency);
            return Result.Fail<WalletDto>(new ValidationError("Not a valid currency"));
        }
        
        var wallet = await _walletService.CreateWalletAsync(request);
        
        if (wallet is null)
        {
            _logger.LogError("Failed to create wallet with request: {Request}", request);
            return Result.Fail(new Error("Failed to create wallet"));
        }

        return wallet.ToDto();
    }
}

