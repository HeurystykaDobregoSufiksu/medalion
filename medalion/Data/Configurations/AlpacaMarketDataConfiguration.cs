using Medalion.Data.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Medalion.Data.Configurations;

public class StockQuoteSnapshotConfiguration : IEntityTypeConfiguration<StockQuoteSnapshot>
{
    public void Configure(EntityTypeBuilder<StockQuoteSnapshot> builder)
    {
        builder.ToTable("StockQuoteSnapshots");

        builder.HasKey(q => q.Id);

        builder.Property(q => q.Symbol)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(q => q.AskPrice).HasPrecision(18, 8);
        builder.Property(q => q.BidPrice).HasPrecision(18, 8);
        builder.Property(q => q.MidPrice).HasPrecision(18, 8);

        // Indexes for efficient querying
        builder.HasIndex(q => new { q.AssetId, q.QuoteTimestamp });
        builder.HasIndex(q => q.Symbol);
        builder.HasIndex(q => q.QuoteTimestamp);

        builder.HasQueryFilter(q => !q.IsDeleted);
    }
}

public class StockBarDataConfiguration : IEntityTypeConfiguration<StockBarData>
{
    public void Configure(EntityTypeBuilder<StockBarData> builder)
    {
        builder.ToTable("StockBarData");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Symbol)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(b => b.Timeframe)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(b => b.Open).HasPrecision(18, 8);
        builder.Property(b => b.High).HasPrecision(18, 8);
        builder.Property(b => b.Low).HasPrecision(18, 8);
        builder.Property(b => b.Close).HasPrecision(18, 8);
        builder.Property(b => b.VWAP).HasPrecision(18, 8);

        // Indexes for time-series queries
        builder.HasIndex(b => new { b.AssetId, b.Timeframe, b.BarTimestamp });
        builder.HasIndex(b => new { b.Symbol, b.BarTimestamp });

        builder.HasQueryFilter(b => !b.IsDeleted);
    }
}

public class CryptoQuoteSnapshotConfiguration : IEntityTypeConfiguration<CryptoQuoteSnapshot>
{
    public void Configure(EntityTypeBuilder<CryptoQuoteSnapshot> builder)
    {
        builder.ToTable("CryptoQuoteSnapshots");

        builder.HasKey(q => q.Id);

        builder.Property(q => q.Symbol)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(q => q.AskPrice).HasPrecision(18, 8);
        builder.Property(q => q.BidPrice).HasPrecision(18, 8);
        builder.Property(q => q.MidPrice).HasPrecision(18, 8);
        builder.Property(q => q.AskSize).HasPrecision(18, 8);
        builder.Property(q => q.BidSize).HasPrecision(18, 8);

        builder.HasIndex(q => new { q.AssetId, q.QuoteTimestamp });
        builder.HasIndex(q => q.Symbol);
        builder.HasIndex(q => q.QuoteTimestamp);

        builder.HasQueryFilter(q => !q.IsDeleted);
    }
}

public class CryptoBarDataConfiguration : IEntityTypeConfiguration<CryptoBarData>
{
    public void Configure(EntityTypeBuilder<CryptoBarData> builder)
    {
        builder.ToTable("CryptoBarData");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Symbol)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(b => b.Timeframe)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(b => b.Open).HasPrecision(18, 8);
        builder.Property(b => b.High).HasPrecision(18, 8);
        builder.Property(b => b.Low).HasPrecision(18, 8);
        builder.Property(b => b.Close).HasPrecision(18, 8);
        builder.Property(b => b.VWAP).HasPrecision(18, 8);
        builder.Property(b => b.Volume).HasPrecision(18, 8);

        builder.HasIndex(b => new { b.AssetId, b.Timeframe, b.BarTimestamp });
        builder.HasIndex(b => new { b.Symbol, b.BarTimestamp });

        builder.HasQueryFilter(b => !b.IsDeleted);
    }
}

public class OptionContractDataConfiguration : IEntityTypeConfiguration<OptionContractData>
{
    public void Configure(EntityTypeBuilder<OptionContractData> builder)
    {
        builder.ToTable("OptionContracts");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.ContractId)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(o => o.Symbol)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(o => o.UnderlyingSymbol)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(o => o.StrikePrice).HasPrecision(18, 8);
        builder.Property(o => o.Multiplier).HasPrecision(18, 8);

        builder.Property(o => o.Metadata)
            .HasColumnType("nvarchar(max)");

        builder.HasIndex(o => o.ContractId).IsUnique();
        builder.HasIndex(o => o.Symbol);
        builder.HasIndex(o => new { o.UnderlyingSymbol, o.ExpirationDate });

        builder.HasMany(o => o.OptionQuotes)
            .WithOne(q => q.OptionContract)
            .HasForeignKey(q => q.OptionContractId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(o => !o.IsDeleted);
    }
}

public class OptionQuoteSnapshotConfiguration : IEntityTypeConfiguration<OptionQuoteSnapshot>
{
    public void Configure(EntityTypeBuilder<OptionQuoteSnapshot> builder)
    {
        builder.ToTable("OptionQuoteSnapshots");

        builder.HasKey(q => q.Id);

        builder.Property(q => q.Symbol)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(q => q.BidPrice).HasPrecision(18, 8);
        builder.Property(q => q.AskPrice).HasPrecision(18, 8);
        builder.Property(q => q.MidPrice).HasPrecision(18, 8);
        builder.Property(q => q.ImpliedVolatility).HasPrecision(18, 8);
        builder.Property(q => q.Delta).HasPrecision(18, 8);
        builder.Property(q => q.Gamma).HasPrecision(18, 8);
        builder.Property(q => q.Theta).HasPrecision(18, 8);
        builder.Property(q => q.Vega).HasPrecision(18, 8);
        builder.Property(q => q.Rho).HasPrecision(18, 8);
        builder.Property(q => q.UnderlyingPrice).HasPrecision(18, 8);

        builder.HasIndex(q => new { q.OptionContractId, q.QuoteTimestamp });
        builder.HasIndex(q => q.QuoteTimestamp);

        builder.HasQueryFilter(q => !q.IsDeleted);
    }
}
