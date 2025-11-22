# Alpaca Markets API - Quick Start Guide

## ⚡ 5-Minute Setup

### Step 1: Get Your API Keys

1. Sign up at [Alpaca Markets](https://alpaca.markets/)
2. Go to your dashboard
3. Generate API keys (use **Paper Trading** for testing)
4. Copy your API Key ID and Secret Key

### Step 2: Configure Your Keys

**Option A: Environment Variables (Recommended)**
```bash
export ALPACA_API_KEY_ID="your_key_id_here"
export ALPACA_API_SECRET_KEY="your_secret_key_here"
```

**Option B: Update appsettings.json**
```json
{
  "Alpaca": {
    "ApiKeyId": "your_key_id_here",
    "ApiSecretKey": "your_secret_key_here",
    "BaseUrl": "https://paper-api.alpaca.markets"
  }
}
```

### Step 3: Register the Service

In your `Program.cs`:

```csharp
using Medalion.Services.Alpaca;
using Medalion.Services.Alpaca.Interfaces;
using Medalion.Services.Alpaca.Models;

// Add this in your service configuration section
builder.Services.AddHttpClient();

builder.Services.AddSingleton(sp =>
{
    var config = builder.Configuration.GetSection("Alpaca");
    return new AlpacaApiConfiguration
    {
        ApiKeyId = config["ApiKeyId"] ?? Environment.GetEnvironmentVariable("ALPACA_API_KEY_ID") ?? "",
        ApiSecretKey = config["ApiSecretKey"] ?? Environment.GetEnvironmentVariable("ALPACA_API_SECRET_KEY") ?? "",
        BaseUrl = config["BaseUrl"] ?? "https://paper-api.alpaca.markets",
        DataBaseUrl = config["DataBaseUrl"] ?? "https://data.alpaca.markets",
        MaxRetryAttempts = 3,
        MaxRequestsPerMinute = 200
    };
});

builder.Services.AddSingleton<IAlpacaApiClient, AlpacaApiClient>();
```

### Step 4: Use the Service

**Example 1: Get Stock Quote**
```csharp
public class MyService
{
    private readonly IAlpacaApiClient _alpacaClient;

    public MyService(IAlpacaApiClient alpacaClient)
    {
        _alpacaClient = alpacaClient;
    }

    public async Task GetStockPrice()
    {
        var quote = await _alpacaClient.GetStockQuoteAsync("AAPL");
        Console.WriteLine($"AAPL Price: ${quote.MidPrice:F2}");
    }
}
```

**Example 2: Get Implied Volatility (PRIMARY USE CASE)**
```csharp
public async Task GetImpliedVolatility()
{
    // Step 1: Get options chain
    var optionsChain = await _alpacaClient.GetOptionsChainAsync("AAPL");

    // Step 2: Pick an option (e.g., first tradable call)
    var option = optionsChain.Options
        .Where(o => o.Tradable && o.Type == "call")
        .OrderBy(o => o.ExpirationDate)
        .First();

    // Step 3: Get IV data
    var ivData = await _alpacaClient.GetImpliedVolatilityAsync(option.Symbol);

    // Step 4: Display results
    Console.WriteLine($"Option: {ivData.Symbol}");
    Console.WriteLine($"Implied Volatility: {ivData.ImpliedVolatility:P2}");
    Console.WriteLine($"Strike: ${ivData.StrikePrice}");
    Console.WriteLine($"Expiration: {ivData.ExpirationDate:yyyy-MM-dd}");
    Console.WriteLine($"Delta: {ivData.Greeks?.Delta:F4}");
}
```

**Example 3: Get IV for Entire Options Chain**
```csharp
public async Task AnalyzeVolatilitySurface()
{
    var ivChain = await _alpacaClient.GetImpliedVolatilityChainAsync("AAPL");

    Console.WriteLine($"Underlying: {ivChain.UnderlyingSymbol} @ ${ivChain.UnderlyingPrice:F2}");
    Console.WriteLine($"Total Options: {ivChain.Options.Count}");
    Console.WriteLine($"Average IV: {ivChain.AverageImpliedVolatility:P2}");

    // Analyze calls vs puts
    var calls = ivChain.Options.Where(o => o.OptionType == "call");
    var puts = ivChain.Options.Where(o => o.OptionType == "put");

    Console.WriteLine($"Average Call IV: {calls.Average(c => c.ImpliedVolatility):P2}");
    Console.WriteLine($"Average Put IV: {puts.Average(p => p.ImpliedVolatility):P2}");
}
```

## 📊 Common Use Cases

### Use Case 1: Monitor High IV Options
```csharp
public async Task FindHighIVOptions()
{
    var ivChain = await _alpacaClient.GetImpliedVolatilityChainAsync("TSLA");

    var highIVOptions = ivChain.Options
        .Where(o => o.ImpliedVolatility > 0.50m) // IV > 50%
        .OrderByDescending(o => o.ImpliedVolatility)
        .Take(10);

    foreach (var option in highIVOptions)
    {
        Console.WriteLine($"{option.Symbol}: IV = {option.ImpliedVolatility:P2}");
    }
}
```

### Use Case 2: Find ATM Options for Trading
```csharp
public async Task FindATMOptions()
{
    var ivChain = await _alpacaClient.GetImpliedVolatilityChainAsync("SPY");

    var atmOptions = ivChain.Options
        .Where(o => o.DaysToExpiration >= 14 && o.DaysToExpiration <= 45)
        .OrderBy(o => Math.Abs(o.StrikePrice - ivChain.UnderlyingPrice))
        .Take(5);

    foreach (var option in atmOptions)
    {
        Console.WriteLine($"{option.OptionType.ToUpper()} ${option.StrikePrice}: IV = {option.ImpliedVolatility:P2}");
    }
}
```

### Use Case 3: Crypto Data
```csharp
public async Task GetCryptoData()
{
    var btcQuote = await _alpacaClient.GetCryptoQuoteAsync("BTCUSD");
    Console.WriteLine($"BTC: ${btcQuote.MidPrice:F2}");

    var ethQuote = await _alpacaClient.GetCryptoQuoteAsync("ETHUSD");
    Console.WriteLine($"ETH: ${ethQuote.MidPrice:F2}");
}
```

## 🔍 Troubleshooting

### Issue: "Invalid API credentials"
- **Solution**: Verify your API keys are correct
- Check if you're using the right keys (paper vs live)
- Ensure environment variables are set correctly

### Issue: "Rate limit exceeded"
- **Solution**: The client handles this automatically
- Default limit: 200 requests/minute
- Upgrade your Alpaca plan for higher limits

### Issue: "No option data found"
- **Solution**: Verify the option symbol is correct
- Check if the option is tradable
- Ensure market hours (data may be unavailable outside market hours)

## 📚 Next Steps

1. **Explore Examples**: See `Examples/AlpacaExampleUsage.cs` for 8 complete examples
2. **Read Documentation**: Check `README.md` for comprehensive documentation
3. **Implement Caching**: Add caching for better performance
4. **Build Trading Bot**: Use `Examples/ProgramIntegration.cs` as a template

## 🎯 Key Methods Reference

| Method | What It Does | Example |
|--------|-------------|---------|
| `GetStockQuoteAsync` | Get latest stock price | `GetStockQuoteAsync("AAPL")` |
| `GetCryptoQuoteAsync` | Get latest crypto price | `GetCryptoQuoteAsync("BTCUSD")` |
| `GetOptionsChainAsync` | Get all options for a stock | `GetOptionsChainAsync("AAPL")` |
| `GetImpliedVolatilityAsync` | **Get IV for one option** | `GetImpliedVolatilityAsync("AAPL230120C00150000")` |
| `GetImpliedVolatilityChainAsync` | **Get IV for all options** | `GetImpliedVolatilityChainAsync("AAPL")` |

## 💡 Pro Tips

1. **Use Paper Trading** for development (free, unlimited)
2. **Cache IV data** for 5-10 minutes (it doesn't change that fast)
3. **Batch requests** when possible to save API calls
4. **Monitor rate limits** with `GetRateLimitInfo()`
5. **Store historical IV** in a database for backtesting

## 🚀 Ready to Go!

You're all set! Start building your options trading strategies with comprehensive IV data from Alpaca Markets.

For more examples and advanced usage, see the full documentation in `README.md`.
