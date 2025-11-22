namespace Medalion.Data.Domain;

/// <summary>
/// Application log entries
/// </summary>
public class ApplicationLog : BaseEntity
{
    /// <summary>
    /// Log level: Debug, Information, Warning, Error, Critical
    /// </summary>
    public LogLevel LogLevel { get; set; }

    /// <summary>
    /// Log category/source (e.g., "TradingEngine", "AlpacaService", "PolymarketService")
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Log message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Exception details (if applicable)
    /// </summary>
    public string? Exception { get; set; }

    /// <summary>
    /// Stack trace (if applicable)
    /// </summary>
    public string? StackTrace { get; set; }

    /// <summary>
    /// Log timestamp
    /// </summary>
    public DateTime LogTimestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// User or service that generated the log
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// Request correlation ID (for tracking related logs)
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Additional context data (JSON)
    /// </summary>
    public string? AdditionalData { get; set; }

    /// <summary>
    /// Related entity type (e.g., "Trade", "Position", "Signal")
    /// </summary>
    public string? EntityType { get; set; }

    /// <summary>
    /// Related entity ID
    /// </summary>
    public Guid? EntityId { get; set; }
}

/// <summary>
/// Error and exception tracking
/// </summary>
public class ErrorLog : BaseEntity
{
    /// <summary>
    /// Error severity: Low, Medium, High, Critical
    /// </summary>
    public ErrorSeverity Severity { get; set; }

    /// <summary>
    /// Error category (e.g., "API", "Database", "Trading", "DataProcessing")
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Error code (if applicable)
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Error message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Exception type
    /// </summary>
    public string? ExceptionType { get; set; }

    /// <summary>
    /// Full exception details
    /// </summary>
    public string? ExceptionDetails { get; set; }

    /// <summary>
    /// Stack trace
    /// </summary>
    public string? StackTrace { get; set; }

    /// <summary>
    /// Inner exception details (if applicable)
    /// </summary>
    public string? InnerException { get; set; }

    /// <summary>
    /// Error timestamp
    /// </summary>
    public DateTime ErrorTimestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Source component/service
    /// </summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// Request correlation ID
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Whether the error was handled
    /// </summary>
    public bool WasHandled { get; set; } = false;

    /// <summary>
    /// Whether the error requires manual intervention
    /// </summary>
    public bool RequiresIntervention { get; set; } = false;

    /// <summary>
    /// Related entity type
    /// </summary>
    public string? EntityType { get; set; }

    /// <summary>
    /// Related entity ID
    /// </summary>
    public Guid? EntityId { get; set; }

    /// <summary>
    /// User or system that reported the error
    /// </summary>
    public string? ReportedBy { get; set; }

    /// <summary>
    /// Resolution notes (if error was resolved)
    /// </summary>
    public string? ResolutionNotes { get; set; }

    /// <summary>
    /// Resolution timestamp
    /// </summary>
    public DateTime? ResolvedAt { get; set; }

    /// <summary>
    /// Additional context data (JSON)
    /// </summary>
    public string? AdditionalData { get; set; }
}

/// <summary>
/// Performance metrics and monitoring data
/// </summary>
public class PerformanceMetric : BaseEntity
{
    /// <summary>
    /// Metric name (e.g., "APILatency", "DatabaseQueryTime", "SignalGenerationTime")
    /// </summary>
    public string MetricName { get; set; } = string.Empty;

    /// <summary>
    /// Metric category
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Metric value
    /// </summary>
    public decimal Value { get; set; }

    /// <summary>
    /// Metric unit (e.g., "ms", "seconds", "count", "percentage")
    /// </summary>
    public string Unit { get; set; } = string.Empty;

    /// <summary>
    /// Metric timestamp
    /// </summary>
    public DateTime MetricTimestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Source component
    /// </summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// Additional tags (JSON)
    /// </summary>
    public string? Tags { get; set; }

    /// <summary>
    /// Additional metadata (JSON)
    /// </summary>
    public string? Metadata { get; set; }
}

/// <summary>
/// Audit trail for critical operations
/// </summary>
public class AuditLog : BaseEntity
{
    /// <summary>
    /// Action performed (e.g., "TradeExecuted", "PositionOpened", "StrategyActivated")
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Entity type affected
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// Entity ID affected
    /// </summary>
    public Guid EntityId { get; set; }

    /// <summary>
    /// User or service that performed the action
    /// </summary>
    public string PerformedBy { get; set; } = string.Empty;

    /// <summary>
    /// Action timestamp
    /// </summary>
    public DateTime ActionTimestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Previous state (JSON, if applicable)
    /// </summary>
    public string? PreviousState { get; set; }

    /// <summary>
    /// New state (JSON)
    /// </summary>
    public string? NewState { get; set; }

    /// <summary>
    /// Changes made (JSON)
    /// </summary>
    public string? Changes { get; set; }

    /// <summary>
    /// Reason for the action
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// IP address (if applicable)
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// Correlation ID
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Additional metadata (JSON)
    /// </summary>
    public string? Metadata { get; set; }
}

/// <summary>
/// Log level enumeration
/// </summary>
public enum LogLevel
{
    Trace = 0,
    Debug = 1,
    Information = 2,
    Warning = 3,
    Error = 4,
    Critical = 5
}

/// <summary>
/// Error severity enumeration
/// </summary>
public enum ErrorSeverity
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}
