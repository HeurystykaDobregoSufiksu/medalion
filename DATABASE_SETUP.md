# Database Setup Guide

This guide explains how to set up and connect the entire codebase to the database using the repository pattern.

## Architecture Overview

The application now uses a comprehensive **Repository Pattern** with the following layers:

```
Blazor Components / Controllers
        ↓
Services (TradingService, MarketDataService)
        ↓
Repositories (Trade, Position, Asset, Market Data, etc.)
        ↓
DbContext (TradingBotDbContext)
        ↓
Entity Framework Core
        ↓
Database (SQL Server / PostgreSQL)
```

## What's Been Implemented

### 1. **Database Context**
- **TradingBotDbContext** (`Data/TradingBotDbContext.cs`)
  - 20+ DbSets for all entities
  - Automatic timestamp management (CreatedAt, UpdatedAt)
  - Soft delete support (IsDeleted, DeletedAt)
  - UTC DateTime handling
  - Configuration auto-discovery

### 2. **Domain Entities (20+ Models)**
All entities inherit from `BaseEntity` with:
- `Id` (Guid)
- `CreatedAt`, `UpdatedAt` (UTC timestamps)
- `IsDeleted`, `DeletedAt` (soft delete)

**Core Trading:**
- Asset
- Trade
- Position
- Strategy
- TradingSignal

**Alpaca Market Data:**
- StockQuoteSnapshot
- StockBarData
- CryptoQuoteSnapshot
- CryptoBarData
- OptionContractData
- OptionQuoteSnapshot

**Polymarket Data:**
- PolymarketEventData
- PolymarketMarketData
- PolymarketSnapshot
- PolymarketTradeData
- PolymarketOrderBookSnapshot

**Logging:**
- ApplicationLog
- ErrorLog
- PerformanceMetric
- AuditLog

### 3. **Repositories (10+ Implementations)**

**Generic Repository:**
- `IRepository<T>` / `Repository<T>`
  - CRUD operations
  - Pagination
  - Bulk operations
  - Soft delete support

**Specialized Repositories:**
- `ITradeRepository` / `TradeRepository`
- `IPositionRepository` / `PositionRepository`
- `IAssetRepository` / `AssetRepository`
- `IStrategyRepository` / `StrategyRepository`
- `ITradingSignalRepository` / `TradingSignalRepository`
- `IPolymarketEventRepository` / `PolymarketEventRepository`
- `IStockBarDataRepository` / `StockBarDataRepository`
- `IStockQuoteSnapshotRepository` / `StockQuoteSnapshotRepository`
- `ICryptoBarDataRepository` / `CryptoBarDataRepository`
- `IPolymarketSnapshotRepository` / `PolymarketSnapshotRepository`

### 4. **Services**

**TradingService** (`Data/Services/TradingService.cs`)
- Execute trades (market, limit orders)
- Manage positions (open, close, partial close)
- Update risk parameters (stop loss, take profit)
- Portfolio analytics
- P&L calculations

**MarketDataService** (`Data/Services/MarketDataService.cs`) - ✅ **NEWLY IMPLEMENTED**
- Store Alpaca stock data (quotes, bars)
- Store Alpaca crypto data (quotes, bars)
- Store Alpaca options data (contracts, quotes with IV)
- Store Polymarket data (events, markets, snapshots, trades, order books)
- Asset management (create, update, query)
- Data retrieval with DTOs

### 5. **Dependency Injection** (`Program.cs`) - ✅ **FULLY CONFIGURED**

All services and repositories are now registered:
```csharp
// Database
- TradingBotDbContext (with SQL Server + retry logic)

// Repositories
- Generic Repository<T>
- All specialized repositories

// Services
- TradingService
- MarketDataService

// External APIs
- AlpacaApiClient (configured from appsettings.json)
- PolymarketWebSocketService (optional, commented out)
```

## Database Migration Setup

### Prerequisites

1. Ensure you have .NET 7.0 SDK installed
2. Ensure EF Core tools are installed:
   ```bash
   dotnet tool install --global dotnet-ef
   ```

### Step 1: Create Initial Migration

Navigate to the project directory and create the initial migration:

```bash
cd /home/user/medalion/medalion
dotnet ef migrations add InitialCreate
```

This will create a `Migrations` folder with:
- `<timestamp>_InitialCreate.cs` - The migration file
- `TradingBotDbContextModelSnapshot.cs` - The model snapshot

### Step 2: Review the Migration

Open the generated migration file and verify it includes:
- All 20+ tables
- Indexes (Symbol, Status, ExecutedAt, etc.)
- Foreign key relationships
- Decimal precision (18, 8) for financial data
- Unique constraints (e.g., Asset.Symbol)

### Step 3: Apply the Migration

**For SQL Server (LocalDB):**
```bash
dotnet ef database update
```

This will:
1. Create the database `TradingBot` on LocalDB
2. Create all tables
3. Apply all indexes and constraints

**For PostgreSQL:**
1. Update `Program.cs` to use PostgreSQL (uncomment lines 36-41)
2. Update connection string in `appsettings.json`
3. Run:
   ```bash
   dotnet ef database update
   ```

### Step 4: Verify Database Creation

**SQL Server:**
```bash
# Connect to LocalDB
sqlcmd -S "(localdb)\mssqllocaldb" -d TradingBot

# List tables
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE';
```

**PostgreSQL:**
```bash
psql -h localhost -U postgres -d tradingbot -c "\dt"
```

## Configuration

### Connection Strings (`appsettings.json`)

The connection strings are already configured:

```json
{
  "ConnectionStrings": {
    "TradingBotDatabase": "Server=(localdb)\\mssqllocaldb;Database=TradingBot;Trusted_Connection=true;MultipleActiveResultSets=true",
    "PostgreSQL": "Host=localhost;Port=5432;Database=tradingbot;Username=postgres;Password=yourpassword"
  }
}
```

### Alpaca API Configuration

Add your Alpaca API credentials to `appsettings.json`:

```json
{
  "Alpaca": {
    "ApiKeyId": "YOUR_API_KEY_ID",
    "ApiSecretKey": "YOUR_API_SECRET_KEY",
    "BaseUrl": "https://paper-api.alpaca.markets",
    "DataBaseUrl": "https://data.alpaca.markets"
  }
}
```

## Usage Examples

### 1. Storing Alpaca Stock Data

```csharp
public class DataCollectionService
{
    private readonly IAlpacaApiClient _alpacaClient;
    private readonly IMarketDataService _marketDataService;

    public async Task CollectStockDataAsync(string symbol)
    {
        // Get or create asset
        var asset = await _marketDataService.GetAssetBySymbolAsync(symbol);
        if (asset == null)
        {
            asset = await _marketDataService.CreateOrUpdateAssetAsync(
                symbol,
                "Apple Inc.",
                AssetType.Stock,
                "us_equity",
                "NASDAQ");
        }

        // Fetch quote from Alpaca
        var quote = await _alpacaClient.GetStockQuoteAsync(symbol);

        // Store in database
        await _marketDataService.StoreStockQuoteAsync(asset.Id, quote);

        // Fetch and store bars
        var barsResponse = await _alpacaClient.GetStockBarsAsync(
            symbol,
            Timeframe.OneMinute,
            DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow);

        await _marketDataService.StoreStockBarsAsync(
            asset.Id,
            barsResponse.Bars,
            "1Min");
    }
}
```

### 2. Storing Polymarket Data

```csharp
public class PolymarketDataCollector
{
    private readonly IPolymarketWebSocketService _polymarketService;
    private readonly IMarketDataService _marketDataService;

    public async Task StartCollectionAsync()
    {
        // Subscribe to market data events
        _polymarketService.OnMarketDataReceived += async (sender, marketData) =>
        {
            // Find or create the market in database
            var markets = await _polymarketService.GetTrackedMarketsAsync();
            var market = markets.FirstOrDefault(m =>
                m.Markets.Any(x => x.Id == marketData.MarketId));

            if (market != null)
            {
                // Store event if not already stored
                var eventId = await _marketDataService.StorePolymarketEventAsync(market);

                // Find the market Guid from database
                // Then store snapshot
                await _marketDataService.StorePolymarketSnapshotAsync(
                    marketGuid,
                    marketData);
            }
        };

        await _polymarketService.StartAsync();
    }
}
```

### 3. Executing Trades

```csharp
public class TradingBot
{
    private readonly ITradingService _tradingService;

    public async Task ExecuteTradeAsync()
    {
        var request = new CreateTradeRequest
        {
            AssetId = assetGuid,
            OrderType = OrderType.Market,
            Side = TradeSide.Buy,
            Quantity = 100,
            LimitPrice = null,
            StrategyId = strategyGuid
        };

        var trade = await _tradingService.ExecuteMarketOrderAsync(request);

        Console.WriteLine($"Trade executed: {trade.Id} - {trade.Status}");
    }
}
```

## Database Schema Highlights

### Financial Precision
All decimal fields use `precision(18, 8)` for accurate financial calculations:
- Prices (Open, High, Low, Close, Strike, etc.)
- Quantities
- P&L calculations
- Implied Volatility

### Indexes for Performance
Strategic indexes on:
- `Asset.Symbol` (unique)
- `Trade.ExecutedAt`, `Trade.Status`
- `Position.Status`, `Position.OpenedAt`, `Position.ClosedAt`
- `TradingSignal.SignalTimestamp`, `TradingSignal.WasActedUpon`
- Composite indexes: `(StrategyId, ExecutedAt)`, `(AssetId, ExecutedAt)`

### Soft Delete
All entities support soft delete:
- `IsDeleted` flag
- `DeletedAt` timestamp
- Automatically applied by DbContext
- Query filters exclude deleted records

### JSON Storage
Large data objects stored as JSON:
- `Trade.AlpacaMarketDataSnapshot`
- `Trade.PolymarketDataSnapshot`
- `Strategy.Configuration`
- `PolymarketOrderBookSnapshot.BidsJson/AsksJson`

## Troubleshooting

### Migration Fails

If migration fails, check:
1. SQL Server LocalDB is running
2. Connection string is correct
3. No existing database conflicts

To reset:
```bash
dotnet ef database drop
dotnet ef database update
```

### Can't Connect to Database

1. Verify connection string in `appsettings.json`
2. Test connection manually:
   ```bash
   sqlcmd -S "(localdb)\mssqllocaldb"
   ```
3. Check SQL Server service is running

### Performance Issues

1. Ensure indexes are created (check migration)
2. Use pagination for large queries:
   ```csharp
   var (items, total) = await repository.GetPagedAsync(
       predicate: t => t.Status == TradeStatus.Filled,
       orderBy: t => t.ExecutedAt,
       offset: 0,
       limit: 100);
   ```
3. Use `.AsNoTracking()` for read-only queries

## Next Steps

1. ✅ **Run migrations** (create database)
2. Configure Alpaca API keys
3. Create background workers for continuous data collection
4. Implement trading strategies
5. Build Blazor UI for monitoring
6. Add analytics and reporting

## Support

For issues or questions, check:
- EF Core Documentation: https://learn.microsoft.com/en-us/ef/core/
- Repository Pattern: https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design

## Summary

🎉 **The entire codebase is now connected to the database!**

- ✅ 20+ domain entities
- ✅ 10+ specialized repositories
- ✅ 2 core services (Trading, MarketData)
- ✅ Full dependency injection setup
- ✅ Integration with Alpaca and Polymarket APIs
- ✅ Ready for migration and deployment

Just run the migrations and start trading!
