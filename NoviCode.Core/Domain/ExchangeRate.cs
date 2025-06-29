namespace NoviCode.Core.Domain;

public class ExchangeRate
{
    /// <summary>
    /// ISO 4217 currency code (e.g. “USD”, “GBP”)
    /// </summary>
    public string Currency { get; set; } = null!;

    /// <summary>
    /// The date (or timestamp) this rate applies to.
    /// If you only care about daily rates, you can store the “Date” (with time = 00:00:00);
    /// if you need intra-day granularity, use a full DateTime.
    /// </summary>
    public DateTime EffectiveAt { get; set; }

    /// <summary>
    /// The rate expressed as 1 base-unit → X foreign units.
    /// If your base is EUR, this is “1 EUR = Rate Currency”.
    /// </summary>
    public decimal Rate { get; set; }
}