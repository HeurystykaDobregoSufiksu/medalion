using medalion.Data.Domain;

namespace medalion.ViewModels;

public class PositionViewModel
{
    public Guid Id { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public string Side { get; set; } = string.Empty; // "Long" or "Short"
    public decimal Size { get; set; }
    public decimal EntryPrice { get; set; }
    public decimal CurrentPrice { get; set; }
    public decimal PnLAbsolute { get; set; }
    public decimal PnLPercent { get; set; }
    public decimal? Leverage { get; set; }
    public bool IsProfit => PnLAbsolute >= 0;

    public static PositionViewModel FromPosition(Position position)
    {
        var unrealizedPnL = position.UnrealizedPnL ?? 0;
        var pnlPercent = position.AverageEntryPrice > 0
            ? (unrealizedPnL / (position.AverageEntryPrice * position.Quantity)) * 100
            : 0;

        return new PositionViewModel
        {
            Id = position.Id,
            Symbol = position.Asset?.Symbol ?? "UNKNOWN",
            Side = position.PositionSide.ToString(),
            Size = position.Quantity,
            EntryPrice = position.AverageEntryPrice,
            CurrentPrice = position.CurrentPrice ?? 0,
            PnLAbsolute = unrealizedPnL,
            PnLPercent = pnlPercent,
            Leverage = null // Can be added if supported
        };
    }
}

public class RecentActionViewModel
{
    public Guid Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public string Side { get; set; } = string.Empty; // "Buy" or "Sell"
    public decimal Size { get; set; }
    public decimal Price { get; set; }
    public string OrderId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string StatusColor => Status switch
    {
        "Filled" => "success",
        "PartiallyFilled" => "warning",
        "Cancelled" or "Rejected" or "Expired" => "danger",
        _ => "info"
    };

    public static RecentActionViewModel FromTrade(Trade trade)
    {
        return new RecentActionViewModel
        {
            Id = trade.Id,
            Timestamp = trade.CreatedAt,
            Symbol = trade.Asset?.Symbol ?? "UNKNOWN",
            Side = trade.TradeSide.ToString(),
            Size = trade.Quantity,
            Price = trade.Price,
            OrderId = trade.Id.ToString()[..8], // Short ID for display
            Status = trade.Status.ToString()
        };
    }
}

public class DailyStatsViewModel
{
    public int TotalTrades { get; set; }
    public decimal TotalProfitLoss { get; set; }
    public decimal WinRate { get; set; }
    public decimal AverageTradePnL { get; set; }
    public DateTime? LastTradeTimestamp { get; set; }
    public List<decimal> PnLSparklineData { get; set; } = new();
    public bool IsProfit => TotalProfitLoss >= 0;
}

public enum ServiceStatus
{
    Online,
    Offline,
    Lagging
}

public class ServiceHealthViewModel
{
    public string ServiceName { get; set; } = string.Empty;
    public ServiceStatus Status { get; set; }
    public DateTime? LastResponseTime { get; set; }
    public string StatusText => Status switch
    {
        ServiceStatus.Online => "Online",
        ServiceStatus.Offline => "Offline",
        ServiceStatus.Lagging => "Lagging",
        _ => "Unknown"
    };
    public string StatusColor => Status switch
    {
        ServiceStatus.Online => "green",
        ServiceStatus.Offline => "red",
        ServiceStatus.Lagging => "yellow",
        _ => "gray"
    };
}

public enum ErrorSeverity
{
    Info,
    Warning,
    Error,
    Critical
}

public class ErrorViewModel
{
    public Guid Id { get; set; }
    public DateTime Timestamp { get; set; }
    public ErrorSeverity Severity { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? RelatedEntity { get; set; }
    public bool IsAcknowledged { get; set; }
    public string SeverityColor => Severity switch
    {
        ErrorSeverity.Info => "blue",
        ErrorSeverity.Warning => "yellow",
        ErrorSeverity.Error => "red",
        ErrorSeverity.Critical => "purple",
        _ => "gray"
    };
    public string SeverityBadge => Severity switch
    {
        ErrorSeverity.Info => "badge-info",
        ErrorSeverity.Warning => "badge-warning",
        ErrorSeverity.Error => "badge-danger",
        ErrorSeverity.Critical => "badge-danger",
        _ => "badge-info"
    };

    public static ErrorViewModel FromErrorLog(ErrorLog errorLog)
    {
        var severity = errorLog.Severity?.ToLower() switch
        {
            "critical" => ErrorSeverity.Critical,
            "error" => ErrorSeverity.Error,
            "warning" => ErrorSeverity.Warning,
            _ => ErrorSeverity.Info
        };

        return new ErrorViewModel
        {
            Id = errorLog.Id,
            Timestamp = errorLog.CreatedAt,
            Severity = severity,
            Message = errorLog.Message ?? "Unknown error",
            RelatedEntity = errorLog.Source,
            IsAcknowledged = false // Could add this field to ErrorLog model
        };
    }
}
