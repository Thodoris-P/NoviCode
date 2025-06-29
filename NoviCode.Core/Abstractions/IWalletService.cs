using NoviCode.Core.Data;

namespace NoviCode.Core.Abstractions;

public interface IWalletService
{
    Task<WalletDto> GetWalletAsync(long walletId);
    Task<WalletDto> CreateWalletAsync(CreateWalletRequest request);
    Task<WalletDto?> AdjustBalanceAsync(AdjustBalanceRequest request);
}