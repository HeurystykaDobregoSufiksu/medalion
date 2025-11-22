using Medalion.Data.Domain;

namespace Medalion.Data.Repositories;

/// <summary>
/// Repository interface for Trade entity with specialized queries
/// </summary>
public interface ITradeRepository : IRepository<Trade>
{
    /// <summary>
    /// Get all trades for a specific asset
    /// </summary>
    Task<IEnumerable<Trade>> GetTradesByAssetAsync(Guid assetId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all trades for a specific Polymarket event
    /// </summary>
    Task<IEnumerable<Trade>> GetTradesByPolymarketEventAsync(Guid eventId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all trades by strategy
    /// </summary>
    Task<IEnumerable<Trade>> GetTradesByStrategyAsync(Guid strategyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get trades within a date range
    /// </summary>
    Task<IEnumerable<Trade>> GetTradesByDateRangeAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get trades by status
    /// </summary>
    Task<IEnumerable<Trade>> GetTradesByStatusAsync(TradeStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get profitable trades
    /// </summary>
    Task<IEnumerable<Trade>> GetProfitableTradesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get trade statistics for a date range
    /// </summary>
    Task<TradeStatistics> GetTradeStatisticsAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Trade statistics model
/// </summary>
public class TradeStatistics
{
    public int TotalTrades { get; set; }
    public int WinningTrades { get; set; }
    public int LosingTrades { get; set; }
    public decimal TotalProfit { get; set; }
    public decimal TotalLoss { get; set; }
    public decimal NetProfit => TotalProfit - TotalLoss;
    public decimal WinRate => TotalTrades > 0 ? (decimal)WinningTrades / TotalTrades * 100 : 0;
    public decimal AverageProfit { get; set; }
    public decimal AverageLoss { get; set; }
    public decimal TotalCommissions { get; set; }
}
