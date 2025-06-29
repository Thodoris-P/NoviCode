using NoviCode.Core.Data;

namespace NoviCode.Core.Abstractions;

public interface IAdjustFundsFactory
{
    IAdjustFundsStrategy GetStrategy(Strategy strategy);
}