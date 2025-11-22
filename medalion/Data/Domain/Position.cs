namespace Medalion.Data.Domain;

/// <summary>
/// Represents a trading position (open or closed)
/// </summary>
public class Position : BaseEntity
{
    /// <summary>
    /// Asset ID (for stock/crypto positions)
    /// </summary>
    public Guid? AssetId { get; set; }

    /// <summary>
    /// Polymarket Event ID (for prediction market positions)
    /// </summary>
    public Guid? PolymarketEventId { get; set; }

    /// <summary>
    /// Symbol (denormalized for query performance)
    /// </summary>
    public string Symbol { get; set; } = string.Empty;

    /// <summary>
    /// Position side: Long or Short
    /// </summary>
    public PositionSide PositionSide { get; set; }

    /// <summary>
    /// Position status: Open, Closed, PartiallyFilled
    /// </summary>
    public PositionStatus Status { get; set; }

    /// <summary>
    /// Total quantity in the position
    /// </summary>
    public decimal Quantity { get; set; }

    /// <summary>
    /// Remaining quantity (for partially closed positions)
    /// </summary>
    public decimal RemainingQuantity { get; set; }

    /// <summary>
    /// Average entry price
    /// </summary>
    public decimal AverageEntryPrice { get; set; }

    /// <summary>
    /// Current market price (updated periodically)
    /// </summary>
    public decimal? CurrentPrice { get; set; }

    /// <summary>
    /// Total cost basis (including commissions)
    /// </summary>
    public decimal CostBasis { get; set; }

    /// <summary>
    /// Current market value
    /// </summary>
    public decimal? MarketValue { get; set; }

    /// <summary>
    /// Unrealized profit/loss
    /// </summary>
    public decimal? UnrealizedPnL { get; set; }

    /// <summary>
    /// Realized profit/loss (for closed positions)
    /// </summary>
    public decimal? RealizedPnL { get; set; }

    /// <summary>
    /// Total commissions paid
    /// </summary>
    public decimal TotalCommissions { get; set; }

    /// <summary>
    /// Position opened timestamp
    /// </summary>
    public DateTime OpenedAt { get; set; }

    /// <summary>
    /// Position closed timestamp (if closed)
    /// </summary>
    public DateTime? ClosedAt { get; set; }

    /// <summary>
    /// Stop loss price
    /// </summary>
    public decimal? StopLoss { get; set; }

    /// <summary>
    /// Take profit price
    /// </summary>
    public decimal? TakeProfit { get; set; }

    /// <summary>
    /// Alpaca market data when position was opened (JSON)
    /// </summary>
    public string? AlpacaOpenSnapshot { get; set; }

    /// <summary>
    /// Polymarket data when position was opened (JSON)
    /// </summary>
    public string? PolymarketOpenSnapshot { get; set; }

    /// <summary>
    /// Alpaca market data when position was closed (JSON)
    /// </summary>
    public string? AlpacaCloseSnapshot { get; set; }

    /// <summary>
    /// Polymarket data when position was closed (JSON)
    /// </summary>
    public string? PolymarketCloseSnapshot { get; set; }

    /// <summary>
    /// Position notes
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Additional metadata (JSON)
    /// </summary>
    public string? Metadata { get; set; }

    // Navigation properties
    public Asset? Asset { get; set; }
    public PolymarketEventData? PolymarketEvent { get; set; }
    public ICollection<Trade> Trades { get; set; } = new List<Trade>();
}

/// <summary>
/// Position side enumeration
/// </summary>
public enum PositionSide
{
    Long = 1,
    Short = 2
}

/// <summary>
/// Position status enumeration
/// </summary>
public enum PositionStatus
{
    Open = 1,
    PartiallyFilled = 2,
    Closed = 3,
    Cancelled = 4
}
