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

// ================== TRADING MODELS ==================

/// <summary>
/// Order side enumeration
/// </summary>
public enum OrderSide
{
    Buy,
    Sell
}

/// <summary>
/// Order type enumeration
/// </summary>
public enum OrderType
{
    /// <summary>
    /// Good Till Cancel - Limit order that stays on the book until filled or cancelled
    /// </summary>
    GTC,

    /// <summary>
    /// Fill or Kill - Market order that executes immediately or fails
    /// </summary>
    FOK,

    /// <summary>
    /// Good Till Date - Order expires at a specific time
    /// </summary>
    GTD
}

/// <summary>
/// Request to create a market order (Fill or Kill)
/// Market orders execute immediately at best available price
/// </summary>
public class MarketOrderRequest
{
    /// <summary>
    /// Token ID of the outcome to trade
    /// </summary>
    public string TokenId { get; set; } = string.Empty;

    /// <summary>
    /// Dollar amount to spend (for buy) or shares value (for sell)
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Side of the order (Buy or Sell)
    /// </summary>
    public OrderSide Side { get; set; }
}

/// <summary>
/// Request to create a limit order (Good Till Cancel)
/// Limit orders rest on the order book at specified price
/// </summary>
public class LimitOrderRequest
{
    /// <summary>
    /// Token ID of the outcome to trade
    /// </summary>
    public string TokenId { get; set; } = string.Empty;

    /// <summary>
    /// Price per share (0.01 to 0.99 for binary outcomes)
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Number of shares to trade
    /// </summary>
    public decimal Size { get; set; }

    /// <summary>
    /// Side of the order (Buy or Sell)
    /// </summary>
    public OrderSide Side { get; set; }

    /// <summary>
    /// Optional: Expiration timestamp (for GTD orders)
    /// </summary>
    public long? Expiration { get; set; }
}

/// <summary>
/// Signed order ready to be posted to the CLOB
/// This is the actual order format expected by Polymarket's API
/// </summary>
public class SignedOrder
{
    [JsonPropertyName("orderType")]
    public string OrderType { get; set; } = string.Empty;

    [JsonPropertyName("salt")]
    public string Salt { get; set; } = string.Empty;

    [JsonPropertyName("maker")]
    public string Maker { get; set; } = string.Empty;

    [JsonPropertyName("signer")]
    public string Signer { get; set; } = string.Empty;

    [JsonPropertyName("taker")]
    public string Taker { get; set; } = string.Empty;

    [JsonPropertyName("tokenId")]
    public string TokenId { get; set; } = string.Empty;

    [JsonPropertyName("makerAmount")]
    public string MakerAmount { get; set; } = string.Empty;

    [JsonPropertyName("takerAmount")]
    public string TakerAmount { get; set; } = string.Empty;

    [JsonPropertyName("side")]
    public string Side { get; set; } = string.Empty;

    [JsonPropertyName("expiration")]
    public string Expiration { get; set; } = string.Empty;

    [JsonPropertyName("nonce")]
    public string Nonce { get; set; } = string.Empty;

    [JsonPropertyName("feeRateBps")]
    public string FeeRateBps { get; set; } = string.Empty;

    [JsonPropertyName("signatureType")]
    public int SignatureType { get; set; }

    [JsonPropertyName("signature")]
    public string Signature { get; set; } = string.Empty;
}

/// <summary>
/// Response from posting an order
/// </summary>
public class OrderResponse
{
    [JsonPropertyName("orderID")]
    public string OrderId { get; set; } = string.Empty;

    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("errorMsg")]
    public string? ErrorMsg { get; set; }

    [JsonPropertyName("transactionHash")]
    public string? TransactionHash { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }
}

/// <summary>
/// Represents an active order on the book
/// </summary>
public class OpenOrder
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("market")]
    public string Market { get; set; } = string.Empty;

    [JsonPropertyName("asset_id")]
    public string AssetId { get; set; } = string.Empty;

    [JsonPropertyName("owner")]
    public string Owner { get; set; } = string.Empty;

    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("original_size")]
    public decimal OriginalSize { get; set; }

    [JsonPropertyName("size_matched")]
    public decimal SizeMatched { get; set; }

    [JsonPropertyName("outcome")]
    public string Outcome { get; set; } = string.Empty;

    [JsonPropertyName("side")]
    public string Side { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("expiration")]
    public long? Expiration { get; set; }

    /// <summary>
    /// Remaining size (original_size - size_matched)
    /// </summary>
    public decimal RemainingSize => OriginalSize - SizeMatched;
}

/// <summary>
/// Request parameters for querying open orders
/// </summary>
public class OpenOrderParams
{
    /// <summary>
    /// Filter by market ID
    /// </summary>
    public string? Market { get; set; }

    /// <summary>
    /// Filter by asset ID (token ID)
    /// </summary>
    public string? AssetId { get; set; }
}

/// <summary>
/// Market price information
/// </summary>
public class PriceInfo
{
    [JsonPropertyName("price")]
    public decimal Price { get; set; }

    [JsonPropertyName("size")]
    public decimal Size { get; set; }

    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }
}

/// <summary>
/// Trading configuration
/// Contains API credentials and settings for trading
/// </summary>
public class TradingConfig
{
    /// <summary>
    /// Private key for signing orders (Ethereum wallet private key)
    /// SECURITY: Never hardcode or commit this to source control
    /// </summary>
    public string PrivateKey { get; set; } = string.Empty;

    /// <summary>
    /// Wallet address (derived from private key)
    /// </summary>
    public string WalletAddress { get; set; } = string.Empty;

    /// <summary>
    /// Chain ID (137 for Polygon mainnet, 80001 for Mumbai testnet)
    /// </summary>
    public int ChainId { get; set; } = 137;

    /// <summary>
    /// Signature type (0 = EOA/MetaMask, 1 = Email/Magic, 2 = Proxy)
    /// </summary>
    public int SignatureType { get; set; } = 0;

    /// <summary>
    /// Optional: Funder address for email/magic wallets
    /// </summary>
    public string? FunderAddress { get; set; }

    /// <summary>
    /// Optional: API key for advanced features
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Optional: API secret for advanced features
    /// </summary>
    public string? ApiSecret { get; set; }

    /// <summary>
    /// Optional: API passphrase for advanced features
    /// </summary>
    public string? ApiPassphrase { get; set; }
}
