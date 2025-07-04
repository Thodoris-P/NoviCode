using NoviCode.Core.Abstractions;
using NoviCode.Core.Data;
using NoviCode.Core.Domain;
using NoviCode.Core.Exceptions;

namespace NoviCode.Core.Services;

public class SubtractFundsStrategy : IAdjustFundsStrategy
{
    public Strategy Strategy => Strategy.SubtractFunds;
    public void Apply(Wallet wallet, decimal amountInBaseCurrency)
    {
        if (wallet.Balance < amountInBaseCurrency)
            throw new InsufficientFundsException("Insufficient funds in wallet to complete the transaction.");
        
        wallet.Balance -= amountInBaseCurrency;
    }
}