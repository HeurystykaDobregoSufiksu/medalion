# Medalion Trading Bot - Complete Documentation

## Table of Contents
- [Quickstart](#quickstart)
- [Application Overview](#application-overview)
- [Core Features](#core-features)
- [User Flows & Workflows](#user-flows--workflows)
- [Dashboard Components](#dashboard-components)
- [Trading Operations](#trading-operations)
- [Market Data Integration](#market-data-integration)
- [Database Architecture](#database-architecture)
- [Configuration](#configuration)
- [API Integration](#api-integration)
- [Development Guide](#development-guide)
- [Troubleshooting](#troubleshooting)

---

## Quickstart

### Prerequisites
- .NET 7.0 SDK or later
- SQL Server or PostgreSQL
- Node.js (for Tailwind CSS compilation)
- Alpaca API credentials (get free paper trading account at [alpaca.markets](https://alpaca.markets))
- (Optional) Polymarket account for prediction market trading

### Installation & Setup (5 minutes)

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd medalion
   ```

2. **Install dependencies**
   ```bash
   dotnet restore
   npm install
   ```

3. **Configure your API keys**

   Edit `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "TradingBotDatabase": "Server=localhost;Database=TradingBot;Trusted_Connection=true;"
     },
     "Alpaca": {
       "ApiKeyId": "YOUR_ALPACA_KEY",
       "ApiSecretKey": "YOUR_ALPACA_SECRET",
       "BaseUrl": "https://paper-api.alpaca.markets"
     }
   }
   ```

4. **Set up the database**
   ```bash
   dotnet ef database update
   ```

5. **Build Tailwind CSS**
   ```bash
   npm run build:css
   ```

6. **Run the application**
   ```bash
   dotnet run
   ```

7. **Access the dashboard**

   Open your browser to `https://localhost:5001/dashboard`

### First Trading Operations

Once the dashboard loads, you can:
- **Monitor positions**: View all open positions with real-time P&L
- **Execute trades**: Use the trading service to place orders
- **Track performance**: See daily stats, win rate, and total P&L
- **Monitor services**: Check Alpaca and Polymarket connection health

For detailed setup instructions, see [SETUP_GUIDE.md](SETUP_GUIDE.md).

---

## Application Overview

**Medalion** is a production-ready algorithmic trading bot built with .NET 7.0 and Blazor that enables automated trading across multiple financial markets with real-time monitoring and analytics.

### Purpose
- Execute automated trading strategies across stocks, crypto, and prediction markets
- Monitor positions and portfolio performance in real-time
- Track Implied Volatility (IV) as a key decision metric
- Maintain detailed audit trails and market snapshots for every trade
- Provide responsive web-based dashboard for trading oversight

### Supported Markets
1. **Traditional Stocks** (via Alpaca)
2. **Cryptocurrency** (via Alpaca)
3. **Prediction Markets** (via Polymarket)
4. **Options Contracts** (via Alpaca, with Greeks and IV data)

### Technology Stack
- **Framework**: .NET 7.0
- **UI**: Blazor Server with Tailwind CSS
- **Database**: Entity Framework Core 7.0 (SQL Server/PostgreSQL)
- **APIs**: Alpaca REST API, Polymarket WebSocket & CLOB
- **Blockchain**: Nethereum for Ethereum-based order signing

---

## Core Features

### 1. Multi-Market Trading

#### Stock Trading
- Market and limit orders for US equities
- Real-time quote and historical bar data (1Min, 5Min, 15Min, 1Hour, 1Day)
- Corporate actions tracking
- Position sizing and risk management

#### Cryptocurrency Trading
- Trade major cryptocurrencies (BTC, ETH, etc.)
- 24/7 market access
- Real-time crypto quotes and OHLCV data
- Integrated with Alpaca Crypto API

#### Options Trading
- Query options chains with filtering
- Greeks calculation (Delta, Gamma, Theta, Vega, Rho)
- Implied Volatility tracking (primary focus)
- Support for calls and puts
- Multi-leg option strategies

#### Prediction Market Trading
- Connect to Polymarket via WebSocket
- Real-time market probability updates
- Ethereum-based order signing
- CLOB (Central Limit Order Book) integration
- Implied Volatility calculation for prediction markets

### 2. Strategy Engine

#### Strategy Types
- **Mean Reversion**: Trade based on price deviations from average
- **Momentum**: Follow trending markets
- **Arbitrage**: Exploit price differences across markets
- **Volatility-Based**: Trade based on IV levels and changes

#### Signal Generation
- Generate BUY/SELL/HOLD signals
- Confidence scoring (0.0 - 1.0)
- Target price and stop-loss recommendations
- Signal metadata and reasoning stored in database

#### Strategy Configuration
```csharp
public class Strategy
{
    public string Name { get; set; }
    public StrategyType Type { get; set; }
    public StrategyStatus Status { get; set; }  // Active, Paused, Stopped
    public string? Configuration { get; set; }  // JSON parameters
    public decimal MaxPositionSize { get; set; }
    public decimal? StopLossPercentage { get; set; }
    public decimal? TakeProfitPercentage { get; set; }
}
```

### 3. Order Execution & Position Management

#### Order Types
- **Market Orders**: Execute immediately at current market price
- **Limit Orders**: Execute only at specified price or better
- **Stop-Loss Orders**: Automatically close position at loss threshold
- **Take-Profit Orders**: Automatically close position at profit target

#### Position Tracking
- Real-time unrealized P&L calculation
- Position-level stop-loss and take-profit management
- Partial position closure (reduce quantity while keeping position open)
- Complete position history with entry/exit tracking
- Market snapshots at trade execution for audit trails

#### Risk Management
- Maximum position size limits per strategy
- Automatic stop-loss execution
- Take-profit target monitoring
- Portfolio-level exposure tracking

### 4. Real-Time Market Data

#### Alpaca Market Data
- **Stock Quotes**: Real-time bid/ask/last prices
- **Stock Bars**: OHLCV candles at multiple timeframes
- **Crypto Quotes**: 24/7 cryptocurrency pricing
- **Crypto Bars**: Historical crypto candle data
- **Option Contracts**: Full option chain data
- **Option Quotes**: Real-time option pricing with Greeks

#### Polymarket Data Streaming
- WebSocket connection for real-time updates
- Market probability changes
- Order book snapshots (bids/asks)
- Trade execution data
- Implied Volatility calculations
- Automatic reconnection with heartbeat monitoring

#### Data Storage
All market data is stored in the database for:
- Historical analysis and backtesting
- Audit compliance
- Performance attribution
- Strategy optimization

### 5. Portfolio Analytics Dashboard

A responsive, real-time dashboard built with Blazor and Tailwind CSS featuring five main widgets:

#### Daily Stats Widget
- Total trades executed today
- Total profit/loss (absolute and percentage)
- Win rate calculation
- Average trade P&L
- Sparkline charts for visual trends

#### Positions Widget
- Table of all open positions
- Real-time unrealized P&L updates
- Side (Long/Short), quantity, and prices
- Entry price vs. current price
- Modify and close position actions
- Color-coded P&L (green for profit, red for loss)

#### Health Stats Widget
- Service connection status monitoring
- Alpaca API health check
- Polymarket WebSocket connection status
- Visual indicators (green=connected, red=disconnected)
- Last update timestamps

#### Recent Actions Widget
- Activity feed of recent trades
- Trade side (BUY/SELL) with color coding
- Timestamp and asset symbol
- Quantity and price information
- Chronological ordering (newest first)

#### Errors Widget
- Real-time error logging display
- Severity level badges (Critical, Error, Warning, Info)
- Error messages and stack traces
- Timestamp tracking
- Unacknowledged error highlighting

### 6. Logging & Audit System

#### Application Logs
- General application events
- Service startup/shutdown
- Configuration changes
- User actions

#### Error Logs
- Exception tracking with stack traces
- Severity levels (Critical, Error, Warning, Info, Debug)
- Automatic error categorization
- Acknowledged/unacknowledged status

#### Performance Metrics
- Operation execution times
- API call latencies
- Database query performance
- Resource utilization tracking

#### Audit Logs
- Complete trade execution history
- Position modification tracking
- User actions and timestamps
- Market snapshot storage for reproducibility

---

## User Flows & Workflows

### Flow 1: Starting the Application

```
1. Application starts → Program.cs
2. Dependency injection configured
3. Database connection established
4. Services registered (TradingService, MarketDataService, etc.)
5. Alpaca API client initialized
6. Polymarket WebSocket connection attempted
7. Dashboard becomes available at /dashboard
8. Auto-refresh begins (5-second intervals)
```

**Key Files**:
- `Program.cs:1-50` - DI configuration
- `Data/TradingBotDbContext.cs:1-100` - Database setup

### Flow 2: Monitoring the Dashboard

```
User navigates to /dashboard
  ↓
DashboardStateService.GetDashboardDataAsync() called
  ↓
Queries database for:
  - Open positions (with unrealized P&L calculation)
  - Today's trades
  - Recent errors
  - Service health status
  ↓
Data mapped to ViewModels
  ↓
Rendered in 5 dashboard widgets
  ↓
Auto-refresh every 5 seconds via Timer
  ↓
Updates reflected in UI (StateHasChanged)
```

**Key Files**:
- `Pages/Dashboard.razor:1-100` - Main dashboard page
- `Services/DashboardStateService.cs:1-200` - Data aggregation
- `Components/Dashboard/*.razor` - Individual widgets

### Flow 3: Executing a Manual Trade

```
Strategy generates trading signal
  ↓
TradingService.ExecuteTradeAsync() called
  ↓
Create Trade entity:
  - Asset, side (Buy/Sell), quantity, price
  - Strategy reference
  - Market snapshot (JSON)
  ↓
Execute order via API:
  - Alpaca: AlpacaApiClient.CreateOrderAsync()
  - Polymarket: PolymarketTradingClient.PlaceOrderAsync()
  ↓
Order confirmation received
  ↓
Update or create Position entity:
  - Increment quantity (Buy) or decrement (Sell)
  - Calculate average entry price
  - Set stop-loss/take-profit levels
  ↓
Store market snapshot:
  - StockQuoteSnapshot or PolymarketSnapshot
  - Captures market conditions at execution time
  ↓
Save Trade and Position to database
  ↓
Dashboard reflects new position immediately
```

**Key Files**:
- `Data/Services/TradingService.cs:1-300` - Trade execution logic
- `Services/Alpaca/AlpacaApiClient.cs:200-250` - Alpaca order placement
- `Services/Polymarket/PolymarketTradingClient.cs:100-200` - Polymarket orders

### Flow 4: Closing a Position

```
User clicks "Close" on position in dashboard
  ↓
TradingService.ClosePositionAsync(positionId) called
  ↓
Retrieve Position from database
  ↓
Calculate quantity to close (full or partial)
  ↓
Create closing Trade:
  - Side opposite to position (Long→Sell, Short→Buy)
  - Quantity equals position size
  - Current market price
  ↓
Execute closing order via API
  ↓
Calculate realized P&L:
  RealizedPnL = (ExitPrice - AvgEntryPrice) × Quantity × (Long ? 1 : -1)
  ↓
Update Position entity:
  - Mark as closed
  - Set ExitPrice and ExitDate
  - Store RealizedPnL
  - Soft delete (IsDeleted = true)
  ↓
Save to database
  ↓
Dashboard removes position from open positions list
```

**Key Files**:
- `Data/Services/TradingService.cs:150-200` - Position closing logic
- `Components/Dashboard/PositionsWidget.razor:50-100` - Close button handler

### Flow 5: Real-Time Market Data Collection (Polymarket)

```
PolymarketWebSocketService starts
  ↓
Connect to wss://ws-subscriptions-clob.polymarket.com/ws/market
  ↓
Send subscription message for tracked markets
  ↓
Receive real-time updates:
  - Market price changes
  - Order book updates
  - Trade executions
  ↓
Parse JSON message
  ↓
Calculate Implied Volatility (if applicable)
  ↓
Create PolymarketSnapshot entity:
  - Timestamp, market ID, prices
  - Order book data (JSON)
  - Calculated IV
  ↓
MarketDataService.StorePolymarketSnapshotAsync()
  ↓
Save to database
  ↓
Fire OnMarketUpdate event
  ↓
Subscribers (strategies, dashboard) receive update
  ↓
Heartbeat monitor checks connection (every 30 seconds)
  ↓
If disconnected → automatic reconnection with exponential backoff
```

**Key Files**:
- `Services/Polymarket/PolymarketWebSocketService.cs:1-500` - WebSocket handling
- `Data/Services/MarketDataService.cs:100-150` - Market data storage

### Flow 6: Retrieving Implied Volatility (Alpaca Options)

```
Request IV for option contract
  ↓
AlpacaApiClient.GetOptionContractsAsync() called
  ↓
Filter by underlying symbol, expiration, strike
  ↓
Send GET request to Alpaca Options API
  ↓
Receive option contract data with Greeks:
  - Delta, Gamma, Theta, Vega, Rho
  - Implied Volatility (primary focus)
  - Bid/Ask prices
  ↓
Parse response into OptionContractDto
  ↓
Store in OptionContract entity (optional)
  ↓
Return IV value to strategy
  ↓
Strategy uses IV for trading decisions
```

**Key Files**:
- `Services/Alpaca/AlpacaApiClient.cs:400-500` - Options data retrieval
- `Services/Alpaca/Models/AlpacaModels.cs:200-250` - Option DTOs

### Flow 7: Error Handling & Recovery

```
Any operation encounters exception
  ↓
Try-catch block captures exception
  ↓
Create ErrorLog entity:
  - Message, StackTrace, Source
  - Severity level determination
  - Timestamp
  - IsAcknowledged = false
  ↓
Log to database via repository
  ↓
Log to console/file via ILogger
  ↓
Dashboard ErrorsWidget queries recent errors
  ↓
Display error with severity badge
  ↓
Critical errors → notify user
  ↓
Retry logic (if applicable):
  - Exponential backoff for API calls
  - Reconnection for WebSocket
  - Database query retry
```

**Key Files**:
- `Data/Domain/Logging.cs:50-100` - ErrorLog entity
- `Components/Dashboard/ErrorsWidget.razor:1-100` - Error display

---

## Dashboard Components

### Layout Structure

```
MainLayout.razor
├── NavMenu.razor (left sidebar)
│   ├── Home
│   ├── Dashboard ← Main feature
│   ├── Counter (demo)
│   └── Fetch data (demo)
└── @Body (page content)
    └── Dashboard.razor
        ├── DailyStatsWidget.razor
        ├── PositionsWidget.razor
        ├── HealthStatsWidget.razor
        ├── RecentActionsWidget.razor
        └── ErrorsWidget.razor
```

### Component Details

#### Dashboard.razor
**Location**: `Pages/Dashboard.razor`

**Responsibilities**:
- Inject `DashboardStateService`
- Set up 5-second auto-refresh timer
- Coordinate all widgets
- Handle loading states
- Manage error display for data fetch failures

**Code Example**:
```csharp
@code {
    protected override async Task OnInitializedAsync()
    {
        await LoadDataAsync();

        // Auto-refresh every 5 seconds
        _timer = new Timer(async _ =>
        {
            await InvokeAsync(async () =>
            {
                await LoadDataAsync();
                StateHasChanged();
            });
        }, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
    }
}
```

#### DailyStatsWidget.razor
**Location**: `Components/Dashboard/DailyStatsWidget.razor`

**Parameters**:
- `DailyStatsViewModel Stats` - Daily trading statistics

**Displays**:
- Total trades count
- Total P&L ($ and %)
- Win rate (%)
- Average trade P&L
- Sparkline chart (visual trend)

**Styling**: Tailwind CSS with gradient backgrounds and responsive grid

#### PositionsWidget.razor
**Location**: `Components/Dashboard/PositionsWidget.razor`

**Parameters**:
- `List<PositionViewModel> Positions` - Open positions

**Features**:
- Table with columns: Symbol, Side, Quantity, Entry Price, Current Price, Unrealized P&L
- Color-coded P&L (text-green-600 / text-red-600)
- Action buttons: Modify, Close
- Empty state message when no positions

**Actions**:
- `OnModifyPosition(positionId)` - Opens modification dialog (to be implemented)
- `OnClosePosition(positionId)` - Calls `TradingService.ClosePositionAsync()`

#### HealthStatsWidget.razor
**Location**: `Components/Dashboard/HealthStatsWidget.razor`

**Parameters**:
- `bool IsAlpacaConnected`
- `bool IsPolymarketConnected`

**Features**:
- Service status indicators with colored badges
- Alpaca API connection status
- Polymarket WebSocket connection status
- Visual health check (🟢 / 🔴)

**Health Check Logic**:
```csharp
IsAlpacaConnected = await AlpacaClient.HealthCheckAsync();
IsPolymarketConnected = PolymarketService.IsConnected;
```

#### RecentActionsWidget.razor
**Location**: `Components/Dashboard/RecentActionsWidget.razor`

**Parameters**:
- `List<TradeViewModel> RecentTrades` - Recent trade executions

**Features**:
- Chronological list (newest first)
- Trade side badges (BUY in green, SELL in red)
- Symbol, quantity, price information
- Timestamp in readable format
- Scrollable container with max height

#### ErrorsWidget.razor
**Location**: `Components/Dashboard/ErrorsWidget.razor`

**Parameters**:
- `List<ErrorLogViewModel> RecentErrors` - Recent error logs

**Features**:
- Severity badges (Critical=red, Error=orange, Warning=yellow, Info=blue)
- Error message display
- Stack trace expansion (to be implemented)
- Timestamp and source information
- Unacknowledged error highlighting

---

## Trading Operations

### Creating and Executing Trades

#### Manual Trade Execution

```csharp
// Inject service
@inject ITradingService TradingService

// Execute trade
var request = new TradeExecutionRequest
{
    AssetId = assetId,
    StrategyId = strategyId,
    Side = TradeSide.Buy,
    Quantity = 10,
    OrderType = OrderType.Market,
    LimitPrice = null,  // Not needed for market orders
    StopLossPrice = 95.00m,
    TakeProfitPrice = 105.00m
};

var result = await TradingService.ExecuteTradeAsync(request);

if (result.Success)
{
    Console.WriteLine($"Trade executed: {result.Trade.Id}");
}
else
{
    Console.WriteLine($"Error: {result.ErrorMessage}");
}
```

#### Strategy-Based Trading

```csharp
// Generate signal
var signal = new TradingSignal
{
    StrategyId = strategy.Id,
    AssetId = asset.Id,
    SignalType = SignalType.Buy,
    Confidence = 0.85m,
    TargetPrice = 102.50m,
    StopLossPrice = 97.00m,
    Reason = "IV below historical average, mean reversion expected"
};

await SignalRepository.AddAsync(signal);

// Process signal
await TradingService.ProcessSignalAsync(signal.Id);
```

### Position Management Operations

#### Modifying Position Parameters

```csharp
var request = new PositionModificationRequest
{
    PositionId = positionId,
    NewStopLoss = 98.00m,      // Update stop-loss
    NewTakeProfit = 110.00m,   // Update take-profit
    // Quantity remains unchanged
};

var result = await TradingService.ModifyPositionAsync(request);
```

#### Partial Position Closure

```csharp
var request = new PositionClosureRequest
{
    PositionId = positionId,
    QuantityToClose = 5,  // Close half of a 10-unit position
    CloseType = CloseType.Partial
};

var result = await TradingService.ClosePositionAsync(request);

// Position remains open with reduced quantity
```

#### Full Position Closure

```csharp
var result = await TradingService.ClosePositionAsync(positionId);

// Position marked as closed, RealizedPnL calculated
```

### Order Types and Parameters

#### Market Order
- Executes immediately at current market price
- No price specified
- Fastest execution
- May experience slippage

```csharp
OrderType = OrderType.Market,
LimitPrice = null
```

#### Limit Order
- Executes only at specified price or better
- Price protection
- May not fill immediately
- No slippage

```csharp
OrderType = OrderType.Limit,
LimitPrice = 100.50m  // Will only buy at $100.50 or lower
```

#### Stop-Loss Order
- Automatically closes position when price reaches threshold
- Protects against large losses
- Part of position management

```csharp
StopLossPrice = 95.00m  // Close if price falls to $95
```

#### Take-Profit Order
- Automatically closes position when profit target reached
- Locks in gains
- Part of position management

```csharp
TakeProfitPrice = 110.00m  // Close if price rises to $110
```

---

## Market Data Integration

### Alpaca Market Data

#### Stock Quotes
Retrieve real-time stock quotes with bid/ask/last prices.

```csharp
@inject IAlpacaApiClient AlpacaClient

var quote = await AlpacaClient.GetLatestStockQuoteAsync("AAPL");

Console.WriteLine($"Bid: {quote.BidPrice} Ask: {quote.AskPrice} Last: {quote.LastPrice}");
```

**API Endpoint**: `GET /v2/stocks/{symbol}/quotes/latest`

**Stored In**: `StockQuote` entity

#### Stock Historical Bars
Get OHLCV candle data for technical analysis.

```csharp
var bars = await AlpacaClient.GetStockBarsAsync(
    symbol: "TSLA",
    timeframe: Timeframe.OneHour,
    start: DateTime.UtcNow.AddDays(-7),
    end: DateTime.UtcNow
);

foreach (var bar in bars)
{
    Console.WriteLine($"{bar.Timestamp}: O={bar.Open} H={bar.High} L={bar.Low} C={bar.Close} V={bar.Volume}");
}
```

**Timeframes**: 1Min, 5Min, 15Min, 30Min, 1Hour, 1Day

**Stored In**: `StockBar` entity

#### Cryptocurrency Quotes & Bars
Same interface as stocks, but for crypto assets.

```csharp
var btcQuote = await AlpacaClient.GetLatestCryptoQuoteAsync("BTC/USD");
var ethBars = await AlpacaClient.GetCryptoBarsAsync("ETH/USD", Timeframe.FiveMin, start, end);
```

**Stored In**: `CryptoQuote` and `CryptoBar` entities

#### Options Contracts with Implied Volatility
Query options chains with Greeks and IV (primary focus).

```csharp
var options = await AlpacaClient.GetOptionContractsAsync(
    underlyingSymbol: "SPY",
    expirationDate: DateTime.Parse("2024-12-20"),
    strikePrice: 450.00m,
    contractType: OptionType.Call
);

foreach (var option in options)
{
    Console.WriteLine($"Strike: {option.StrikePrice}");
    Console.WriteLine($"IV: {option.ImpliedVolatility}%");  // Primary focus
    Console.WriteLine($"Delta: {option.Greeks.Delta}");
    Console.WriteLine($"Gamma: {option.Greeks.Gamma}");
    Console.WriteLine($"Theta: {option.Greeks.Theta}");
    Console.WriteLine($"Vega: {option.Greeks.Vega}");
}
```

**Stored In**: `OptionContract` entity with full Greeks

#### Rate Limiting
Alpaca enforces 200 requests per minute. The client includes:
- Automatic rate limiting
- Retry logic with exponential backoff
- Request queuing

**Configuration**:
```json
"Alpaca": {
  "MaxRequestsPerMinute": 200,
  "MaxRetryAttempts": 3,
  "InitialRetryDelayMs": 1000
}
```

### Polymarket Integration

#### WebSocket Connection
Real-time streaming of prediction market data.

```csharp
@inject IPolymarketWebSocketService PolymarketService

// Subscribe to events
PolymarketService.OnMarketUpdate += (sender, data) =>
{
    Console.WriteLine($"Market {data.MarketId}: Price={data.Price}, IV={data.ImpliedVolatility}");
};

// Connect
await PolymarketService.ConnectAsync();

// Subscribe to specific markets
await PolymarketService.SubscribeToMarketAsync("market-id-here");
```

**WebSocket URL**: `wss://ws-subscriptions-clob.polymarket.com/ws/market`

**Features**:
- Automatic reconnection
- Heartbeat monitoring (30-second intervals)
- Event-driven architecture
- Exponential backoff on failures

#### Market Data Messages
The WebSocket receives:
- **Price updates**: Current market prices
- **Order book snapshots**: Bids and asks
- **Trade executions**: Recent trades
- **Market metadata**: Descriptions, outcomes

**Stored In**: `PolymarketSnapshot` entity (includes JSON order book data)

#### Implied Volatility Calculation
Custom IV calculation for prediction markets.

```csharp
// Automatic IV calculation in WebSocket service
private decimal CalculateImpliedVolatility(decimal probability, int daysToExpiration)
{
    // Custom algorithm for prediction market IV
    // Based on probability distance from 50% and time decay
}
```

**Stored In**: `PolymarketSnapshot.ImpliedVolatility` property

#### Trading Client (CLOB)
Execute trades on Polymarket using Ethereum signing.

```csharp
@inject IPolymarketTradingClient TradingClient

var order = new PolymarketOrderRequest
{
    MarketId = "market-id",
    Side = OrderSide.Buy,
    Quantity = 100,
    Price = 0.65m,  // 65% probability
    OrderType = OrderType.Limit
};

var result = await TradingClient.PlaceOrderAsync(order, ethereumPrivateKey);

if (result.Success)
{
    Console.WriteLine($"Order placed: {result.OrderId}");
}
```

**Requirements**:
- Ethereum wallet with USDC
- Private key for signing
- Nethereum library for crypto operations

### Data Storage Strategy

All market data is persisted to the database for:

1. **Historical Analysis**: Backtest strategies on real market data
2. **Audit Compliance**: Prove trade execution conditions
3. **Performance Attribution**: Understand what drove P&L
4. **Strategy Optimization**: Tune parameters based on historical performance

**Snapshot Timing**:
- Stock/Crypto quotes: On-demand or periodic polling
- Polymarket: Real-time via WebSocket (continuous)
- Trade execution: Always snapshot market conditions at trade time

---

## Database Architecture

### Entity Relationship Overview

```
Strategy (1) ──→ (N) TradingSignal
    ↓                     ↓
    └────→ (N) Trade ←────┘
              ↓
         (1) Position (N)
              ↓
         (1) Asset (1)
              ↓
    ┌─────────┴─────────┐
StockQuote          CryptoQuote
StockBar            CryptoBar
OptionContract      PolymarketSnapshot
```

### Core Trading Entities

#### Asset
Represents tradable instruments (stocks, crypto, options, prediction markets).

```csharp
public class Asset : BaseEntity
{
    public string Symbol { get; set; }                    // "AAPL", "BTC/USD"
    public AssetClass AssetClass { get; set; }            // Stock, Crypto, Option, PredictionMarket
    public AssetStatus Status { get; set; }               // Active, Inactive, Halted
    public string? Name { get; set; }
    public bool IsTradable { get; set; }
    public bool IsFractionable { get; set; }

    // Relationships
    public ICollection<Trade> Trades { get; set; }
    public ICollection<Position> Positions { get; set; }
}
```

**Enums**:
- `AssetClass`: Stock, Crypto, Option, PredictionMarket
- `AssetStatus`: Active, Inactive, Halted, Delisted

#### Strategy
Defines algorithmic trading strategies.

```csharp
public class Strategy : BaseEntity
{
    public string Name { get; set; }
    public StrategyType Type { get; set; }                // MeanReversion, Momentum, etc.
    public StrategyStatus Status { get; set; }            // Active, Paused, Stopped
    public string? Description { get; set; }
    public string? Configuration { get; set; }            // JSON parameters

    // Risk management
    public decimal MaxPositionSize { get; set; }
    public decimal? StopLossPercentage { get; set; }
    public decimal? TakeProfitPercentage { get; set; }

    // Relationships
    public ICollection<TradingSignal> Signals { get; set; }
    public ICollection<Trade> Trades { get; set; }
}
```

**Enums**:
- `StrategyType`: MeanReversion, Momentum, Arbitrage, VolatilityBased, Custom
- `StrategyStatus`: Active, Paused, Stopped, Backtesting

#### TradingSignal
Signals generated by strategies indicating trading opportunities.

```csharp
public class TradingSignal : BaseEntity
{
    public int StrategyId { get; set; }
    public int AssetId { get; set; }
    public SignalType SignalType { get; set; }            // Buy, Sell, Hold
    public decimal Confidence { get; set; }               // 0.0 - 1.0
    public decimal? TargetPrice { get; set; }
    public decimal? StopLossPrice { get; set; }
    public string? Reason { get; set; }                   // Why signal generated
    public SignalStatus Status { get; set; }              // Pending, Executed, Expired, Cancelled

    // Relationships
    public Strategy Strategy { get; set; }
    public Asset Asset { get; set; }
}
```

**Enums**:
- `SignalType`: Buy, Sell, Hold
- `SignalStatus`: Pending, Executed, Expired, Cancelled, Ignored

#### Trade
Records individual trade executions.

```csharp
public class Trade : BaseEntity
{
    public int AssetId { get; set; }
    public int? StrategyId { get; set; }
    public int? SignalId { get; set; }
    public int? PositionId { get; set; }

    public TradeSide Side { get; set; }                   // Buy, Sell
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public OrderType OrderType { get; set; }              // Market, Limit
    public TradeStatus Status { get; set; }               // Pending, Executed, Cancelled, Failed

    public decimal? Commission { get; set; }
    public DateTime? ExecutedAt { get; set; }
    public string? MarketSnapshot { get; set; }           // JSON snapshot at execution

    // Relationships
    public Asset Asset { get; set; }
    public Strategy? Strategy { get; set; }
    public TradingSignal? Signal { get; set; }
    public Position? Position { get; set; }
}
```

**Enums**:
- `TradeSide`: Buy, Sell
- `OrderType`: Market, Limit, StopLoss, TakeProfit
- `TradeStatus`: Pending, Executed, PartiallyFilled, Cancelled, Failed, Expired

#### Position
Tracks open and closed trading positions.

```csharp
public class Position : BaseEntity
{
    public int AssetId { get; set; }
    public int? StrategyId { get; set; }

    public PositionSide Side { get; set; }                // Long, Short
    public decimal Quantity { get; set; }
    public decimal AverageEntryPrice { get; set; }
    public DateTime EntryDate { get; set; }

    public decimal? CurrentPrice { get; set; }
    public decimal? UnrealizedPnL { get; set; }
    public decimal? RealizedPnL { get; set; }

    public decimal? StopLossPrice { get; set; }
    public decimal? TakeProfitPrice { get; set; }

    public bool IsClosed { get; set; }
    public DateTime? ExitDate { get; set; }
    public decimal? ExitPrice { get; set; }

    // Relationships
    public Asset Asset { get; set; }
    public Strategy? Strategy { get; set; }
    public ICollection<Trade> Trades { get; set; }
}
```

**Enums**:
- `PositionSide`: Long, Short

**Unrealized P&L Calculation**:
```csharp
UnrealizedPnL = (CurrentPrice - AverageEntryPrice) × Quantity × (Side == Long ? 1 : -1)
```

### Market Data Entities

#### StockQuote & CryptoQuote
Real-time price quotes.

```csharp
public class StockQuote : BaseEntity
{
    public int AssetId { get; set; }
    public DateTime Timestamp { get; set; }
    public decimal BidPrice { get; set; }
    public decimal AskPrice { get; set; }
    public decimal LastPrice { get; set; }
    public long BidSize { get; set; }
    public long AskSize { get; set; }

    public Asset Asset { get; set; }
}

// CryptoQuote has identical structure
```

#### StockBar & CryptoBar
OHLCV candle data.

```csharp
public class StockBar : BaseEntity
{
    public int AssetId { get; set; }
    public DateTime Timestamp { get; set; }
    public Timeframe Timeframe { get; set; }              // 1Min, 5Min, 1Hour, etc.
    public decimal Open { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal Close { get; set; }
    public long Volume { get; set; }
    public decimal? VWAP { get; set; }                    // Volume-weighted average price

    public Asset Asset { get; set; }
}
```

**Enums**:
- `Timeframe`: OneMin, FiveMin, FifteenMin, ThirtyMin, OneHour, OneDay, OneWeek, OneMonth

#### OptionContract
Options with Greeks and IV.

```csharp
public class OptionContract : BaseEntity
{
    public int UnderlyingAssetId { get; set; }
    public string ContractSymbol { get; set; }
    public OptionType Type { get; set; }                  // Call, Put
    public decimal StrikePrice { get; set; }
    public DateTime ExpirationDate { get; set; }

    // Greeks (primary focus on IV)
    public decimal? ImpliedVolatility { get; set; }       // PRIMARY FOCUS
    public decimal? Delta { get; set; }
    public decimal? Gamma { get; set; }
    public decimal? Theta { get; set; }
    public decimal? Vega { get; set; }
    public decimal? Rho { get; set; }

    public Asset UnderlyingAsset { get; set; }
}
```

**Enums**:
- `OptionType`: Call, Put

#### PolymarketSnapshot
Prediction market data with order book.

```csharp
public class PolymarketSnapshot : BaseEntity
{
    public string MarketId { get; set; }
    public DateTime Timestamp { get; set; }
    public decimal? Price { get; set; }
    public decimal? ImpliedVolatility { get; set; }       // Calculated IV
    public string? OrderBookData { get; set; }            // JSON: bids/asks
    public string? Metadata { get; set; }                 // JSON: additional info
}
```

### Logging Entities

#### ErrorLog
Exception and error tracking.

```csharp
public class ErrorLog : BaseEntity
{
    public string Message { get; set; }
    public string? StackTrace { get; set; }
    public LogSeverity Severity { get; set; }
    public string? Source { get; set; }
    public bool IsAcknowledged { get; set; }
}
```

**Enums**:
- `LogSeverity`: Critical, Error, Warning, Info, Debug

#### PerformanceMetric
Track operation performance.

```csharp
public class PerformanceMetric : BaseEntity
{
    public string OperationName { get; set; }
    public long ExecutionTimeMs { get; set; }
    public DateTime Timestamp { get; set; }
    public string? Metadata { get; set; }                 // JSON
}
```

### Soft Delete Pattern

All entities inherit from `BaseEntity`:

```csharp
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }                   // Soft delete flag
}
```

**Global Query Filter** in `TradingBotDbContext`:
```csharp
modelBuilder.Entity<Asset>().HasQueryFilter(e => !e.IsDeleted);
modelBuilder.Entity<Trade>().HasQueryFilter(e => !e.IsDeleted);
// ... applied to all entities
```

**Result**: Deleted entities are automatically excluded from all queries unless explicitly included.

### Database Indexes

Key indexes for performance:

```csharp
// Asset lookups
builder.HasIndex(a => a.Symbol);

// Trade queries
builder.HasIndex(t => t.ExecutedAt);
builder.HasIndex(t => new { t.AssetId, t.ExecutedAt });

// Position queries
builder.HasIndex(p => new { p.IsClosed, p.AssetId });

// Market data queries
builder.HasIndex(q => new { q.AssetId, q.Timestamp });
builder.HasIndex(b => new { b.AssetId, b.Timeframe, b.Timestamp });
```

For complete database documentation, see:
- [DATABASE_ARCHITECTURE.md](DATABASE_ARCHITECTURE.md) - Full schema and design patterns
- [DATABASE_SETUP.md](DATABASE_SETUP.md) - Setup and migration instructions

---

## Configuration

### appsettings.json Structure

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  },
  "AllowedHosts": "*",

  "ConnectionStrings": {
    "TradingBotDatabase": "Server=localhost;Database=TradingBot;Trusted_Connection=true;TrustServerCertificate=true;",
    "PostgreSQL": "Host=localhost;Database=tradingbot;Username=postgres;Password=yourpassword"
  },

  "Alpaca": {
    "ApiKeyId": "YOUR_ALPACA_API_KEY",
    "ApiSecretKey": "YOUR_ALPACA_SECRET_KEY",
    "BaseUrl": "https://paper-api.alpaca.markets",
    "DataBaseUrl": "https://data.alpaca.markets",
    "TimeoutSeconds": 30,
    "MaxRetryAttempts": 3,
    "InitialRetryDelayMs": 1000,
    "MaxRequestsPerMinute": 200
  },

  "Polymarket": {
    "WebSocketUrl": "wss://ws-subscriptions-clob.polymarket.com/ws/market",
    "RestApiUrl": "https://clob.polymarket.com",
    "ReconnectDelayMs": 5000,
    "HeartbeatIntervalSeconds": 30
  }
}
```

### Environment-Specific Configuration

**Development** (`appsettings.Development.json`):
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  },
  "Alpaca": {
    "BaseUrl": "https://paper-api.alpaca.markets"  // Paper trading
  }
}
```

**Production** (`appsettings.Production.json`):
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  },
  "Alpaca": {
    "BaseUrl": "https://api.alpaca.markets"  // Live trading (BE CAREFUL!)
  }
}
```

### Database Configuration

#### SQL Server (Default)
```json
"ConnectionStrings": {
  "TradingBotDatabase": "Server=localhost;Database=TradingBot;Trusted_Connection=true;TrustServerCertificate=true;"
}
```

#### PostgreSQL (Alternative)
```json
"ConnectionStrings": {
  "TradingBotDatabase": "Host=localhost;Database=tradingbot;Username=postgres;Password=yourpassword;Include Error Detail=true"
}
```

**Switching to PostgreSQL**:

1. Install Npgsql package (already in .csproj)
2. Update connection string in appsettings.json
3. Modify `Program.cs`:
```csharp
builder.Services.AddDbContext<TradingBotDbContext>(options =>
    options.UseNpgsql(connectionString));  // Instead of UseSqlServer
```
4. Run migrations: `dotnet ef database update`

### API Key Management

**Security Best Practices**:

1. **Never commit API keys to git**
   - Add `appsettings.*.json` to `.gitignore` (except template files)

2. **Use environment variables**
   ```bash
   export ALPACA_API_KEY="your-key"
   export ALPACA_SECRET_KEY="your-secret"
   ```

   Access in code:
   ```csharp
   var apiKey = Environment.GetEnvironmentVariable("ALPACA_API_KEY");
   ```

3. **Use User Secrets (Development)**
   ```bash
   dotnet user-secrets init
   dotnet user-secrets set "Alpaca:ApiKeyId" "your-key"
   dotnet user-secrets set "Alpaca:ApiSecretKey" "your-secret"
   ```

4. **Use Azure Key Vault (Production)**
   ```csharp
   builder.Configuration.AddAzureKeyVault(/* ... */);
   ```

### Tailwind CSS Configuration

**tailwind.config.js**:
```javascript
module.exports = {
  content: [
    './Pages/**/*.{html,razor}',
    './Components/**/*.{html,razor}',
    './Shared/**/*.{html,razor}'
  ],
  theme: {
    extend: {}
  },
  plugins: []
}
```

**package.json** scripts:
```json
{
  "scripts": {
    "build:css": "npx tailwindcss -i ./Styles/app.css -o ./wwwroot/css/app.css --minify",
    "watch:css": "npx tailwindcss -i ./Styles/app.css -o ./wwwroot/css/app.css --watch"
  }
}
```

**Development workflow**:
- Run `npm run watch:css` while developing (auto-rebuilds CSS)
- Run `npm run build:css` before deployment (minified output)

---

## API Integration

### Alpaca API Client

#### Initialization

The `AlpacaApiClient` is automatically registered in DI via `Program.cs`:

```csharp
builder.Services.AddSingleton<IAlpacaApiClient, AlpacaApiClient>();
```

**Configuration** is loaded from `appsettings.json`:
```csharp
var alpacaConfig = builder.Configuration.GetSection("Alpaca");
```

#### Available Methods

##### Health Check
```csharp
bool isHealthy = await alpacaClient.HealthCheckAsync();
```

##### Stock Operations

**Get Latest Quote**:
```csharp
var quote = await alpacaClient.GetLatestStockQuoteAsync("AAPL");
Console.WriteLine($"AAPL: ${quote.LastPrice}");
```

**Get Historical Bars**:
```csharp
var bars = await alpacaClient.GetStockBarsAsync(
    symbol: "TSLA",
    timeframe: Timeframe.OneHour,
    start: DateTime.UtcNow.AddDays(-7),
    end: DateTime.UtcNow,
    limit: 1000
);
```

**Create Stock Order**:
```csharp
var order = new OrderRequest
{
    Symbol = "AAPL",
    Quantity = 10,
    Side = OrderSide.Buy,
    Type = OrderType.Market,
    TimeInForce = TimeInForce.Day
};

var response = await alpacaClient.CreateOrderAsync(order);
```

##### Cryptocurrency Operations

**Get Crypto Quote**:
```csharp
var btcQuote = await alpacaClient.GetLatestCryptoQuoteAsync("BTC/USD");
```

**Get Crypto Bars**:
```csharp
var ethBars = await alpacaClient.GetCryptoBarsAsync(
    symbol: "ETH/USD",
    timeframe: Timeframe.FiveMin,
    start: startDate,
    end: endDate
);
```

##### Options Operations (IV Focus)

**Get Option Contracts**:
```csharp
var contracts = await alpacaClient.GetOptionContractsAsync(
    underlyingSymbol: "SPY",
    expirationDate: DateTime.Parse("2024-12-20"),
    strikePrice: 450.00m,
    contractType: OptionType.Call
);

foreach (var contract in contracts)
{
    Console.WriteLine($"IV: {contract.ImpliedVolatility}%");  // PRIMARY FOCUS
    Console.WriteLine($"Delta: {contract.Greeks.Delta}");
}
```

**Get Option Quote**:
```csharp
var optionQuote = await alpacaClient.GetOptionQuoteAsync("SPY241220C00450000");
```

#### Error Handling & Retry Logic

The client includes automatic retry with exponential backoff:

```csharp
private async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation)
{
    int attempt = 0;
    while (attempt < _maxRetryAttempts)
    {
        try
        {
            return await operation();
        }
        catch (HttpRequestException ex)
        {
            attempt++;
            if (attempt >= _maxRetryAttempts) throw;

            var delay = _initialRetryDelay * Math.Pow(2, attempt - 1);
            await Task.Delay(TimeSpan.FromMilliseconds(delay));
        }
    }
}
```

#### Rate Limiting

Automatic rate limiting to respect Alpaca's 200 requests/minute limit:

```csharp
private SemaphoreSlim _rateLimiter = new SemaphoreSlim(200, 200);

private async Task<T> SendRequestAsync<T>(string endpoint)
{
    await _rateLimiter.WaitAsync();
    try
    {
        // Execute request
    }
    finally
    {
        // Release after 60 seconds
        _ = Task.Delay(60000).ContinueWith(_ => _rateLimiter.Release());
    }
}
```

#### Full Documentation

See [Services/Alpaca/README.md](Services/Alpaca/README.md) for complete API documentation with examples.

### Polymarket Integration

#### WebSocket Service

**Initialization**:
```csharp
builder.Services.AddSingleton<IPolymarketWebSocketService, PolymarketWebSocketService>();
```

**Connecting**:
```csharp
@inject IPolymarketWebSocketService polymarketService

await polymarketService.ConnectAsync();
```

**Subscribing to Markets**:
```csharp
await polymarketService.SubscribeToMarketAsync("0x1234...");  // Market ID
```

**Handling Updates**:
```csharp
polymarketService.OnMarketUpdate += async (sender, update) =>
{
    Console.WriteLine($"Market: {update.MarketId}");
    Console.WriteLine($"Price: {update.Price}");
    Console.WriteLine($"IV: {update.ImpliedVolatility}");

    // Store snapshot
    await marketDataService.StorePolymarketSnapshotAsync(update);
};

polymarketService.OnError += (sender, error) =>
{
    Console.WriteLine($"WebSocket error: {error.Message}");
};
```

**Disconnecting**:
```csharp
await polymarketService.DisconnectAsync();
```

#### Trading Client (CLOB)

**Initialization**:
```csharp
builder.Services.AddSingleton<IPolymarketTradingClient, PolymarketTradingClient>();
```

**Placing Orders**:
```csharp
@inject IPolymarketTradingClient tradingClient

var orderRequest = new PolymarketOrderRequest
{
    MarketId = "0x1234...",
    TokenId = "outcome-token-id",
    Side = OrderSide.Buy,
    Price = 0.65m,          // 65 cents = 65% probability
    Quantity = 100,
    OrderType = OrderType.Limit
};

var result = await tradingClient.PlaceOrderAsync(
    orderRequest,
    ethereumPrivateKey: "0xYourPrivateKey"  // For signing
);

if (result.Success)
{
    Console.WriteLine($"Order ID: {result.OrderId}");
}
```

**Canceling Orders**:
```csharp
await tradingClient.CancelOrderAsync(orderId, privateKey);
```

**Checking Order Status**:
```csharp
var order = await tradingClient.GetOrderAsync(orderId);
Console.WriteLine($"Status: {order.Status}");  // Open, Filled, Cancelled
```

#### Ethereum Signing (Nethereum)

Polymarket uses Ethereum-based signatures for order authentication:

```csharp
using Nethereum.Signer;

var signer = new EthereumMessageSigner();
var signature = signer.EncodeUTF8AndSign(message, new EthECKey(privateKey));
```

**Required**:
- Ethereum wallet address
- Private key (keep secure!)
- USDC balance for trading

#### Full Documentation

See [Services/Polymarket/README.md](Services/Polymarket/README.md) for complete integration guide.

---

## Development Guide

### Project Structure for Developers

```
medalion/
├── Program.cs                       # DI & app configuration - START HERE
├── Data/
│   ├── TradingBotDbContext.cs      # EF Core context
│   ├── Domain/                      # Entity models
│   ├── Configurations/              # EF configurations
│   ├── Repositories/                # Data access layer
│   └── Services/                    # Business logic
├── Services/                        # External API integrations
│   ├── Alpaca/
│   ├── Polymarket/
│   └── DashboardStateService.cs
├── Pages/                           # Blazor pages
├── Components/Dashboard/            # Reusable UI widgets
└── ViewModels/                      # UI data models
```

### Adding a New Feature: Step-by-Step

#### Example: Add Support for Futures Trading

**Step 1: Create Entity** (`Data/Domain/FuturesContract.cs`)
```csharp
public class FuturesContract : BaseEntity
{
    public string Symbol { get; set; }
    public DateTime ExpirationDate { get; set; }
    public decimal ContractSize { get; set; }
    // ... additional properties
}
```

**Step 2: Add DbSet** (`Data/TradingBotDbContext.cs`)
```csharp
public DbSet<FuturesContract> FuturesContracts { get; set; }
```

**Step 3: Create Configuration** (`Data/Configurations/FuturesConfiguration.cs`)
```csharp
public class FuturesContractConfiguration : IEntityTypeConfiguration<FuturesContract>
{
    public void Configure(EntityTypeBuilder<FuturesContract> builder)
    {
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Symbol).IsRequired().HasMaxLength(50);
        builder.HasQueryFilter(f => !f.IsDeleted);  // Soft delete
    }
}
```

**Step 4: Apply Configuration** (`TradingBotDbContext.OnModelCreating`)
```csharp
modelBuilder.ApplyConfiguration(new FuturesContractConfiguration());
```

**Step 5: Create Migration**
```bash
dotnet ef migrations add AddFuturesContracts
dotnet ef database update
```

**Step 6: Create Repository Interface** (`Data/Repositories/IFuturesRepository.cs`)
```csharp
public interface IFuturesRepository : IRepository<FuturesContract>
{
    Task<List<FuturesContract>> GetActiveFuturesAsync();
}
```

**Step 7: Implement Repository** (`Data/Repositories/FuturesRepository.cs`)
```csharp
public class FuturesRepository : Repository<FuturesContract>, IFuturesRepository
{
    public FuturesRepository(TradingBotDbContext context) : base(context) { }

    public async Task<List<FuturesContract>> GetActiveFuturesAsync()
    {
        return await _dbSet
            .Where(f => f.ExpirationDate > DateTime.UtcNow)
            .ToListAsync();
    }
}
```

**Step 8: Register in DI** (`Program.cs`)
```csharp
builder.Services.AddScoped<IFuturesRepository, FuturesRepository>();
```

**Step 9: Add UI Component** (`Components/Dashboard/FuturesWidget.razor`)
```razor
@inject IFuturesRepository FuturesRepo

<div class="bg-white rounded-lg shadow p-6">
    <h3 class="text-lg font-semibold mb-4">Active Futures</h3>
    @foreach (var futures in activeFutures)
    {
        <div>@futures.Symbol - Expires @futures.ExpirationDate.ToShortDateString()</div>
    }
</div>

@code {
    private List<FuturesContract> activeFutures = new();

    protected override async Task OnInitializedAsync()
    {
        activeFutures = await FuturesRepo.GetActiveFuturesAsync();
    }
}
```

**Step 10: Add to Dashboard** (`Pages/Dashboard.razor`)
```razor
<FuturesWidget />
```

### Running Migrations

**Create Migration**:
```bash
dotnet ef migrations add MigrationName
```

**Update Database**:
```bash
dotnet ef database update
```

**Rollback Migration**:
```bash
dotnet ef database update PreviousMigrationName
```

**Remove Last Migration** (if not applied):
```bash
dotnet ef migrations remove
```

**Generate SQL Script**:
```bash
dotnet ef migrations script
```

### Development Workflow

1. **Start CSS Watch Mode**:
   ```bash
   npm run watch:css
   ```
   Automatically rebuilds Tailwind CSS on file changes.

2. **Run Application**:
   ```bash
   dotnet watch run
   ```
   Auto-restarts on C# file changes.

3. **Access Dashboard**:
   ```
   https://localhost:5001/dashboard
   ```

4. **View Logs**:
   - Console output (stdout)
   - Application logs in database (`ApplicationLogs` table)
   - Error logs in dashboard (`ErrorsWidget`)

### Testing

#### Unit Testing Example

```csharp
using Xunit;

public class TradingServiceTests
{
    [Fact]
    public async Task ExecuteTrade_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var tradingService = new TradingService(dbContext, /* ... */);

        var request = new TradeExecutionRequest
        {
            AssetId = 1,
            Side = TradeSide.Buy,
            Quantity = 10,
            OrderType = OrderType.Market
        };

        // Act
        var result = await tradingService.ExecuteTradeAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Trade);
    }
}
```

### Debugging Tips

1. **Enable EF Core SQL Logging**:
   ```json
   "Logging": {
     "LogLevel": {
       "Microsoft.EntityFrameworkCore.Database.Command": "Information"
     }
   }
   ```

2. **Inspect WebSocket Messages**:
   Add logging in `PolymarketWebSocketService`:
   ```csharp
   _logger.LogInformation("WebSocket message: {Message}", message);
   ```

3. **Check API Responses**:
   Add breakpoints in `AlpacaApiClient`:
   ```csharp
   var responseContent = await response.Content.ReadAsStringAsync();
   Console.WriteLine(responseContent);  // Inspect raw JSON
   ```

4. **Database Inspection**:
   - Use SQL Server Management Studio (SSMS)
   - Use pgAdmin (PostgreSQL)
   - Use `dotnet ef dbcontext info` for connection info

### Code Style Guidelines

1. **Use nullable reference types** (enabled in .csproj)
   ```csharp
   public string? NullableProperty { get; set; }  // Can be null
   public string RequiredProperty { get; set; }   // Cannot be null
   ```

2. **Follow async/await patterns**
   ```csharp
   public async Task<Result> DoSomethingAsync()  // Always suffix with Async
   {
       await SomeOperationAsync();
   }
   ```

3. **Use dependency injection**
   ```csharp
   public class MyService
   {
       private readonly IRepository _repository;

       public MyService(IRepository repository)  // Constructor injection
       {
           _repository = repository;
       }
   }
   ```

4. **Apply soft delete** to all entities
   ```csharp
   public class MyEntity : BaseEntity  // Inherits IsDeleted
   {
       // ...
   }
   ```

---

## Troubleshooting

### Common Issues

#### 1. Database Connection Errors

**Problem**: `Cannot open database` or `Login failed`

**Solutions**:
- Verify SQL Server is running: `services.msc` → SQL Server
- Check connection string in `appsettings.json`
- Test connection: `sqlcmd -S localhost -E` (Windows Authentication)
- For PostgreSQL: `psql -h localhost -U postgres`

**Connection String Examples**:
```json
// Windows Authentication (SQL Server)
"Server=localhost;Database=TradingBot;Trusted_Connection=true;TrustServerCertificate=true;"

// SQL Authentication
"Server=localhost;Database=TradingBot;User Id=sa;Password=YourPassword;TrustServerCertificate=true;"

// PostgreSQL
"Host=localhost;Database=tradingbot;Username=postgres;Password=yourpassword"
```

#### 2. Migration Errors

**Problem**: `The migration '...' has already been applied to the database`

**Solution**:
```bash
dotnet ef database update 0        # Rollback all
dotnet ef migrations remove         # Remove migrations
dotnet ef migrations add Initial    # Recreate
dotnet ef database update           # Apply
```

**Problem**: `Cannot resolve service IRepository`

**Solution**: Ensure repository is registered in `Program.cs`:
```csharp
builder.Services.AddScoped<IRepository, Repository>();
```

#### 3. Alpaca API Errors

**Problem**: `401 Unauthorized`

**Solution**:
- Verify API keys in `appsettings.json`
- Check Alpaca dashboard for key status
- Ensure using paper trading URL for paper keys: `https://paper-api.alpaca.markets`

**Problem**: `429 Too Many Requests`

**Solution**:
- Rate limiter is working correctly
- Wait 60 seconds and retry
- Reduce request frequency
- Check `MaxRequestsPerMinute` in config (default: 200)

**Problem**: `No data returned for symbol`

**Solution**:
- Verify symbol is correct (e.g., "AAPL", not "Apple")
- Check market hours (9:30 AM - 4:00 PM ET for stocks)
- For crypto, use "BTC/USD" format
- Some symbols may not have data in paper trading

#### 4. Polymarket WebSocket Issues

**Problem**: `WebSocket connection failed`

**Solutions**:
- Check internet connection
- Verify WebSocket URL: `wss://ws-subscriptions-clob.polymarket.com/ws/market`
- Check firewall settings (allow WebSocket connections)
- Inspect console logs for error details

**Problem**: `No market updates received`

**Solution**:
- Ensure you've called `SubscribeToMarketAsync(marketId)`
- Verify market ID is correct
- Check if market is still active
- Inspect WebSocket messages in logs

#### 5. Dashboard Not Loading

**Problem**: Blank dashboard or components not rendering

**Solutions**:
- Check browser console for JavaScript errors
- Verify Tailwind CSS is compiled: `npm run build:css`
- Check if `DashboardStateService` is registered in DI
- Inspect `Dashboard.razor` for exceptions in `OnInitializedAsync`

**Problem**: `No services available` or `Cannot resolve service`

**Solution**: Check `Program.cs` for missing service registrations:
```csharp
builder.Services.AddScoped<DashboardStateService>();
builder.Services.AddSingleton<IAlpacaApiClient, AlpacaApiClient>();
builder.Services.AddSingleton<IPolymarketWebSocketService, PolymarketWebSocketService>();
```

#### 6. Tailwind CSS Not Working

**Problem**: Styles not applied in UI

**Solutions**:
1. Build CSS:
   ```bash
   npm run build:css
   ```

2. Verify output file exists:
   ```bash
   ls wwwroot/css/app.css
   ```

3. Check `_Host.cshtml` includes CSS:
   ```html
   <link href="css/app.css" rel="stylesheet" />
   ```

4. Clear browser cache (Ctrl+Shift+R)

**Problem**: `npx: command not found`

**Solution**: Install Node.js and npm:
```bash
# Windows: Download from nodejs.org
# macOS: brew install node
# Linux: sudo apt install nodejs npm
```

#### 7. Position Unrealized P&L Shows Null

**Problem**: `UnrealizedPnL` is null in dashboard

**Cause**: `CurrentPrice` not updated

**Solution**: Ensure price updates:
```csharp
position.CurrentPrice = await GetCurrentMarketPrice(position.AssetId);
position.UnrealizedPnL = (position.CurrentPrice.Value - position.AverageEntryPrice)
                         × position.Quantity
                         × (position.Side == PositionSide.Long ? 1 : -1);
```

#### 8. Trades Not Executing

**Problem**: Trade status remains `Pending`

**Debugging Steps**:
1. Check `ErrorLogs` table for exceptions
2. Inspect Alpaca/Polymarket API responses
3. Verify asset is tradable: `Asset.IsTradable == true`
4. Check strategy status: `Strategy.Status == StrategyStatus.Active`
5. Ensure sufficient funds in account

**Common Causes**:
- Invalid symbol
- Market closed (for stocks)
- Insufficient buying power
- Order rejected by exchange

#### 9. Performance Issues

**Problem**: Dashboard slow to load or high database query time

**Solutions**:
1. Add indexes to frequently queried columns:
   ```csharp
   builder.HasIndex(t => new { t.AssetId, t.ExecutedAt });
   ```

2. Use pagination for large datasets:
   ```csharp
   var trades = await _tradeRepository.GetAllAsync()
       .OrderByDescending(t => t.ExecutedAt)
       .Take(100)
       .ToListAsync();
   ```

3. Enable query result caching
4. Use `AsNoTracking()` for read-only queries:
   ```csharp
   var positions = await _dbContext.Positions
       .AsNoTracking()
       .ToListAsync();
   ```

5. Monitor `PerformanceMetrics` table for slow operations

### Getting Help

1. **Check existing documentation**:
   - [DATABASE_ARCHITECTURE.md](DATABASE_ARCHITECTURE.md)
   - [DATABASE_SETUP.md](DATABASE_SETUP.md)
   - [SETUP_GUIDE.md](SETUP_GUIDE.md)
   - [Services/Alpaca/README.md](Services/Alpaca/README.md)
   - [Services/Polymarket/README.md](Services/Polymarket/README.md)

2. **Enable verbose logging**:
   ```json
   "Logging": {
     "LogLevel": {
       "Default": "Debug"
     }
   }
   ```

3. **Inspect database directly**:
   - Check `ErrorLogs` table for recent errors
   - Review `ApplicationLogs` for operation history
   - Query `PerformanceMetrics` for bottlenecks

4. **API-specific issues**:
   - Alpaca: Check [status.alpaca.markets](https://status.alpaca.markets)
   - Polymarket: Check platform status and Discord

5. **Create detailed issue reports**:
   - Include error messages and stack traces
   - Provide steps to reproduce
   - Share relevant configuration (redact API keys!)
   - Mention environment (OS, .NET version, database)

---

## Additional Resources

### Official Documentation
- **.NET**: https://docs.microsoft.com/en-us/dotnet/
- **Blazor**: https://docs.microsoft.com/en-us/aspnet/core/blazor/
- **Entity Framework Core**: https://docs.microsoft.com/en-us/ef/core/
- **Tailwind CSS**: https://tailwindcss.com/docs

### API Documentation
- **Alpaca Markets**: https://alpaca.markets/docs/
- **Polymarket CLOB**: https://docs.polymarket.com/

### Related Projects
- **Nethereum**: https://nethereum.com/ (Ethereum .NET library)

### Trading Concepts
- **Implied Volatility**: https://www.investopedia.com/terms/i/iv.asp
- **Options Greeks**: https://www.investopedia.com/terms/g/greeks.asp
- **Mean Reversion**: https://www.investopedia.com/terms/m/meanreversion.asp
- **Prediction Markets**: https://en.wikipedia.org/wiki/Prediction_market

---

## License

(Add your license information here)

## Contributing

(Add contribution guidelines here)

## Changelog

### Version 1.0.0 (November 2025)
- Initial release
- Multi-market trading support (stocks, crypto, options, prediction markets)
- Real-time dashboard with Tailwind CSS
- Alpaca API integration with IV focus
- Polymarket WebSocket integration
- Complete database architecture with 20+ entities
- Comprehensive logging and error tracking
- Position management with P&L calculation

---

**Last Updated**: November 22, 2025

For questions or support, please refer to the project repository or contact the development team.
