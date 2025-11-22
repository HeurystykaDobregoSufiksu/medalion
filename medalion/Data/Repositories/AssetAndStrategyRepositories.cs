using Medalion.Data.Domain;
using Microsoft.EntityFrameworkCore;

namespace Medalion.Data.Repositories;

#region Asset Repository

/// <summary>
/// Repository interface for Asset entity
/// </summary>
public interface IAssetRepository : IRepository<Asset>
{
    Task<Asset?> GetBySymbolAsync(string symbol, CancellationToken cancellationToken = default);
    Task<IEnumerable<Asset>> GetByAssetTypeAsync(AssetType assetType, CancellationToken cancellationToken = default);
    Task<IEnumerable<Asset>> GetActiveAssetsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Asset>> GetTradableAssetsAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository implementation for Asset entity
/// </summary>
public class AssetRepository : Repository<Asset>, IAssetRepository
{
    public AssetRepository(TradingBotDbContext context) : base(context)
    {
    }

    public async Task<Asset?> GetBySymbolAsync(
        string symbol,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(a => a.Symbol == symbol, cancellationToken);
    }

    public async Task<IEnumerable<Asset>> GetByAssetTypeAsync(
        AssetType assetType,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.AssetType == assetType)
            .OrderBy(a => a.Symbol)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Asset>> GetActiveAssetsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.IsActive)
            .OrderBy(a => a.Symbol)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Asset>> GetTradableAssetsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.IsActive && a.IsTradable)
            .OrderBy(a => a.Symbol)
            .ToListAsync(cancellationToken);
    }
}

#endregion

#region Strategy Repository

/// <summary>
/// Repository interface for Strategy entity
/// </summary>
public interface IStrategyRepository : IRepository<Strategy>
{
    Task<IEnumerable<Strategy>> GetActiveStrategiesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Strategy>> GetByStrategyTypeAsync(string strategyType, CancellationToken cancellationToken = default);
    Task<Strategy?> GetStrategyWithSignalsAsync(Guid strategyId, CancellationToken cancellationToken = default);
    Task<Strategy?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository implementation for Strategy entity
/// </summary>
public class StrategyRepository : Repository<Strategy>, IStrategyRepository
{
    public StrategyRepository(TradingBotDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Strategy>> GetActiveStrategiesAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Strategy>> GetByStrategyTypeAsync(
        string strategyType,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.StrategyType == strategyType)
            .OrderBy(s => s.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Strategy?> GetStrategyWithSignalsAsync(
        Guid strategyId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.Signals)
            .FirstOrDefaultAsync(s => s.Id == strategyId, cancellationToken);
    }

    public async Task<Strategy?> GetByNameAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(s => s.Name == name, cancellationToken);
    }
}

#endregion

#region Trading Signal Repository

/// <summary>
/// Repository interface for TradingSignal entity
/// </summary>
public interface ITradingSignalRepository : IRepository<TradingSignal>
{
    Task<IEnumerable<TradingSignal>> GetActiveSignalsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<TradingSignal>> GetSignalsByStrategyAsync(Guid strategyId, CancellationToken cancellationToken = default);
    Task<IEnumerable<TradingSignal>> GetUnactedSignalsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<TradingSignal>> GetSignalsByAssetAsync(Guid assetId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository implementation for TradingSignal entity
/// </summary>
public class TradingSignalRepository : Repository<TradingSignal>, ITradingSignalRepository
{
    public TradingSignalRepository(TradingBotDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<TradingSignal>> GetActiveSignalsAsync(
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _dbSet
            .Where(s => !s.WasActedUpon &&
                       (s.ExpiresAt == null || s.ExpiresAt > now))
            .OrderByDescending(s => s.SignalTimestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TradingSignal>> GetSignalsByStrategyAsync(
        Guid strategyId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.StrategyId == strategyId)
            .OrderByDescending(s => s.SignalTimestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TradingSignal>> GetUnactedSignalsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => !s.WasActedUpon)
            .OrderByDescending(s => s.SignalTimestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TradingSignal>> GetSignalsByAssetAsync(
        Guid assetId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.AssetId == assetId)
            .OrderByDescending(s => s.SignalTimestamp)
            .ToListAsync(cancellationToken);
    }
}

#endregion

#region Polymarket Event Repository

/// <summary>
/// Repository interface for PolymarketEventData entity
/// </summary>
public interface IPolymarketEventRepository : IRepository<PolymarketEventData>
{
    Task<PolymarketEventData?> GetByEventIdAsync(string eventId, CancellationToken cancellationToken = default);
    Task<IEnumerable<PolymarketEventData>> GetMonitoredEventsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<PolymarketEventData>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default);
    Task<PolymarketEventData?> GetEventWithMarketsAsync(Guid id, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository implementation for PolymarketEventData entity
/// </summary>
public class PolymarketEventRepository : Repository<PolymarketEventData>, IPolymarketEventRepository
{
    public PolymarketEventRepository(TradingBotDbContext context) : base(context)
    {
    }

    public async Task<PolymarketEventData?> GetByEventIdAsync(
        string eventId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(e => e.EventId == eventId, cancellationToken);
    }

    public async Task<IEnumerable<PolymarketEventData>> GetMonitoredEventsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(e => e.IsMonitored && e.IsActive)
            .OrderByDescending(e => e.EventCreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<PolymarketEventData>> GetByCategoryAsync(
        string category,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(e => e.Category == category)
            .OrderByDescending(e => e.EventCreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<PolymarketEventData?> GetEventWithMarketsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(e => e.Markets)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }
}

#endregion
