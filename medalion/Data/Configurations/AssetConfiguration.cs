using Medalion.Data.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Medalion.Data.Configurations;

public class AssetConfiguration : IEntityTypeConfiguration<Asset>
{
    public void Configure(EntityTypeBuilder<Asset> builder)
    {
        builder.ToTable("Assets");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Symbol)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.AssetType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(a => a.AssetClass)
            .HasMaxLength(50);

        builder.Property(a => a.Exchange)
            .HasMaxLength(50);

        builder.Property(a => a.MinOrderQuantity)
            .HasPrecision(18, 8);

        builder.Property(a => a.Metadata)
            .HasColumnType("nvarchar(max)");

        // Indexes
        builder.HasIndex(a => a.Symbol)
            .IsUnique();

        builder.HasIndex(a => a.AssetType);
        builder.HasIndex(a => a.IsActive);

        // Relationships
        builder.HasMany(a => a.StockQuotes)
            .WithOne(q => q.Asset)
            .HasForeignKey(q => q.AssetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.StockBars)
            .WithOne(b => b.Asset)
            .HasForeignKey(b => b.AssetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.CryptoQuotes)
            .WithOne(q => q.Asset)
            .HasForeignKey(q => q.AssetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.CryptoBars)
            .WithOne(b => b.Asset)
            .HasForeignKey(b => b.AssetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.OptionContracts)
            .WithOne(o => o.Asset)
            .HasForeignKey(o => o.AssetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.Trades)
            .WithOne(t => t.Asset)
            .HasForeignKey(t => t.AssetId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(a => a.Positions)
            .WithOne(p => p.Asset)
            .HasForeignKey(p => p.AssetId)
            .OnDelete(DeleteBehavior.Restrict);

        // Query filter for soft delete
        builder.HasQueryFilter(a => !a.IsDeleted);
    }
}
