using Medalion.Data.Domain;
using Microsoft.EntityFrameworkCore;

namespace Medalion.Data.Repositories;

/// <summary>
/// Repository implementation for Trade entity
/// </summary>
public class TradeRepository : Repository<Trade>, ITradeRepository
{
    public TradeRepository(TradingBotDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Trade>> GetTradesByAssetAsync(
        Guid assetId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.AssetId == assetId)
            .OrderByDescending(t => t.ExecutedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Trade>> GetTradesByPolymarketEventAsync(
        Guid eventId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.PolymarketEventId == eventId)
            .OrderByDescending(t => t.ExecutedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Trade>> GetTradesByStrategyAsync(
        Guid strategyId,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.StrategyId == strategyId)
            .OrderByDescending(t => t.ExecutedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Trade>> GetTradesByDateRangeAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.ExecutedAt >= startDate && t.ExecutedAt <= endDate)
            .OrderByDescending(t => t.ExecutedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Trade>> GetTradesByStatusAsync(
        TradeStatus status,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.Status == status)
            .OrderByDescending(t => t.ExecutedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Trade>> GetProfitableTradesAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(t => t.Status == TradeStatus.Filled &&
                       t.Position != null &&
                       t.Position.RealizedPnL != null &&
                       t.Position.RealizedPnL > 0)
            .OrderByDescending(t => t.ExecutedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<TradeStatistics> GetTradeStatisticsAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var trades = await _dbSet
            .Where(t => t.ExecutedAt >= startDate &&
                       t.ExecutedAt <= endDate &&
                       t.Status == TradeStatus.Filled)
            .Include(t => t.Position)
            .ToListAsync(cancellationToken);

        var closedPositions = trades
            .Where(t => t.Position != null && t.Position.Status == PositionStatus.Closed)
            .Select(t => t.Position!)
            .Distinct()
            .ToList();

        var winningTrades = closedPositions.Count(p => p.RealizedPnL > 0);
        var losingTrades = closedPositions.Count(p => p.RealizedPnL < 0);

        var totalProfit = closedPositions.Where(p => p.RealizedPnL > 0).Sum(p => p.RealizedPnL ?? 0);
        var totalLoss = Math.Abs(closedPositions.Where(p => p.RealizedPnL < 0).Sum(p => p.RealizedPnL ?? 0));

        return new TradeStatistics
        {
            TotalTrades = closedPositions.Count,
            WinningTrades = winningTrades,
            LosingTrades = losingTrades,
            TotalProfit = totalProfit,
            TotalLoss = totalLoss,
            AverageProfit = winningTrades > 0 ? totalProfit / winningTrades : 0,
            AverageLoss = losingTrades > 0 ? totalLoss / losingTrades : 0,
            TotalCommissions = trades.Sum(t => t.Commission)
        };
    }
}
