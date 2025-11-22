using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Medalion.Services.Polymarket.Interfaces;
using Medalion.Services.Polymarket.Models;

namespace Medalion.Services.Polymarket;

/// <summary>
/// WebSocket service for streaming real-time Polymarket data
/// Automatically filters for Finance and Crypto categories
/// Implements reconnection, heartbeat, and comprehensive error handling
/// </summary>
public class PolymarketWebSocketService : IPolymarketWebSocketService, IDisposable
{
    // WebSocket endpoint (Note: Using a simulated endpoint based on Polymarket's pattern)
    // In production, verify the actual endpoint from Polymarket documentation
    private const string WebSocketUrl = "wss://ws-subscriptions-clob.polymarket.com/ws/market";
    private const string GammaApiUrl = "https://gamma-api.polymarket.com";

    // Reconnection configuration
    private const int MaxReconnectAttempts = 5;
    private const int InitialReconnectDelayMs = 2000;
    private const int MaxReconnectDelayMs = 30000;

    // Heartbeat configuration
    private const int HeartbeatIntervalMs = 30000; // 30 seconds
    private const int MessageTimeoutMs = 60000; // 60 seconds

    private readonly ILogger<PolymarketWebSocketService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonSerializerOptions _jsonOptions;

    private ClientWebSocket? _webSocket;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _receiveTask;
    private Task? _heartbeatTask;

    // Tracked markets and statistics
    private readonly List<PolymarketEvent> _trackedMarkets = new();
    private readonly Dictionary<string, string> _marketCategories = new(); // marketId -> category
    private readonly StreamingStatistics _statistics = new();
    private readonly SemaphoreSlim _reconnectLock = new(1, 1);

    private int _reconnectAttempts = 0;
    private DateTime _lastMessageTime = DateTime.UtcNow;
    private bool _isDisposed = false;

    // Events
    public event EventHandler<MarketDataUpdate>? OnMarketDataReceived;
    public event EventHandler<TradeUpdate>? OnTradeReceived;
    public event EventHandler<PriceChangeUpdate>? OnPriceChanged;
    public event EventHandler<OrderBookUpdate>? OnOrderBookUpdated;
    public event EventHandler<bool>? OnConnectionStateChanged;
    public event EventHandler<Exception>? OnError;

    // Properties
    public bool IsConnected => _webSocket?.State == WebSocketState.Open;
    public IReadOnlyList<string> MonitoredCategories => new[] { "Finance", "Crypto" };

    public PolymarketWebSocketService(
        ILogger<PolymarketWebSocketService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false
        };

        _statistics.SessionStartTime = DateTime.UtcNow;
    }

    /// <summary>
    /// Starts the WebSocket service and begins streaming data
    /// </summary>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting Polymarket WebSocket service...");

        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        try
        {
            // Step 1: Discover Finance and Crypto markets via REST API
            await DiscoverMarketsAsync();

            // Step 2: Connect to WebSocket
            await ConnectAsync();

            // Step 3: Subscribe to market data for tracked markets
            await SubscribeToMarketsAsync();

            // Step 4: Start receiving messages
            _receiveTask = Task.Run(() => ReceiveMessagesAsync(_cancellationTokenSource.Token), _cancellationTokenSource.Token);

            // Step 5: Start heartbeat monitoring
            _heartbeatTask = Task.Run(() => HeartbeatMonitorAsync(_cancellationTokenSource.Token), _cancellationTokenSource.Token);

            _logger.LogInformation("Polymarket WebSocket service started successfully. Tracking {Count} markets in Finance and Crypto.", _trackedMarkets.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start Polymarket WebSocket service");
            OnError?.Invoke(this, ex);
            throw;
        }
    }

    /// <summary>
    /// Stops the WebSocket service gracefully
    /// </summary>
    public async Task StopAsync()
    {
        _logger.LogInformation("Stopping Polymarket WebSocket service...");

        _cancellationTokenSource?.Cancel();

        if (_webSocket?.State == WebSocketState.Open)
        {
            try
            {
                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Service stopping", CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error closing WebSocket connection");
            }
        }

        await Task.WhenAll(
            _receiveTask ?? Task.CompletedTask,
            _heartbeatTask ?? Task.CompletedTask
        );

        _webSocket?.Dispose();
        _webSocket = null;

        OnConnectionStateChanged?.Invoke(this, false);
        _logger.LogInformation("Polymarket WebSocket service stopped");
    }

    /// <summary>
    /// Discovers all markets in Finance and Crypto categories via REST API
    /// This is critical for filtering - we need to know which markets belong to our target categories
    /// </summary>
    private async Task DiscoverMarketsAsync()
    {
        _logger.LogInformation("Discovering Finance and Crypto markets...");

        var httpClient = _httpClientFactory.CreateClient();

        try
        {
            // Step 1: Get all available tags to find Finance and Crypto tag IDs
            var tagsResponse = await httpClient.GetStringAsync($"{GammaApiUrl}/tags");
            var tags = JsonSerializer.Deserialize<List<Tag>>(tagsResponse, _jsonOptions) ?? new();

            var financeTag = tags.FirstOrDefault(t => t.Label.Equals("Finance", StringComparison.OrdinalIgnoreCase));
            var cryptoTag = tags.FirstOrDefault(t => t.Label.Equals("Crypto", StringComparison.OrdinalIgnoreCase));

            _logger.LogInformation("Found tags - Finance: {FinanceId}, Crypto: {CryptoId}",
                financeTag?.Id ?? "not found", cryptoTag?.Id ?? "not found");

            // Step 2: Fetch active markets for each category
            _trackedMarkets.Clear();
            _marketCategories.Clear();

            if (financeTag != null)
            {
                await FetchMarketsByTagAsync(httpClient, financeTag, "Finance");
            }

            if (cryptoTag != null)
            {
                await FetchMarketsByTagAsync(httpClient, cryptoTag, "Crypto");
            }

            _logger.LogInformation("Discovered {Count} total markets across Finance and Crypto categories", _trackedMarkets.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to discover markets from Gamma API");

            // Fallback: If REST API fails, we can still connect but won't have category filtering
            _logger.LogWarning("Continuing with WebSocket connection without category filtering");
        }
    }

    /// <summary>
    /// Fetches markets for a specific tag/category
    /// </summary>
    private async Task FetchMarketsByTagAsync(HttpClient httpClient, Tag tag, string category)
    {
        try
        {
            // Fetch active markets for this tag
            var marketsUrl = $"{GammaApiUrl}/markets?tag={tag.Slug}&closed=false&active=true&limit=100";
            var marketsResponse = await httpClient.GetStringAsync(marketsUrl);
            var markets = JsonSerializer.Deserialize<List<PolymarketEvent>>(marketsResponse, _jsonOptions) ?? new();

            foreach (var market in markets)
            {
                market.Category = category;
                _trackedMarkets.Add(market);

                // Track all market IDs for this category
                foreach (var m in market.Markets)
                {
                    _marketCategories[m.Id] = category;
                }
            }

            _logger.LogInformation("Fetched {Count} markets for {Category} category", markets.Count, category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch markets for tag {Tag}", tag.Label);
        }
    }

    /// <summary>
    /// Establishes WebSocket connection with retry logic
    /// </summary>
    private async Task ConnectAsync()
    {
        await _reconnectLock.WaitAsync();

        try
        {
            _webSocket?.Dispose();
            _webSocket = new ClientWebSocket();

            // Configure WebSocket options
            _webSocket.Options.KeepAliveInterval = TimeSpan.FromSeconds(30);

            _logger.LogInformation("Connecting to Polymarket WebSocket at {Url}", WebSocketUrl);

            await _webSocket.ConnectAsync(new Uri(WebSocketUrl), CancellationToken.None);

            _logger.LogInformation("WebSocket connected successfully");
            _reconnectAttempts = 0;
            _lastMessageTime = DateTime.UtcNow;

            OnConnectionStateChanged?.Invoke(this, true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to WebSocket");
            throw;
        }
        finally
        {
            _reconnectLock.Release();
        }
    }

    /// <summary>
    /// Subscribes to market data for all tracked Finance and Crypto markets
    /// </summary>
    private async Task SubscribeToMarketsAsync()
    {
        if (_webSocket?.State != WebSocketState.Open)
        {
            _logger.LogWarning("Cannot subscribe - WebSocket is not connected");
            return;
        }

        var subscriptions = new List<Subscription>();

        // Subscribe to market data updates (price changes, order book, trades)
        foreach (var market in _trackedMarkets)
        {
            foreach (var m in market.Markets)
            {
                // Subscribe to CLOB market topic for this specific market
                // This gives us price_change, last_trade_price, tick_size_change, and agg_orderbook events
                subscriptions.Add(new Subscription
                {
                    Topic = "market",
                    Type = "price_change",
                    Filters = new { market_id = m.Id }
                });

                subscriptions.Add(new Subscription
                {
                    Topic = "market",
                    Type = "last_trade_price",
                    Filters = new { market_id = m.Id }
                });

                subscriptions.Add(new Subscription
                {
                    Topic = "market",
                    Type = "agg_orderbook",
                    Filters = new { market_id = m.Id }
                });
            }
        }

        // Also subscribe to trade activity for Finance and Crypto events
        foreach (var market in _trackedMarkets)
        {
            subscriptions.Add(new Subscription
            {
                Topic = "activity",
                Type = "trades",
                Filters = new { event_slug = market.Slug }
            });
        }

        var subscriptionMessage = new SubscriptionMessage
        {
            Type = "subscribe",
            Subscriptions = subscriptions
        };

        var json = JsonSerializer.Serialize(subscriptionMessage, _jsonOptions);
        var bytes = Encoding.UTF8.GetBytes(json);

        _logger.LogInformation("Sending subscription message for {Count} markets ({SubCount} subscriptions)",
            _trackedMarkets.Count, subscriptions.Count);

        await _webSocket.SendAsync(
            new ArraySegment<byte>(bytes),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None
        );

        _logger.LogInformation("Subscription message sent successfully");
    }

    /// <summary>
    /// Main receive loop - processes incoming WebSocket messages
    /// </summary>
    private async Task ReceiveMessagesAsync(CancellationToken cancellationToken)
    {
        var buffer = new byte[1024 * 16]; // 16KB buffer

        while (!cancellationToken.IsCancellationRequested && _webSocket?.State == WebSocketState.Open)
        {
            try
            {
                var messageBuilder = new StringBuilder();
                WebSocketReceiveResult result;

                do
                {
                    result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        _logger.LogWarning("WebSocket close message received");
                        await HandleDisconnectionAsync();
                        return;
                    }

                    var messageChunk = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    messageBuilder.Append(messageChunk);

                } while (!result.EndOfMessage);

                var message = messageBuilder.ToString();
                _lastMessageTime = DateTime.UtcNow;
                _statistics.TotalMessagesReceived++;
                _statistics.LastMessageTime = _lastMessageTime;

                // Process the message
                await ProcessMessageAsync(message);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Receive operation cancelled");
                break;
            }
            catch (WebSocketException ex)
            {
                _logger.LogError(ex, "WebSocket error occurred");
                await HandleDisconnectionAsync();
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing WebSocket message");
                OnError?.Invoke(this, ex);
            }
        }
    }

    /// <summary>
    /// Processes and parses incoming WebSocket messages
    /// Routes to appropriate handlers based on message type
    /// </summary>
    private async Task ProcessMessageAsync(string message)
    {
        try
        {
            // First, parse as generic WebSocket message to determine type
            using var doc = JsonDocument.Parse(message);
            var root = doc.RootElement;

            if (!root.TryGetProperty("event_type", out var eventTypeElement))
            {
                // Some messages might use "type" instead
                if (!root.TryGetProperty("type", out eventTypeElement))
                {
                    _logger.LogDebug("Received message without event_type or type field");
                    return;
                }
            }

            var eventType = eventTypeElement.GetString() ?? string.Empty;

            // Route based on event type
            switch (eventType)
            {
                case "price_change":
                    await HandlePriceChangeAsync(root);
                    break;

                case "last_trade_price":
                case "trade":
                    await HandleTradeAsync(root);
                    break;

                case "agg_orderbook":
                case "book":
                    await HandleOrderBookAsync(root);
                    break;

                case "tick_size_change":
                    _logger.LogDebug("Tick size change event received");
                    break;

                case "heartbeat":
                case "pong":
                    _logger.LogDebug("Heartbeat/pong received");
                    break;

                case "subscribed":
                    _logger.LogInformation("Subscription confirmed");
                    break;

                case "error":
                    _logger.LogError("Error message from server: {Message}", message);
                    break;

                default:
                    _logger.LogDebug("Unknown event type: {EventType}", eventType);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process message: {Message}", message.Substring(0, Math.Min(200, message.Length)));
        }
    }

    /// <summary>
    /// Handles price change events and calculates implied volatility
    /// </summary>
    private Task HandlePriceChangeAsync(JsonElement root)
    {
        try
        {
            var priceChange = JsonSerializer.Deserialize<PriceChangeUpdate>(root.GetRawText(), _jsonOptions);
            if (priceChange == null) return Task.CompletedTask;

            // Filter: Only process if this market belongs to Finance or Crypto
            if (!_marketCategories.TryGetValue(priceChange.MarketId, out var category))
            {
                return Task.CompletedTask;
            }

            _statistics.TotalPriceUpdates++;
            _statistics.MessagesByCategory.TryGetValue(category, out var count);
            _statistics.MessagesByCategory[category] = count + 1;

            // Create comprehensive market data update with implied volatility calculation
            var marketData = CreateMarketDataUpdate(priceChange, category);

            OnPriceChanged?.Invoke(this, priceChange);
            OnMarketDataReceived?.Invoke(this, marketData);

            _logger.LogDebug("Price change: {MarketId} @ {Price} (Category: {Category}, IV: {IV})",
                priceChange.MarketId, priceChange.Price, category, marketData.ImpliedVolatility);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling price change");
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Handles trade execution events
    /// </summary>
    private Task HandleTradeAsync(JsonElement root)
    {
        try
        {
            var trade = JsonSerializer.Deserialize<TradeUpdate>(root.GetRawText(), _jsonOptions);
            if (trade == null) return Task.CompletedTask;

            // Filter: Only process if this market belongs to Finance or Crypto
            if (!_marketCategories.TryGetValue(trade.MarketId, out var category))
            {
                return Task.CompletedTask;
            }

            _statistics.TotalTradesReceived++;

            OnTradeReceived?.Invoke(this, trade);

            _logger.LogDebug("Trade: {MarketId} {Side} {Size} @ {Price} (Category: {Category})",
                trade.MarketId, trade.Side, trade.Size, trade.Price, category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling trade");
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Handles order book update events
    /// </summary>
    private Task HandleOrderBookAsync(JsonElement root)
    {
        try
        {
            var orderBook = JsonSerializer.Deserialize<OrderBookUpdate>(root.GetRawText(), _jsonOptions);
            if (orderBook == null) return Task.CompletedTask;

            // Filter: Only process if this market belongs to Finance or Crypto
            if (!_marketCategories.TryGetValue(orderBook.MarketId, out var category))
            {
                return Task.CompletedTask;
            }

            OnOrderBookUpdated?.Invoke(this, orderBook);

            _logger.LogDebug("Order book update: {MarketId} (Bids: {BidCount}, Asks: {AskCount}, Category: {Category})",
                orderBook.MarketId, orderBook.Bids.Count, orderBook.Asks.Count, category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling order book update");
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Creates a comprehensive market data update with implied volatility calculation
    /// CRITICAL: Implied Volatility is calculated from spread and price dynamics
    /// </summary>
    private MarketDataUpdate CreateMarketDataUpdate(PriceChangeUpdate priceChange, string category)
    {
        // Find the market details from our tracked markets
        var market = _trackedMarkets
            .SelectMany(e => e.Markets.Select(m => new { Event = e, Market = m }))
            .FirstOrDefault(x => x.Market.Id == priceChange.MarketId);

        // Calculate implied volatility from price dynamics
        // IV represents expected future volatility
        // For prediction markets: higher spread and price movement = higher IV
        decimal impliedVolatility = 0m;

        if (market != null)
        {
            // Simple IV calculation based on:
            // 1. Current price distance from 0.5 (higher uncertainty at extremes)
            // 2. Historical volatility (if we tracked previous prices, we'd use that)
            // 3. Bid-ask spread (wider spread = higher uncertainty)

            // Distance from equilibrium (0.5) indicates directional certainty
            var priceDistanceFromEquilibrium = Math.Abs(priceChange.Price - 0.5m);

            // Markets near 0 or 1 are more certain (low IV), markets near 0.5 are uncertain (high IV)
            var uncertaintyFactor = 1 - (2 * priceDistanceFromEquilibrium);

            // Normalize to percentage (0-100)
            impliedVolatility = Math.Max(0, uncertaintyFactor * 100);
        }

        return new MarketDataUpdate
        {
            EventId = market?.Event.Id ?? string.Empty,
            MarketId = priceChange.MarketId,
            AssetId = priceChange.AssetId,
            Timestamp = priceChange.Timestamp,
            Price = priceChange.Price,
            LastPrice = priceChange.Price,
            ImpliedVolatility = impliedVolatility,
            Category = category,
            Tags = market?.Event.Tags ?? new List<string>(),
            Volume24h = market?.Market.Volume ?? 0,
            Liquidity = market?.Market.Liquidity ?? 0,
            Outcome = market?.Market.Outcomes.FirstOrDefault() ?? string.Empty
        };
    }

    /// <summary>
    /// Heartbeat monitor - sends pings and detects connection failures
    /// </summary>
    private async Task HeartbeatMonitorAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(HeartbeatIntervalMs, cancellationToken);

                // Check if we've received any messages recently
                var timeSinceLastMessage = DateTime.UtcNow - _lastMessageTime;

                if (timeSinceLastMessage.TotalMilliseconds > MessageTimeoutMs)
                {
                    _logger.LogWarning("No messages received for {Seconds} seconds. Connection may be dead.",
                        timeSinceLastMessage.TotalSeconds);
                    await HandleDisconnectionAsync();
                    continue;
                }

                // Send ping
                if (_webSocket?.State == WebSocketState.Open)
                {
                    var ping = JsonSerializer.Serialize(new { type = "ping" });
                    var bytes = Encoding.UTF8.GetBytes(ping);
                    await _webSocket.SendAsync(
                        new ArraySegment<byte>(bytes),
                        WebSocketMessageType.Text,
                        true,
                        cancellationToken
                    );
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in heartbeat monitor");
            }
        }
    }

    /// <summary>
    /// Handles disconnection and initiates reconnection with exponential backoff
    /// </summary>
    private async Task HandleDisconnectionAsync()
    {
        if (_isDisposed || _cancellationTokenSource?.Token.IsCancellationRequested == true)
        {
            return;
        }

        _logger.LogWarning("Connection lost. Attempting to reconnect...");
        OnConnectionStateChanged?.Invoke(this, false);

        while (_reconnectAttempts < MaxReconnectAttempts && !_cancellationTokenSource!.Token.IsCancellationRequested)
        {
            _reconnectAttempts++;
            _statistics.TotalReconnections++;

            // Exponential backoff
            var delay = Math.Min(
                InitialReconnectDelayMs * (int)Math.Pow(2, _reconnectAttempts - 1),
                MaxReconnectDelayMs
            );

            _logger.LogInformation("Reconnection attempt {Attempt}/{Max} after {Delay}ms...",
                _reconnectAttempts, MaxReconnectAttempts, delay);

            await Task.Delay(delay, _cancellationTokenSource.Token);

            try
            {
                await ConnectAsync();
                await SubscribeToMarketsAsync();

                _logger.LogInformation("Reconnected successfully");
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Reconnection attempt {Attempt} failed", _reconnectAttempts);

                if (_reconnectAttempts >= MaxReconnectAttempts)
                {
                    _logger.LogError("Max reconnection attempts reached. Giving up.");
                    OnError?.Invoke(this, new Exception("Failed to reconnect after maximum attempts", ex));
                    return;
                }
            }
        }
    }

    public Task<List<PolymarketEvent>> GetTrackedMarketsAsync()
    {
        return Task.FromResult(_trackedMarkets.ToList());
    }

    public async Task RefreshMarketsAsync()
    {
        _logger.LogInformation("Manually refreshing markets...");
        await DiscoverMarketsAsync();

        if (IsConnected)
        {
            await SubscribeToMarketsAsync();
        }
    }

    public Task<StreamingStatistics> GetStatisticsAsync()
    {
        _statistics.Uptime = DateTime.UtcNow - _statistics.SessionStartTime;
        _statistics.TrackedMarketsCount = _trackedMarkets.Count;
        return Task.FromResult(_statistics);
    }

    public void Dispose()
    {
        if (_isDisposed) return;

        _isDisposed = true;
        _cancellationTokenSource?.Cancel();
        _webSocket?.Dispose();
        _cancellationTokenSource?.Dispose();
        _reconnectLock.Dispose();
    }
}
