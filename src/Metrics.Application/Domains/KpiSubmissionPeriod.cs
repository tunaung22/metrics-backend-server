using Metrics.Application.Interfaces;

namespace Metrics.Application.Domains;

public partial class KpiSubmissionPeriod : IAuditColumn
{
    public long Id { get; set; }
    public string PeriodName { get; set; } = null!;
    public DateTimeOffset SubmissionStartDate { get; set; }
    public DateTimeOffset SubmissionEndDate { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ModifiedAt { get; set; }

    // Collection Navigational Properties
    public List<KpiSubmission> KpiSubmissions { get; set; } = [];
    public List<KeyKpiSubmission> KeyKpiSubmissions { get; set; } = [];
    public List<DepartmentKeyMetric> DepartmentKeyMetrics { get; set; } = [];
    public List<CaseFeedbackSubmission> CaseFeedbackSubmissions { get; set; } = [];
}
