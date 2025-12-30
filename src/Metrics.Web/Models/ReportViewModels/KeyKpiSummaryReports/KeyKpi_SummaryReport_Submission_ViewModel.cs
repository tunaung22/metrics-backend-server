namespace Metrics.Web.Models.ReportViewModels.KeyKpiSummaryReports;

public class KeyKpi_SummaryReport_Submission_ViewModel
{
    public DepartmentViewModel CandidateDepartment { get; set; } = null!;
    public decimal TotalScoreByCandidateDepartment { get; set; }
    public List<KeyKpi_SummaryReport_SubmissionDetail_ViewModel> SubmissionDetails { get; set; } = [];

}
