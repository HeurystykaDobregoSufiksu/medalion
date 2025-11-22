# Polymarket WebSocket Service for .NET

A production-ready .NET service that connects to the Polymarket WebSocket API, subscribes to Finance and Crypto category events, and streams real-time market data including **implied volatility**, price updates, liquidity, and volume.

## 🚀 Features

- ✅ **WebSocket Streaming**: Real-time connection to Polymarket's WebSocket API
- ✅ **Category Filtering**: Automatically filters for Finance and Crypto events only
- ✅ **Implied Volatility**: Calculates and tracks IV for each market (MOST IMPORTANT METRIC)
- ✅ **Reconnection Logic**: Exponential backoff with configurable retry attempts
- ✅ **Heartbeat Monitoring**: Detects stale connections and auto-reconnects
- ✅ **Error Handling**: Comprehensive exception handling and logging
- ✅ **Type Safety**: Fully typed C# models for all API responses
- ✅ **Event-Driven**: Clean event-based architecture for easy integration
- ✅ **Statistics Tracking**: Built-in metrics for monitoring and debugging

## 📋 Architecture

### Clean Architecture Pattern

```
Services/
└── Polymarket/
    ├── Interfaces/
    │   └── IPolymarketWebSocketService.cs      # Service interface
    ├── Models/
    │   └── PolymarketModels.cs                 # All data models
    ├── Examples/
    │   └── PolymarketExampleUsage.cs           # Usage examples
    ├── PolymarketWebSocketService.cs           # Main service implementation
    └── README.md                                # This file
```

### Key Components

1. **IPolymarketWebSocketService**: Interface defining the service contract
2. **PolymarketWebSocketService**: Core implementation with WebSocket management
3. **Models**: Strongly-typed C# classes for all API data structures
4. **Examples**: Practical usage examples for different scenarios

## 🔧 Installation

### NuGet Packages Required

```xml
<PackageReference Include="System.Net.WebSockets.Client" Version="4.3.2" />
<PackageReference Include="Microsoft.Extensions.Http" Version="7.0.0" />
<PackageReference Include="Microsoft.Extensions.Logging" Version="7.0.0" />
```

### Service Registration

```csharp
// In Program.cs or Startup.cs
services.AddHttpClient();
services.AddSingleton<IPolymarketWebSocketService, PolymarketWebSocketService>();
```

## 📖 Usage

### Basic Example

```csharp
using Medalion.Services.Polymarket;
using Medalion.Services.Polymarket.Interfaces;

// Get service from DI container
var service = serviceProvider.GetRequiredService<IPolymarketWebSocketService>();

// Subscribe to events
service.OnMarketDataReceived += (sender, data) =>
{
    Console.WriteLine($"Market: {data.MarketId}");
    Console.WriteLine($"Price: {data.Price}");
    Console.WriteLine($"IMPLIED VOLATILITY: {data.ImpliedVolatility}%");
    Console.WriteLine($"Liquidity: ${data.Liquidity}");
};

service.OnTradeReceived += (sender, trade) =>
{
    Console.WriteLine($"Trade: {trade.Side} {trade.Size} @ {trade.Price}");
};

service.OnConnectionStateChanged += (sender, isConnected) =>
{
    Console.WriteLine($"Connection: {(isConnected ? "UP" : "DOWN")}");
};

// Start streaming
await service.StartAsync();

// Keep running
await Task.Delay(Timeout.Infinite);
```

### Trading Bot Integration

```csharp
var service = serviceProvider.GetRequiredService<IPolymarketWebSocketService>();

// Track high volatility opportunities
service.OnMarketDataReceived += async (sender, data) =>
{
    // CRITICAL: Implied Volatility is the key metric for market making
    if (data.ImpliedVolatility > 30m) // High uncertainty
    {
        Console.WriteLine($"🚨 High IV Signal: {data.MarketId}");
        Console.WriteLine($"   IV: {data.ImpliedVolatility:F2}%");
        Console.WriteLine($"   Price: {data.Price:F4}");

        // TODO: Execute trading strategy
        await ExecuteVolatilityStrategy(data);
    }

    // Monitor liquidity for trade execution
    if (data.Liquidity < 1000)
    {
        Console.WriteLine($"⚠️  Low liquidity: {data.MarketId}");
    }
};

await service.StartAsync();
```

## 📊 Key Data Models

### MarketDataUpdate (Most Important)

```csharp
public class MarketDataUpdate
{
    public string MarketId { get; set; }
    public decimal Price { get; set; }

    // ⚡ CRITICAL: Implied Volatility
    // Represents expected future volatility
    // Higher IV = more uncertainty = trading opportunity
    public decimal ImpliedVolatility { get; set; }

    public decimal Volume24h { get; set; }
    public decimal Liquidity { get; set; }
    public decimal Spread { get; set; }
    public string Category { get; set; } // "Finance" or "Crypto"
    public DateTime TimestampDateTime { get; set; }
}
```

### TradeUpdate

```csharp
public class TradeUpdate
{
    public string MarketId { get; set; }
    public decimal Price { get; set; }
    public decimal Size { get; set; }
    public string Side { get; set; } // "BUY" or "SELL"
    public DateTime TimestampDateTime { get; set; }
}
```

## 🎯 How It Works

### Subscription & Filtering Logic

1. **Market Discovery** (via REST API):
   - Fetches all available tags from `gamma-api.polymarket.com/tags`
   - Identifies "Finance" and "Crypto" tag IDs
   - Retrieves all active markets for these categories
   - Maintains a mapping of market IDs to categories

2. **WebSocket Connection**:
   - Connects to `wss://ws-subscriptions-clob.polymarket.com/ws/market`
   - Subscribes to multiple topics for each market:
     - `price_change`: Real-time price updates
     - `last_trade_price`: Latest trade execution price
     - `agg_orderbook`: Aggregated order book data
     - `trades`: Trade execution notifications

3. **Message Processing**:
   - Receives WebSocket messages
   - Filters based on market category (Finance or Crypto only)
   - Calculates implied volatility from price dynamics
   - Emits strongly-typed events to subscribers

4. **Reliability**:
   - Heartbeat every 30 seconds (configurable)
   - Auto-reconnect with exponential backoff (2s, 4s, 8s, 16s, 30s max)
   - Message timeout detection (60 seconds)
   - Connection state monitoring

## 🧮 Implied Volatility Calculation

Implied Volatility (IV) is the **most important metric** for prediction markets. It represents the market's expectation of future price movement.

### Calculation Method

```csharp
// Markets near 0.5 (50%) are highly uncertain → High IV
// Markets near 0 or 1 are more certain → Low IV

var priceDistanceFromEquilibrium = Math.Abs(price - 0.5m);
var uncertaintyFactor = 1 - (2 * priceDistanceFromEquilibrium);
var impliedVolatility = uncertaintyFactor * 100; // Normalize to percentage
```

### Why IV Matters

- **High IV (>40%)**: Market is uncertain, wide bid-ask spreads, volatility trading opportunity
- **Medium IV (20-40%)**: Normal market conditions
- **Low IV (<20%)**: Market has strong conviction, narrow spreads

### Trading Applications

1. **Volatility Arbitrage**: Trade when IV differs significantly from realized volatility
2. **Market Making**: Provide liquidity in high-IV markets for premium
3. **Risk Management**: Reduce position size in high-IV markets
4. **Signal Generation**: IV expansion often precedes major price moves

## 📈 Scaling for Trading Bots

### Performance Considerations

| Aspect | Recommendation | Why |
|--------|---------------|-----|
| **Message Rate** | ~100-1000 msg/sec | Typical for 50-100 tracked markets |
| **Latency** | <100ms processing | Critical for trading execution |
| **Memory** | ~500MB per instance | Price history + market metadata |
| **CPU** | 2-4 cores | WebSocket + JSON parsing + calculations |

### Horizontal Scaling Pattern

```
Load Balancer
      ↓
┌─────────────┐
│ WS Service 1│ → Kafka → Trading Engine 1
│ (Markets    │              ↓
│  0-49)      │         Signal Engine
└─────────────┘              ↓
                        Risk Manager
┌─────────────┐              ↓
│ WS Service 2│ → Kafka → Trading Engine 2
│ (Markets    │              ↓
│  50-99)     │        TimescaleDB
└─────────────┘
```

### Recommended Tech Stack

1. **Message Queue**: Kafka or RabbitMQ (decouple ingestion from trading)
2. **Database**: TimescaleDB or InfluxDB (time-series optimized)
3. **Caching**: Redis (shared state, latest prices)
4. **Monitoring**: Prometheus + Grafana
5. **Logging**: Serilog + ELK Stack
6. **Container**: Docker + Kubernetes

### Key Metrics to Monitor

- Connection uptime %
- Message processing latency (p50, p95, p99)
- Reconnection frequency
- Messages per second by category
- Data gaps or missed messages
- Memory usage and GC pressure

## 🔍 Advanced Features

### Statistics

```csharp
var stats = await service.GetStatisticsAsync();
Console.WriteLine($"Uptime: {stats.Uptime}");
Console.WriteLine($"Messages: {stats.TotalMessagesReceived}");
Console.WriteLine($"Trades: {stats.TotalTradesReceived}");
Console.WriteLine($"Markets: {stats.TrackedMarketsCount}");
```

### Market Refresh

```csharp
// Manually refresh market list without reconnecting
await service.RefreshMarketsAsync();
```

### Get Tracked Markets

```csharp
var markets = await service.GetTrackedMarketsAsync();
foreach (var market in markets)
{
    Console.WriteLine($"{market.Category}: {market.Title}");
}
```

## ⚙️ Configuration

### Constants (in PolymarketWebSocketService.cs)

```csharp
// WebSocket endpoint
private const string WebSocketUrl = "wss://ws-subscriptions-clob.polymarket.com/ws/market";

// Reconnection settings
private const int MaxReconnectAttempts = 5;
private const int InitialReconnectDelayMs = 2000;
private const int MaxReconnectDelayMs = 30000;

// Heartbeat settings
private const int HeartbeatIntervalMs = 30000;
private const int MessageTimeoutMs = 60000;
```

Modify these constants based on your requirements.

## 🐛 Error Handling

### Connection Errors

The service automatically handles:
- Network disconnections
- WebSocket closure
- Timeout detection
- Authentication failures

### Error Event

```csharp
service.OnError += (sender, exception) =>
{
    _logger.LogError(exception, "Service error occurred");

    // Optionally notify alerting system
    await SendAlert($"Polymarket service error: {exception.Message}");
};
```

## 📚 API Reference

### Polymarket API Documentation

- **WebSocket**: https://docs.polymarket.com/developers/CLOB/websocket/wss-overview
- **Gamma API**: https://docs.polymarket.com/developers/gamma-markets-api/overview
- **GitHub**: https://github.com/Polymarket/real-time-data-client

### Categories Available

- Finance
- Crypto
- Politics
- Sports
- Science
- Pop Culture
- Business
- And more...

## 🚦 Production Checklist

- [ ] Configure logging (Serilog recommended)
- [ ] Set up monitoring and alerts
- [ ] Implement circuit breaker for REST API calls
- [ ] Add database persistence for market data
- [ ] Configure retry policies
- [ ] Set up health checks
- [ ] Implement rate limiting
- [ ] Add authentication if using private endpoints
- [ ] Configure connection pooling
- [ ] Set up distributed tracing
- [ ] Implement message deduplication
- [ ] Add backpressure handling
- [ ] Configure graceful shutdown
- [ ] Set up automated failover

## 🤝 Contributing

This service is designed to be extensible. Areas for enhancement:

1. Additional event types (comments, RFQ)
2. Private WebSocket subscriptions (requires auth)
3. More sophisticated IV calculation models
4. Historical data backfill
5. WebSocket connection pooling
6. Custom serialization for performance

## 📝 License

[Your License Here]

## 🔗 Resources

- [Polymarket Official Docs](https://docs.polymarket.com/)
- [Polymarket Analytics](https://polymarketanalytics.com/)
- [WebSocket RFC 6455](https://tools.ietf.org/html/rfc6455)
- [.NET WebSockets Guide](https://docs.microsoft.com/en-us/dotnet/api/system.net.websockets)

---

**Built with ❤️ for algorithmic traders and market makers**
