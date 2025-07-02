using NoviCode.Core.Data;
using NoviCode.Core.Domain;

namespace NoviCode.Core.Utils;

public static class WalletMapper
{
    public static WalletDto ToDto(this Wallet wallet)
    {
        return new WalletDto
        {
            Id = wallet.Id,
            Currency = wallet.Currency,
            Balance = wallet.Balance
        };
    }
    
    public static Wallet ToDomain(this CreateWalletRequest request)
    {
        return new Wallet
        {
            Currency = request.Currency,
            Balance = request.StartingBalance
        };
    }
}