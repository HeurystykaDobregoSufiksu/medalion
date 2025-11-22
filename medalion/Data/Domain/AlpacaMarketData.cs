namespace Medalion.Data.Domain;

/// <summary>
/// Stores historical stock quote snapshots from Alpaca
/// </summary>
public class StockQuoteSnapshot : BaseEntity
{
    /// <summary>
    /// Reference to the asset
    /// </summary>
    public Guid AssetId { get; set; }

    /// <summary>
    /// Symbol (denormalized for query performance)
    /// </summary>
    public string Symbol { get; set; } = string.Empty;

    /// <summary>
    /// Ask price
    /// </summary>
    public decimal AskPrice { get; set; }

    /// <summary>
    /// Ask size
    /// </summary>
    public int AskSize { get; set; }

    /// <summary>
    /// Bid price
    /// </summary>
    public decimal BidPrice { get; set; }

    /// <summary>
    /// Bid size
    /// </summary>
    public int BidSize { get; set; }

    /// <summary>
    /// Mid price (calculated)
    /// </summary>
    public decimal MidPrice { get; set; }

    /// <summary>
    /// Quote timestamp from Alpaca
    /// </summary>
    public DateTime QuoteTimestamp { get; set; }

    // Navigation properties
    public Asset Asset { get; set; } = null!;
}

/// <summary>
/// Stores historical stock bar/candle data from Alpaca (OHLCV)
/// </summary>
public class StockBarData : BaseEntity
{
    /// <summary>
    /// Reference to the asset
    /// </summary>
    public Guid AssetId { get; set; }

    /// <summary>
    /// Symbol (denormalized for query performance)
    /// </summary>
    public string Symbol { get; set; } = string.Empty;

    /// <summary>
    /// Bar timestamp
    /// </summary>
    public DateTime BarTimestamp { get; set; }

    /// <summary>
    /// Timeframe (1Min, 5Min, 1Hour, 1Day, etc.)
    /// </summary>
    public string Timeframe { get; set; } = string.Empty;

    /// <summary>
    /// Open price
    /// </summary>
    public decimal Open { get; set; }

    /// <summary>
    /// High price
    /// </summary>
    public decimal High { get; set; }

    /// <summary>
    /// Low price
    /// </summary>
    public decimal Low { get; set; }

    /// <summary>
    /// Close price
    /// </summary>
    public decimal Close { get; set; }

    /// <summary>
    /// Volume
    /// </summary>
    public long Volume { get; set; }

    /// <summary>
    /// Number of trades
    /// </summary>
    public int TradeCount { get; set; }

    /// <summary>
    /// Volume-weighted average price
    /// </summary>
    public decimal VWAP { get; set; }

    // Navigation properties
    public Asset Asset { get; set; } = null!;
}

/// <summary>
/// Stores historical crypto quote snapshots from Alpaca
/// </summary>
public class CryptoQuoteSnapshot : BaseEntity
{
    /// <summary>
    /// Reference to the asset
    /// </summary>
    public Guid AssetId { get; set; }

    /// <summary>
    /// Symbol (denormalized for query performance)
    /// </summary>
    public string Symbol { get; set; } = string.Empty;

    /// <summary>
    /// Ask price
    /// </summary>
    public decimal AskPrice { get; set; }

    /// <summary>
    /// Ask size
    /// </summary>
    public decimal AskSize { get; set; }

    /// <summary>
    /// Bid price
    /// </summary>
    public decimal BidPrice { get; set; }

    /// <summary>
    /// Bid size
    /// </summary>
    public decimal BidSize { get; set; }

    /// <summary>
    /// Mid price (calculated)
    /// </summary>
    public decimal MidPrice { get; set; }

    /// <summary>
    /// Quote timestamp from Alpaca
    /// </summary>
    public DateTime QuoteTimestamp { get; set; }

    // Navigation properties
    public Asset Asset { get; set; } = null!;
}

/// <summary>
/// Stores historical crypto bar/candle data from Alpaca
/// </summary>
public class CryptoBarData : BaseEntity
{
    /// <summary>
    /// Reference to the asset
    /// </summary>
    public Guid AssetId { get; set; }

    /// <summary>
    /// Symbol (denormalized for query performance)
    /// </summary>
    public string Symbol { get; set; } = string.Empty;

    /// <summary>
    /// Bar timestamp
    /// </summary>
    public DateTime BarTimestamp { get; set; }

    /// <summary>
    /// Timeframe (1Min, 5Min, 1Hour, 1Day, etc.)
    /// </summary>
    public string Timeframe { get; set; } = string.Empty;

    /// <summary>
    /// Open price
    /// </summary>
    public decimal Open { get; set; }

    /// <summary>
    /// High price
    /// </summary>
    public decimal High { get; set; }

    /// <summary>
    /// Low price
    /// </summary>
    public decimal Low { get; set; }

    /// <summary>
    /// Close price
    /// </summary>
    public decimal Close { get; set; }

    /// <summary>
    /// Volume
    /// </summary>
    public decimal Volume { get; set; }

    /// <summary>
    /// Number of trades
    /// </summary>
    public int TradeCount { get; set; }

    /// <summary>
    /// Volume-weighted average price
    /// </summary>
    public decimal VWAP { get; set; }

    // Navigation properties
    public Asset Asset { get; set; } = null!;
}

/// <summary>
/// Stores options contract data from Alpaca
/// </summary>
public class OptionContractData : BaseEntity
{
    /// <summary>
    /// Reference to underlying asset
    /// </summary>
    public Guid AssetId { get; set; }

    /// <summary>
    /// Option contract ID from Alpaca
    /// </summary>
    public string ContractId { get; set; } = string.Empty;

    /// <summary>
    /// Option symbol (e.g., "AAPL230120C00150000")
    /// </summary>
    public string Symbol { get; set; } = string.Empty;

    /// <summary>
    /// Underlying symbol (e.g., "AAPL")
    /// </summary>
    public string UnderlyingSymbol { get; set; } = string.Empty;

    /// <summary>
    /// Contract status
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Whether tradable
    /// </summary>
    public bool IsTradable { get; set; }

    /// <summary>
    /// Expiration date
    /// </summary>
    public DateTime ExpirationDate { get; set; }

    /// <summary>
    /// Option type: Call or Put
    /// </summary>
    public string OptionType { get; set; } = string.Empty;

    /// <summary>
    /// Option style: American or European
    /// </summary>
    public string OptionStyle { get; set; } = string.Empty;

    /// <summary>
    /// Strike price
    /// </summary>
    public decimal StrikePrice { get; set; }

    /// <summary>
    /// Contract multiplier
    /// </summary>
    public decimal Multiplier { get; set; }

    /// <summary>
    /// Open interest
    /// </summary>
    public long? OpenInterest { get; set; }

    /// <summary>
    /// Additional metadata (JSON)
    /// </summary>
    public string? Metadata { get; set; }

    // Navigation properties
    public Asset Asset { get; set; } = null!;
    public ICollection<OptionQuoteSnapshot> OptionQuotes { get; set; } = new List<OptionQuoteSnapshot>();
}

/// <summary>
/// Stores option quote snapshots with Greeks and IV
/// </summary>
public class OptionQuoteSnapshot : BaseEntity
{
    /// <summary>
    /// Reference to option contract
    /// </summary>
    public Guid OptionContractId { get; set; }

    /// <summary>
    /// Symbol (denormalized)
    /// </summary>
    public string Symbol { get; set; } = string.Empty;

    /// <summary>
    /// Quote timestamp
    /// </summary>
    public DateTime QuoteTimestamp { get; set; }

    /// <summary>
    /// Bid price
    /// </summary>
    public decimal BidPrice { get; set; }

    /// <summary>
    /// Bid size
    /// </summary>
    public int BidSize { get; set; }

    /// <summary>
    /// Ask price
    /// </summary>
    public decimal AskPrice { get; set; }

    /// <summary>
    /// Ask size
    /// </summary>
    public int AskSize { get; set; }

    /// <summary>
    /// Mid price
    /// </summary>
    public decimal MidPrice { get; set; }

    /// <summary>
    /// Implied Volatility
    /// </summary>
    public decimal? ImpliedVolatility { get; set; }

    /// <summary>
    /// Delta (Greek)
    /// </summary>
    public decimal? Delta { get; set; }

    /// <summary>
    /// Gamma (Greek)
    /// </summary>
    public decimal? Gamma { get; set; }

    /// <summary>
    /// Theta (Greek)
    /// </summary>
    public decimal? Theta { get; set; }

    /// <summary>
    /// Vega (Greek)
    /// </summary>
    public decimal? Vega { get; set; }

    /// <summary>
    /// Rho (Greek)
    /// </summary>
    public decimal? Rho { get; set; }

    /// <summary>
    /// Underlying price at time of quote
    /// </summary>
    public decimal? UnderlyingPrice { get; set; }

    /// <summary>
    /// Days to expiration
    /// </summary>
    public int? DaysToExpiration { get; set; }

    // Navigation properties
    public OptionContractData OptionContract { get; set; } = null!;
}
