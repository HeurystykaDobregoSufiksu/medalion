using Medalion.Data.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Medalion.Data.Configurations;

public class StrategyConfiguration : IEntityTypeConfiguration<Strategy>
{
    public void Configure(EntityTypeBuilder<Strategy> builder)
    {
        builder.ToTable("Strategies");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Description)
            .HasMaxLength(1000);

        builder.Property(s => s.StrategyType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.Version)
            .HasMaxLength(20);

        builder.Property(s => s.Configuration)
            .HasColumnType("nvarchar(max)");

        builder.Property(s => s.RiskParameters)
            .HasColumnType("nvarchar(max)");

        builder.Property(s => s.PerformanceMetrics)
            .HasColumnType("nvarchar(max)");

        builder.Property(s => s.MaxPositionSize).HasPrecision(18, 8);
        builder.Property(s => s.MaxLossPerTrade).HasPrecision(18, 8);
        builder.Property(s => s.MaxDailyLoss).HasPrecision(18, 8);

        builder.HasIndex(s => s.Name);
        builder.HasIndex(s => s.StrategyType);
        builder.HasIndex(s => s.IsActive);

        builder.HasMany(s => s.Signals)
            .WithOne(sig => sig.Strategy)
            .HasForeignKey(sig => sig.StrategyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Trades)
            .WithOne(t => t.Strategy)
            .HasForeignKey(t => t.StrategyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(s => !s.IsDeleted);
    }
}

public class TradingSignalConfiguration : IEntityTypeConfiguration<TradingSignal>
{
    public void Configure(EntityTypeBuilder<TradingSignal> builder)
    {
        builder.ToTable("TradingSignals");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.SignalType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(s => s.Confidence).HasPrecision(18, 8);
        builder.Property(s => s.TargetPrice).HasPrecision(18, 8);
        builder.Property(s => s.StopLoss).HasPrecision(18, 8);
        builder.Property(s => s.TakeProfit).HasPrecision(18, 8);
        builder.Property(s => s.SuggestedQuantity).HasPrecision(18, 8);

        builder.Property(s => s.SourceData)
            .HasColumnType("nvarchar(max)");

        builder.Property(s => s.Metadata)
            .HasColumnType("nvarchar(max)");

        builder.HasIndex(s => new { s.StrategyId, s.SignalTimestamp });
        builder.HasIndex(s => s.AssetId);
        builder.HasIndex(s => s.PolymarketEventId);
        builder.HasIndex(s => s.SignalType);
        builder.HasIndex(s => s.WasActedUpon);

        builder.HasMany(s => s.Trades)
            .WithOne(t => t.Signal)
            .HasForeignKey(t => t.SignalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(s => !s.IsDeleted);
    }
}
