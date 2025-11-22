namespace Medalion.Data.Domain;

/// <summary>
/// Represents a tradable asset (stock, crypto, etc.)
/// </summary>
public class Asset : BaseEntity
{
    /// <summary>
    /// Ticker symbol (e.g., "AAPL", "BTCUSD")
    /// </summary>
    public string Symbol { get; set; } = string.Empty;

    /// <summary>
    /// Full name of the asset
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Asset type: Stock, Crypto, Option
    /// </summary>
    public AssetType AssetType { get; set; }

    /// <summary>
    /// Asset class: US Equity, Crypto, etc.
    /// </summary>
    public string AssetClass { get; set; } = string.Empty;

    /// <summary>
    /// Exchange where the asset is traded
    /// </summary>
    public string Exchange { get; set; } = string.Empty;

    /// <summary>
    /// Whether this asset is currently being tracked by the bot
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether this asset is tradable
    /// </summary>
    public bool IsTradable { get; set; } = true;

    /// <summary>
    /// Whether this asset is fractionable
    /// </summary>
    public bool IsFractionable { get; set; } = false;

    /// <summary>
    /// Minimum order quantity
    /// </summary>
    public decimal MinOrderQuantity { get; set; } = 1;

    /// <summary>
    /// Additional metadata (JSON)
    /// </summary>
    public string? Metadata { get; set; }

    // Navigation properties
    public ICollection<StockQuoteSnapshot> StockQuotes { get; set; } = new List<StockQuoteSnapshot>();
    public ICollection<StockBarData> StockBars { get; set; } = new List<StockBarData>();
    public ICollection<CryptoQuoteSnapshot> CryptoQuotes { get; set; } = new List<CryptoQuoteSnapshot>();
    public ICollection<CryptoBarData> CryptoBars { get; set; } = new List<CryptoBarData>();
    public ICollection<OptionContractData> OptionContracts { get; set; } = new List<OptionContractData>();
    public ICollection<Trade> Trades { get; set; } = new List<Trade>();
    public ICollection<Position> Positions { get; set; } = new List<Position>();
}

/// <summary>
/// Asset type enumeration
/// </summary>
public enum AssetType
{
    Stock = 1,
    Crypto = 2,
    Option = 3,
    Forex = 4,
    Future = 5
}
