# Alpaca Markets REST API Service for .NET

A comprehensive, production-ready .NET service for consuming Alpaca Markets' REST API with **primary focus on accessing Implied Volatility (IV)** from options data endpoints.

## 📋 Table of Contents

- [Features](#features)
- [Architecture](#architecture)
- [Getting Started](#getting-started)
- [Implied Volatility (IV) Retrieval](#implied-volatility-iv-retrieval)
- [API Methods](#api-methods)
- [Configuration](#configuration)
- [Error Handling & Retry Logic](#error-handling--retry-logic)
- [Rate Limiting](#rate-limiting)
- [Caching Strategies](#caching-strategies)
- [Scaling Recommendations](#scaling-recommendations)
- [Trading Bot Integration](#trading-bot-integration)
- [Examples](#examples)

## ✨ Features

- ✅ **Full Alpaca API Coverage**: Stock, Crypto, and Options data
- ✅ **Implied Volatility Focus**: Dedicated methods for IV retrieval and analysis
- ✅ **Clean Architecture**: Interface-based design with dependency injection
- ✅ **Retry Logic**: Exponential backoff for transient failures
- ✅ **Rate Limiting**: Automatic rate limit handling (200 req/min)
- ✅ **Typed DTOs**: Strongly-typed C# models for all API responses
- ✅ **Logging**: Comprehensive logging with Microsoft.Extensions.Logging
- ✅ **Authentication**: Secure API key-based authentication
- ✅ **Async/Await**: Modern async patterns throughout
- ✅ **Production Ready**: Error handling, health checks, and monitoring

## 🏗️ Architecture

```
Services/Alpaca/
├── Interfaces/
│   └── IAlpacaApiClient.cs          # Service interface
├── Models/
│   └── AlpacaModels.cs              # DTOs and domain models
├── Examples/
│   ├── AlpacaExampleUsage.cs        # Usage examples
│   └── ProgramIntegration.cs        # DI setup and integration
├── AlpacaApiClient.cs               # Main implementation
└── README.md                        # This file
```

### Design Principles

1. **Interface Segregation**: `IAlpacaApiClient` defines the contract
2. **Dependency Injection**: Works with .NET DI container
3. **Single Responsibility**: Focused on API communication only
4. **Open/Closed**: Extensible through inheritance and composition

## 🚀 Getting Started

### Installation

1. **Add NuGet Packages** (already included in project):
```xml
<PackageReference Include="Microsoft.Extensions.Http" Version="7.0.0" />
<PackageReference Include="Microsoft.Extensions.Logging" Version="7.0.0" />
<PackageReference Include="System.Text.Json" Version="7.0.0" />
```

2. **Get Alpaca API Keys**:
   - Sign up at [Alpaca Markets](https://alpaca.markets/)
   - Navigate to your dashboard
   - Generate API keys (use Paper Trading for testing)

3. **Configure the Service**:

```csharp
using Medalion.Services.Alpaca;
using Medalion.Services.Alpaca.Interfaces;
using Medalion.Services.Alpaca.Models;

// In Program.cs or Startup.cs
services.AddSingleton(new AlpacaApiConfiguration
{
    ApiKeyId = "YOUR_API_KEY_ID",
    ApiSecretKey = "YOUR_API_SECRET_KEY",
    BaseUrl = "https://paper-api.alpaca.markets",  // Paper trading
    DataBaseUrl = "https://data.alpaca.markets",
    MaxRetryAttempts = 3,
    MaxRequestsPerMinute = 200
});

services.AddSingleton<IAlpacaApiClient, AlpacaApiClient>();
```

4. **Use the Service**:

```csharp
public class MyService
{
    private readonly IAlpacaApiClient _alpacaClient;

    public MyService(IAlpacaApiClient alpacaClient)
    {
        _alpacaClient = alpacaClient;
    }

    public async Task GetStockData()
    {
        var quote = await _alpacaClient.GetStockQuoteAsync("AAPL");
        Console.WriteLine($"AAPL: ${quote.MidPrice}");
    }
}
```

## 📊 Implied Volatility (IV) Retrieval

### Understanding IV in Alpaca

**Implied Volatility (IV)** represents the market's expectation of future volatility and is crucial for options trading strategies. Alpaca provides IV data through their **Options Data API**.

### How Alpaca Calculates IV

1. **Black-Scholes Model**: Alpaca uses the Black-Scholes model to calculate IV
2. **Real-time Updates**: IV is updated with market data
3. **Greeks Included**: Delta, Gamma, Theta, Vega, and Rho are provided alongside IV
4. **Format**: IV is returned as a decimal (e.g., 0.25 = 25% annualized volatility)

### Primary IV Retrieval Methods

#### 1. Get IV for a Single Option

```csharp
var ivData = await _alpacaClient.GetImpliedVolatilityAsync("AAPL230120C00150000");

Console.WriteLine($"Symbol: {ivData.Symbol}");
Console.WriteLine($"Implied Volatility: {ivData.ImpliedVolatility:P2}");
Console.WriteLine($"Underlying Price: ${ivData.UnderlyingPrice:F2}");
Console.WriteLine($"Strike: ${ivData.StrikePrice}");
Console.WriteLine($"Days to Expiration: {ivData.DaysToExpiration}");
Console.WriteLine($"Delta: {ivData.Greeks.Delta:F4}");
```

**Output:**
```
Symbol: AAPL230120C00150000
Implied Volatility: 28.50%
Underlying Price: $148.25
Strike: $150.00
Days to Expiration: 45
Delta: 0.4523
```

#### 2. Get IV for Entire Options Chain (Volatility Surface)

```csharp
var ivChain = await _alpacaClient.GetImpliedVolatilityChainAsync("AAPL");

Console.WriteLine($"Underlying: {ivChain.UnderlyingSymbol}");
Console.WriteLine($"Current Price: ${ivChain.UnderlyingPrice}");
Console.WriteLine($"Total Options: {ivChain.Options.Count}");
Console.WriteLine($"Average IV: {ivChain.AverageImpliedVolatility:P2}");

// Analyze volatility skew
var calls = ivChain.Options.Where(o => o.OptionType == "call");
var puts = ivChain.Options.Where(o => o.OptionType == "put");

Console.WriteLine($"Avg Call IV: {calls.Average(c => c.ImpliedVolatility):P2}");
Console.WriteLine($"Avg Put IV: {puts.Average(p => p.ImpliedVolatility):P2}");
```

#### 3. Batch IV Retrieval

```csharp
var optionSymbols = new List<string>
{
    "AAPL230120C00150000",
    "AAPL230120P00150000",
    "TSLA230120C00200000"
};

var ivDataDict = await _alpacaClient.GetImpliedVolatilityBatchAsync(optionSymbols);

foreach (var (symbol, ivData) in ivDataDict)
{
    Console.WriteLine($"{symbol}: IV = {ivData.ImpliedVolatility:P2}");
}
```

### IV Data Model

```csharp
public class ImpliedVolatilityData
{
    public string Symbol { get; set; }                    // Option symbol
    public string UnderlyingSymbol { get; set; }          // Underlying stock
    public decimal ImpliedVolatility { get; set; }        // IV (0.25 = 25%)
    public DateTime Timestamp { get; set; }               // Data timestamp
    public decimal StrikePrice { get; set; }              // Strike price
    public DateTime ExpirationDate { get; set; }          // Expiration date
    public string OptionType { get; set; }                // "call" or "put"
    public decimal? OptionPrice { get; set; }             // Current option price
    public decimal? UnderlyingPrice { get; set; }         // Current underlying price
    public int DaysToExpiration { get; set; }             // Days to expiration
    public OptionGreeks? Greeks { get; set; }             // Delta, Gamma, etc.
    public long? OpenInterest { get; set; }               // Open interest
    public long? Volume { get; set; }                     // Volume
}
```

## 🔧 API Methods

### Stock Data

| Method | Description | Example |
|--------|-------------|---------|
| `GetStockQuoteAsync` | Latest stock quote | `GetStockQuoteAsync("AAPL")` |
| `GetStockBarsAsync` | Historical bars/candles | `GetStockBarsAsync("AAPL", Timeframe.OneHour, start, end)` |
| `GetStockSnapshotAsync` | Current snapshot | `GetStockSnapshotAsync("AAPL")` |
| `GetStockLatestTradeAsync` | Latest trade | `GetStockLatestTradeAsync("AAPL")` |

### Crypto Data

| Method | Description | Example |
|--------|-------------|---------|
| `GetCryptoQuoteAsync` | Latest crypto quote | `GetCryptoQuoteAsync("BTCUSD")` |
| `GetCryptoBarsAsync` | Historical crypto bars | `GetCryptoBarsAsync("BTCUSD", Timeframe.OneHour, start, end)` |
| `GetCryptoSnapshotAsync` | Current crypto snapshot | `GetCryptoSnapshotAsync("ETHUSD")` |
| `GetCryptoLatestTradeAsync` | Latest crypto trade | `GetCryptoLatestTradeAsync("BTCUSD")` |

### Options Data

| Method | Description | Example |
|--------|-------------|---------|
| `GetOptionsChainAsync` | Full options chain | `GetOptionsChainAsync("AAPL", expirationDate)` |
| `GetOptionContractAsync` | Option contract details | `GetOptionContractAsync("AAPL230120C00150000")` |
| `GetOptionQuoteAsync` | Latest option quote | `GetOptionQuoteAsync("AAPL230120C00150000")` |
| `GetOptionSnapshotAsync` | Option snapshot with Greeks | `GetOptionSnapshotAsync("AAPL230120C00150000")` |

### Implied Volatility (PRIMARY FOCUS)

| Method | Description | Use Case |
|--------|-------------|----------|
| `GetImpliedVolatilityAsync` | **Get IV for single option** | Real-time IV monitoring |
| `GetImpliedVolatilityChainAsync` | **Get IV for entire chain** | Volatility surface, skew analysis |
| `GetImpliedVolatilityBatchAsync` | **Batch IV retrieval** | Efficient multi-option IV |

## ⚙️ Configuration

### AlpacaApiConfiguration Properties

```csharp
public class AlpacaApiConfiguration
{
    // Required
    public string ApiKeyId { get; set; }           // Your API key ID
    public string ApiSecretKey { get; set; }       // Your API secret key

    // URLs
    public string BaseUrl { get; set; }            // Default: https://api.alpaca.markets
                                                   // Paper: https://paper-api.alpaca.markets
    public string DataBaseUrl { get; set; }        // Default: https://data.alpaca.markets

    // Timeouts and Retries
    public int TimeoutSeconds { get; set; }        // Default: 30
    public int MaxRetryAttempts { get; set; }      // Default: 3
    public int InitialRetryDelayMs { get; set; }   // Default: 1000 (exponential backoff)

    // Rate Limiting
    public int MaxRequestsPerMinute { get; set; }  // Default: 200 (Alpaca free tier)
}
```

### Environment Variables (Recommended for Production)

```bash
export ALPACA_API_KEY_ID="your_key_id"
export ALPACA_API_SECRET_KEY="your_secret_key"
export ALPACA_BASE_URL="https://api.alpaca.markets"
```

```csharp
services.AddSingleton(new AlpacaApiConfiguration
{
    ApiKeyId = Environment.GetEnvironmentVariable("ALPACA_API_KEY_ID") ?? "",
    ApiSecretKey = Environment.GetEnvironmentVariable("ALPACA_API_SECRET_KEY") ?? "",
    BaseUrl = Environment.GetEnvironmentVariable("ALPACA_BASE_URL") ?? "https://paper-api.alpaca.markets"
});
```

## 🔄 Error Handling & Retry Logic

### Built-in Retry Mechanism

The client implements **exponential backoff** retry logic:

1. **Initial delay**: 1000ms
2. **Backoff multiplier**: 2x
3. **Max attempts**: 3 (configurable)

```csharp
Attempt 1: Immediate
Attempt 2: Wait 1000ms
Attempt 3: Wait 2000ms
Attempt 4: Wait 4000ms
```

### Automatic Retry Scenarios

- ✅ **5xx Server Errors**: Retries automatically
- ✅ **429 Too Many Requests**: Waits 60 seconds, then retries
- ✅ **Network Timeouts**: Retries with exponential backoff
- ❌ **4xx Client Errors** (except 429): No retry (fix the request)

### Error Handling Example

```csharp
try
{
    var ivData = await _alpacaClient.GetImpliedVolatilityAsync("AAPL230120C00150000");
    // Process data
}
catch (UnauthorizedAccessException ex)
{
    // Invalid API credentials
    _logger.LogError(ex, "Invalid Alpaca API credentials");
}
catch (HttpRequestException ex)
{
    // Network or API error
    _logger.LogError(ex, "Failed to retrieve IV data");
}
catch (Exception ex)
{
    // Unexpected error
    _logger.LogError(ex, "Unexpected error");
}
```

## ⏱️ Rate Limiting

### Alpaca Rate Limits

- **Free Tier**: 200 requests/minute
- **Unlimited Tier**: No rate limit (check your plan)

### Built-in Rate Limit Handling

The client automatically:

1. **Tracks requests**: Maintains a sliding window of request timestamps
2. **Waits proactively**: If limit is reached, waits until window resets
3. **Reads headers**: Updates rate limit info from API response headers

### Monitoring Rate Limits

```csharp
var rateLimitInfo = _alpacaClient.GetRateLimitInfo();
Console.WriteLine($"Requests Remaining: {rateLimitInfo.Remaining}/{rateLimitInfo.Limit}");
Console.WriteLine($"Resets at: {rateLimitInfo.ResetTime:HH:mm:ss}");
```

### Batch Operations for Efficiency

Use batch methods to minimize API calls:

```csharp
// ❌ Bad: 100 API calls
foreach (var symbol in optionSymbols)
{
    var iv = await _alpacaClient.GetImpliedVolatilityAsync(symbol);
}

// ✅ Good: Parallelized with rate limiting (automatic)
var ivDataDict = await _alpacaClient.GetImpliedVolatilityBatchAsync(optionSymbols);
```

## 💾 Caching Strategies

### Recommended Caching Approach

#### 1. In-Memory Caching with MemoryCache

```csharp
using Microsoft.Extensions.Caching.Memory;

public class CachedAlpacaService
{
    private readonly IAlpacaApiClient _alpacaClient;
    private readonly IMemoryCache _cache;

    public async Task<ImpliedVolatilityData> GetImpliedVolatilityCachedAsync(string optionSymbol)
    {
        var cacheKey = $"iv_{optionSymbol}";

        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            // Cache for 5 minutes (IV doesn't change that rapidly)
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);

            return await _alpacaClient.GetImpliedVolatilityAsync(optionSymbol);
        });
    }
}
```

#### 2. Distributed Caching with Redis

```csharp
using StackExchange.Redis;

public class RedisAlpacaCacheService
{
    private readonly IAlpacaApiClient _alpacaClient;
    private readonly IConnectionMultiplexer _redis;

    public async Task<ImpliedVolatilityData> GetIVCachedAsync(string optionSymbol)
    {
        var db = _redis.GetDatabase();
        var cacheKey = $"alpaca:iv:{optionSymbol}";

        // Try to get from cache
        var cached = await db.StringGetAsync(cacheKey);
        if (cached.HasValue)
        {
            return JsonSerializer.Deserialize<ImpliedVolatilityData>(cached);
        }

        // Get from API
        var ivData = await _alpacaClient.GetImpliedVolatilityAsync(optionSymbol);

        // Store in cache (5 minute expiration)
        await db.StringSetAsync(
            cacheKey,
            JsonSerializer.Serialize(ivData),
            TimeSpan.FromMinutes(5));

        return ivData;
    }
}
```

### Caching Recommendations by Data Type

| Data Type | Cache Duration | Rationale |
|-----------|---------------|-----------|
| **Stock Quotes** | 30-60 seconds | High volatility, real-time needed |
| **Crypto Quotes** | 30-60 seconds | High volatility |
| **Options Chain** | 5-15 minutes | Changes less frequently |
| **Implied Volatility** | 5-10 minutes | Relatively stable intraday |
| **Historical Bars** | 1 hour - 1 day | Static historical data |
| **Option Contracts** | 1 day | Contract details rarely change |

### Cache Invalidation Strategies

1. **Time-based (TTL)**: Most common, works for most use cases
2. **Event-based**: Invalidate on market events (e.g., earnings)
3. **Manual**: Allow users to force refresh
4. **Composite**: Combine multiple strategies

## 📈 Scaling Recommendations

### 1. Horizontal Scaling

```csharp
// Use multiple instances with shared cache
services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "redis:6379";
});
```

### 2. Connection Pooling

```csharp
// Use IHttpClientFactory (already implemented)
services.AddHttpClient("AlpacaApi", client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    MaxConnectionsPerServer = 100
});
```

### 3. Async/Parallel Processing

```csharp
// Process multiple symbols in parallel
var symbols = new[] { "AAPL", "TSLA", "SPY" };

var tasks = symbols.Select(symbol =>
    _alpacaClient.GetImpliedVolatilityChainAsync(symbol));

var results = await Task.WhenAll(tasks);
```

### 4. Message Queues for Background Processing

```csharp
// Use RabbitMQ or Azure Service Bus
public class IVProcessor
{
    public async Task ProcessIVRequest(string optionSymbol)
    {
        var ivData = await _alpacaClient.GetImpliedVolatilityAsync(optionSymbol);

        // Store in database or cache
        await _database.SaveIVDataAsync(ivData);
    }
}
```

### 5. Database Storage

Store IV data for historical analysis:

```sql
CREATE TABLE ImpliedVolatilityHistory (
    Id BIGINT IDENTITY PRIMARY KEY,
    OptionSymbol NVARCHAR(50) NOT NULL,
    UnderlyingSymbol NVARCHAR(10) NOT NULL,
    ImpliedVolatility DECIMAL(10, 6) NOT NULL,
    StrikePrice DECIMAL(18, 2) NOT NULL,
    ExpirationDate DATE NOT NULL,
    Timestamp DATETIME2 NOT NULL,
    Delta DECIMAL(10, 6),
    Gamma DECIMAL(10, 6),
    Theta DECIMAL(10, 6),
    Vega DECIMAL(10, 6),
    INDEX IX_OptionSymbol_Timestamp (OptionSymbol, Timestamp),
    INDEX IX_UnderlyingSymbol_Timestamp (UnderlyingSymbol, Timestamp)
);
```

## 🤖 Trading Bot Integration

### Architecture for Trading Bots

```
┌─────────────────────────────────────────────────────────┐
│                   Trading Bot System                     │
├─────────────────────────────────────────────────────────┤
│                                                           │
│  ┌─────────────┐    ┌──────────────┐   ┌─────────────┐ │
│  │ Data Layer  │───>│ Strategy     │──>│ Execution   │ │
│  │ (Alpaca)    │    │ Engine       │   │ Engine      │ │
│  └─────────────┘    └──────────────┘   └─────────────┘ │
│        │                    │                   │        │
│        v                    v                   v        │
│  ┌─────────────┐    ┌──────────────┐   ┌─────────────┐ │
│  │ IV Monitor  │    │ Signal Gen   │   │ Order Mgmt  │ │
│  └─────────────┘    └──────────────┘   └─────────────┘ │
│                                                           │
└─────────────────────────────────────────────────────────┘
```

### Example: IV-Based Trading Bot

```csharp
public class IVTradingBot : BackgroundService
{
    private readonly IAlpacaApiClient _alpacaClient;
    private readonly ILogger<IVTradingBot> _logger;
    private readonly List<string> _watchlist = new() { "AAPL", "TSLA", "SPY" };

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                foreach (var symbol in _watchlist)
                {
                    await AnalyzeAndTrade(symbol, stoppingToken);
                }

                // Run every 5 minutes
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in trading bot loop");
            }
        }
    }

    private async Task AnalyzeAndTrade(string symbol, CancellationToken ct)
    {
        // 1. Get IV data
        var ivChain = await _alpacaClient.GetImpliedVolatilityChainAsync(symbol, null, ct);

        // 2. Analyze IV
        var avgIV = ivChain.AverageImpliedVolatility;
        var historicalAvgIV = await GetHistoricalAverageIV(symbol); // From database

        // 3. Generate signal
        if (avgIV > historicalAvgIV * 1.5m)
        {
            _logger.LogWarning("{Symbol} IV is elevated: {CurrentIV:P2} vs {HistoricalIV:P2}",
                symbol, avgIV, historicalAvgIV);

            // Strategy: Sell premium when IV is high
            var atmOptions = FindATMOptions(ivChain);
            // Execute trades...
        }
        else if (avgIV < historicalAvgIV * 0.7m)
        {
            _logger.LogInformation("{Symbol} IV is low: {CurrentIV:P2} vs {HistoricalIV:P2}",
                symbol, avgIV, historicalAvgIV);

            // Strategy: Buy options when IV is low
            var atmOptions = FindATMOptions(ivChain);
            // Execute trades...
        }
    }
}
```

### Key Considerations for Trading Bots

1. **Data Freshness**: Balance between API costs and data recency
2. **Error Handling**: Implement circuit breakers and fallbacks
3. **Risk Management**: Always validate signals before execution
4. **Backtesting**: Test strategies with historical IV data
5. **Monitoring**: Set up alerts for anomalies
6. **Compliance**: Ensure regulatory compliance

## 📚 Examples

See the `Examples/` folder for comprehensive examples:

- **AlpacaExampleUsage.cs**: 8 complete examples covering all features
- **ProgramIntegration.cs**: DI setup, background services, and trading bot examples

### Quick Start Example

```csharp
using Medalion.Services.Alpaca;
using Medalion.Services.Alpaca.Interfaces;
using Medalion.Services.Alpaca.Models;
using Microsoft.Extensions.Logging;

// Setup
var config = new AlpacaApiConfiguration
{
    ApiKeyId = "YOUR_API_KEY",
    ApiSecretKey = "YOUR_SECRET_KEY",
    BaseUrl = "https://paper-api.alpaca.markets"
};

var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
var logger = loggerFactory.CreateLogger<AlpacaApiClient>();

using var client = new AlpacaApiClient(config, logger);

// Get stock quote
var quote = await client.GetStockQuoteAsync("AAPL");
Console.WriteLine($"AAPL: ${quote.MidPrice}");

// Get options chain
var optionsChain = await client.GetOptionsChainAsync("AAPL");
var firstOption = optionsChain.Options.First();

// Get implied volatility (PRIMARY USE CASE)
var ivData = await client.GetImpliedVolatilityAsync(firstOption.Symbol);
Console.WriteLine($"IV: {ivData.ImpliedVolatility:P2}");
Console.WriteLine($"Delta: {ivData.Greeks?.Delta:F4}");
```

## 🔒 Security Best Practices

1. **Never commit API keys**: Use environment variables or secret management
2. **Use Paper Trading** for development and testing
3. **Rotate keys regularly**: Generate new API keys periodically
4. **Limit key permissions**: Use read-only keys when possible
5. **Monitor usage**: Track API calls for unusual activity

## 📝 Logging

The service uses `Microsoft.Extensions.Logging` for all logging:

```csharp
// Logs are automatically generated for:
- API requests and responses
- Rate limiting events
- Retry attempts
- Errors and exceptions
- IV calculations

// Example log output:
[INFO] AlpacaApiClient: Getting implied volatility for AAPL230120C00150000
[INFO] AlpacaApiClient: Retrieved IV for AAPL230120C00150000: 28.50% (Strike: $150, Expiration: 2023-01-20, Underlying: $148.25)
```

## 🧪 Testing

### Unit Testing

```csharp
public class AlpacaApiClientTests
{
    private readonly Mock<ILogger<AlpacaApiClient>> _mockLogger;
    private readonly AlpacaApiConfiguration _config;

    [Fact]
    public async Task GetImpliedVolatilityAsync_ReturnsValidData()
    {
        // Arrange
        var client = new AlpacaApiClient(_config, _mockLogger.Object);

        // Act
        var result = await client.GetImpliedVolatilityAsync("TEST_OPTION_SYMBOL");

        // Assert
        Assert.NotNull(result);
        Assert.True(result.ImpliedVolatility > 0);
    }
}
```

## 📖 Additional Resources

- [Alpaca Markets API Documentation](https://alpaca.markets/docs/api-references/market-data-api/)
- [Options Trading Basics](https://alpaca.markets/learn/options/)
- [Understanding Implied Volatility](https://alpaca.markets/learn/implied-volatility/)
- [Black-Scholes Model](https://en.wikipedia.org/wiki/Black%E2%80%93Scholes_model)

## 🤝 Contributing

Contributions are welcome! Please follow these guidelines:

1. Fork the repository
2. Create a feature branch
3. Write tests for new functionality
4. Ensure all tests pass
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License.

## 🙋 Support

For issues or questions:

1. Check the [Examples](Examples/) folder
2. Review the [Alpaca API docs](https://alpaca.markets/docs/)
3. Open an issue on GitHub

---

**Happy Trading! 🚀📈**
