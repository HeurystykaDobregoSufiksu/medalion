namespace Medalion.Data.Domain;

/// <summary>
/// Represents a completed trade (buy or sell transaction)
/// </summary>
public class Trade : BaseEntity
{
    /// <summary>
    /// External trade ID (from broker/exchange)
    /// </summary>
    public string? ExternalTradeId { get; set; }

    /// <summary>
    /// Reference to the strategy that initiated this trade
    /// </summary>
    public Guid? StrategyId { get; set; }

    /// <summary>
    /// Reference to the signal that triggered this trade
    /// </summary>
    public Guid? SignalId { get; set; }

    /// <summary>
    /// Asset ID (for stock/crypto trades)
    /// </summary>
    public Guid? AssetId { get; set; }

    /// <summary>
    /// Polymarket Event ID (for prediction market trades)
    /// </summary>
    public Guid? PolymarketEventId { get; set; }

    /// <summary>
    /// Symbol (denormalized for query performance)
    /// </summary>
    public string Symbol { get; set; } = string.Empty;

    /// <summary>
    /// Trade type: Market, Limit, Stop, StopLimit
    /// </summary>
    public TradeType TradeType { get; set; }

    /// <summary>
    /// Trade side: Buy or Sell
    /// </summary>
    public TradeSide TradeSide { get; set; }

    /// <summary>
    /// Quantity/size of the trade
    /// </summary>
    public decimal Quantity { get; set; }

    /// <summary>
    /// Executed price
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Total trade value (Quantity × Price)
    /// </summary>
    public decimal TotalValue { get; set; }

    /// <summary>
    /// Commission/fees paid
    /// </summary>
    public decimal Commission { get; set; }

    /// <summary>
    /// Trade status
    /// </summary>
    public TradeStatus Status { get; set; }

    /// <summary>
    /// Execution timestamp
    /// </summary>
    public DateTime ExecutedAt { get; set; }

    /// <summary>
    /// Position ID (if this trade is part of a position)
    /// </summary>
    public Guid? PositionId { get; set; }

    /// <summary>
    /// Alpaca market data snapshot at time of trade (JSON)
    /// Stores the StockQuote, CryptoQuote, or OptionQuote data
    /// </summary>
    public string? AlpacaMarketDataSnapshot { get; set; }

    /// <summary>
    /// Polymarket data snapshot at time of trade (JSON)
    /// Stores the MarketDataUpdate from WebSocket
    /// </summary>
    public string? PolymarketDataSnapshot { get; set; }

    /// <summary>
    /// Trade decision rationale/notes
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Additional metadata (JSON)
    /// </summary>
    public string? Metadata { get; set; }

    // Navigation properties
    public Strategy? Strategy { get; set; }
    public TradingSignal? Signal { get; set; }
    public Asset? Asset { get; set; }
    public PolymarketEventData? PolymarketEvent { get; set; }
    public Position? Position { get; set; }
}

/// <summary>
/// Trade type enumeration
/// </summary>
public enum TradeType
{
    Market = 1,
    Limit = 2,
    Stop = 3,
    StopLimit = 4,
    TrailingStop = 5
}

/// <summary>
/// Trade side enumeration
/// </summary>
public enum TradeSide
{
    Buy = 1,
    Sell = 2
}

/// <summary>
/// Trade status enumeration
/// </summary>
public enum TradeStatus
{
    Pending = 1,
    Submitted = 2,
    PartiallyFilled = 3,
    Filled = 4,
    Cancelled = 5,
    Rejected = 6,
    Expired = 7
}
