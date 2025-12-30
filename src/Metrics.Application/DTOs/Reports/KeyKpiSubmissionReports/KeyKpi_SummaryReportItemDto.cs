using Metrics.Application.DTOs.Department;
using Metrics.Application.DTOs.KeyMetric;

namespace Metrics.Application.DTOs.Reports.KeyKpiSubmissionReports;

// Row items in the summary report
public class KeyKpi_SummaryReportItemDto
{
    public string PeriodName { get; set; } = null!;
    public DepartmentDto KeyIssueDepartment { get; set; } = null!;
    public KeyMetricDto KeyMetric { get; set; } = null!;
    public List<KeyKpi_SummaryReport_SubmissionDto> Submissions { get; set; } = [];
    public long ReceivedSubmissions { get; set; }
    public decimal ReceivedScore { get; set; } // total score received on each department's key
    public decimal AverageScore { get; set; } // scoreTotalPerKey / no. of candidate department with actual score submitted 
}