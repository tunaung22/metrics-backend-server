using Metrics.Application.Interfaces;

namespace Metrics.Application.Domains;

public class KeyMetric : IAuditColumn, ISoftDelete
{
    public long Id { get; set; }
    public Guid MetricCode { get; set; }
    public string MetricTitle { get; set; } = null!;
    public string? Description { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ModifiedAt { get; set; }

    // Foreign Keys

    // Reference Navigational Properties

    // Collection Navigational Properties
    public List<DepartmentKeyMetric> DepartmentKeyMetrics { get; set; } = [];
    public List<KeyKpiSubmission> KeyKpiSubmissions { get; set; } = [];
    public List<KeyKpiSubmissionItem> KeyKpiSubmissionItems { get; set; } = [];
}