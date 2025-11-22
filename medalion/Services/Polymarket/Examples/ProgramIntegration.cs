using Medalion.Services.Polymarket;
using Medalion.Services.Polymarket.Interfaces;

namespace Medalion.Services.Polymarket.Examples;

/// <summary>
/// Example of how to integrate the Polymarket service into Program.cs or Startup.cs
/// </summary>
public static class ProgramIntegration
{
    /// <summary>
    /// Add this to your Program.cs to register the Polymarket service
    /// </summary>
    public static void ConfigurePolymarketService(WebApplicationBuilder builder)
    {
        // Register HttpClient factory (required for REST API calls)
        builder.Services.AddHttpClient();

        // Register the Polymarket WebSocket service as a singleton
        // Singleton ensures only one WebSocket connection is maintained
        builder.Services.AddSingleton<IPolymarketWebSocketService, PolymarketWebSocketService>();

        // Configure logging
        builder.Services.AddLogging(logging =>
        {
            logging.AddConsole();
            logging.AddDebug();
            logging.SetMinimumLevel(LogLevel.Information);
        });

        // Optional: Register as a hosted service for automatic lifecycle management
        builder.Services.AddHostedService<PolymarketBackgroundService>();
    }
}

/// <summary>
/// Background service that automatically starts/stops the Polymarket WebSocket service
/// with the application lifecycle
/// </summary>
public class PolymarketBackgroundService : BackgroundService
{
    private readonly IPolymarketWebSocketService _polymarketService;
    private readonly ILogger<PolymarketBackgroundService> _logger;

    public PolymarketBackgroundService(
        IPolymarketWebSocketService polymarketService,
        ILogger<PolymarketBackgroundService> logger)
    {
        _polymarketService = polymarketService;
        _logger = logger;

        // Subscribe to events
        SetupEventHandlers();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting Polymarket WebSocket background service...");

        try
        {
            // Start the WebSocket service
            await _polymarketService.StartAsync(stoppingToken);

            // Keep running until cancellation
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Polymarket background service is stopping.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error in Polymarket background service");
            throw;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping Polymarket WebSocket service...");
        await _polymarketService.StopAsync();
        await base.StopAsync(cancellationToken);
    }

    private void SetupEventHandlers()
    {
        // Market data updates (with implied volatility)
        _polymarketService.OnMarketDataReceived += (sender, data) =>
        {
            _logger.LogInformation(
                "[{Category}] Market {MarketId}: Price={Price:F4}, IV={IV:F2}%, Volume=${Volume:N0}, Liquidity=${Liquidity:N0}",
                data.Category,
                data.MarketId,
                data.Price,
                data.ImpliedVolatility,
                data.Volume24h,
                data.Liquidity
            );

            // TODO: Add your trading logic here
            // Example: if (data.ImpliedVolatility > 40) { /* High volatility signal */ }
        };

        // Trade executions
        _polymarketService.OnTradeReceived += (sender, trade) =>
        {
            _logger.LogInformation(
                "Trade: {MarketId} {Side} {Size} @ {Price}",
                trade.MarketId,
                trade.Side,
                trade.Size,
                trade.Price
            );

            // TODO: Track trade flow and market sentiment
        };

        // Price changes
        _polymarketService.OnPriceChanged += (sender, priceChange) =>
        {
            _logger.LogDebug(
                "Price change: {MarketId} -> {Price:F4}",
                priceChange.MarketId,
                priceChange.Price
            );
        };

        // Order book updates
        _polymarketService.OnOrderBookUpdated += (sender, orderBook) =>
        {
            _logger.LogDebug(
                "Order book: {MarketId} (Bids: {BidCount}, Asks: {AskCount})",
                orderBook.MarketId,
                orderBook.Bids.Count,
                orderBook.Asks.Count
            );

            // TODO: Analyze order book depth and liquidity
        };

        // Connection state changes
        _polymarketService.OnConnectionStateChanged += (sender, isConnected) =>
        {
            if (isConnected)
            {
                _logger.LogInformation("✅ Connected to Polymarket WebSocket");
            }
            else
            {
                _logger.LogWarning("❌ Disconnected from Polymarket WebSocket");
            }
        };

        // Errors
        _polymarketService.OnError += (sender, error) =>
        {
            _logger.LogError(error, "Polymarket service error occurred");

            // TODO: Send alert to monitoring system
        };
    }
}

/// <summary>
/// USAGE IN PROGRAM.CS:
/// ====================
///
/// using Medalion.Services.Polymarket.Examples;
///
/// var builder = WebApplication.CreateBuilder(args);
///
/// // Add Polymarket service
/// ProgramIntegration.ConfigurePolymarketService(builder);
///
/// // Add other services
/// builder.Services.AddRazorPages();
/// builder.Services.AddServerSideBlazor();
///
/// var app = builder.Build();
///
/// // Configure HTTP pipeline
/// app.UseHttpsRedirection();
/// app.UseStaticFiles();
/// app.UseRouting();
/// app.MapBlazorHub();
/// app.MapFallbackToPage("/_Host");
///
/// app.Run();
///
/// ====================
///
/// The service will now automatically:
/// 1. Start when the application starts
/// 2. Connect to Polymarket WebSocket
/// 3. Subscribe to Finance and Crypto markets
/// 4. Stream real-time data with events
/// 5. Reconnect automatically if connection drops
/// 6. Stop gracefully when application shuts down
/// </summary>
public static class ProgramUsageExample { }
