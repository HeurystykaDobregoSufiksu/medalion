using Medalion.Data.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Medalion.Data.Configurations;

public class PolymarketEventDataConfiguration : IEntityTypeConfiguration<PolymarketEventData>
{
    public void Configure(EntityTypeBuilder<PolymarketEventData> builder)
    {
        builder.ToTable("PolymarketEvents");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.EventId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Slug)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.Description)
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.Category)
            .HasMaxLength(100);

        builder.Property(e => e.Tags)
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.TotalVolume).HasPrecision(18, 8);
        builder.Property(e => e.TotalLiquidity).HasPrecision(18, 8);

        builder.Property(e => e.Metadata)
            .HasColumnType("nvarchar(max)");

        // Indexes
        builder.HasIndex(e => e.EventId).IsUnique();
        builder.HasIndex(e => e.Slug);
        builder.HasIndex(e => e.Category);
        builder.HasIndex(e => e.IsMonitored);

        // Relationships
        builder.HasMany(e => e.Markets)
            .WithOne(m => m.PolymarketEvent)
            .HasForeignKey(m => m.PolymarketEventId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Trades)
            .WithOne(t => t.PolymarketEvent)
            .HasForeignKey(t => t.PolymarketEventId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Positions)
            .WithOne(p => p.PolymarketEvent)
            .HasForeignKey(p => p.PolymarketEventId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}

public class PolymarketMarketDataConfiguration : IEntityTypeConfiguration<PolymarketMarketData>
{
    public void Configure(EntityTypeBuilder<PolymarketMarketData> builder)
    {
        builder.ToTable("PolymarketMarkets");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.MarketId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.ConditionId)
            .HasMaxLength(100);

        builder.Property(m => m.QuestionId)
            .HasMaxLength(100);

        builder.Property(m => m.Question)
            .HasMaxLength(1000);

        builder.Property(m => m.Outcomes)
            .HasColumnType("nvarchar(max)");

        builder.Property(m => m.OutcomePrices)
            .HasColumnType("nvarchar(max)");

        builder.Property(m => m.Volume).HasPrecision(18, 8);
        builder.Property(m => m.Liquidity).HasPrecision(18, 8);

        builder.Property(m => m.Metadata)
            .HasColumnType("nvarchar(max)");

        builder.HasIndex(m => m.MarketId);
        builder.HasIndex(m => new { m.PolymarketEventId, m.MarketId });

        builder.HasMany(m => m.Snapshots)
            .WithOne(s => s.Market)
            .HasForeignKey(s => s.PolymarketMarketId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(m => !m.IsDeleted);
    }
}

public class PolymarketSnapshotConfiguration : IEntityTypeConfiguration<PolymarketSnapshot>
{
    public void Configure(EntityTypeBuilder<PolymarketSnapshot> builder)
    {
        builder.ToTable("PolymarketSnapshots");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.MarketId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.AssetId)
            .HasMaxLength(100);

        builder.Property(s => s.Price).HasPrecision(18, 8);
        builder.Property(s => s.LastPrice).HasPrecision(18, 8);
        builder.Property(s => s.Bid).HasPrecision(18, 8);
        builder.Property(s => s.Ask).HasPrecision(18, 8);
        builder.Property(s => s.Spread).HasPrecision(18, 8);
        builder.Property(s => s.ImpliedVolatility).HasPrecision(18, 8);
        builder.Property(s => s.Volume24h).HasPrecision(18, 8);
        builder.Property(s => s.Liquidity).HasPrecision(18, 8);

        builder.Property(s => s.Outcome).HasMaxLength(200);
        builder.Property(s => s.Category).HasMaxLength(100);

        builder.Property(s => s.Tags)
            .HasColumnType("nvarchar(max)");

        // Indexes for time-series queries
        builder.HasIndex(s => new { s.PolymarketMarketId, s.SnapshotTimestamp });
        builder.HasIndex(s => new { s.MarketId, s.SnapshotTimestamp });
        builder.HasIndex(s => s.SnapshotTimestamp);

        builder.HasQueryFilter(s => !s.IsDeleted);
    }
}

public class PolymarketTradeDataConfiguration : IEntityTypeConfiguration<PolymarketTradeData>
{
    public void Configure(EntityTypeBuilder<PolymarketTradeData> builder)
    {
        builder.ToTable("PolymarketTradeData");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.TradeId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.MarketId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.AssetId)
            .HasMaxLength(100);

        builder.Property(t => t.Price).HasPrecision(18, 8);
        builder.Property(t => t.Size).HasPrecision(18, 8);

        builder.Property(t => t.Side).HasMaxLength(10);
        builder.Property(t => t.Outcome).HasMaxLength(200);

        builder.Property(t => t.MakerAddress).HasMaxLength(100);
        builder.Property(t => t.TakerAddress).HasMaxLength(100);

        builder.HasIndex(t => t.TradeId).IsUnique();
        builder.HasIndex(t => new { t.MarketId, t.TradeTimestamp });
        builder.HasIndex(t => t.TradeTimestamp);

        builder.HasQueryFilter(t => !t.IsDeleted);
    }
}

public class PolymarketOrderBookSnapshotConfiguration : IEntityTypeConfiguration<PolymarketOrderBookSnapshot>
{
    public void Configure(EntityTypeBuilder<PolymarketOrderBookSnapshot> builder)
    {
        builder.ToTable("PolymarketOrderBookSnapshots");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.AssetId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(o => o.MarketId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(o => o.Bids)
            .HasColumnType("nvarchar(max)");

        builder.Property(o => o.Asks)
            .HasColumnType("nvarchar(max)");

        builder.Property(o => o.TotalBidSize).HasPrecision(18, 8);
        builder.Property(o => o.TotalAskSize).HasPrecision(18, 8);

        builder.HasIndex(o => new { o.MarketId, o.SnapshotTimestamp });
        builder.HasIndex(o => o.SnapshotTimestamp);

        builder.HasQueryFilter(o => !o.IsDeleted);
    }
}
