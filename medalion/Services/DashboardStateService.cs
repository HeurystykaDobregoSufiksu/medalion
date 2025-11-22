using medalion.ViewModels;
using Medalion.Data;
using Medalion.Data.Domain;
using Medalion.Services.Alpaca.Interfaces;
using Medalion.Services.Polymarket.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace medalion.Services;

public interface IDashboardStateService
{
    Task<List<PositionViewModel>> GetOpenPositionsAsync();
    Task<List<RecentActionViewModel>> GetRecentActionsAsync(int count = 20);
    Task<DailyStatsViewModel> GetDailyStatsAsync();
    Task<List<ServiceHealthViewModel>> GetServiceHealthAsync();
    Task<List<ErrorViewModel>> GetRecentErrorsAsync(int count = 10);
    Task<int> GetUnacknowledgedErrorCountAsync();
    event Action? OnStateChanged;
    void NotifyStateChanged();
}

public class DashboardStateService : IDashboardStateService
{
    private readonly TradingBotDbContext _dbContext;
    private readonly IAlpacaApiClient? _alpacaClient;
    private readonly IPolymarketWebSocketService? _polymarketService;
    private readonly ILogger<DashboardStateService> _logger;

    public event Action? OnStateChanged;

    public DashboardStateService(
        TradingBotDbContext dbContext,
        ILogger<DashboardStateService> logger,
        IAlpacaApiClient? alpacaClient = null,
        IPolymarketWebSocketService? polymarketService = null)
    {
        _dbContext = dbContext;
        _logger = logger;
        _alpacaClient = alpacaClient;
        _polymarketService = polymarketService;
    }

    public void NotifyStateChanged() => OnStateChanged?.Invoke();

    public async Task<List<PositionViewModel>> GetOpenPositionsAsync()
    {
        try
        {
            var positions = await _dbContext.Positions
                .Include(p => p.Asset)
                .Where(p => p.Status == PositionStatus.Open && !p.IsDeleted)
                .OrderByDescending(p => p.OpenedAt)
                .Take(50)
                .ToListAsync();

            return positions.Select(PositionViewModel.FromPosition).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching open positions");
            return new List<PositionViewModel>();
        }
    }

    public async Task<List<RecentActionViewModel>> GetRecentActionsAsync(int count = 20)
    {
        try
        {
            var trades = await _dbContext.Trades
                .Include(t => t.Asset)
                .Where(t => !t.IsDeleted)
                .OrderByDescending(t => t.CreatedAt)
                .Take(count)
                .ToListAsync();

            return trades.Select(RecentActionViewModel.FromTrade).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching recent actions");
            return new List<RecentActionViewModel>();
        }
    }

    public async Task<DailyStatsViewModel> GetDailyStatsAsync()
    {
        try
        {
            var today = DateTime.UtcNow.Date;
            var trades = await _dbContext.Trades
                .Where(t => t.CreatedAt >= today && !t.IsDeleted)
                .ToListAsync();

            var totalTrades = trades.Count;
            var profitableTrades = trades.Count(t => (t.Price * t.Quantity) > 0);
            var winRate = totalTrades > 0 ? (decimal)profitableTrades / totalTrades * 100 : 0;

            // Calculate total P&L from closed positions today
            var closedPositions = await _dbContext.Positions
                .Where(p => p.ClosedAt >= today && p.Status == PositionStatus.Closed && !p.IsDeleted)
                .ToListAsync();

            var totalPnL = closedPositions.Sum(p => p.RealizedPnL);
            var avgPnL = closedPositions.Any() ? closedPositions.Average(p => p.RealizedPnL) : 0;

            // Get sparkline data (hourly P&L for the last 24 hours)
            var sparklineData = new List<decimal>();
            for (int i = 23; i >= 0; i--)
            {
                var hourStart = DateTime.UtcNow.AddHours(-i);
                var hourEnd = hourStart.AddHours(1);
                var hourPnL = closedPositions
                    .Where(p => p.ClosedAt >= hourStart && p.ClosedAt < hourEnd)
                    .Sum(p => p.RealizedPnL);
                sparklineData.Add(hourPnL??0);
            }

            return new DailyStatsViewModel
            {
                TotalTrades = totalTrades,
                TotalProfitLoss = totalPnL??0,
                WinRate = winRate,
                AverageTradePnL = avgPnL??0,
                LastTradeTimestamp = trades.OrderByDescending(t => t.CreatedAt).FirstOrDefault()?.CreatedAt,
                PnLSparklineData = sparklineData
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating daily stats");
            return new DailyStatsViewModel();
        }
    }

    public async Task<List<ServiceHealthViewModel>> GetServiceHealthAsync()
    {
        var healthStats = new List<ServiceHealthViewModel>();

        // Alpaca service health
        try
        {
            if (_alpacaClient != null)
            {
                var isHealthy = await _alpacaClient.HealthCheckAsync();
                healthStats.Add(new ServiceHealthViewModel
                {
                    ServiceName = "Alpaca Markets",
                    Status = isHealthy ? ServiceStatus.Online : ServiceStatus.Offline,
                    LastResponseTime = DateTime.UtcNow
                });
            }
            else
            {
                healthStats.Add(new ServiceHealthViewModel
                {
                    ServiceName = "Alpaca Markets",
                    Status = ServiceStatus.Offline,
                    LastResponseTime = null
                });
            }
        }
        catch
        {
            healthStats.Add(new ServiceHealthViewModel
            {
                ServiceName = "Alpaca Markets",
                Status = ServiceStatus.Offline,
                LastResponseTime = null
            });
        }

        // Polymarket service health
        try
        {
            if (_polymarketService != null)
            {
                var stats = await _polymarketService.GetStatisticsAsync();
                var isConnected = _polymarketService.IsConnected;
                var lastUpdate = stats.LastMessageTime;

                var status = ServiceStatus.Offline;
                if (isConnected)
                {
                    var timeSinceLastMessage = DateTime.UtcNow - (lastUpdate ?? DateTime.UtcNow);
                    status = timeSinceLastMessage.TotalSeconds < 30 ? ServiceStatus.Online : ServiceStatus.Lagging;
                }

                healthStats.Add(new ServiceHealthViewModel
                {
                    ServiceName = "Polymarket",
                    Status = status,
                    LastResponseTime = lastUpdate
                });
            }
            else
            {
                healthStats.Add(new ServiceHealthViewModel
                {
                    ServiceName = "Polymarket",
                    Status = ServiceStatus.Offline,
                    LastResponseTime = null
                });
            }
        }
        catch
        {
            healthStats.Add(new ServiceHealthViewModel
            {
                ServiceName = "Polymarket",
                Status = ServiceStatus.Offline,
                LastResponseTime = null
            });
        }

        return await Task.FromResult(healthStats);
    }

    public async Task<List<ErrorViewModel>> GetRecentErrorsAsync(int count = 10)
    {
        try
        {
            var errors = await _dbContext.ErrorLogs
                .Where(e => !e.IsDeleted)
                .OrderByDescending(e => e.CreatedAt)
                .Take(count)
                .ToListAsync();

            return errors.Select(ErrorViewModel.FromErrorLog).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching error logs");
            return new List<ErrorViewModel>();
        }
    }

    public async Task<int> GetUnacknowledgedErrorCountAsync()
    {
        try
        {
            // Get errors from the last 24 hours
            var yesterday = DateTime.UtcNow.AddDays(-1);
            return await _dbContext.ErrorLogs
                .Where(e => e.CreatedAt >= yesterday && !e.IsDeleted)
                .CountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error counting unacknowledged errors");
            return 0;
        }
    }
}
