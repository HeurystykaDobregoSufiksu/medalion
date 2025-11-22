using Medalion.Data.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Medalion.Data.Configurations;

public class TradeConfiguration : IEntityTypeConfiguration<Trade>
{
    public void Configure(EntityTypeBuilder<Trade> builder)
    {
        builder.ToTable("Trades");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.ExternalTradeId)
            .HasMaxLength(100);

        builder.Property(t => t.Symbol)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(t => t.TradeType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(t => t.TradeSide)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(t => t.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(t => t.Quantity).HasPrecision(18, 8);
        builder.Property(t => t.Price).HasPrecision(18, 8);
        builder.Property(t => t.TotalValue).HasPrecision(18, 8);
        builder.Property(t => t.Commission).HasPrecision(18, 8);

        builder.Property(t => t.AlpacaMarketDataSnapshot)
            .HasColumnType("nvarchar(max)");

        builder.Property(t => t.PolymarketDataSnapshot)
            .HasColumnType("nvarchar(max)");

        builder.Property(t => t.Notes)
            .HasMaxLength(2000);

        builder.Property(t => t.Metadata)
            .HasColumnType("nvarchar(max)");

        // Indexes
        builder.HasIndex(t => t.ExternalTradeId);
        builder.HasIndex(t => new { t.StrategyId, t.ExecutedAt });
        builder.HasIndex(t => new { t.AssetId, t.ExecutedAt });
        builder.HasIndex(t => new { t.PolymarketEventId, t.ExecutedAt });
        builder.HasIndex(t => t.Symbol);
        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.ExecutedAt);

        builder.HasQueryFilter(t => !t.IsDeleted);
    }
}

public class PositionConfiguration : IEntityTypeConfiguration<Position>
{
    public void Configure(EntityTypeBuilder<Position> builder)
    {
        builder.ToTable("Positions");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Symbol)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.PositionSide)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(p => p.Quantity).HasPrecision(18, 8);
        builder.Property(p => p.RemainingQuantity).HasPrecision(18, 8);
        builder.Property(p => p.AverageEntryPrice).HasPrecision(18, 8);
        builder.Property(p => p.CurrentPrice).HasPrecision(18, 8);
        builder.Property(p => p.CostBasis).HasPrecision(18, 8);
        builder.Property(p => p.MarketValue).HasPrecision(18, 8);
        builder.Property(p => p.UnrealizedPnL).HasPrecision(18, 8);
        builder.Property(p => p.RealizedPnL).HasPrecision(18, 8);
        builder.Property(p => p.TotalCommissions).HasPrecision(18, 8);
        builder.Property(p => p.StopLoss).HasPrecision(18, 8);
        builder.Property(p => p.TakeProfit).HasPrecision(18, 8);

        builder.Property(p => p.AlpacaOpenSnapshot)
            .HasColumnType("nvarchar(max)");

        builder.Property(p => p.PolymarketOpenSnapshot)
            .HasColumnType("nvarchar(max)");

        builder.Property(p => p.AlpacaCloseSnapshot)
            .HasColumnType("nvarchar(max)");

        builder.Property(p => p.PolymarketCloseSnapshot)
            .HasColumnType("nvarchar(max)");

        builder.Property(p => p.Notes)
            .HasMaxLength(2000);

        builder.Property(p => p.Metadata)
            .HasColumnType("nvarchar(max)");

        // Indexes
        builder.HasIndex(p => new { p.AssetId, p.Status });
        builder.HasIndex(p => new { p.PolymarketEventId, p.Status });
        builder.HasIndex(p => p.Symbol);
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.OpenedAt);
        builder.HasIndex(p => p.ClosedAt);

        // Relationships
        builder.HasMany(p => p.Trades)
            .WithOne(t => t.Position)
            .HasForeignKey(t => t.PositionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}
