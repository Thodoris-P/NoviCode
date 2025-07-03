using NoviCode.Core.Abstractions;
using NoviCode.Core.Data;
using NoviCode.Core.Domain;
using NoviCode.Core.Services;

namespace NoviCode.Core.Tests;

public class AdjustFundsFactoryTests
{
    private class FakeStrategy : IAdjustFundsStrategy
    {
        public Strategy Strategy { get; }

        public FakeStrategy(Strategy strategy)
        {
            Strategy = strategy;
        }

        public void Apply(Wallet wallet, decimal amountInBaseCurrency)
        {
            // no-op for factory tests
        }
    }

    [Theory]
    [InlineData(Strategy.AddFunds)]
    [InlineData(Strategy.SubtractFunds)]
    [InlineData(Strategy.ForceSubtractFunds)]
    public void GetStrategy_ReturnsCorrectInstance_ForEachEnum(Strategy strategy)
    {
        // Arrange: create one stub per enum value
        var strategies = new IAdjustFundsStrategy[]
        {
            new FakeStrategy(Strategy.AddFunds),
            new FakeStrategy(Strategy.SubtractFunds),
            new FakeStrategy(Strategy.ForceSubtractFunds)
        };
        var factory = new AdjustFundsFactory(strategies);

        // Act
        var result = factory.GetStrategy(strategy);

        // Assert: the factory returns the exact stub whose Strategy property matches
        var expected = strategies.Single(s => s.Strategy == strategy);
        Assert.Same(expected, result);
    }

    [Fact]
    public void GetStrategy_UnknownEnum_ThrowsArgumentException()
    {
        // Arrange: only one known strategy
        var strategies = new IAdjustFundsStrategy[] { new FakeStrategy(Strategy.AddFunds) };
        var factory = new AdjustFundsFactory(strategies);
        var unknown = (Strategy)999;

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => factory.GetStrategy(unknown));
        Assert.Contains($"Unknown strategy: {unknown}", ex.Message);
    }

    [Fact]
    public void Constructor_DuplicateStrategies_ThrowsArgumentException()
    {
        // Arrange: two stubs with the same Strategy enum
        var duplicates = new IAdjustFundsStrategy[]
        {
            new FakeStrategy(Strategy.AddFunds),
            new FakeStrategy(Strategy.AddFunds)
        };

        // Act & Assert: ToDictionary will throw if keys duplicate
        Assert.Throws<ArgumentException>(() => new AdjustFundsFactory(duplicates));
    }
}