using Bogus;
using Moq;
using NoviCode.Core.Abstractions;
using NoviCode.Core.Domain;
using NoviCode.Core.Services;
using Shouldly;

namespace NoviCode.Core.Tests;

public class CurrencyConverterTests
{
    private readonly CurrencyConverter _sut;
    private readonly Mock<IExchangeRatesRepository> _providerMock;
    private readonly string _fromCurrency;
    private readonly string _toCurrency;
    private readonly decimal _fromRate;
    private readonly decimal _toRate;
    private readonly decimal _amount;

    public CurrencyConverterTests()
    {
        var faker = new Faker();
        _providerMock = new Mock<IExchangeRatesRepository>(MockBehavior.Strict);
        _sut = new CurrencyConverter(_providerMock.Object);
        _fromCurrency = faker.Finance.Currency().Code;
        _toCurrency = faker.Finance.Currency().Code;
        _fromRate = faker.Random.Decimal();
        _toRate = faker.Random.Decimal();
        _amount = faker.Random.Decimal();
    }

    [Fact]
    public async Task ConvertAsync_SameCurrency_ReturnsOriginalAmount()
    {
        // Arrange
        
        // Act
        var result = await _sut.ConvertAsync(_amount, _fromCurrency, _fromCurrency);

        // Assert
        result.ShouldBe(_amount);
        _providerMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ConvertAsync_DifferentCurrencies_ReturnsExpectedConversion()
    {
        // Arrange
        var fromRateInfo = new ExchangeRate { Currency = _fromCurrency, Rate = _fromRate };
        var toRateInfo = new ExchangeRate { Currency = _toCurrency, Rate = _toRate };

        _providerMock.Setup(r => r.GetExchangeRate(_fromCurrency)).ReturnsAsync(fromRateInfo);
        _providerMock.Setup(r => r.GetExchangeRate(_toCurrency)).ReturnsAsync(toRateInfo);

        // Act
        var result = await _sut.ConvertAsync(_amount, _fromCurrency, _toCurrency);

        // Assert
        var expected = _amount / _fromRate * _toRate;
        result.ShouldBe(expected);
        _providerMock.Verify(r => r.GetExchangeRate(_fromCurrency), Times.Once);
        _providerMock.Verify(r => r.GetExchangeRate(_toCurrency), Times.Once);
        _providerMock.VerifyNoOtherCalls();
    }
}