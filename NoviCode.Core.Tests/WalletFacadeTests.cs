using Bogus;
using FluentResults;
using Microsoft.Extensions.Logging;
using Moq;
using NoviCode.Core.Abstractions;
using NoviCode.Core.Data;
using NoviCode.Core.Domain;
using NoviCode.Core.Errors;
using NoviCode.Core.Services;
using NoviCode.Core.Utils;
using Shouldly;

namespace NoviCode.Core.Tests;

public class WalletFacadeTests
{
    private readonly Faker _faker;
    private readonly Mock<IWalletService> _walletServiceMock;
    private readonly Mock<ICurrencyConverter> _currencyConverterMock;
    private readonly Mock<IExchangeRatesService> _exchangeRatesServiceMock;
    private readonly Wallet _defaultWallet;
    private readonly CreateWalletRequest _createWalletRequest;
    private readonly AdjustBalanceRequest _adjustBalanceRequest;
    
    
    // System under test
    private readonly WalletFacade _sut;

    public WalletFacadeTests()
    {
        _faker                  = new Faker();
        _walletServiceMock      = new Mock<IWalletService>();
        _currencyConverterMock  = new Mock<ICurrencyConverter>();
        _exchangeRatesServiceMock = new Mock<IExchangeRatesService>();
        var loggerMock = new Mock<ILogger<WalletFacade>>();

        _sut = new WalletFacade(
            _walletServiceMock.Object,
            _currencyConverterMock.Object,
            loggerMock.Object,
            _exchangeRatesServiceMock.Object
        );
        
        _defaultWallet = new Wallet
        {
            Id       = _faker.Random.Long(1),
            Balance  = _faker.Finance.Amount(),
            Currency = _faker.Finance.Currency().Code
        };
        _createWalletRequest = new CreateWalletRequest
        {
            StartingBalance = _faker.Finance.Amount(),
            Currency       = _faker.Finance.Currency().Code
        };
        _adjustBalanceRequest = new AdjustBalanceRequest(_faker.Finance.Currency().Code, _faker.Random.Long(1),
            Strategy.AddFunds, _faker.Finance.Amount());
    }

    [Fact]
    public async Task GetWalletAsync_WalletNotFound_ReturnsNotFoundResult()
    {
        // Arrange
        _walletServiceMock
            .Setup(s => s.GetWalletAsync(It.IsAny<long>()))
            .ReturnsAsync((Wallet?)null);
        
        // Act
        var result = await _sut.GetWalletAsync(_faker.Random.Long(1), _faker.Finance.Currency().Code);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldContain(e => e is NotFoundError);
    }

    [Fact]
    public async Task GetWalletAsync_NoRequestedCurrency_ReturnsOriginalBalanceAndCurrency()
    {
        // Arrange
        _walletServiceMock
            .Setup(s => s.GetWalletAsync(_defaultWallet.Id))
            .ReturnsAsync(_defaultWallet);
        var expectedDto = new WalletDto()
        {
            Id = _defaultWallet.Id,
            Balance = _defaultWallet.Balance,
            Currency = _defaultWallet.Currency
        };

        // Act
        var actual = await _sut.GetWalletAsync(_defaultWallet.Id, null);

        // Assert
        actual.IsSuccess.ShouldBeTrue();
        var dto = actual.Value;
        dto.Id.ShouldBe(expectedDto.Id);
        dto.Balance.ShouldBe(expectedDto.Balance);
        dto.Currency.ShouldBe(expectedDto.Currency);
    }

    [Fact]
    public async Task GetWalletAsync_WithRequestedCurrency_AppliesConversion()
    {
        // Arrange
        var targetCurrency   = _faker.Finance.Currency().Code;
        var convertedBalance = _faker.Finance.Amount();

        _walletServiceMock
            .Setup(s => s.GetWalletAsync(_defaultWallet.Id))
            .ReturnsAsync(_defaultWallet);

        _currencyConverterMock
            .Setup(c => c.ConvertAsync(_defaultWallet.Balance, _defaultWallet.Currency, targetCurrency))
            .ReturnsAsync(convertedBalance);

        // Act
        var actual = await _sut.GetWalletAsync(_defaultWallet.Id, targetCurrency);

        // Assert
        actual.IsSuccess.ShouldBeTrue();
        var dto = actual.Value;
        dto.Balance.ShouldBe(convertedBalance);
        dto.Currency.ShouldBe(targetCurrency);
        _currencyConverterMock.Verify(c => c.ConvertAsync(_defaultWallet.Balance, _defaultWallet.Currency, targetCurrency), Times.Once);
    }

    [Fact]
    public async Task AdjustBalanceAsync_WalletNotFound_ReturnsNotFoundResult()
    {
        // Arrange
        _walletServiceMock
            .Setup(s => s.GetWalletAsync(It.IsAny<long>()))
            .ReturnsAsync((Wallet?)null);

        // Act
        var result = await _sut.AdjustBalanceAsync(_adjustBalanceRequest);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldContain(e => e is NotFoundError);
    }

    [Fact]
    public async Task AdjustBalanceAsync_ValidRequest_ConvertsAndAdjustsBalance()
    {
        // Arrange
        var request = _adjustBalanceRequest;
        var convertedAmount = _faker.Finance.Amount(5, 50);

        _walletServiceMock
            .Setup(s => s.GetWalletAsync(request.WalletId))
            .ReturnsAsync(_defaultWallet);

        _currencyConverterMock
            .Setup(c => c.ConvertAsync(request.Amount, _defaultWallet.Currency, request.Currency))
            .ReturnsAsync(convertedAmount);

        _walletServiceMock
            .Setup(s => s.AdjustBalanceAsync(_defaultWallet.Id, convertedAmount, request.Strategy))
            .ReturnsAsync(_defaultWallet);

        // Act
        var actual = await _sut.AdjustBalanceAsync(request);

        // Assert
        actual.IsSuccess.ShouldBeTrue();
        _currencyConverterMock.Verify(c => c.ConvertAsync(request.Amount, _defaultWallet.Currency, request.Currency), Times.Once);
        _walletServiceMock.Verify(s => s.AdjustBalanceAsync(_defaultWallet.Id, convertedAmount, request.Strategy), Times.Once);
    }

    [Fact]
    public async Task CreateWalletAsync_Failure_ReturnsError()
    {
        // Arrange
        _walletServiceMock
            .Setup(s => s.CreateWalletAsync(_createWalletRequest))
            .ReturnsAsync((Wallet?)null);
        _exchangeRatesServiceMock.Setup(s => s.GetExchangeRate(It.IsAny<string>()))
            .ReturnsAsync(new ExchangeRate());

        // Act
        var actual = await _sut.CreateWalletAsync(_createWalletRequest);

        // Assert
        actual.IsFailed.ShouldBeTrue();
        actual.Errors.ShouldContain(e => e is Error);
    }

    [Fact]
    public async Task CreateWalletAsync_Success_ReturnsDto()
    {
        // Arrange
        var expected = _defaultWallet.ToDto();

        _walletServiceMock
            .Setup(s => s.CreateWalletAsync(_createWalletRequest))
            .ReturnsAsync(_defaultWallet);
        _exchangeRatesServiceMock.Setup(s => s.GetExchangeRate(It.IsAny<string>()))
            .ReturnsAsync(new ExchangeRate());

        // Act
        var actual = await _sut.CreateWalletAsync(_createWalletRequest);

        // Assert
        actual.IsSuccess.ShouldBeTrue();
        actual.Value.ShouldBeEquivalentTo(expected);
    }
    
    [Fact]
    public async Task CreateWalletAsync_InvalidCurrency_ReturnsError()
    {
        // Arrange
        var expected = _defaultWallet.ToDto();

        _walletServiceMock
            .Setup(s => s.CreateWalletAsync(_createWalletRequest))
            .ReturnsAsync(_defaultWallet);
        _exchangeRatesServiceMock.Setup(s => s.GetExchangeRate(It.IsAny<string>()))
            .ReturnsAsync((ExchangeRate?)null);

        // Act
        var actual = await _sut.CreateWalletAsync(_createWalletRequest);

        // Assert
        actual.IsSuccess.ShouldBeFalse();
        actual.Errors.ShouldContain(e => e is ValidationError);
    }
}