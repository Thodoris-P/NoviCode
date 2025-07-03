using NoviCode.Core.Data;
using NoviCode.Core.Domain;

namespace NoviCode.Core.Abstractions;

public interface IWalletService
{
    Task<Wallet?> GetWalletAsync(long walletId);
    Task<Wallet?> CreateWalletAsync(CreateWalletRequest request);
    Task<Wallet> AdjustBalanceAsync(Wallet wallet, decimal amount, Strategy strategy);
}