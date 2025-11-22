using System.Text.Json.Serialization;

namespace Medalion.Services.Alpaca.Models;

#region Configuration

/// <summary>
/// Configuration for Alpaca API client
/// </summary>
public class AlpacaApiConfiguration
{
    /// <summary>
    /// API Key ID (required)
    /// </summary>
    public string ApiKeyId { get; set; } = string.Empty;

    /// <summary>
    /// API Secret Key (required)
    /// </summary>
    public string ApiSecretKey { get; set; } = string.Empty;

    /// <summary>
    /// Base URL for Alpaca API
    /// Default: https://api.alpaca.markets (live)
    /// Paper trading: https://paper-api.alpaca.markets
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.alpaca.markets";

    /// <summary>
    /// Base URL for market data API
    /// Default: https://data.alpaca.markets
    /// </summary>
    public string DataBaseUrl { get; set; } = "https://data.alpaca.markets";

    /// <summary>
    /// Request timeout in seconds (default: 30)
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Maximum retry attempts for failed requests (default: 3)
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Initial retry delay in milliseconds (default: 1000)
    /// Uses exponential backoff
    /// </summary>
    public int InitialRetryDelayMs { get; set; } = 1000;

    /// <summary>
    /// Rate limit: max requests per minute (default: 200)
    /// Alpaca free tier: 200 requests/minute
    /// </summary>
    public int MaxRequestsPerMinute { get; set; } = 200;
}

#endregion

#region Stock Data Models

/// <summary>
/// Stock quote data (real-time or latest)
/// </summary>
public class StockQuote
{
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = string.Empty;

    [JsonPropertyName("ask_price")]
    public decimal AskPrice { get; set; }

    [JsonPropertyName("ask_size")]
    public int AskSize { get; set; }

    [JsonPropertyName("bid_price")]
    public decimal BidPrice { get; set; }

    [JsonPropertyName("bid_size")]
    public int BidSize { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Mid price calculated from bid/ask
    /// </summary>
    public decimal MidPrice => (BidPrice + AskPrice) / 2;
}

/// <summary>
/// Stock bar/candle data (OHLCV)
/// </summary>
public class StockBar
{
    [JsonPropertyName("t")]
    public DateTime Timestamp { get; set; }

    [JsonPropertyName("o")]
    public decimal Open { get; set; }

    [JsonPropertyName("h")]
    public decimal High { get; set; }

    [JsonPropertyName("l")]
    public decimal Low { get; set; }

    [JsonPropertyName("c")]
    public decimal Close { get; set; }

    [JsonPropertyName("v")]
    public long Volume { get; set; }

    [JsonPropertyName("n")]
    public int TradeCount { get; set; }

    [JsonPropertyName("vw")]
    public decimal VolumeWeightedAveragePrice { get; set; }
}

/// <summary>
/// Response containing multiple bars for a symbol
/// </summary>
public class StockBarsResponse
{
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = string.Empty;

    [JsonPropertyName("bars")]
    public List<StockBar> Bars { get; set; } = new();

    [JsonPropertyName("next_page_token")]
    public string? NextPageToken { get; set; }
}

/// <summary>
/// Stock snapshot with quote, trade, and daily bar
/// </summary>
public class StockSnapshot
{
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = string.Empty;

    [JsonPropertyName("latestTrade")]
    public StockTrade? LatestTrade { get; set; }

    [JsonPropertyName("latestQuote")]
    public StockQuote? LatestQuote { get; set; }

    [JsonPropertyName("minuteBar")]
    public StockBar? MinuteBar { get; set; }

    [JsonPropertyName("dailyBar")]
    public StockBar? DailyBar { get; set; }

    [JsonPropertyName("prevDailyBar")]
    public StockBar? PrevDailyBar { get; set; }
}

/// <summary>
/// Stock trade data
/// </summary>
public class StockTrade
{
    [JsonPropertyName("t")]
    public DateTime Timestamp { get; set; }

    [JsonPropertyName("p")]
    public decimal Price { get; set; }

    [JsonPropertyName("s")]
    public int Size { get; set; }

    [JsonPropertyName("x")]
    public string Exchange { get; set; } = string.Empty;
}

#endregion

#region Crypto Data Models

/// <summary>
/// Crypto quote data
/// </summary>
public class CryptoQuote
{
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = string.Empty;

    [JsonPropertyName("ask_price")]
    public decimal AskPrice { get; set; }

    [JsonPropertyName("ask_size")]
    public decimal AskSize { get; set; }

    [JsonPropertyName("bid_price")]
    public decimal BidPrice { get; set; }

    [JsonPropertyName("bid_size")]
    public decimal BidSize { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    public decimal MidPrice => (BidPrice + AskPrice) / 2;
}

/// <summary>
/// Crypto bar/candle data
/// </summary>
public class CryptoBar
{
    [JsonPropertyName("t")]
    public DateTime Timestamp { get; set; }

    [JsonPropertyName("o")]
    public decimal Open { get; set; }

    [JsonPropertyName("h")]
    public decimal High { get; set; }

    [JsonPropertyName("l")]
    public decimal Low { get; set; }

    [JsonPropertyName("c")]
    public decimal Close { get; set; }

    [JsonPropertyName("v")]
    public decimal Volume { get; set; }

    [JsonPropertyName("n")]
    public int TradeCount { get; set; }

    [JsonPropertyName("vw")]
    public decimal VolumeWeightedAveragePrice { get; set; }
}

/// <summary>
/// Response containing crypto bars
/// </summary>
public class CryptoBarsResponse
{
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = string.Empty;

    [JsonPropertyName("bars")]
    public List<CryptoBar> Bars { get; set; } = new();

    [JsonPropertyName("next_page_token")]
    public string? NextPageToken { get; set; }
}

/// <summary>
/// Crypto snapshot
/// </summary>
public class CryptoSnapshot
{
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = string.Empty;

    [JsonPropertyName("latestTrade")]
    public CryptoTrade? LatestTrade { get; set; }

    [JsonPropertyName("latestQuote")]
    public CryptoQuote? LatestQuote { get; set; }

    [JsonPropertyName("minuteBar")]
    public CryptoBar? MinuteBar { get; set; }

    [JsonPropertyName("dailyBar")]
    public CryptoBar? DailyBar { get; set; }

    [JsonPropertyName("prevDailyBar")]
    public CryptoBar? PrevDailyBar { get; set; }
}

/// <summary>
/// Crypto trade data
/// </summary>
public class CryptoTrade
{
    [JsonPropertyName("t")]
    public DateTime Timestamp { get; set; }

    [JsonPropertyName("p")]
    public decimal Price { get; set; }

    [JsonPropertyName("s")]
    public decimal Size { get; set; }

    [JsonPropertyName("tks")]
    public string TakerSide { get; set; } = string.Empty;
}

#endregion

#region Options Data Models

/// <summary>
/// Options contract details
/// </summary>
public class OptionContract
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("tradable")]
    public bool Tradable { get; set; }

    [JsonPropertyName("expiration_date")]
    public DateTime ExpirationDate { get; set; }

    [JsonPropertyName("root_symbol")]
    public string RootSymbol { get; set; } = string.Empty;

    [JsonPropertyName("underlying_symbol")]
    public string UnderlyingSymbol { get; set; } = string.Empty;

    [JsonPropertyName("underlying_asset_id")]
    public string UnderlyingAssetId { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty; // "call" or "put"

    [JsonPropertyName("style")]
    public string Style { get; set; } = string.Empty; // "american" or "european"

    [JsonPropertyName("strike_price")]
    public decimal StrikePrice { get; set; }

    [JsonPropertyName("multiplier")]
    public decimal Multiplier { get; set; }

    [JsonPropertyName("size")]
    public decimal Size { get; set; }

    [JsonPropertyName("open_interest")]
    public long? OpenInterest { get; set; }

    [JsonPropertyName("open_interest_date")]
    public DateTime? OpenInterestDate { get; set; }

    [JsonPropertyName("close_price")]
    public decimal? ClosePrice { get; set; }

    [JsonPropertyName("close_price_date")]
    public DateTime? ClosePriceDate { get; set; }
}

/// <summary>
/// Options chain response
/// </summary>
public class OptionsChainResponse
{
    [JsonPropertyName("options")]
    public List<OptionContract> Options { get; set; } = new();

    [JsonPropertyName("next_page_token")]
    public string? NextPageToken { get; set; }
}

/// <summary>
/// Option quote with Greeks and IV
/// </summary>
public class OptionQuote
{
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    [JsonPropertyName("bid_price")]
    public decimal BidPrice { get; set; }

    [JsonPropertyName("bid_size")]
    public int BidSize { get; set; }

    [JsonPropertyName("ask_price")]
    public decimal AskPrice { get; set; }

    [JsonPropertyName("ask_size")]
    public int AskSize { get; set; }

    [JsonPropertyName("bid_exchange")]
    public string BidExchange { get; set; } = string.Empty;

    [JsonPropertyName("ask_exchange")]
    public string AskExchange { get; set; } = string.Empty;

    public decimal MidPrice => (BidPrice + AskPrice) / 2;
}

/// <summary>
/// Option trade data
/// </summary>
public class OptionTrade
{
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("size")]
    public int Size { get; set; }

    [JsonPropertyName("exchange")]
    public string Exchange { get; set; } = string.Empty;
}

/// <summary>
/// Option snapshot with latest quote and trade
/// </summary>
public class OptionSnapshot
{
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } = string.Empty;

    [JsonPropertyName("latestTrade")]
    public OptionTrade? LatestTrade { get; set; }

    [JsonPropertyName("latestQuote")]
    public OptionQuote? LatestQuote { get; set; }

    [JsonPropertyName("greeks")]
    public OptionGreeks? Greeks { get; set; }

    [JsonPropertyName("impliedVolatility")]
    public decimal? ImpliedVolatility { get; set; }
}

/// <summary>
/// Option Greeks (Delta, Gamma, Theta, Vega, Rho)
/// IMPORTANT: Alpaca provides Greeks through Option Data API
/// </summary>
public class OptionGreeks
{
    /// <summary>
    /// Delta: Rate of change of option price with respect to underlying price
    /// Range: 0 to 1 (calls), -1 to 0 (puts)
    /// </summary>
    [JsonPropertyName("delta")]
    public decimal Delta { get; set; }

    /// <summary>
    /// Gamma: Rate of change of delta with respect to underlying price
    /// </summary>
    [JsonPropertyName("gamma")]
    public decimal Gamma { get; set; }

    /// <summary>
    /// Theta: Rate of change of option price with respect to time (time decay)
    /// Usually negative
    /// </summary>
    [JsonPropertyName("theta")]
    public decimal Theta { get; set; }

    /// <summary>
    /// Vega: Rate of change of option price with respect to volatility
    /// </summary>
    [JsonPropertyName("vega")]
    public decimal Vega { get; set; }

    /// <summary>
    /// Rho: Rate of change of option price with respect to interest rate
    /// </summary>
    [JsonPropertyName("rho")]
    public decimal Rho { get; set; }
}

/// <summary>
/// Implied Volatility data for an option
/// This is the PRIMARY model for IV retrieval
/// </summary>
public class ImpliedVolatilityData
{
    /// <summary>
    /// Option symbol (e.g., "AAPL230120C00150000")
    /// </summary>
    public string Symbol { get; set; } = string.Empty;

    /// <summary>
    /// Underlying stock symbol (e.g., "AAPL")
    /// </summary>
    public string UnderlyingSymbol { get; set; } = string.Empty;

    /// <summary>
    /// Implied Volatility as a percentage (e.g., 0.25 = 25%)
    /// </summary>
    public decimal ImpliedVolatility { get; set; }

    /// <summary>
    /// Timestamp of the IV calculation
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Strike price
    /// </summary>
    public decimal StrikePrice { get; set; }

    /// <summary>
    /// Expiration date
    /// </summary>
    public DateTime ExpirationDate { get; set; }

    /// <summary>
    /// Option type: "call" or "put"
    /// </summary>
    public string OptionType { get; set; } = string.Empty;

    /// <summary>
    /// Current option price (mid of bid/ask)
    /// </summary>
    public decimal? OptionPrice { get; set; }

    /// <summary>
    /// Current underlying price
    /// </summary>
    public decimal? UnderlyingPrice { get; set; }

    /// <summary>
    /// Days to expiration
    /// </summary>
    public int DaysToExpiration { get; set; }

    /// <summary>
    /// Option Greeks (if available)
    /// </summary>
    public OptionGreeks? Greeks { get; set; }

    /// <summary>
    /// Open interest
    /// </summary>
    public long? OpenInterest { get; set; }

    /// <summary>
    /// Volume
    /// </summary>
    public long? Volume { get; set; }
}

/// <summary>
/// Response for bulk IV retrieval across an options chain
/// </summary>
public class ImpliedVolatilityChainResponse
{
    /// <summary>
    /// Underlying symbol
    /// </summary>
    public string UnderlyingSymbol { get; set; } = string.Empty;

    /// <summary>
    /// List of IV data for each option in the chain
    /// </summary>
    public List<ImpliedVolatilityData> Options { get; set; } = new();

    /// <summary>
    /// Average IV across all options (useful for volatility surface)
    /// </summary>
    public decimal AverageImpliedVolatility { get; set; }

    /// <summary>
    /// Timestamp of retrieval
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Current underlying price
    /// </summary>
    public decimal UnderlyingPrice { get; set; }
}

#endregion

#region Error and Response Models

/// <summary>
/// API error response
/// </summary>
public class AlpacaErrorResponse
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Rate limit information
/// </summary>
public class RateLimitInfo
{
    public int Limit { get; set; }
    public int Remaining { get; set; }
    public DateTime ResetTime { get; set; }
}

#endregion

#region Enums

/// <summary>
/// Timeframe for historical data
/// </summary>
public enum Timeframe
{
    OneMinute,
    FiveMinutes,
    FifteenMinutes,
    ThirtyMinutes,
    OneHour,
    FourHours,
    OneDay,
    OneWeek,
    OneMonth
}

/// <summary>
/// Option type
/// </summary>
public enum OptionType
{
    Call,
    Put
}

/// <summary>
/// Feed type for market data
/// </summary>
public enum Feed
{
    SIP,  // Securities Information Processor (all US exchanges)
    IEX,  // Investors Exchange (IEX only, free)
    OTC   // Over-the-counter
}

#endregion
