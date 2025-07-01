using NoviCode.Core.Domain;
using NoviCode.Gateway.Models;

namespace NoviCode.Gateway.Utils;

public static class EcbRatesToExchangeRatesMapper
{
    public static IEnumerable<ExchangeRate> Map(Envelope envelope)
    {
        DateTime date = envelope.Cube.DailyRates.First().Time;
        foreach (var rate in envelope.Cube.DailyRates.First().Rates)
        {
            var exchangeRate = new ExchangeRate
            {
                Rate = rate.Rate,
                Currency = rate.Currency,
                EffectiveAt = date
            };
            yield return exchangeRate;
        }
    }
}