using Metrics.Application.Interfaces;

namespace Metrics.Application.Domains;

public class KeyKpiMetricCategory : IAuditColumn
{
    public long Id { get; set; }
    public Guid CategoryCode { get; set; }

    public string CategoryName { get; set; } = null!;
    public string? Description { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ModifiedAt { get; set; }
}
