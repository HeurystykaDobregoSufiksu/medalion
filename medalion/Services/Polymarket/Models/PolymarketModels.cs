using System.Text.Json.Serialization;

namespace Medalion.Services.Polymarket.Models;

/// <summary>
/// Represents a Polymarket event/market with all metadata
/// </summary>
public class PolymarketEvent
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("slug")]
    public string Slug { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = new();

    [JsonPropertyName("markets")]
    public List<Market> Markets { get; set; } = new();

    [JsonPropertyName("closed")]
    public bool Closed { get; set; }

    [JsonPropertyName("active")]
    public bool Active { get; set; }

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("endDate")]
    public DateTime? EndDate { get; set; }

    [JsonPropertyName("volume")]
    public decimal Volume { get; set; }

    [JsonPropertyName("liquidity")]
    public decimal Liquidity { get; set; }
}

/// <summary>
/// Represents a specific market within an event (binary outcome)
/// </summary>
public class Market
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("conditionId")]
    public string ConditionId { get; set; } = string.Empty;

    [JsonPropertyName("questionId")]
    public string QuestionId { get; set; } = string.Empty;

    [JsonPropertyName("tokens")]
    public List<Token> Tokens { get; set; } = new();

    [JsonPropertyName("clobTokenIds")]
    public List<string> ClobTokenIds { get; set; } = new();

    [JsonPropertyName("question")]
    public string Question { get; set; } = string.Empty;

    [JsonPropertyName("outcomes")]
    public List<string> Outcomes { get; set; } = new();

    [JsonPropertyName("outcomePrices")]
    public List<decimal> OutcomePrices { get; set; } = new();

    [JsonPropertyName("volume")]
    public decimal Volume { get; set; }

    [JsonPropertyName("liquidity")]
    public decimal Liquidity { get; set; }
}

/// <summary>
/// Represents a token (outcome) in a market
/// </summary>
public class Token
{
    [JsonPropertyName("token_id")]
    public string TokenId { get; set; } = string.Empty;

    [JsonPropertyName("outcome")]
    public string Outcome { get; set; } = string.Empty;

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("winner")]
    public bool? Winner { get; set; }
}

/// <summary>
/// WebSocket subscription message
/// </summary>
public class SubscriptionMessage
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "subscribe";

    [JsonPropertyName("subscriptions")]
    public List<Subscription> Subscriptions { get; set; } = new();
}

/// <summary>
/// Individual subscription within a WebSocket message
/// </summary>
public class Subscription
{
    [JsonPropertyName("topic")]
    public string Topic { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("filters")]
    public object? Filters { get; set; }
}

/// <summary>
/// Real-time market data update from WebSocket
/// MOST IMPORTANT: Contains Implied Volatility metrics
/// </summary>
public class MarketDataUpdate
{
    [JsonPropertyName("event_id")]
    public string EventId { get; set; } = string.Empty;

    [JsonPropertyName("market_id")]
    public string MarketId { get; set; } = string.Empty;

    [JsonPropertyName("asset_id")]
    public string AssetId { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("last_price")]
    public decimal LastPrice { get; set; }

    [JsonPropertyName("bid")]
    public decimal Bid { get; set; }

    [JsonPropertyName("ask")]
    public decimal Ask { get; set; }

    [JsonPropertyName("spread")]
    public decimal Spread { get; set; }

    /// <summary>
    /// CRITICAL: Implied Volatility - measure of expected price movement
    /// Higher IV = more uncertainty/volatility expected in the market
    /// Calculated from bid-ask spread and price levels
    /// </summary>
    [JsonPropertyName("implied_volatility")]
    public decimal ImpliedVolatility { get; set; }

    [JsonPropertyName("volume_24h")]
    public decimal Volume24h { get; set; }

    [JsonPropertyName("liquidity")]
    public decimal Liquidity { get; set; }

    [JsonPropertyName("outcome")]
    public string Outcome { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("tags")]
    public List<string> Tags { get; set; } = new();

    public DateTime TimestampDateTime => DateTimeOffset.FromUnixTimeMilliseconds(Timestamp).DateTime;
}

/// <summary>
/// Trade execution data from WebSocket
/// </summary>
public class TradeUpdate
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("market_id")]
    public string MarketId { get; set; } = string.Empty;

    [JsonPropertyName("asset_id")]
    public string AssetId { get; set; } = string.Empty;

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("size")]
    public decimal Size { get; set; }

    [JsonPropertyName("side")]
    public string Side { get; set; } = string.Empty; // "BUY" or "SELL"

    [JsonPropertyName("outcome")]
    public string Outcome { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }

    [JsonPropertyName("maker_address")]
    public string? MakerAddress { get; set; }

    [JsonPropertyName("taker_address")]
    public string? TakerAddress { get; set; }

    public DateTime TimestampDateTime => DateTimeOffset.FromUnixTimeMilliseconds(Timestamp).DateTime;
}

/// <summary>
/// Price change notification
/// </summary>
public class PriceChangeUpdate
{
    [JsonPropertyName("asset_id")]
    public string AssetId { get; set; } = string.Empty;

    [JsonPropertyName("market_id")]
    public string MarketId { get; set; } = string.Empty;

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }

    public DateTime TimestampDateTime => DateTimeOffset.FromUnixTimeMilliseconds(Timestamp).DateTime;
}

/// <summary>
/// Order book aggregated data
/// </summary>
public class OrderBookUpdate
{
    [JsonPropertyName("asset_id")]
    public string AssetId { get; set; } = string.Empty;

    [JsonPropertyName("market_id")]
    public string MarketId { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }

    [JsonPropertyName("bids")]
    public List<OrderBookLevel> Bids { get; set; } = new();

    [JsonPropertyName("asks")]
    public List<OrderBookLevel> Asks { get; set; } = new();

    public DateTime TimestampDateTime => DateTimeOffset.FromUnixTimeMilliseconds(Timestamp).DateTime;
}

/// <summary>
/// Order book price level
/// </summary>
public class OrderBookLevel
{
    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("size")]
    public decimal Size { get; set; }
}

/// <summary>
/// Generic WebSocket message wrapper
/// </summary>
public class WebSocketMessage
{
    [JsonPropertyName("event_type")]
    public string EventType { get; set; } = string.Empty;

    [JsonPropertyName("topic")]
    public string Topic { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public object? Data { get; set; }

    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }
}

/// <summary>
/// Tag metadata for categorization
/// </summary>
public class Tag
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    [JsonPropertyName("slug")]
    public string Slug { get; set; } = string.Empty;
}
