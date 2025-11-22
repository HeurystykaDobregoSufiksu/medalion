using Medalion.Data.Domain;
using Microsoft.EntityFrameworkCore;

namespace Medalion.Data.Repositories;

#region Stock Market Data Repositories

/// <summary>
/// Repository interface for StockBarData
/// </summary>
public interface IStockBarDataRepository : IRepository<StockBarData>
{
    Task<IEnumerable<StockBarData>> GetBarsByAssetAndTimeframeAsync(
        Guid assetId,
        string timeframe,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<StockBarData>> GetLatestBarsAsync(
        Guid assetId,
        string timeframe,
        int count,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository implementation for StockBarData
/// </summary>
public class StockBarDataRepository : Repository<StockBarData>, IStockBarDataRepository
{
    public StockBarDataRepository(TradingBotDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<StockBarData>> GetBarsByAssetAndTimeframeAsync(
        Guid assetId,
        string timeframe,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(b => b.AssetId == assetId &&
                       b.Timeframe == timeframe &&
                       b.BarTimestamp >= startDate &&
                       b.BarTimestamp <= endDate)
            .OrderBy(b => b.BarTimestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<StockBarData>> GetLatestBarsAsync(
        Guid assetId,
        string timeframe,
        int count,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(b => b.AssetId == assetId && b.Timeframe == timeframe)
            .OrderByDescending(b => b.BarTimestamp)
            .Take(count)
            .OrderBy(b => b.BarTimestamp)
            .ToListAsync(cancellationToken);
    }
}

/// <summary>
/// Repository interface for StockQuoteSnapshot
/// </summary>
public interface IStockQuoteSnapshotRepository : IRepository<StockQuoteSnapshot>
{
    Task<StockQuoteSnapshot?> GetLatestQuoteAsync(
        Guid assetId,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<StockQuoteSnapshot>> GetQuotesByDateRangeAsync(
        Guid assetId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository implementation for StockQuoteSnapshot
/// </summary>
public class StockQuoteSnapshotRepository : Repository<StockQuoteSnapshot>, IStockQuoteSnapshotRepository
{
    public StockQuoteSnapshotRepository(TradingBotDbContext context) : base(context)
    {
    }

    public async Task<StockQuoteSnapshot?> GetLatestQuoteAsync(
        Guid assetId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(q => q.AssetId == assetId)
            .OrderByDescending(q => q.QuoteTimestamp)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<StockQuoteSnapshot>> GetQuotesByDateRangeAsync(
        Guid assetId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(q => q.AssetId == assetId &&
                       q.QuoteTimestamp >= startDate &&
                       q.QuoteTimestamp <= endDate)
            .OrderBy(q => q.QuoteTimestamp)
            .ToListAsync(cancellationToken);
    }
}

#endregion

#region Crypto Market Data Repositories

/// <summary>
/// Repository interface for CryptoBarData
/// </summary>
public interface ICryptoBarDataRepository : IRepository<CryptoBarData>
{
    Task<IEnumerable<CryptoBarData>> GetBarsByAssetAndTimeframeAsync(
        Guid assetId,
        string timeframe,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<CryptoBarData>> GetLatestBarsAsync(
        Guid assetId,
        string timeframe,
        int count,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository implementation for CryptoBarData
/// </summary>
public class CryptoBarDataRepository : Repository<CryptoBarData>, ICryptoBarDataRepository
{
    public CryptoBarDataRepository(TradingBotDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<CryptoBarData>> GetBarsByAssetAndTimeframeAsync(
        Guid assetId,
        string timeframe,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(b => b.AssetId == assetId &&
                       b.Timeframe == timeframe &&
                       b.BarTimestamp >= startDate &&
                       b.BarTimestamp <= endDate)
            .OrderBy(b => b.BarTimestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<CryptoBarData>> GetLatestBarsAsync(
        Guid assetId,
        string timeframe,
        int count,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(b => b.AssetId == assetId && b.Timeframe == timeframe)
            .OrderByDescending(b => b.BarTimestamp)
            .Take(count)
            .OrderBy(b => b.BarTimestamp)
            .ToListAsync(cancellationToken);
    }
}

#endregion

#region Polymarket Data Repositories

/// <summary>
/// Repository interface for PolymarketSnapshot
/// </summary>
public interface IPolymarketSnapshotRepository : IRepository<PolymarketSnapshot>
{
    Task<PolymarketSnapshot?> GetLatestSnapshotAsync(
        Guid marketId,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<PolymarketSnapshot>> GetSnapshotsByDateRangeAsync(
        Guid marketId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<PolymarketSnapshot>> GetLatestSnapshotsAsync(
        Guid marketId,
        int count,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository implementation for PolymarketSnapshot
/// </summary>
public class PolymarketSnapshotRepository : Repository<PolymarketSnapshot>, IPolymarketSnapshotRepository
{
    public PolymarketSnapshotRepository(TradingBotDbContext context) : base(context)
    {
    }

    public async Task<PolymarketSnapshot?> GetLatestSnapshotAsync(
        Guid marketId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.PolymarketMarketId == marketId)
            .OrderByDescending(s => s.SnapshotTimestamp)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<PolymarketSnapshot>> GetSnapshotsByDateRangeAsync(
        Guid marketId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.PolymarketMarketId == marketId &&
                       s.SnapshotTimestamp >= startDate &&
                       s.SnapshotTimestamp <= endDate)
            .OrderBy(s => s.SnapshotTimestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<PolymarketSnapshot>> GetLatestSnapshotsAsync(
        Guid marketId,
        int count,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.PolymarketMarketId == marketId)
            .OrderByDescending(s => s.SnapshotTimestamp)
            .Take(count)
            .OrderBy(s => s.SnapshotTimestamp)
            .ToListAsync(cancellationToken);
    }
}

#endregion
