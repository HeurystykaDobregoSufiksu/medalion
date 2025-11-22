using Medalion.Services.Alpaca;
using Medalion.Services.Alpaca.Examples;
using Medalion.Services.Alpaca.Interfaces;
using Medalion.Services.Alpaca.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Medalion.Services.Alpaca.Examples;

/// <summary>
/// Demonstrates how to integrate Alpaca API client into your application
/// Shows dependency injection setup, configuration, and usage
/// </summary>
public static class ProgramIntegration
{
    /// <summary>
    /// Example 1: Basic setup with manual configuration
    /// </summary>
    public static async Task BasicSetupExample()
    {
        // Create service collection
        var services = new ServiceCollection();

        // Add logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        // Add HTTP client factory
        services.AddHttpClient();

        // Configure Alpaca API
        var config = new AlpacaApiConfiguration
        {
            ApiKeyId = "YOUR_API_KEY_ID",
            ApiSecretKey = "YOUR_API_SECRET_KEY",
            BaseUrl = "https://paper-api.alpaca.markets", // Use paper trading for testing
            DataBaseUrl = "https://data.alpaca.markets",
            MaxRetryAttempts = 3,
            InitialRetryDelayMs = 1000,
            MaxRequestsPerMinute = 200
        };

        // Register Alpaca client as singleton
        services.AddSingleton(config);
        services.AddSingleton<IAlpacaApiClient>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<AlpacaApiClient>>();
            var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
            return new AlpacaApiClient(config, logger, httpClientFactory);
        });

        // Register example usage
        services.AddTransient<AlpacaExampleUsage>();

        // Build service provider
        var serviceProvider = services.BuildServiceProvider();

        // Use the client
        var alpacaClient = serviceProvider.GetRequiredService<IAlpacaApiClient>();
        var exampleUsage = serviceProvider.GetRequiredService<AlpacaExampleUsage>();

        // Run examples
        await exampleUsage.GetImpliedVolatilityExample();

        // Cleanup
        await serviceProvider.DisposeAsync();
    }

    /// <summary>
    /// Example 2: Setup with configuration from appsettings.json
    /// </summary>
    public static void ConfigureServicesFromAppSettings(IServiceCollection services)
    {
        // In your Startup.cs or Program.cs, add this to ConfigureServices:

        // Add logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        // Add HTTP client factory with named clients
        services.AddHttpClient("AlpacaApi", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddHttpClient("AlpacaDataApi", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        // Register Alpaca configuration
        // This assumes you have an "Alpaca" section in appsettings.json
        services.AddSingleton(sp =>
        {
            // In a real app, you'd use IConfiguration to bind this
            return new AlpacaApiConfiguration
            {
                ApiKeyId = Environment.GetEnvironmentVariable("ALPACA_API_KEY_ID") ?? "",
                ApiSecretKey = Environment.GetEnvironmentVariable("ALPACA_API_SECRET_KEY") ?? "",
                BaseUrl = Environment.GetEnvironmentVariable("ALPACA_BASE_URL") ?? "https://paper-api.alpaca.markets",
                DataBaseUrl = "https://data.alpaca.markets",
                MaxRetryAttempts = 3,
                InitialRetryDelayMs = 1000,
                MaxRequestsPerMinute = 200
            };
        });

        // Register Alpaca API client
        services.AddSingleton<IAlpacaApiClient, AlpacaApiClient>();

        // Register example usage (for testing)
        services.AddTransient<AlpacaExampleUsage>();
    }

    /// <summary>
    /// Example 3: Using the client in a background service
    /// Useful for trading bots that monitor IV continuously
    /// </summary>
    public class ImpliedVolatilityMonitorService : BackgroundService
    {
        private readonly IAlpacaApiClient _alpacaClient;
        private readonly ILogger<ImpliedVolatilityMonitorService> _logger;
        private readonly List<string> _symbolsToMonitor = new() { "AAPL", "TSLA", "SPY" };

        public ImpliedVolatilityMonitorService(
            IAlpacaApiClient alpacaClient,
            ILogger<ImpliedVolatilityMonitorService> logger)
        {
            _alpacaClient = alpacaClient;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("IV Monitor Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    foreach (var symbol in _symbolsToMonitor)
                    {
                        await MonitorSymbolIV(symbol, stoppingToken);
                    }

                    // Wait 5 minutes before next check
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in IV monitoring loop");
                    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                }
            }

            _logger.LogInformation("IV Monitor Service stopped");
        }

        private async Task MonitorSymbolIV(string symbol, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Monitoring IV for {Symbol}", symbol);

                // Get options chain for the next 30 days
                var expirationDate = DateTime.UtcNow.AddDays(30);
                var ivChain = await _alpacaClient.GetImpliedVolatilityChainAsync(
                    symbol,
                    expirationDate,
                    cancellationToken);

                _logger.LogInformation(
                    "{Symbol} - Options: {Count}, Avg IV: {IV:P2}, Underlying: {Price:C2}",
                    symbol,
                    ivChain.Options.Count,
                    ivChain.AverageImpliedVolatility,
                    ivChain.UnderlyingPrice);

                // Check for high IV conditions (example: IV > 50%)
                var highIVOptions = ivChain.Options
                    .Where(o => o.ImpliedVolatility > 0.50m)
                    .ToList();

                if (highIVOptions.Any())
                {
                    _logger.LogWarning(
                        "HIGH IV ALERT: {Symbol} has {Count} options with IV > 50%",
                        symbol,
                        highIVOptions.Count);

                    foreach (var option in highIVOptions.Take(3))
                    {
                        _logger.LogWarning(
                            "  {OptionSymbol}: IV = {IV:P2}, Strike = {Strike:C2}",
                            option.Symbol,
                            option.ImpliedVolatility,
                            option.StrikePrice);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error monitoring IV for {Symbol}", symbol);
            }
        }
    }

    /// <summary>
    /// Example 4: Trading bot that uses IV data
    /// Demonstrates how to use IV for trading decisions
    /// </summary>
    public class IVBasedTradingBot
    {
        private readonly IAlpacaApiClient _alpacaClient;
        private readonly ILogger<IVBasedTradingBot> _logger;

        public IVBasedTradingBot(
            IAlpacaApiClient alpacaClient,
            ILogger<IVBasedTradingBot> logger)
        {
            _alpacaClient = alpacaClient;
            _logger = logger;
        }

        /// <summary>
        /// Strategy: Sell options when IV is high, buy when IV is low
        /// This is a simplified example for demonstration purposes
        /// </summary>
        public async Task<List<TradingSignal>> GenerateTradingSignalsAsync(
            string underlyingSymbol,
            CancellationToken cancellationToken = default)
        {
            var signals = new List<TradingSignal>();

            try
            {
                // Get IV chain
                var ivChain = await _alpacaClient.GetImpliedVolatilityChainAsync(
                    underlyingSymbol,
                    null,
                    cancellationToken);

                // Calculate IV percentile (simplified - in production, use historical data)
                var avgIV = ivChain.AverageImpliedVolatility;
                var ivPercentile = CalculateIVPercentile(ivChain.Options);

                _logger.LogInformation(
                    "{Symbol} - Avg IV: {IV:P2}, IV Percentile: {Percentile:F2}",
                    underlyingSymbol,
                    avgIV,
                    ivPercentile);

                // High IV (> 70th percentile) -> Sell options (premium collection)
                if (ivPercentile > 70)
                {
                    var atmOptions = FindATMOptions(ivChain);
                    foreach (var option in atmOptions.Take(2))
                    {
                        signals.Add(new TradingSignal
                        {
                            Symbol = option.Symbol,
                            Action = "SELL",
                            Reason = $"High IV ({option.ImpliedVolatility:P2}) - Premium collection opportunity",
                            ImpliedVolatility = option.ImpliedVolatility,
                            StrikePrice = option.StrikePrice,
                            ExpirationDate = option.ExpirationDate
                        });
                    }
                }
                // Low IV (< 30th percentile) -> Buy options (cheap premium)
                else if (ivPercentile < 30)
                {
                    var atmOptions = FindATMOptions(ivChain);
                    foreach (var option in atmOptions.Take(2))
                    {
                        signals.Add(new TradingSignal
                        {
                            Symbol = option.Symbol,
                            Action = "BUY",
                            Reason = $"Low IV ({option.ImpliedVolatility:P2}) - Cheap premium",
                            ImpliedVolatility = option.ImpliedVolatility,
                            StrikePrice = option.StrikePrice,
                            ExpirationDate = option.ExpirationDate
                        });
                    }
                }

                _logger.LogInformation("Generated {Count} trading signals for {Symbol}",
                    signals.Count,
                    underlyingSymbol);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating trading signals for {Symbol}", underlyingSymbol);
            }

            return signals;
        }

        private decimal CalculateIVPercentile(List<ImpliedVolatilityData> options)
        {
            if (!options.Any()) return 0;

            var avgIV = options.Average(o => o.ImpliedVolatility);
            var sortedIVs = options.Select(o => o.ImpliedVolatility).OrderBy(iv => iv).ToList();
            var index = sortedIVs.BinarySearch(avgIV);
            if (index < 0) index = ~index;

            return (decimal)index / sortedIVs.Count * 100;
        }

        private List<ImpliedVolatilityData> FindATMOptions(ImpliedVolatilityChainResponse ivChain)
        {
            var underlyingPrice = ivChain.UnderlyingPrice;

            return ivChain.Options
                .Where(o => o.DaysToExpiration >= 14 && o.DaysToExpiration <= 45) // 2-6 weeks out
                .OrderBy(o => Math.Abs(o.StrikePrice - underlyingPrice))
                .Take(4)
                .ToList();
        }
    }

    public class TradingSignal
    {
        public string Symbol { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public decimal ImpliedVolatility { get; set; }
        public decimal StrikePrice { get; set; }
        public DateTime ExpirationDate { get; set; }
    }
}
