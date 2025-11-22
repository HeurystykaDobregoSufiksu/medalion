using Medalion.Services.Polymarket.Models;

namespace Medalion.Services.Polymarket.Interfaces;

/// <summary>
/// Interface for the Polymarket WebSocket service
/// Provides real-time market data streaming with category filtering
/// </summary>
public interface IPolymarketWebSocketService
{
    /// <summary>
    /// Event fired when a market data update is received
    /// Contains price, liquidity, volume, and IMPLIED VOLATILITY
    /// </summary>
    event EventHandler<MarketDataUpdate>? OnMarketDataReceived;

    /// <summary>
    /// Event fired when a trade is executed
    /// </summary>
    event EventHandler<TradeUpdate>? OnTradeReceived;

    /// <summary>
    /// Event fired when a price changes
    /// </summary>
    event EventHandler<PriceChangeUpdate>? OnPriceChanged;

    /// <summary>
    /// Event fired when order book updates are received
    /// </summary>
    event EventHandler<OrderBookUpdate>? OnOrderBookUpdated;

    /// <summary>
    /// Event fired when connection state changes
    /// </summary>
    event EventHandler<bool>? OnConnectionStateChanged;

    /// <summary>
    /// Event fired when an error occurs
    /// </summary>
    event EventHandler<Exception>? OnError;

    /// <summary>
    /// Gets the current connection state
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Gets the list of monitored categories (e.g., "Finance", "Crypto")
    /// </summary>
    IReadOnlyList<string> MonitoredCategories { get; }

    /// <summary>
    /// Indicates whether trading functionality is enabled
    /// </summary>
    bool IsTradingEnabled { get; }

    /// <summary>
    /// Starts the WebSocket connection and begins streaming data
    /// Automatically subscribes to Finance and Crypto categories
    /// </summary>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops the WebSocket connection gracefully
    /// </summary>
    Task StopAsync();

    /// <summary>
    /// Gets all currently tracked markets in Finance and Crypto categories
    /// </summary>
    Task<List<PolymarketEvent>> GetTrackedMarketsAsync();

    /// <summary>
    /// Manually refresh the list of Finance and Crypto markets
    /// Useful for discovering new markets without reconnecting
    /// </summary>
    Task RefreshMarketsAsync();

    /// <summary>
    /// Get statistics about the current streaming session
    /// </summary>
    Task<StreamingStatistics> GetStatisticsAsync();

    // ================== TRADING METHODS ==================

    /// <summary>
    /// Places a market order to buy or sell shares immediately
    /// REQUIRES: Trading configuration to be provided in constructor
    /// </summary>
    Task<OrderResponse> PlaceMarketOrderAsync(string tokenId, decimal amount, OrderSide side);

    /// <summary>
    /// Places a limit order at a specific price
    /// REQUIRES: Trading configuration to be provided in constructor
    /// </summary>
    Task<OrderResponse> PlaceLimitOrderAsync(string tokenId, decimal price, decimal size, OrderSide side, long? expiration = null);

    /// <summary>
    /// Quick helper to buy shares at market price
    /// </summary>
    Task<OrderResponse> BuyAsync(string tokenId, decimal dollarAmount);

    /// <summary>
    /// Quick helper to sell shares at market price
    /// </summary>
    Task<OrderResponse> SellAsync(string tokenId, decimal sharesValue);

    /// <summary>
    /// Cancels a specific order by ID
    /// REQUIRES: Trading configuration to be provided in constructor
    /// </summary>
    Task<bool> CancelOrderAsync(string orderId);

    /// <summary>
    /// Cancels all open orders
    /// REQUIRES: Trading configuration to be provided in constructor
    /// </summary>
    Task<int> CancelAllOrdersAsync();

    /// <summary>
    /// Gets all open orders for the trading wallet
    /// REQUIRES: Trading configuration to be provided in constructor
    /// </summary>
    Task<List<OpenOrder>> GetOpenOrdersAsync(string? marketFilter = null, string? assetFilter = null);

    /// <summary>
    /// Gets the best available price for buying or selling a token
    /// </summary>
    Task<decimal> GetBestPriceAsync(string tokenId, OrderSide side);

    /// <summary>
    /// Gets the midpoint price (average of bid and ask) for a token
    /// </summary>
    Task<decimal> GetMidpointPriceAsync(string tokenId);
}

/// <summary>
/// Statistics about the WebSocket streaming session
/// </summary>
public class StreamingStatistics
{
    public DateTime SessionStartTime { get; set; }
    public TimeSpan Uptime { get; set; }
    public int TotalMessagesReceived { get; set; }
    public int TotalTradesReceived { get; set; }
    public int TotalPriceUpdates { get; set; }
    public int TotalReconnections { get; set; }
    public int TrackedMarketsCount { get; set; }
    public DateTime? LastMessageTime { get; set; }
    public Dictionary<string, int> MessagesByCategory { get; set; } = new();
}
