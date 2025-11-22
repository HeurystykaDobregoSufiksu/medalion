# Trading Bot Database Setup Guide

## Quick Start

### 1. Install Dependencies

```bash
cd medalion
dotnet restore
```

### 2. Configure Database Connection

Edit `appsettings.Development.json` and update the connection string:

**For SQL Server (LocalDB):**
```json
"ConnectionStrings": {
  "TradingBotDatabase": "Server=(localdb)\\mssqllocaldb;Database=TradingBotDev;Trusted_Connection=true;MultipleActiveResultSets=true"
}
```

**For PostgreSQL:**
```json
"ConnectionStrings": {
  "PostgreSQL": "Host=localhost;Port=5432;Database=tradingbot_dev;Username=postgres;Password=yourpassword"
}
```

### 3. Register Services in Program.cs

Add this code to your `Program.cs`:

```csharp
using Medalion.Data;
using Medalion.Data.Repositories;
using Medalion.Data.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddRazorPages();

// ========== DATABASE CONFIGURATION ==========

// Option A: SQL Server
builder.Services.AddDbContext<TradingBotDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("TradingBotDatabase"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null)
    ));

// Option B: PostgreSQL (comment out SQL Server above if using this)
// builder.Services.AddDbContext<TradingBotDbContext>(options =>
//     options.UseNpgsql(
//         builder.Configuration.GetConnectionString("PostgreSQL"),
//         npgsqlOptions => npgsqlOptions.EnableRetryOnFailure(
//             maxRetryCount: 3,
//             maxRetryDelay: TimeSpan.FromSeconds(5))
//     ));

// ========== REPOSITORY LAYER ==========

// Generic repository
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Specialized repositories
builder.Services.AddScoped<ITradeRepository, TradeRepository>();
builder.Services.AddScoped<IPositionRepository, PositionRepository>();
builder.Services.AddScoped<IAssetRepository, AssetRepository>();
builder.Services.AddScoped<IStrategyRepository, StrategyRepository>();
builder.Services.AddScoped<ITradingSignalRepository, TradingSignalRepository>();
builder.Services.AddScoped<IPolymarketEventRepository, PolymarketEventRepository>();

// Market data repositories
builder.Services.AddScoped<IStockBarDataRepository, StockBarDataRepository>();
builder.Services.AddScoped<IStockQuoteSnapshotRepository, StockQuoteSnapshotRepository>();
builder.Services.AddScoped<ICryptoBarDataRepository, CryptoBarDataRepository>();
builder.Services.AddScoped<IPolymarketSnapshotRepository, PolymarketSnapshotRepository>();

// ========== SERVICE LAYER ==========

builder.Services.AddScoped<ITradingService, TradingService>();
// Add MarketDataService implementation when ready

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    // Optional: Auto-migrate database in development
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<TradingBotDbContext>();
        // Uncomment to auto-migrate on startup (use with caution):
        // await dbContext.Database.MigrateAsync();
    }
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();

app.Run();
```

### 4. Create Initial Migration

```bash
# Install EF Core tools globally (if not already installed)
dotnet tool install --global dotnet-ef

# Verify installation
dotnet ef --version

# Create initial migration
dotnet ef migrations add InitialCreate --project medalion

# Review the generated migration in Migrations folder
```

### 5. Create Database

```bash
# Apply migration to create database
dotnet ef database update --project medalion
```

**Expected output:**
```
Build started...
Build succeeded.
Applying migration '20240101000000_InitialCreate'.
Done.
```

### 6. Verify Database Creation

**SQL Server:**
- Open SQL Server Management Studio or Azure Data Studio
- Connect to `(localdb)\mssqllocaldb`
- Verify database `TradingBotDev` exists with all tables

**PostgreSQL:**
- Use pgAdmin or command line: `psql -U postgres`
- List databases: `\l`
- Connect: `\c tradingbot_dev`
- List tables: `\dt`

## Database Tables

After migration, you should see these tables:

**Core:**
- Assets
- PolymarketEvents
- PolymarketMarkets

**Market Data:**
- StockQuoteSnapshots
- StockBarData
- CryptoQuoteSnapshots
- CryptoBarData
- OptionContracts
- OptionQuoteSnapshots
- PolymarketSnapshots
- PolymarketTradeData
- PolymarketOrderBookSnapshots

**Trading:**
- Strategies
- TradingSignals
- Trades
- Positions

**Logging:**
- ApplicationLogs
- ErrorLogs
- PerformanceMetrics
- AuditLogs

## Sample Usage

### Example: Create an Asset and Open a Position

```csharp
// In a controller or service
public class TradingController : ControllerBase
{
    private readonly ITradingService _tradingService;
    private readonly IAssetRepository _assetRepository;

    public TradingController(ITradingService tradingService, IAssetRepository assetRepository)
    {
        _tradingService = tradingService;
        _assetRepository = assetRepository;
    }

    [HttpPost("open-position")]
    public async Task<IActionResult> OpenPosition()
    {
        // 1. Create or get asset
        var asset = await _assetRepository.GetBySymbolAsync("AAPL");
        if (asset == null)
        {
            asset = new Asset
            {
                Symbol = "AAPL",
                Name = "Apple Inc.",
                AssetType = AssetType.Stock,
                AssetClass = "US Equity",
                Exchange = "NASDAQ",
                IsActive = true,
                IsTradable = true
            };
            asset = await _assetRepository.AddAsync(asset);
        }

        // 2. Open a position
        var request = new OpenPositionRequest
        {
            AssetId = asset.Id,
            Symbol = "AAPL",
            PositionSide = PositionSide.Long,
            Quantity = 10,
            EntryPrice = 175.50m,
            StopLoss = 170.00m,
            TakeProfit = 185.00m,
            Notes = "Bullish on tech earnings"
        };

        var position = await _tradingService.OpenPositionAsync(request);

        return Ok(position);
    }

    [HttpGet("portfolio-summary")]
    public async Task<IActionResult> GetPortfolioSummary()
    {
        var summary = await _tradingService.GetPortfolioSummaryAsync();
        return Ok(summary);
    }
}
```

### Example: Store Market Data

```csharp
public class MarketDataWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IAlpacaApiClient _alpacaClient;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var assetRepo = scope.ServiceProvider.GetRequiredService<IAssetRepository>();
        var stockQuoteRepo = scope.ServiceProvider.GetRequiredService<IStockQuoteSnapshotRepository>();

        while (!stoppingToken.IsCancellationRequested)
        {
            // Fetch quote from Alpaca
            var quote = await _alpacaClient.GetLatestStockQuoteAsync("AAPL");

            // Get asset
            var asset = await assetRepo.GetBySymbolAsync("AAPL");

            if (asset != null && quote != null)
            {
                // Store in database
                var snapshot = new StockQuoteSnapshot
                {
                    AssetId = asset.Id,
                    Symbol = quote.Symbol,
                    AskPrice = quote.AskPrice,
                    AskSize = quote.AskSize,
                    BidPrice = quote.BidPrice,
                    BidSize = quote.BidSize,
                    MidPrice = quote.MidPrice,
                    QuoteTimestamp = quote.Timestamp
                };

                await stockQuoteRepo.AddAsync(snapshot);
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}
```

## Troubleshooting

### Migration Issues

**Error: "Build failed"**
```bash
# Clean and rebuild
dotnet clean
dotnet build
```

**Error: "No DbContext was found"**
- Ensure `TradingBotDbContext` is in the correct namespace
- Verify the `--project` parameter points to the correct .csproj file

### Connection Issues

**SQL Server:**
```bash
# Test connection
sqlcmd -S "(localdb)\mssqllocaldb" -Q "SELECT @@VERSION"
```

**PostgreSQL:**
```bash
# Test connection
psql -U postgres -c "SELECT version();"
```

### View Generated SQL

```bash
# Generate SQL script without applying
dotnet ef migrations script --project medalion --output schema.sql

# Review schema.sql to see what will be created
```

## Advanced Configuration

### Enable Query Logging

In `appsettings.Development.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  }
}
```

### Connection Pooling

```csharp
// In Program.cs for SQL Server
options.UseSqlServer(connectionString, sqlOptions =>
{
    sqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
    sqlOptions.CommandTimeout(30);
});
```

### Add Indexes After Initial Migration

```bash
# Create a new migration for index optimization
dotnet ef migrations add AddPerformanceIndexes

# Edit the generated migration to add custom indexes if needed

# Apply migration
dotnet ef database update
```

## Next Steps

1. ✅ Database schema created
2. ✅ Repositories and services registered
3. ⬜ Implement MarketDataService
4. ⬜ Create background workers for data collection
5. ⬜ Integrate with Alpaca and Polymarket services
6. ⬜ Build trading strategies
7. ⬜ Create web UI for monitoring

See `DATABASE_ARCHITECTURE.md` for detailed schema documentation.
