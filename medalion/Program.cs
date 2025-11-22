using medalion.Data;
using Medalion.Data;
using Medalion.Data.Repositories;
using Medalion.Data.Services;
using Medalion.Data.Domain;
using Medalion.Services.Alpaca;
using Medalion.Services.Alpaca.Interfaces;
using Medalion.Services.Alpaca.Models;
using Medalion.Services.Polymarket;
using Medalion.Services.Polymarket.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();

// ============================================
// DATABASE CONFIGURATION
// ============================================

// Add DbContext with SQL Server
builder.Services.AddDbContext<TradingBotDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("TradingBotDatabase"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null)));

// Alternative: PostgreSQL (uncomment to use)
// builder.Services.AddDbContext<TradingBotDbContext>(options =>
//     options.UseNpgsql(
//         builder.Configuration.GetConnectionString("PostgreSQL"),
//         npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(
//             maxRetryCount: 3,
//             maxRetryDelay: TimeSpan.FromSeconds(5))));

// ============================================
// REPOSITORY REGISTRATION
// ============================================

// Generic Repository
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Specialized Repositories
builder.Services.AddScoped<ITradeRepository, TradeRepository>();
builder.Services.AddScoped<IPositionRepository, PositionRepository>();
builder.Services.AddScoped<IAssetRepository, AssetRepository>();
builder.Services.AddScoped<IStrategyRepository, StrategyRepository>();
builder.Services.AddScoped<ITradingSignalRepository, TradingSignalRepository>();
builder.Services.AddScoped<IPolymarketEventRepository, PolymarketEventRepository>();

// Market Data Repositories
builder.Services.AddScoped<IStockBarDataRepository, StockBarDataRepository>();
builder.Services.AddScoped<IStockQuoteSnapshotRepository, StockQuoteSnapshotRepository>();
builder.Services.AddScoped<ICryptoBarDataRepository, CryptoBarDataRepository>();
builder.Services.AddScoped<IPolymarketSnapshotRepository, PolymarketSnapshotRepository>();

// ============================================
// SERVICE LAYER REGISTRATION
// ============================================

// Core Trading Services
builder.Services.AddScoped<ITradingService, TradingService>();
builder.Services.AddScoped<IMarketDataService, MarketDataService>();

// ============================================
// EXTERNAL API SERVICES
// ============================================

// HttpClient Factory
builder.Services.AddHttpClient();

// Alpaca API Client
builder.Services.AddScoped<IAlpacaApiClient>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<AlpacaApiClient>>();
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var configuration = sp.GetRequiredService<IConfiguration>();

    var config = new AlpacaApiConfiguration
    {
        ApiKeyId = configuration["Alpaca:ApiKeyId"] ?? string.Empty,
        ApiSecretKey = configuration["Alpaca:ApiSecretKey"] ?? string.Empty,
        BaseUrl = configuration["Alpaca:BaseUrl"] ?? "https://paper-api.alpaca.markets",
        DataBaseUrl = configuration["Alpaca:DataBaseUrl"] ?? "https://data.alpaca.markets",
        TimeoutSeconds = int.Parse(configuration["Alpaca:TimeoutSeconds"] ?? "30"),
        MaxRetryAttempts = int.Parse(configuration["Alpaca:MaxRetryAttempts"] ?? "3"),
        InitialRetryDelayMs = int.Parse(configuration["Alpaca:InitialRetryDelayMs"] ?? "1000"),
        MaxRequestsPerMinute = int.Parse(configuration["Alpaca:MaxRequestsPerMinute"] ?? "200")
    };

    return new AlpacaApiClient(config, logger, httpClientFactory);
});

// Polymarket WebSocket Service (optional - requires trading config)
// Uncomment and configure if you want to enable Polymarket trading
// builder.Services.AddSingleton<IPolymarketWebSocketService>(sp =>
// {
//     var logger = sp.GetRequiredService<ILogger<PolymarketWebSocketService>>();
//     var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
//
//     // Optional: Add trading configuration
//     // var tradingConfig = new TradingConfig
//     // {
//     //     WalletAddress = "your_wallet_address",
//     //     PrivateKey = "your_private_key",
//     //     ChainId = 137
//     // };
//
//     return new PolymarketWebSocketService(logger, httpClientFactory, tradingConfig: null);
// });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
