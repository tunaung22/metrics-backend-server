namespace Metrics.Application.DTOs.KeyMetric;

public record KeyMetricDto
{
    public long Id { get; init; }
    public Guid MetricCode { get; init; }
    public string MetricTitle { get; init; } = null!;
    public string? Description { get; init; } = string.Empty;
    public bool IsDeleted { get; init; }
    // public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    // public DateTimeOffset ModifiedAt { get; init; }
}
