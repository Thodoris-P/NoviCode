using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NoviCode.Core.Abstractions;
using NoviCode.Core.Domain;
using NoviCode.Data.Data;

namespace NoviCode.Core.Services;

public class ExchangeRatesRepository : IExchangeRatesRepository
{
    private readonly NoviCodeContext  _context;

    public ExchangeRatesRepository(NoviCodeContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }
    
    public async Task UpdateRates(IEnumerable<ExchangeRate> rates)
    {
        // 1. Build a DataTable for the TVP
        var table = new DataTable();
        table.Columns.Add("Currency", typeof(string));
        table.Columns.Add("EffectiveAt", typeof(DateTime));
        table.Columns.Add("Rate", typeof(decimal));

        foreach (var rate in rates)
        {
            table.Rows.Add(rate.Currency, rate.EffectiveAt, rate.Rate);
        }

        // 2. Configure the table‐valued parameter (ensure you have created a SQL type dbo.ExchangeRateType)
        var param = new SqlParameter("@Rates", table)
        {
            TypeName = "dbo.ExchangeRateType",
            SqlDbType = SqlDbType.Structured
        };

        // 3. MERGE statement to upsert
        const string sql = @"
MERGE dbo.ExchangeRates AS target
USING @Rates AS source
  ON target.Currency = source.Currency
  AND target.EffectiveAt = source.EffectiveAt
WHEN MATCHED THEN
  UPDATE SET
    Rate = source.Rate
WHEN NOT MATCHED BY TARGET THEN
  INSERT (Currency, EffectiveAt, Rate)
  VALUES (source.Currency, source.EffectiveAt, source.Rate);
";

        // 4. Execute
        await _context.Database.ExecuteSqlRawAsync(sql, param);
    }

    public async Task<ExchangeRate?> GetExchangeRate(string currency)
    {
        return await _context.ExchangeRates
            .Where(r => r.Currency == currency)
            .OrderByDescending(r => r.EffectiveAt)
            .FirstOrDefaultAsync();
    }
}