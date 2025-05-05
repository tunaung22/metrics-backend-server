using Metrics.Application.Interfaces;

namespace Metrics.Application.Domains;

public class KpiSubmission : IAuditColumn
{
    public long Id { get; set; }
    public DateTimeOffset SubmittedAt { get; set; }
    public DateOnly SubmissionDate { get; set; } // Generated Field
    public decimal KpiScore { get; set; }
    public string? Comments { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ModifiedAt { get; set; } = DateTimeOffset.UtcNow;

    // Foreign Keys
    public long KpiPeriodId { get; set; }
    public long DepartmentId { get; set; }
    public long EmployeeId { get; set; }

    // Reference Navigational Properties
    public KpiPeriod KpiPeriod { get; set; } = null!;
    public Department Department { get; set; } = null!;
    public Employee Employee { get; set; } = null!;
}
