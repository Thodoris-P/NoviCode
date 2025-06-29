using NoviCode.Core.Abstractions;
using NoviCode.Core.Data;
using NoviCode.Core.Domain;

namespace NoviCode.Core.Services;

public class ForceSubtractFundsStrategy : IAdjustFundsStrategy
{
    public Strategy Strategy => Strategy.ForceSubtractFunds;
    public void Apply(Wallet wallet, decimal amountInBaseCurrency)
    {
        wallet.Balance -= amountInBaseCurrency;
    }
}