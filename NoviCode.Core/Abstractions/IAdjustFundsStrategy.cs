using NoviCode.Core.Data;
using NoviCode.Core.Domain;

namespace NoviCode.Core.Abstractions;

public interface IAdjustFundsStrategy
{
    Strategy Strategy { get; }
    void Apply(Wallet wallet, decimal amountInBaseCurrency);
}