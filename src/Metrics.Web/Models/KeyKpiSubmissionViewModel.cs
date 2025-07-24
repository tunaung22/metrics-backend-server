namespace Metrics.Web.Models;

public class KeyKpiSubmissionViewModel
{
    public long Id { get; set; }
    public DateTimeOffset SubmittedAt { get; set; }
    public long KpiPeriodId { get; set; }
    public KpiPeriodViewModel TargetKpiPeriod { get; set; } = null!;
    public long DepartmentId { get; set; }
    public DepartmentViewModel TargetDepartment { get; set; } = null!;
    public string SubmitterId { get; set; } = null!;
    public UserViewModel SubmittedBy { get; set; } = null!;
    public List<KeyKpiSubmissionItemViewModel> KeyKpiSubmissionItems { get; set; } = [];
}
