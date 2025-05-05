using Metrics.Application.Interfaces;

namespace Metrics.Application.Domains;

public partial class KpiPeriod : IAuditColumn
{
    public long Id { get; set; }
    public string PeriodCode { get; set; } = null!;
    public DateTimeOffset SubmissionStartDate { get; set; }
    public DateTimeOffset SubmissionEndDate { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ModifiedAt { get; set; } = DateTimeOffset.UtcNow;

    // Collection Navigational Properties
    public List<KpiSubmission> KpiSubmissions { get; set; } = [];

}
