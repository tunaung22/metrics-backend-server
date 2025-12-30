using Metrics.Web.Models.KeyMetric;

namespace Metrics.Web.Models.ReportViewModels.KeyKpiSummaryReports;

public class KeyKpi_SummaryReportItem_ViewModel
{
    public string PeriodName { get; set; } = null!;
    public DepartmentViewModel KeyIssueDepartment { get; set; } = null!;
    public KeyMetricViewModel KeyMetric { get; set; } = null!;
    public List<KeyKpi_SummaryReport_Submission_ViewModel> Submissions { get; set; } = [];
    public long ReceivedSubmissions { get; set; }
    public decimal ReceivedScore { get; set; } // total score received on each department's key
    public decimal AverageScore { get; set; } // scoreTotalPerKey / no. of candidate department with actual score submitted 

}
