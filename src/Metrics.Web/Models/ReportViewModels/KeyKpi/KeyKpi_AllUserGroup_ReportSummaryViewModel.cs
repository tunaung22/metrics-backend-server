namespace Metrics.Web.Models.ReportViewModels.KeyKpi;

public class KeyKpi_AllUserGroup_ReportSummaryViewModel
{
    public string? PeriodName { get; set; }
    public string? DepartmentName { get; set; }
    public List<KeyKpi_UserGroup_SubmissionInfo> UserGroupSubmissionInfo { get; set; } = [];
    public long TotalKeys { get; set; }
    public long TotalSubmissions { get; set; }
    public decimal TotalScore { get; set; }
    public decimal KpiScore { get; set; }
}