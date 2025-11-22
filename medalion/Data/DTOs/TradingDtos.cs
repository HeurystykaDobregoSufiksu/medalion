using Medalion.Data.Domain;

namespace Medalion.Data.DTOs;

#region Trade DTOs

/// <summary>
/// Data transfer object for Trade entity
/// </summary>
public class TradeDto
{
    public Guid Id { get; set; }
    public string? ExternalTradeId { get; set; }
    public Guid? StrategyId { get; set; }
    public string? StrategyName { get; set; }
    public Guid? SignalId { get; set; }
    public Guid? AssetId { get; set; }
    public Guid? PolymarketEventId { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public string TradeType { get; set; } = string.Empty;
    public string TradeSide { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public decimal TotalValue { get; set; }
    public decimal Commission { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime ExecutedAt { get; set; }
    public Guid? PositionId { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Request model for creating a trade
/// </summary>
public class CreateTradeRequest
{
    public Guid? StrategyId { get; set; }
    public Guid? SignalId { get; set; }
    public Guid? AssetId { get; set; }
    public Guid? PolymarketEventId { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public TradeType TradeType { get; set; }
    public TradeSide TradeSide { get; set; }
    public decimal Quantity { get; set; }
    public decimal? LimitPrice { get; set; }
    public decimal? StopPrice { get; set; }
    public string? AlpacaMarketDataSnapshot { get; set; }
    public string? PolymarketDataSnapshot { get; set; }
    public string? Notes { get; set; }
}

#endregion

#region Position DTOs

/// <summary>
/// Data transfer object for Position entity
/// </summary>
public class PositionDto
{
    public Guid Id { get; set; }
    public Guid? AssetId { get; set; }
    public Guid? PolymarketEventId { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public string PositionSide { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal RemainingQuantity { get; set; }
    public decimal AverageEntryPrice { get; set; }
    public decimal? CurrentPrice { get; set; }
    public decimal CostBasis { get; set; }
    public decimal? MarketValue { get; set; }
    public decimal? UnrealizedPnL { get; set; }
    public decimal? RealizedPnL { get; set; }
    public decimal TotalCommissions { get; set; }
    public DateTime OpenedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public decimal? StopLoss { get; set; }
    public decimal? TakeProfit { get; set; }
    public string? Notes { get; set; }
    public int TradeCount { get; set; }
}

/// <summary>
/// Request model for opening a position
/// </summary>
public class OpenPositionRequest
{
    public Guid? AssetId { get; set; }
    public Guid? PolymarketEventId { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public PositionSide PositionSide { get; set; }
    public decimal Quantity { get; set; }
    public decimal EntryPrice { get; set; }
    public decimal? StopLoss { get; set; }
    public decimal? TakeProfit { get; set; }
    public string? AlpacaMarketDataSnapshot { get; set; }
    public string? PolymarketDataSnapshot { get; set; }
    public string? Notes { get; set; }
}

#endregion

#region Portfolio DTOs

/// <summary>
/// Portfolio summary data transfer object
/// </summary>
public class PortfolioSummaryDto
{
    public int OpenPositionsCount { get; set; }
    public int TotalPositionsCount { get; set; }
    public decimal TotalMarketValue { get; set; }
    public decimal TotalCostBasis { get; set; }
    public decimal TotalUnrealizedPnL { get; set; }
    public decimal TotalRealizedPnL { get; set; }
    public decimal TotalPnL { get; set; }
    public decimal TotalCommissions { get; set; }
    public decimal NetPnL { get; set; }
    public decimal ReturnPercentage { get; set; }
    public List<PositionDto> OpenPositions { get; set; } = new();
}

/// <summary>
/// Portfolio performance metrics data transfer object
/// </summary>
public class PortfolioPerformanceDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalTrades { get; set; }
    public int WinningTrades { get; set; }
    public int LosingTrades { get; set; }
    public decimal WinRate { get; set; }
    public decimal TotalProfit { get; set; }
    public decimal TotalLoss { get; set; }
    public decimal NetProfit { get; set; }
    public decimal AverageProfit { get; set; }
    public decimal AverageLoss { get; set; }
    public decimal ProfitFactor { get; set; }
    public decimal TotalCommissions { get; set; }
    public decimal ReturnPercentage { get; set; }
    public int OpenPositionsCount { get; set; }
    public int ClosedPositionsCount { get; set; }
}

#endregion

#region Strategy DTOs

/// <summary>
/// Data transfer object for Strategy entity
/// </summary>
public class StrategyDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string StrategyType { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsBacktesting { get; set; }
    public string Configuration { get; set; } = "{}";
    public string RiskParameters { get; set; } = "{}";
    public decimal? MaxPositionSize { get; set; }
    public decimal? MaxLossPerTrade { get; set; }
    public decimal? MaxDailyLoss { get; set; }
    public string? PerformanceMetrics { get; set; }
    public DateTime? LastExecutedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Data transfer object for TradingSignal entity
/// </summary>
public class TradingSignalDto
{
    public Guid Id { get; set; }
    public Guid StrategyId { get; set; }
    public string? StrategyName { get; set; }
    public Guid? AssetId { get; set; }
    public string? AssetSymbol { get; set; }
    public Guid? PolymarketEventId { get; set; }
    public string? EventTitle { get; set; }
    public string SignalType { get; set; } = string.Empty;
    public decimal Confidence { get; set; }
    public DateTime SignalTimestamp { get; set; }
    public decimal? TargetPrice { get; set; }
    public decimal? StopLoss { get; set; }
    public decimal? TakeProfit { get; set; }
    public decimal? SuggestedQuantity { get; set; }
    public string SourceData { get; set; } = "{}";
    public bool WasActedUpon { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

#endregion

#region Asset DTOs

/// <summary>
/// Data transfer object for Asset entity
/// </summary>
public class AssetDto
{
    public Guid Id { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public AssetType AssetType { get; set; }
    public string AssetClass { get; set; } = string.Empty;
    public string Exchange { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsTradable { get; set; }
    public bool IsFractionable { get; set; }
    public decimal MinOrderQuantity { get; set; }
}

#endregion

#region Market Data DTOs

/// <summary>
/// Data transfer object for market bar data (stocks/crypto)
/// </summary>
public class MarketBarDto
{
    public Guid Id { get; set; }
    public Guid AssetId { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public DateTime BarTimestamp { get; set; }
    public string Timeframe { get; set; } = string.Empty;
    public decimal Open { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal Close { get; set; }
    public decimal Volume { get; set; }
    public int TradeCount { get; set; }
    public decimal VWAP { get; set; }
}

/// <summary>
/// Data transfer object for Polymarket event data
/// </summary>
public class PolymarketEventDto
{
    public Guid Id { get; set; }
    public string EventId { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public bool IsClosed { get; set; }
    public bool IsActive { get; set; }
    public DateTime EventCreatedAt { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal TotalVolume { get; set; }
    public decimal TotalLiquidity { get; set; }
    public bool IsMonitored { get; set; }
}

#endregion
