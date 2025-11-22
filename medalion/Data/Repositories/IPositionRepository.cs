using Medalion.Data.Domain;

namespace Medalion.Data.Repositories;

/// <summary>
/// Repository interface for Position entity with specialized queries
/// </summary>
public interface IPositionRepository : IRepository<Position>
{
    /// <summary>
    /// Get all open positions
    /// </summary>
    Task<IEnumerable<Position>> GetOpenPositionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all closed positions
    /// </summary>
    Task<IEnumerable<Position>> GetClosedPositionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get positions by asset
    /// </summary>
    Task<IEnumerable<Position>> GetPositionsByAssetAsync(Guid assetId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get positions by Polymarket event
    /// </summary>
    Task<IEnumerable<Position>> GetPositionsByPolymarketEventAsync(Guid eventId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get positions by status
    /// </summary>
    Task<IEnumerable<Position>> GetPositionsByStatusAsync(PositionStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get position with all trades
    /// </summary>
    Task<Position?> GetPositionWithTradesAsync(Guid positionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get positions by date range
    /// </summary>
    Task<IEnumerable<Position>> GetPositionsByDateRangeAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get position performance metrics
    /// </summary>
    Task<PositionPerformanceMetrics> GetPerformanceMetricsAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update position market values (current price, unrealized P&L)
    /// </summary>
    Task UpdatePositionMarketValuesAsync(
        Guid positionId,
        decimal currentPrice,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Position performance metrics model
/// </summary>
public class PositionPerformanceMetrics
{
    public int TotalPositions { get; set; }
    public int OpenPositions { get; set; }
    public int ClosedPositions { get; set; }
    public decimal TotalRealizedPnL { get; set; }
    public decimal TotalUnrealizedPnL { get; set; }
    public decimal TotalPnL => TotalRealizedPnL + TotalUnrealizedPnL;
    public decimal AverageRealizedPnL { get; set; }
    public decimal WinningPositionsCount { get; set; }
    public decimal LosingPositionsCount { get; set; }
    public decimal WinRate => ClosedPositions > 0 ? WinningPositionsCount / ClosedPositions * 100 : 0;
    public decimal TotalCommissions { get; set; }
    public decimal NetPnL => TotalPnL - TotalCommissions;
}
