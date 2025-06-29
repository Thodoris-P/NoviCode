using Microsoft.EntityFrameworkCore;
using NoviCode.Core.Domain;

namespace NoviCode.Data.Data;

public class NoviCodeContext : DbContext
{
    public DbSet<Wallet> Wallets { get; set; }
    public DbSet<ExchangeRate> ExchangeRates { get; set; }
    
    public NoviCodeContext(DbContextOptions<NoviCodeContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Wallet>(wallet =>
        {
            wallet.HasKey(e => e.Id);
            wallet.Property(e => e.Currency)
                .IsRequired();
            wallet.Property(e => e.Balance)
                .HasColumnType("decimal(18,4)");
        });

        modelBuilder.Entity<ExchangeRate>(exchangeRate =>
        {
            // Composite PK on (Currency, EffectiveAt)
            exchangeRate.HasKey(x => new { x.Currency, x.EffectiveAt });

            exchangeRate.Property(x => x.Currency)
                .HasMaxLength(3)
                .IsRequired();

            exchangeRate.Property(x => x.Rate)
                .HasColumnType("decimal(18,6)")
                .IsRequired();

            exchangeRate.Property(x => x.EffectiveAt)
                .IsRequired();
        });
    }
}