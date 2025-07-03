using Bogus;
using Microsoft.Extensions.Logging;
using Moq;
using NoviCode.Core.Abstractions;
using NoviCode.Core.Data;
using NoviCode.Core.Domain;
using NoviCode.Core.Services;
using Shouldly;

namespace NoviCode.Core.Tests;

public class WalletServiceTests
{
    private readonly Faker _faker;
    private readonly Mock<IWalletRepository> _walletRepositoryMock;
    private readonly Mock<IAdjustFundsFactory> _adjustFundsFactoryMock;
    private readonly IWalletService _walletService;

    public WalletServiceTests()
    {
        _faker = new Faker();
        _walletRepositoryMock = new Mock<IWalletRepository>();
        _adjustFundsFactoryMock = new Mock<IAdjustFundsFactory>();
        var logger = new Mock<ILogger<WalletService>>();
        _walletService = new WalletService(
            _walletRepositoryMock.Object,
            _adjustFundsFactoryMock.Object,
            logger.Object);
    }

    [Fact]
    public async Task GetWalletAsync_ReturnsWallet_WhenExists()
    {
        // Arrange
        var walletId = _faker.Random.Long(1);
        var balance = _faker.Finance.Amount();
        var expected = new Wallet { Id = walletId, Balance = balance };
        
        _walletRepositoryMock
            .Setup(r => r.GetByIdAsync(walletId))
            .ReturnsAsync(expected);

        // Act
        var actual = await _walletService.GetWalletAsync(walletId);

        // Assert
        actual.ShouldNotBeNull();
        actual.ShouldBe(expected);
        actual.Id.ShouldBe(walletId);
        actual.Balance.ShouldBe(balance);
        
        _walletRepositoryMock.Verify(r => r.GetByIdAsync(walletId), Times.Once);
    }

    [Fact]
    public async Task GetWalletAsync_ReturnsNull_WhenNotFound()
    {
        // Arrange
        var walletId = 999L;
        _walletRepositoryMock
            .Setup(r => r.GetByIdAsync(walletId))
            .ReturnsAsync((Wallet?)null);

        // Act
        var actual = await _walletService.GetWalletAsync(walletId);

        // Assert
        Assert.Null(actual);
        _walletRepositoryMock.Verify(r => r.GetByIdAsync(walletId), Times.Once);
    }

    [Fact]
    public async Task CreateWalletAsync_AddsAndReturnsWallet()
    {
        // Arrange
        var request = new CreateWalletRequest { Currency = "EUR", StartingBalance = 100m };
        Wallet? captured = null;
        _walletRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Wallet>()))
            .Callback<Wallet>(w => captured = w)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _walletService.CreateWalletAsync(request);

        // Assert
        Assert.Same(captured, result);
        _walletRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Wallet>()), Times.Once);
    }

    [Fact]
    public async Task AdjustBalanceAsync_AppliesStrategyAndUpdatesAndLogs()
    {
        // Arrange
        var startingBalance = _faker.Finance.Amount();
        var wallet = new Wallet { Id = _faker.Random.Long(1), Balance = startingBalance };
        var amount = _faker.Finance.Amount();
        var strategy = Strategy.AddFunds;

        var strategyMock = new Mock<IAdjustFundsStrategy>();
        _adjustFundsFactoryMock
            .Setup(f => f.GetStrategy(strategy))
            .Returns(strategyMock.Object);

        strategyMock
            .Setup(s => s.Apply(wallet, amount))
            .Callback<Wallet, decimal>((w, a) => w.Balance += a);

        _walletRepositoryMock
            .Setup(r => r.UpdateAsync(wallet))
            .Returns(Task.CompletedTask);

        // Act
        _ = await _walletService.AdjustBalanceAsync(wallet, amount, strategy);

        // Assert
        // // Strategy was applied
        strategyMock.Verify(s => s.Apply(wallet, amount), Times.Once);
        // // Repository updated
        _walletRepositoryMock.Verify(r => r.UpdateAsync(wallet), Times.Once);
    }
}