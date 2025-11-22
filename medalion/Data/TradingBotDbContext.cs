using Medalion.Data.Domain;
using Medalion.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Medalion.Data;

/// <summary>
/// Main database context for the trading bot system
/// </summary>
public class TradingBotDbContext : DbContext
{
    public TradingBotDbContext(DbContextOptions<TradingBotDbContext> options)
        : base(options)
    {
    }

    #region Asset DbSets

    public DbSet<Asset> Assets => Set<Asset>();

    #endregion

    #region Alpaca Market Data DbSets

    public DbSet<StockQuoteSnapshot> StockQuoteSnapshots => Set<StockQuoteSnapshot>();
    public DbSet<StockBarData> StockBarData => Set<StockBarData>();
    public DbSet<CryptoQuoteSnapshot> CryptoQuoteSnapshots => Set<CryptoQuoteSnapshot>();
    public DbSet<CryptoBarData> CryptoBarData => Set<CryptoBarData>();
    public DbSet<OptionContractData> OptionContracts => Set<OptionContractData>();
    public DbSet<OptionQuoteSnapshot> OptionQuoteSnapshots => Set<OptionQuoteSnapshot>();

    #endregion

    #region Polymarket DbSets

    public DbSet<PolymarketEventData> PolymarketEvents => Set<PolymarketEventData>();
    public DbSet<PolymarketMarketData> PolymarketMarkets => Set<PolymarketMarketData>();
    public DbSet<PolymarketSnapshot> PolymarketSnapshots => Set<PolymarketSnapshot>();
    public DbSet<PolymarketTradeData> PolymarketTradeData => Set<PolymarketTradeData>();
    public DbSet<PolymarketOrderBookSnapshot> PolymarketOrderBookSnapshots => Set<PolymarketOrderBookSnapshot>();

    #endregion

    #region Strategy and Signal DbSets

    public DbSet<Strategy> Strategies => Set<Strategy>();
    public DbSet<TradingSignal> TradingSignals => Set<TradingSignal>();

    #endregion

    #region Trading DbSets

    public DbSet<Trade> Trades => Set<Trade>();
    public DbSet<Position> Positions => Set<Position>();

    #endregion

    #region Logging DbSets

    public DbSet<ApplicationLog> ApplicationLogs => Set<ApplicationLog>();
    public DbSet<ErrorLog> ErrorLogs => Set<ErrorLog>();
    public DbSet<PerformanceMetric> PerformanceMetrics => Set<PerformanceMetric>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TradingBotDbContext).Assembly);

        // Configure global query filter for soft delete
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                var property = System.Linq.Expressions.Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
                var filterExpression = System.Linq.Expressions.Expression.Lambda(
                    System.Linq.Expressions.Expression.Not(property),
                    parameter);

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filterExpression);
            }
        }

        // Additional global configurations
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Set default datetime kind to UTC for all DateTime properties
            var dateTimeProperties = entityType.ClrType.GetProperties()
                .Where(p => p.PropertyType == typeof(DateTime) || p.PropertyType == typeof(DateTime?));

            foreach (var property in dateTimeProperties)
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property(property.Name)
                    .HasConversion(
                        v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : (DateTime?)null,
                        v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : (DateTime?)null
                    );
            }
        }
    }

    /// <summary>
    /// Override SaveChanges to automatically update timestamps
    /// </summary>
    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    /// <summary>
    /// Override SaveChangesAsync to automatically update timestamps
    /// </summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Automatically update CreatedAt and UpdatedAt timestamps
    /// </summary>
    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;

                case EntityState.Deleted:
                    // Implement soft delete
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }
    }
}
