using Bogus;
using NoviCode.Core.Domain;
using NoviCode.Core.Services;
using Shouldly;

namespace NoviCode.Core.Tests;

public class AdjustFundsStrategyTests
{
    private readonly Faker _faker;
    private readonly decimal _startingBalance;
    private readonly decimal _adjustmentAmount;
    private readonly Wallet _wallet;
    
    

    public AdjustFundsStrategyTests()
    {
        _faker = new Faker();
        _startingBalance = _faker.Random.Decimal();
        _adjustmentAmount = _faker.Random.Decimal();
        _wallet = new Wallet
        {
            Balance = _startingBalance, 
            Currency = "EUR", 
            Id = _faker.Random.Long(1)
        };
    }
    
    [Fact]
    public void AddFundsStrategy_ReturnsSum()
    {
        // Arrange
        var expected = _adjustmentAmount + _startingBalance;
        var strategy = new AddFundsStrategy();

        // Act
        strategy.Apply(_wallet, _adjustmentAmount);
        var actual = _wallet.Balance;

        // Assert
        actual.ShouldBe(expected);
    }

    [Fact]
    public void SubtractFundsStrategy_SmallerAdjustmentAmount_ReturnsDifference()
    {
        // Arrange
        var subtractAmount = _faker.Random.Decimal(0m, _startingBalance);
        var expected = _startingBalance - subtractAmount;
        var strategy = new SubtractFundsStrategy();

        // Act
        strategy.Apply(_wallet, subtractAmount);
        var actual = _wallet.Balance;

        // Assert
        actual.ShouldBe(expected);
    }
    
    [Fact]
    public void SubtractFundsStrategy_BiggerAdjustmentAmount_ReturnsUntouched()
    {
        // Arrange
        var subtractAmount = _faker.Random.Decimal(_startingBalance);
        var expected = _startingBalance;
        var strategy = new SubtractFundsStrategy();

        // Act
        strategy.Apply(_wallet, subtractAmount);
        var actual = _wallet.Balance;

        // Assert
        actual.ShouldBe(expected);
    }
    
    [Fact]
    public void ForceSubtractFundsStrategy_SmallerAdjustmentAmount_ReturnsDifference()
    {
        // Arrange
        var subtractAmount = _faker.Random.Decimal(0m, _startingBalance);
        var expected = _startingBalance - subtractAmount;
        var strategy = new ForceSubtractFundsStrategy();

        // Act
        strategy.Apply(_wallet, subtractAmount);
        var actual = _wallet.Balance;

        // Assert
        actual.ShouldBe(expected);
    }
    
    [Fact]
    public void ForceSubtractFundsStrategy_BiggerAdjustmentAmount_ReturnsDifference()
    {
        // Arrange
        var subtractAmount = _faker.Random.Decimal(_startingBalance);
        var expected = _startingBalance - subtractAmount;
        var strategy = new ForceSubtractFundsStrategy();

        // Act
        strategy.Apply(_wallet, subtractAmount);
        var actual = _wallet.Balance;

        // Assert
        actual.ShouldBe(expected);
    }
}