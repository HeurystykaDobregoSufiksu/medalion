using Medalion.Services.Polymarket;
using Medalion.Services.Polymarket.Interfaces;
using Medalion.Services.Polymarket.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Medalion.Services.Polymarket.Examples;

/// <summary>
/// Example usage of the Polymarket WebSocket service
/// Demonstrates how to integrate into a .NET application or trading bot
/// </summary>
public class PolymarketExampleUsage
{
    /// <summary>
    /// Example 1: Basic setup and streaming
    /// </summary>
    public static async Task BasicStreamingExample()
    {
        // Create a host with dependency injection
        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Register HttpClient factory (required for REST API calls)
                services.AddHttpClient();

                // Register the Polymarket service as a singleton
                services.AddSingleton<IPolymarketWebSocketService, PolymarketWebSocketService>();

                // Configure logging
                services.AddLogging(builder =>
                {
                    builder.AddConsole();
                    builder.SetMinimumLevel(LogLevel.Information);
                });
            })
            .Build();

        var service = host.Services.GetRequiredService<IPolymarketWebSocketService>();

        // Subscribe to events
        service.OnMarketDataReceived += (sender, data) =>
        {
            Console.WriteLine($"[{data.Category}] Market {data.MarketId}:");
            Console.WriteLine($"  Price: {data.Price:F4}");
            Console.WriteLine($"  IMPLIED VOLATILITY: {data.ImpliedVolatility:F2}%");
            Console.WriteLine($"  Volume 24h: ${data.Volume24h:N2}");
            Console.WriteLine($"  Liquidity: ${data.Liquidity:N2}");
            Console.WriteLine($"  Time: {data.TimestampDateTime:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine();
        };

        service.OnTradeReceived += (sender, trade) =>
        {
            Console.WriteLine($"Trade: {trade.Side} {trade.Size} @ {trade.Price}");
        };

        service.OnConnectionStateChanged += (sender, isConnected) =>
        {
            Console.WriteLine($"Connection state: {(isConnected ? "CONNECTED" : "DISCONNECTED")}");
        };

        service.OnError += (sender, error) =>
        {
            Console.WriteLine($"ERROR: {error.Message}");
        };

        // Start streaming
        await service.StartAsync();

        Console.WriteLine("Streaming started. Press Ctrl+C to stop...");

        // Keep running
        await Task.Delay(Timeout.Infinite);
    }

    /// <summary>
    /// Example 2: Trading bot integration with volatility-based signals
    /// </summary>
    public static async Task TradingBotExample()
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddHttpClient();
                services.AddSingleton<IPolymarketWebSocketService, PolymarketWebSocketService>();
                services.AddLogging(builder => builder.AddConsole());
            })
            .Build();

        var service = host.Services.GetRequiredService<IPolymarketWebSocketService>();

        // Trading bot state
        var priceHistory = new Dictionary<string, List<decimal>>();
        var volatilityThreshold = 30m; // 30% implied volatility threshold

        service.OnMarketDataReceived += async (sender, data) =>
        {
            // Track price history for each market
            if (!priceHistory.ContainsKey(data.MarketId))
            {
                priceHistory[data.MarketId] = new List<decimal>();
            }

            priceHistory[data.MarketId].Add(data.Price);

            // Keep only last 100 prices
            if (priceHistory[data.MarketId].Count > 100)
            {
                priceHistory[data.MarketId].RemoveAt(0);
            }

            // TRADING SIGNAL: High implied volatility indicates uncertainty
            // This is when market makers can profit from mispricing
            if (data.ImpliedVolatility > volatilityThreshold)
            {
                Console.WriteLine($"🚨 HIGH VOLATILITY SIGNAL 🚨");
                Console.WriteLine($"Market: {data.MarketId}");
                Console.WriteLine($"Category: {data.Category}");
                Console.WriteLine($"Current Price: {data.Price:F4}");
                Console.WriteLine($"Implied Volatility: {data.ImpliedVolatility:F2}%");
                Console.WriteLine($"Liquidity: ${data.Liquidity:N2}");

                // Calculate realized volatility from price history
                if (priceHistory[data.MarketId].Count >= 20)
                {
                    var realizedVol = CalculateRealizedVolatility(priceHistory[data.MarketId]);
                    Console.WriteLine($"Realized Volatility (20-period): {realizedVol:F2}%");

                    // Trading opportunity: IV vs RV disparity
                    var volDisparity = data.ImpliedVolatility - realizedVol;
                    if (Math.Abs(volDisparity) > 10)
                    {
                        Console.WriteLine($"⚡ VOLATILITY ARBITRAGE OPPORTUNITY ⚡");
                        Console.WriteLine($"IV-RV Spread: {volDisparity:F2}%");

                        // TODO: Execute trade through Polymarket API
                        // await ExecuteTradeAsync(data.MarketId, volDisparity > 0 ? "SELL" : "BUY");
                    }
                }

                Console.WriteLine();
            }

            // TRADING SIGNAL: Liquidity monitoring
            if (data.Liquidity < 1000)
            {
                Console.WriteLine($"⚠️  LOW LIQUIDITY: Market {data.MarketId} (${data.Liquidity:N2})");
            }

            // TRADING SIGNAL: Price extremes (near 0 or 1)
            if (data.Price < 0.05m || data.Price > 0.95m)
            {
                Console.WriteLine($"📊 EXTREME PRICE: {data.MarketId} @ {data.Price:F4}");
                Console.WriteLine($"   Consider counter-trend position if IV is elevated");
            }
        };

        await service.StartAsync();

        // Periodically print statistics
        var statsTimer = new Timer(async _ =>
        {
            var stats = await service.GetStatisticsAsync();
            Console.WriteLine($"\n=== TRADING BOT STATISTICS ===");
            Console.WriteLine($"Uptime: {stats.Uptime:hh\\:mm\\:ss}");
            Console.WriteLine($"Messages: {stats.TotalMessagesReceived}");
            Console.WriteLine($"Trades: {stats.TotalTradesReceived}");
            Console.WriteLine($"Price Updates: {stats.TotalPriceUpdates}");
            Console.WriteLine($"Tracked Markets: {stats.TrackedMarketsCount}");
            Console.WriteLine($"Reconnections: {stats.TotalReconnections}");
            Console.WriteLine($"By Category:");
            foreach (var kvp in stats.MessagesByCategory)
            {
                Console.WriteLine($"  {kvp.Key}: {kvp.Value} messages");
            }
            Console.WriteLine("===============================\n");
        }, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));

        await Task.Delay(Timeout.Infinite);
    }

    /// <summary>
    /// Example 3: Market monitoring and alerts
    /// </summary>
    public static async Task MarketMonitoringExample()
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddHttpClient();
                services.AddSingleton<IPolymarketWebSocketService, PolymarketWebSocketService>();
                services.AddLogging(builder => builder.AddConsole());
            })
            .Build();

        var service = host.Services.GetRequiredService<IPolymarketWebSocketService>();

        // Define alert conditions
        var alerts = new Dictionary<string, AlertCondition>
        {
            ["high_volume"] = new AlertCondition
            {
                Name = "High Volume",
                Condition = data => data.Volume24h > 100000,
                Action = data => Console.WriteLine($"🔥 HIGH VOLUME: {data.MarketId} - ${data.Volume24h:N2}")
            },
            ["price_spike"] = new AlertCondition
            {
                Name = "Price Spike",
                LastPrice = new Dictionary<string, decimal>(),
                Condition = data =>
                {
                    var alert = alerts["price_spike"];
                    if (alert.LastPrice.TryGetValue(data.MarketId, out var lastPrice))
                    {
                        var change = Math.Abs(data.Price - lastPrice) / lastPrice;
                        alert.LastPrice[data.MarketId] = data.Price;
                        return change > 0.1m; // 10% price change
                    }
                    alert.LastPrice[data.MarketId] = data.Price;
                    return false;
                },
                Action = data => Console.WriteLine($"📈 PRICE SPIKE: {data.MarketId} - {data.Price:F4}")
            },
            ["volatility_expansion"] = new AlertCondition
            {
                Name = "Volatility Expansion",
                Condition = data => data.ImpliedVolatility > 40,
                Action = data => Console.WriteLine($"💥 VOL EXPANSION: {data.MarketId} - IV: {data.ImpliedVolatility:F2}%")
            }
        };

        service.OnMarketDataReceived += (sender, data) =>
        {
            foreach (var alert in alerts.Values)
            {
                if (alert.Condition(data))
                {
                    alert.Action(data);
                }
            }
        };

        await service.StartAsync();

        // List tracked markets
        var markets = await service.GetTrackedMarketsAsync();
        Console.WriteLine($"\n=== TRACKED MARKETS ({markets.Count}) ===");
        foreach (var market in markets.Take(10))
        {
            Console.WriteLine($"{market.Category}: {market.Title}");
        }
        Console.WriteLine("=================================\n");

        await Task.Delay(Timeout.Infinite);
    }

    /// <summary>
    /// Example 4: Data collection for analysis
    /// </summary>
    public static async Task DataCollectionExample()
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddHttpClient();
                services.AddSingleton<IPolymarketWebSocketService, PolymarketWebSocketService>();
                services.AddLogging(builder => builder.AddConsole());
            })
            .Build();

        var service = host.Services.GetRequiredService<IPolymarketWebSocketService>();

        // Data storage (in production, use a database)
        var dataStore = new List<MarketDataSnapshot>();

        service.OnMarketDataReceived += (sender, data) =>
        {
            // Store snapshot
            var snapshot = new MarketDataSnapshot
            {
                Timestamp = data.TimestampDateTime,
                MarketId = data.MarketId,
                Category = data.Category,
                Price = data.Price,
                ImpliedVolatility = data.ImpliedVolatility,
                Volume = data.Volume24h,
                Liquidity = data.Liquidity,
                Spread = data.Spread
            };

            dataStore.Add(snapshot);

            // Periodically persist to database/file
            if (dataStore.Count >= 1000)
            {
                // TODO: Save to database
                Console.WriteLine($"Persisting {dataStore.Count} records to database...");
                dataStore.Clear();
            }
        };

        await service.StartAsync();
        await Task.Delay(Timeout.Infinite);
    }

    /// <summary>
    /// Calculates realized volatility from price history
    /// </summary>
    private static decimal CalculateRealizedVolatility(List<decimal> prices)
    {
        if (prices.Count < 2) return 0;

        // Calculate log returns
        var returns = new List<decimal>();
        for (int i = 1; i < prices.Count; i++)
        {
            if (prices[i - 1] > 0)
            {
                var logReturn = (decimal)Math.Log((double)(prices[i] / prices[i - 1]));
                returns.Add(logReturn);
            }
        }

        if (returns.Count == 0) return 0;

        // Calculate standard deviation of returns
        var mean = returns.Average();
        var variance = returns.Sum(r => (r - mean) * (r - mean)) / returns.Count;
        var stdDev = (decimal)Math.Sqrt((double)variance);

        // Annualize (assuming each price is 1 minute apart, 525600 minutes per year)
        var annualizationFactor = (decimal)Math.Sqrt(525600);
        var realizedVol = stdDev * annualizationFactor * 100; // Convert to percentage

        return realizedVol;
    }

    /// <summary>
    /// Helper class for alert conditions
    /// </summary>
    private class AlertCondition
    {
        public string Name { get; set; } = string.Empty;
        public Func<MarketDataUpdate, bool> Condition { get; set; } = _ => false;
        public Action<MarketDataUpdate> Action { get; set; } = _ => { };
        public Dictionary<string, decimal> LastPrice { get; set; } = new();
    }

    /// <summary>
    /// Data snapshot for storage
    /// </summary>
    private class MarketDataSnapshot
    {
        public DateTime Timestamp { get; set; }
        public string MarketId { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal ImpliedVolatility { get; set; }
        public decimal Volume { get; set; }
        public decimal Liquidity { get; set; }
        public decimal Spread { get; set; }
    }
}

/// <summary>
/// SCALING RECOMMENDATIONS FOR TRADING BOT USE
/// ==============================================
///
/// 1. ARCHITECTURE PATTERNS:
///    - Use message queue (RabbitMQ/Kafka) to decouple data ingestion from trading logic
///    - Implement separate microservices for:
///      * Data collection (this service)
///      * Signal generation (analysis and indicators)
///      * Order execution (trading engine)
///      * Risk management (position sizing, stop-loss)
///
/// 2. DATA PERSISTENCE:
///    - Use time-series database (InfluxDB, TimescaleDB) for market data
///    - Store tick data for backtesting and analysis
///    - Maintain separate hot/cold storage for recent vs historical data
///
/// 3. PERFORMANCE OPTIMIZATION:
///    - Implement connection pooling for WebSocket connections
///    - Use memory-efficient data structures (circular buffers for price history)
///    - Consider multiple WebSocket connections with load balancing for high throughput
///    - Implement data compression for storage
///
/// 4. RELIABILITY & FAULT TOLERANCE:
///    - Deploy redundant instances with failover
///    - Use distributed caching (Redis) for shared state
///    - Implement circuit breakers for external API calls
///    - Set up dead letter queues for failed messages
///
/// 5. MONITORING & OBSERVABILITY:
///    - Use structured logging (Serilog with ELK stack)
///    - Implement metrics collection (Prometheus + Grafana)
///    - Track key metrics: latency, message rate, reconnection frequency
///    - Set up alerts for connection failures, high latency, data gaps
///
/// 6. TRADING-SPECIFIC FEATURES:
///    - Implement position tracking and P&L calculation
///    - Add order management system (OMS) integration
///    - Build risk management module (max position size, drawdown limits)
///    - Create backtesting framework using historical data
///
/// 7. ADVANCED ANALYTICS:
///    - Calculate additional metrics: Sharpe ratio, maximum drawdown
///    - Implement machine learning models for price prediction
///    - Use historical IV data to build volatility surfaces
///    - Add correlation analysis between markets
///
/// 8. COMPLIANCE & RISK:
///    - Log all trading decisions for audit trail
///    - Implement kill switch for emergency shutdown
///    - Add position limits per market/category
///    - Monitor for unusual market conditions
///
/// 9. HORIZONTAL SCALING:
///    - Use Kubernetes for container orchestration
///    - Implement sharding by market category or ID range
///    - Use distributed tracing (Jaeger) for debugging
///    - Consider serverless functions for sporadic tasks
///
/// 10. COST OPTIMIZATION:
///     - Implement adaptive sampling (reduce frequency for low-activity markets)
///     - Use spot instances for non-critical workloads
///     - Cache REST API responses with appropriate TTL
///     - Compress stored data and use tiered storage
///
/// EXAMPLE DEPLOYMENT ARCHITECTURE:
/// ================================
///
///  ┌─────────────────┐
///  │  Load Balancer  │
///  └────────┬────────┘
///           │
///    ┌──────┴──────┐
///    │   WebSocket  │◄────────── Polymarket API
///    │   Service    │
///    │ (Multiple    │
///    │  Instances)  │
///    └──────┬───────┘
///           │
///    ┌──────▼───────┐
///    │  Message Bus │
///    │ (Kafka/NATS) │
///    └──────┬───────┘
///           │
///    ┌──────┴───────┬────────────┬────────────┐
///    │              │            │            │
///    ▼              ▼            ▼            ▼
/// ┌────────┐  ┌─────────┐  ┌─────────┐  ┌──────────┐
/// │ Signal │  │ Trading │  │  Risk   │  │  Data    │
/// │ Engine │  │ Engine  │  │  Mgmt   │  │ Storage  │
/// └────────┘  └─────────┘  └─────────┘  └──────────┘
///                                              │
///                                              ▼
///                                        ┌──────────┐
///                                        │   DB     │
///                                        │TimescaleDB
///                                        └──────────┘
/// </summary>
public static class ScalingRecommendations { }
