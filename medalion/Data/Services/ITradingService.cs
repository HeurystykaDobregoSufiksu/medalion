using Medalion.Data.Domain;
using Medalion.Data.DTOs;

namespace Medalion.Data.Services;

/// <summary>
/// Service interface for trading operations
/// </summary>
public interface ITradingService
{
    #region Trade Operations

    /// <summary>
    /// Execute a market order
    /// </summary>
    Task<TradeDto> ExecuteMarketOrderAsync(
        CreateTradeRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute a limit order
    /// </summary>
    Task<TradeDto> ExecuteLimitOrderAsync(
        CreateTradeRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get trade by ID
    /// </summary>
    Task<TradeDto?> GetTradeByIdAsync(Guid tradeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all trades for a position
    /// </summary>
    Task<IEnumerable<TradeDto>> GetTradesByPositionAsync(
        Guid positionId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get trades by date range
    /// </summary>
    Task<IEnumerable<TradeDto>> GetTradesByDateRangeAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancel a pending trade
    /// </summary>
    Task<bool> CancelTradeAsync(Guid tradeId, CancellationToken cancellationToken = default);

    #endregion

    #region Position Operations

    /// <summary>
    /// Open a new position
    /// </summary>
    Task<PositionDto> OpenPositionAsync(
        OpenPositionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Close a position
    /// </summary>
    Task<PositionDto> ClosePositionAsync(
        Guid positionId,
        decimal? closePrice = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Partially close a position
    /// </summary>
    Task<PositionDto> PartiallyClosePositionAsync(
        Guid positionId,
        decimal quantity,
        decimal? closePrice = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get position by ID
    /// </summary>
    Task<PositionDto?> GetPositionByIdAsync(Guid positionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all open positions
    /// </summary>
    Task<IEnumerable<PositionDto>> GetOpenPositionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all positions for an asset
    /// </summary>
    Task<IEnumerable<PositionDto>> GetPositionsByAssetAsync(
        Guid assetId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update position stop loss and take profit
    /// </summary>
    Task<bool> UpdatePositionRiskParametersAsync(
        Guid positionId,
        decimal? stopLoss,
        decimal? takeProfit,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update position market values (for open positions)
    /// </summary>
    Task<bool> UpdatePositionMarketDataAsync(
        Guid positionId,
        decimal currentPrice,
        CancellationToken cancellationToken = default);

    #endregion

    #region Portfolio Operations

    /// <summary>
    /// Get portfolio summary
    /// </summary>
    Task<PortfolioSummaryDto> GetPortfolioSummaryAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get portfolio performance metrics
    /// </summary>
    Task<PortfolioPerformanceDto> GetPortfolioPerformanceAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);

    #endregion
}
