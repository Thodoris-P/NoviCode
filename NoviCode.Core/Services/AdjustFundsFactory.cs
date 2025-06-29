using NoviCode.Core.Abstractions;
using NoviCode.Core.Data;
using Microsoft.Extensions.DependencyInjection;

namespace NoviCode.Core.Services;

public class AdjustFundsFactory : IAdjustFundsFactory
{
    private readonly Dictionary<Strategy, IAdjustFundsStrategy> _strategies;

    public AdjustFundsFactory(IEnumerable<IAdjustFundsStrategy> strategies)
    {
        _strategies = strategies
            .ToDictionary(s => s.Strategy, s => s);  // assume each strategy exposes its enum Type
    }

    public IAdjustFundsStrategy GetStrategy(Strategy strategy)
    {
        if (!_strategies.TryGetValue(strategy, out var adjustFundsStrategy))
            throw new ArgumentException($"Unknown strategy: {strategy}");
        return adjustFundsStrategy;
    }
}