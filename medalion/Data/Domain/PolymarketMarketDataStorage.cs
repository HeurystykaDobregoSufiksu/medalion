namespace Medalion.Data.Domain;

/// <summary>
/// Stores real-time market data snapshots from Polymarket WebSocket
/// </summary>
public class PolymarketSnapshot : BaseEntity
{
    /// <summary>
    /// Reference to the market
    /// </summary>
    public Guid PolymarketMarketId { get; set; }

    /// <summary>
    /// Market ID (denormalized)
    /// </summary>
    public string MarketId { get; set; } = string.Empty;

    /// <summary>
    /// Asset ID from Polymarket
    /// </summary>
    public string AssetId { get; set; } = string.Empty;

    /// <summary>
    /// Snapshot timestamp from Polymarket
    /// </summary>
    public DateTime SnapshotTimestamp { get; set; }

    /// <summary>
    /// Current price
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Last trade price
    /// </summary>
    public decimal LastPrice { get; set; }

    /// <summary>
    /// Best bid
    /// </summary>
    public decimal Bid { get; set; }

    /// <summary>
    /// Best ask
    /// </summary>
    public decimal Ask { get; set; }

    /// <summary>
    /// Bid-ask spread
    /// </summary>
    public decimal Spread { get; set; }

    /// <summary>
    /// Implied Volatility - CRITICAL metric for trading decisions
    /// Higher IV indicates more uncertainty/volatility expected
    /// </summary>
    public decimal ImpliedVolatility { get; set; }

    /// <summary>
    /// 24-hour trading volume
    /// </summary>
    public decimal Volume24h { get; set; }

    /// <summary>
    /// Current liquidity
    /// </summary>
    public decimal Liquidity { get; set; }

    /// <summary>
    /// Outcome name
    /// </summary>
    public string Outcome { get; set; } = string.Empty;

    /// <summary>
    /// Market category
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Tags (JSON array)
    /// </summary>
    public string Tags { get; set; } = "[]";

    // Navigation properties
    public PolymarketMarketData Market { get; set; } = null!;
}

/// <summary>
/// Stores Polymarket trade executions from WebSocket
/// </summary>
public class PolymarketTradeData : BaseEntity
{
    /// <summary>
    /// Trade ID from Polymarket
    /// </summary>
    public string TradeId { get; set; } = string.Empty;

    /// <summary>
    /// Market ID
    /// </summary>
    public string MarketId { get; set; } = string.Empty;

    /// <summary>
    /// Asset ID
    /// </summary>
    public string AssetId { get; set; } = string.Empty;

    /// <summary>
    /// Trade price
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Trade size
    /// </summary>
    public decimal Size { get; set; }

    /// <summary>
    /// Trade side: BUY or SELL
    /// </summary>
    public string Side { get; set; } = string.Empty;

    /// <summary>
    /// Outcome
    /// </summary>
    public string Outcome { get; set; } = string.Empty;

    /// <summary>
    /// Trade timestamp
    /// </summary>
    public DateTime TradeTimestamp { get; set; }

    /// <summary>
    /// Maker address (optional)
    /// </summary>
    public string? MakerAddress { get; set; }

    /// <summary>
    /// Taker address (optional)
    /// </summary>
    public string? TakerAddress { get; set; }
}

/// <summary>
/// Stores Polymarket order book snapshots
/// </summary>
public class PolymarketOrderBookSnapshot : BaseEntity
{
    /// <summary>
    /// Asset ID
    /// </summary>
    public string AssetId { get; set; } = string.Empty;

    /// <summary>
    /// Market ID
    /// </summary>
    public string MarketId { get; set; } = string.Empty;

    /// <summary>
    /// Snapshot timestamp
    /// </summary>
    public DateTime SnapshotTimestamp { get; set; }

    /// <summary>
    /// Bids data (JSON array of OrderBookLevel)
    /// </summary>
    public string Bids { get; set; } = "[]";

    /// <summary>
    /// Asks data (JSON array of OrderBookLevel)
    /// </summary>
    public string Asks { get; set; } = "[]";

    /// <summary>
    /// Total bid depth
    /// </summary>
    public decimal TotalBidSize { get; set; }

    /// <summary>
    /// Total ask depth
    /// </summary>
    public decimal TotalAskSize { get; set; }
}
