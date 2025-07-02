using FluentResults;
using NoviCode.Core.Data;

namespace NoviCode.Core.Abstractions;

public interface IWalletFacade
{
    Task<Result<WalletDto>> GetWalletAsync(long walletId, string? requestedCurrency); 
    Task<Result<WalletDto>> AdjustBalanceAsync(AdjustBalanceRequest request);
    Task<Result<WalletDto>> CreateWalletAsync(CreateWalletRequest request);
}