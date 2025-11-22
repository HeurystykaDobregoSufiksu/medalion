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
