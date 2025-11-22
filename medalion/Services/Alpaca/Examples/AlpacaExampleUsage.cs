using Medalion.Services.Alpaca.Interfaces;
using Medalion.Services.Alpaca.Models;
using Microsoft.Extensions.Logging;

namespace Medalion.Services.Alpaca.Examples;

/// <summary>
/// Example usage of the Alpaca API client
/// Demonstrates all major features including IV retrieval
/// </summary>
public class AlpacaExampleUsage
{
    private readonly IAlpacaApiClient _alpacaClient;
    private readonly ILogger<AlpacaExampleUsage> _logger;

    public AlpacaExampleUsage(
        IAlpacaApiClient alpacaClient,
        ILogger<AlpacaExampleUsage> logger)
    {
        _alpacaClient = alpacaClient;
        _logger = logger;
    }

    /// <summary>
    /// Example 1: Get stock data (quote, bars, snapshot)
    /// </summary>
    public async Task GetStockDataExample()
    {
        _logger.LogInformation("=== STOCK DATA EXAMPLE ===");

        try
        {
            // Get latest quote
            var quote = await _alpacaClient.GetStockQuoteAsync("AAPL");
            _logger.LogInformation(
                "AAPL Quote - Bid: {Bid:C2}, Ask: {Ask:C2}, Mid: {Mid:C2}",
                quote.BidPrice,
                quote.AskPrice,
                quote.MidPrice);

            // Get historical bars (1 hour timeframe, last 24 hours)
            var end = DateTime.UtcNow;
            var start = end.AddDays(-1);
            var bars = await _alpacaClient.GetStockBarsAsync(
                "AAPL",
                Timeframe.OneHour,
                start,
                end);

            _logger.LogInformation("Retrieved {Count} hourly bars for AAPL", bars.Bars.Count);
            if (bars.Bars.Any())
            {
                var latestBar = bars.Bars.Last();
                _logger.LogInformation(
                    "Latest Bar - Open: {Open:C2}, High: {High:C2}, Low: {Low:C2}, Close: {Close:C2}, Volume: {Volume:N0}",
                    latestBar.Open,
                    latestBar.High,
                    latestBar.Low,
                    latestBar.Close,
                    latestBar.Volume);
            }

            // Get snapshot
            var snapshot = await _alpacaClient.GetStockSnapshotAsync("AAPL");
            _logger.LogInformation(
                "AAPL Snapshot - Latest Trade: {Price:C2} @ {Time}",
                snapshot.LatestTrade?.Price ?? 0,
                snapshot.LatestTrade?.Timestamp.ToString("HH:mm:ss") ?? "N/A");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in stock data example");
        }
    }

    /// <summary>
    /// Example 2: Get crypto data
    /// </summary>
    public async Task GetCryptoDataExample()
    {
        _logger.LogInformation("=== CRYPTO DATA EXAMPLE ===");

        try
        {
            // Get latest crypto quote
            var quote = await _alpacaClient.GetCryptoQuoteAsync("BTCUSD");
            _logger.LogInformation(
                "BTC/USD Quote - Bid: {Bid:C2}, Ask: {Ask:C2}, Mid: {Mid:C2}",
                quote.BidPrice,
                quote.AskPrice,
                quote.MidPrice);

            // Get crypto bars (1 hour, last 24 hours)
            var end = DateTime.UtcNow;
            var start = end.AddDays(-1);
            var bars = await _alpacaClient.GetCryptoBarsAsync(
                "BTCUSD",
                Timeframe.OneHour,
                start,
                end);

            _logger.LogInformation("Retrieved {Count} hourly bars for BTC/USD", bars.Bars.Count);

            // Get crypto snapshot
            var snapshot = await _alpacaClient.GetCryptoSnapshotAsync("ETHUSD");
            _logger.LogInformation(
                "ETH/USD Latest Trade: {Price:C2}",
                snapshot.LatestTrade?.Price ?? 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in crypto data example");
        }
    }

    /// <summary>
    /// Example 3: Get options chain
    /// </summary>
    public async Task GetOptionsChainExample()
    {
        _logger.LogInformation("=== OPTIONS CHAIN EXAMPLE ===");

        try
        {
            // Get all options for AAPL
            var optionsChain = await _alpacaClient.GetOptionsChainAsync("AAPL");
            _logger.LogInformation("Retrieved {Count} option contracts for AAPL", optionsChain.Options.Count);

            // Filter by expiration (next 30 days)
            var futureExpiration = DateTime.UtcNow.AddDays(30);
            var nearTermOptions = optionsChain.Options
                .Where(o => o.ExpirationDate <= futureExpiration)
                .OrderBy(o => o.ExpirationDate)
                .ThenBy(o => o.StrikePrice)
                .ToList();

            _logger.LogInformation("Near-term options (next 30 days): {Count}", nearTermOptions.Count);

            // Show first 5 options
            foreach (var option in nearTermOptions.Take(5))
            {
                _logger.LogInformation(
                    "Option: {Symbol} - {Type} Strike: {Strike:C2} Exp: {Expiration:yyyy-MM-dd} OI: {OI}",
                    option.Symbol,
                    option.Type.ToUpper(),
                    option.StrikePrice,
                    option.ExpirationDate,
                    option.OpenInterest ?? 0);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in options chain example");
        }
    }

    /// <summary>
    /// Example 4: Get IMPLIED VOLATILITY for a specific option
    /// THIS IS THE PRIMARY USE CASE
    /// </summary>
    public async Task GetImpliedVolatilityExample()
    {
        _logger.LogInformation("=== IMPLIED VOLATILITY EXAMPLE ===");

        try
        {
            // First, get an options chain to find a valid option symbol
            var optionsChain = await _alpacaClient.GetOptionsChainAsync("AAPL");

            // Get the first tradable option
            var option = optionsChain.Options
                .Where(o => o.Tradable && o.ExpirationDate > DateTime.UtcNow)
                .OrderBy(o => o.ExpirationDate)
                .FirstOrDefault();

            if (option == null)
            {
                _logger.LogWarning("No tradable options found for AAPL");
                return;
            }

            _logger.LogInformation("Selected option: {Symbol}", option.Symbol);

            // *** PRIMARY METHOD: Get Implied Volatility ***
            var ivData = await _alpacaClient.GetImpliedVolatilityAsync(option.Symbol);

            _logger.LogInformation("===== IMPLIED VOLATILITY DATA =====");
            _logger.LogInformation("Option Symbol: {Symbol}", ivData.Symbol);
            _logger.LogInformation("Underlying: {Underlying} @ {Price:C2}",
                ivData.UnderlyingSymbol,
                ivData.UnderlyingPrice);
            _logger.LogInformation("Strike Price: {Strike:C2}", ivData.StrikePrice);
            _logger.LogInformation("Option Type: {Type}", ivData.OptionType.ToUpper());
            _logger.LogInformation("Expiration: {Expiration:yyyy-MM-dd} ({DTE} days)",
                ivData.ExpirationDate,
                ivData.DaysToExpiration);
            _logger.LogInformation("Option Price: {Price:C2}", ivData.OptionPrice ?? 0);
            _logger.LogInformation("");
            _logger.LogInformation("*** IMPLIED VOLATILITY: {IV:P2} ***", ivData.ImpliedVolatility);
            _logger.LogInformation("");

            if (ivData.Greeks != null)
            {
                _logger.LogInformation("===== GREEKS =====");
                _logger.LogInformation("Delta: {Delta:F4}", ivData.Greeks.Delta);
                _logger.LogInformation("Gamma: {Gamma:F4}", ivData.Greeks.Gamma);
                _logger.LogInformation("Theta: {Theta:F4}", ivData.Greeks.Theta);
                _logger.LogInformation("Vega: {Vega:F4}", ivData.Greeks.Vega);
                _logger.LogInformation("Rho: {Rho:F4}", ivData.Greeks.Rho);
            }

            _logger.LogInformation("Open Interest: {OI:N0}", ivData.OpenInterest ?? 0);
            _logger.LogInformation("Volume: {Volume:N0}", ivData.Volume ?? 0);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in implied volatility example");
        }
    }

    /// <summary>
    /// Example 5: Get IV for entire options chain (Volatility Surface)
    /// </summary>
    public async Task GetVolatilitySurfaceExample()
    {
        _logger.LogInformation("=== VOLATILITY SURFACE EXAMPLE ===");

        try
        {
            // Get IV for all options expiring in the next 60 days
            var expirationDate = DateTime.UtcNow.AddDays(60);
            var ivChain = await _alpacaClient.GetImpliedVolatilityChainAsync("AAPL", expirationDate);

            _logger.LogInformation("Volatility Surface for {Symbol}", ivChain.UnderlyingSymbol);
            _logger.LogInformation("Underlying Price: {Price:C2}", ivChain.UnderlyingPrice);
            _logger.LogInformation("Total Options: {Count}", ivChain.Options.Count);
            _logger.LogInformation("Average IV: {IV:P2}", ivChain.AverageImpliedVolatility);
            _logger.LogInformation("");

            // Group by expiration date
            var byExpiration = ivChain.Options
                .GroupBy(o => o.ExpirationDate.Date)
                .OrderBy(g => g.Key)
                .ToList();

            foreach (var expirationGroup in byExpiration.Take(3))
            {
                _logger.LogInformation("Expiration: {Date:yyyy-MM-dd}", expirationGroup.Key);

                var calls = expirationGroup.Where(o => o.OptionType.Equals("call", StringComparison.OrdinalIgnoreCase));
                var puts = expirationGroup.Where(o => o.OptionType.Equals("put", StringComparison.OrdinalIgnoreCase));

                _logger.LogInformation("  Calls: {Count}, Avg IV: {IV:P2}",
                    calls.Count(),
                    calls.Any() ? calls.Average(c => c.ImpliedVolatility) : 0);

                _logger.LogInformation("  Puts: {Count}, Avg IV: {IV:P2}",
                    puts.Count(),
                    puts.Any() ? puts.Average(p => p.ImpliedVolatility) : 0);

                // Show volatility skew (ATM vs OTM)
                var atmStrike = ivChain.UnderlyingPrice;
                var atmOptions = expirationGroup
                    .OrderBy(o => Math.Abs(o.StrikePrice - atmStrike))
                    .Take(2);

                _logger.LogInformation("  Near ATM Options:");
                foreach (var option in atmOptions)
                {
                    _logger.LogInformation("    {Type} {Strike:C2}: IV = {IV:P2}",
                        option.OptionType.ToUpper(),
                        option.StrikePrice,
                        option.ImpliedVolatility);
                }

                _logger.LogInformation("");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in volatility surface example");
        }
    }

    /// <summary>
    /// Example 6: Batch IV retrieval for multiple options
    /// </summary>
    public async Task GetBatchImpliedVolatilityExample()
    {
        _logger.LogInformation("=== BATCH IMPLIED VOLATILITY EXAMPLE ===");

        try
        {
            // Get options chain
            var optionsChain = await _alpacaClient.GetOptionsChainAsync("AAPL");

            // Select 5 tradable options
            var optionSymbols = optionsChain.Options
                .Where(o => o.Tradable && o.ExpirationDate > DateTime.UtcNow)
                .OrderBy(o => o.ExpirationDate)
                .Take(5)
                .Select(o => o.Symbol)
                .ToList();

            if (!optionSymbols.Any())
            {
                _logger.LogWarning("No tradable options found");
                return;
            }

            _logger.LogInformation("Fetching IV for {Count} options in batch...", optionSymbols.Count);

            // Batch retrieval (more efficient)
            var ivDataDict = await _alpacaClient.GetImpliedVolatilityBatchAsync(optionSymbols);

            _logger.LogInformation("Successfully retrieved IV for {Count} options", ivDataDict.Count);
            _logger.LogInformation("");

            foreach (var (symbol, ivData) in ivDataDict)
            {
                _logger.LogInformation("{Symbol}: IV = {IV:P2}, Strike = {Strike:C2}, Type = {Type}",
                    symbol,
                    ivData.ImpliedVolatility,
                    ivData.StrikePrice,
                    ivData.OptionType.ToUpper());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in batch IV example");
        }
    }

    /// <summary>
    /// Example 7: Monitor rate limits
    /// </summary>
    public async Task MonitorRateLimitsExample()
    {
        _logger.LogInformation("=== RATE LIMIT MONITORING EXAMPLE ===");

        try
        {
            // Make a request
            await _alpacaClient.GetStockQuoteAsync("AAPL");

            // Check rate limit info
            var rateLimitInfo = _alpacaClient.GetRateLimitInfo();
            _logger.LogInformation("Rate Limit: {Remaining}/{Limit}",
                rateLimitInfo.Remaining,
                rateLimitInfo.Limit);
            _logger.LogInformation("Resets at: {ResetTime:HH:mm:ss}", rateLimitInfo.ResetTime);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in rate limit example");
        }
    }

    /// <summary>
    /// Example 8: Health check
    /// </summary>
    public async Task HealthCheckExample()
    {
        _logger.LogInformation("=== HEALTH CHECK EXAMPLE ===");

        try
        {
            var isHealthy = await _alpacaClient.HealthCheckAsync();
            _logger.LogInformation("API Health Status: {Status}", isHealthy ? "HEALTHY" : "UNHEALTHY");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in health check example");
        }
    }

    /// <summary>
    /// Run all examples
    /// </summary>
    public async Task RunAllExamplesAsync()
    {
        _logger.LogInformation("========================================");
        _logger.LogInformation("ALPACA API CLIENT - EXAMPLE USAGE");
        _logger.LogInformation("========================================");
        _logger.LogInformation("");

        await HealthCheckExample();
        _logger.LogInformation("");

        await GetStockDataExample();
        _logger.LogInformation("");

        await GetCryptoDataExample();
        _logger.LogInformation("");

        await GetOptionsChainExample();
        _logger.LogInformation("");

        await GetImpliedVolatilityExample();
        _logger.LogInformation("");

        await GetVolatilitySurfaceExample();
        _logger.LogInformation("");

        await GetBatchImpliedVolatilityExample();
        _logger.LogInformation("");

        await MonitorRateLimitsExample();
        _logger.LogInformation("");

        _logger.LogInformation("========================================");
        _logger.LogInformation("ALL EXAMPLES COMPLETED");
        _logger.LogInformation("========================================");
    }
}
