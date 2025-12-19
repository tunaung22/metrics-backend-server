namespace Metrics.Web.Models;

public class KpiSubmissionViewModel
{
    // public long KpiSubmissionPeriodId { get; set; }
    public DateOnly SubmissionDate { get; set; }
    // public long DepartmentId { get; set; }
    public string? DepartmentName { get; set; } = string.Empty;
    public decimal ScoreValue { get; set; }
    public string? PositiveAspects { get; set; } = string.Empty;
    public string? NegativeAspects { get; set; } = string.Empty;
    public string? Comments { get; set; } = string.Empty;
}
