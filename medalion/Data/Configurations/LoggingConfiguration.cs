using Medalion.Data.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Medalion.Data.Configurations;

public class ApplicationLogConfiguration : IEntityTypeConfiguration<ApplicationLog>
{
    public void Configure(EntityTypeBuilder<ApplicationLog> builder)
    {
        builder.ToTable("ApplicationLogs");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.LogLevel)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(l => l.Category)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(l => l.Message)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(l => l.Exception)
            .HasColumnType("nvarchar(max)");

        builder.Property(l => l.StackTrace)
            .HasColumnType("nvarchar(max)");

        builder.Property(l => l.Source)
            .HasMaxLength(200);

        builder.Property(l => l.CorrelationId)
            .HasMaxLength(100);

        builder.Property(l => l.EntityType)
            .HasMaxLength(100);

        builder.Property(l => l.AdditionalData)
            .HasColumnType("nvarchar(max)");

        builder.HasIndex(l => l.LogLevel);
        builder.HasIndex(l => l.LogTimestamp);
        builder.HasIndex(l => l.Category);
        builder.HasIndex(l => l.CorrelationId);

        builder.HasQueryFilter(l => !l.IsDeleted);
    }
}

public class ErrorLogConfiguration : IEntityTypeConfiguration<ErrorLog>
{
    public void Configure(EntityTypeBuilder<ErrorLog> builder)
    {
        builder.ToTable("ErrorLogs");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Severity)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(e => e.Category)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.ErrorCode)
            .HasMaxLength(50);

        builder.Property(e => e.Message)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.ExceptionType)
            .HasMaxLength(500);

        builder.Property(e => e.ExceptionDetails)
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.StackTrace)
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.InnerException)
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.Source)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.CorrelationId)
            .HasMaxLength(100);

        builder.Property(e => e.EntityType)
            .HasMaxLength(100);

        builder.Property(e => e.ReportedBy)
            .HasMaxLength(200);

        builder.Property(e => e.ResolutionNotes)
            .HasMaxLength(2000);

        builder.Property(e => e.AdditionalData)
            .HasColumnType("nvarchar(max)");

        builder.HasIndex(e => e.Severity);
        builder.HasIndex(e => e.ErrorTimestamp);
        builder.HasIndex(e => e.Category);
        builder.HasIndex(e => e.CorrelationId);
        builder.HasIndex(e => e.WasHandled);
        builder.HasIndex(e => e.RequiresIntervention);

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}

public class PerformanceMetricConfiguration : IEntityTypeConfiguration<PerformanceMetric>
{
    public void Configure(EntityTypeBuilder<PerformanceMetric> builder)
    {
        builder.ToTable("PerformanceMetrics");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.MetricName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Category)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Value).HasPrecision(18, 8);

        builder.Property(p => p.Unit)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.Source)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Tags)
            .HasColumnType("nvarchar(max)");

        builder.Property(p => p.Metadata)
            .HasColumnType("nvarchar(max)");

        builder.HasIndex(p => new { p.MetricName, p.MetricTimestamp });
        builder.HasIndex(p => p.Category);
        builder.HasIndex(p => p.MetricTimestamp);

        builder.HasQueryFilter(p => !p.IsDeleted);
    }
}

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Action)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.EntityType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.PerformedBy)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.PreviousState)
            .HasColumnType("nvarchar(max)");

        builder.Property(a => a.NewState)
            .HasColumnType("nvarchar(max)");

        builder.Property(a => a.Changes)
            .HasColumnType("nvarchar(max)");

        builder.Property(a => a.Reason)
            .HasMaxLength(1000);

        builder.Property(a => a.IpAddress)
            .HasMaxLength(50);

        builder.Property(a => a.CorrelationId)
            .HasMaxLength(100);

        builder.Property(a => a.Metadata)
            .HasColumnType("nvarchar(max)");

        builder.HasIndex(a => new { a.EntityType, a.EntityId, a.ActionTimestamp });
        builder.HasIndex(a => a.Action);
        builder.HasIndex(a => a.ActionTimestamp);
        builder.HasIndex(a => a.PerformedBy);

        builder.HasQueryFilter(a => !a.IsDeleted);
    }
}
