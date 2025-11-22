namespace Medalion.Data.Domain;

/// <summary>
/// Represents a Polymarket event being tracked by the bot
/// </summary>
public class PolymarketEventData : BaseEntity
{
    /// <summary>
    /// Polymarket event ID
    /// </summary>
    public string EventId { get; set; } = string.Empty;

    /// <summary>
    /// Event slug/URL identifier
    /// </summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// Event title
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Event description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Event category (e.g., Politics, Sports, Crypto)
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Event tags (JSON array)
    /// </summary>
    public string Tags { get; set; } = "[]";

    /// <summary>
    /// Whether the event is closed
    /// </summary>
    public bool IsClosed { get; set; }

    /// <summary>
    /// Whether the event is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Event creation timestamp (from Polymarket)
    /// </summary>
    public DateTime EventCreatedAt { get; set; }

    /// <summary>
    /// Event end date (resolution date)
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Total trading volume for this event
    /// </summary>
    public decimal TotalVolume { get; set; }

    /// <summary>
    /// Total liquidity for this event
    /// </summary>
    public decimal TotalLiquidity { get; set; }

    /// <summary>
    /// Whether this event is being actively monitored
    /// </summary>
    public bool IsMonitored { get; set; } = true;

    /// <summary>
    /// Additional metadata (JSON)
    /// </summary>
    public string? Metadata { get; set; }

    // Navigation properties
    public ICollection<PolymarketMarketData> Markets { get; set; } = new List<PolymarketMarketData>();
    public ICollection<Trade> Trades { get; set; } = new List<Trade>();
    public ICollection<Position> Positions { get; set; } = new List<Position>();
}

/// <summary>
/// Represents a specific market within a Polymarket event
/// </summary>
public class PolymarketMarketData : BaseEntity
{
    /// <summary>
    /// Market ID from Polymarket
    /// </summary>
    public string MarketId { get; set; } = string.Empty;

    /// <summary>
    /// Condition ID
    /// </summary>
    public string ConditionId { get; set; } = string.Empty;

    /// <summary>
    /// Question ID
    /// </summary>
    public string QuestionId { get; set; } = string.Empty;

    /// <summary>
    /// Market question text
    /// </summary>
    public string Question { get; set; } = string.Empty;

    /// <summary>
    /// Possible outcomes (JSON array)
    /// </summary>
    public string Outcomes { get; set; } = "[]";

    /// <summary>
    /// Current outcome prices (JSON array)
    /// </summary>
    public string OutcomePrices { get; set; } = "[]";

    /// <summary>
    /// Market trading volume
    /// </summary>
    public decimal Volume { get; set; }

    /// <summary>
    /// Market liquidity
    /// </summary>
    public decimal Liquidity { get; set; }

    /// <summary>
    /// Parent event ID
    /// </summary>
    public Guid PolymarketEventId { get; set; }

    /// <summary>
    /// Additional metadata (JSON)
    /// </summary>
    public string? Metadata { get; set; }

    // Navigation properties
    public PolymarketEventData PolymarketEvent { get; set; } = null!;
    public ICollection<PolymarketSnapshot> Snapshots { get; set; } = new List<PolymarketSnapshot>();
}
