using NoviCode.Core.Abstractions;
using NoviCode.Core.Data;
using NoviCode.Core.Domain;

namespace NoviCode.Core.Services;

public class AddFundsStrategy : IAdjustFundsStrategy
{
    public Strategy Strategy => Strategy.AddFunds;
    
    public void Apply(Wallet wallet, decimal amountInBaseCurrency)
    {
        wallet.Balance += amountInBaseCurrency;
    }
}