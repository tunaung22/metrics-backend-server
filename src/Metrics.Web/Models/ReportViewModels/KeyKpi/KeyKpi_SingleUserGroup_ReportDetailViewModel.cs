namespace Metrics.Web.Models.ReportViewModels.KeyKpi;

public class KeyKpi_SingleUserGroup_ReportDetailViewModel
{
    public string? PeriodName { get; set; }
    public UserViewModel SubmittedBy { get; set; } = null!;
    public List<KeyKpi_DepartmentScoreSummary> KeyKpi_DepartmentScoreSummary { get; set; } = [];
}
