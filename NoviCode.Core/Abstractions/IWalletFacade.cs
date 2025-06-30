using NoviCode.Core.Data;

namespace NoviCode.Core.Abstractions;

public interface IWalletFacade
{
    Task<WalletDto?> GetWalletAsync(long walletId, string? requestedCurrency); 
    Task<WalletDto> AdjustBalanceAsync(AdjustBalanceRequest request);
    Task<WalletDto> CreateWalletAsync(CreateWalletRequest request);
}