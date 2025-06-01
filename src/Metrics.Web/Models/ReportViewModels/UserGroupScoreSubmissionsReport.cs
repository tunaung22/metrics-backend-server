namespace Metrics.Web.Models.ReportViewModels;

public class UserGroupScoreSubmissionsReport
{
    public string? PeriodName { get; set; }
    public string? DepartmentName { get; set; }
    public string? UserGroupName { get; set; }
    // public long TotalUser { get; set; }
    public long TotalSubmissions { get; set; }
    public decimal TotalScore { get; set; }
    public decimal KpiScore { get; set; }
}
