using Medalion.Data.Domain;
using Microsoft.EntityFrameworkCore;

namespace Medalion.Data.Repositories;

/// <summary>
/// Repository implementation for Position entity
/// </summary>
public class PositionRepository : Repository<Position>, IPositionRepository
{
    public PositionRepository(TradingBotDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Position>> GetOpenPositionsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.Status == PositionStatus.Open)
            .Include(p => p.Asset)
            .Include(p => p.PolymarketEvent)
            .OrderByDescending(p => p.OpenedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Position>> GetClosedPositionsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.Status == PositionStatus.Closed)
            .Include(p => p.Asset)
            .Include(p => p.PolymarketEvent)
            .OrderByDescending(p => p.ClosedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Position>> GetPositionsByAssetAsync(
        Guid assetId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.AssetId == assetId)
            .Include(p => p.Asset)
            .OrderByDescending(p => p.OpenedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Position>> GetPositionsByPolymarketEventAsync(
        Guid eventId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.PolymarketEventId == eventId)
            .Include(p => p.PolymarketEvent)
            .OrderByDescending(p => p.OpenedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Position>> GetPositionsByStatusAsync(
        PositionStatus status,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.Status == status)
            .OrderByDescending(p => p.OpenedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Position?> GetPositionWithTradesAsync(
        Guid positionId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Trades)
            .Include(p => p.Asset)
            .Include(p => p.PolymarketEvent)
            .FirstOrDefaultAsync(p => p.Id == positionId, cancellationToken);
    }

    public async Task<IEnumerable<Position>> GetPositionsByDateRangeAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.OpenedAt >= startDate && p.OpenedAt <= endDate)
            .Include(p => p.Asset)
            .Include(p => p.PolymarketEvent)
            .OrderByDescending(p => p.OpenedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<PositionPerformanceMetrics> GetPerformanceMetricsAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var positions = await _dbSet
            .Where(p => p.OpenedAt >= startDate && p.OpenedAt <= endDate)
            .ToListAsync(cancellationToken);

        var closedPositions = positions.Where(p => p.Status == PositionStatus.Closed).ToList();
        var openPositions = positions.Where(p => p.Status == PositionStatus.Open).ToList();

        return new PositionPerformanceMetrics
        {
            TotalPositions = positions.Count,
            OpenPositions = openPositions.Count,
            ClosedPositions = closedPositions.Count,
            TotalRealizedPnL = closedPositions.Sum(p => p.RealizedPnL ?? 0),
            TotalUnrealizedPnL = openPositions.Sum(p => p.UnrealizedPnL ?? 0),
            AverageRealizedPnL = closedPositions.Any() ?
                closedPositions.Average(p => p.RealizedPnL ?? 0) : 0,
            WinningPositionsCount = closedPositions.Count(p => (p.RealizedPnL ?? 0) > 0),
            LosingPositionsCount = closedPositions.Count(p => (p.RealizedPnL ?? 0) < 0),
            TotalCommissions = positions.Sum(p => p.TotalCommissions)
        };
    }

    public async Task UpdatePositionMarketValuesAsync(
        Guid positionId,
        decimal currentPrice,
        CancellationToken cancellationToken = default)
    {
        var position = await GetByIdAsync(positionId, cancellationToken);
        if (position == null || position.Status != PositionStatus.Open)
            return;

        position.CurrentPrice = currentPrice;
        position.MarketValue = position.RemainingQuantity * currentPrice;

        // Calculate unrealized P&L
        if (position.PositionSide == PositionSide.Long)
        {
            position.UnrealizedPnL = (currentPrice - position.AverageEntryPrice) * position.RemainingQuantity;
        }
        else // Short
        {
            position.UnrealizedPnL = (position.AverageEntryPrice - currentPrice) * position.RemainingQuantity;
        }

        await UpdateAsync(position, cancellationToken);
    }
}
