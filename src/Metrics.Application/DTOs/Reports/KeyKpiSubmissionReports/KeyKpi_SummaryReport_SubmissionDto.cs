using Metrics.Application.DTOs.Department;

namespace Metrics.Application.DTOs.Reports.KeyKpiSubmissionReports;

public class KeyKpi_SummaryReport_SubmissionDto
{
    public DepartmentDto CandidateDepartment { get; set; } = null!;
    public decimal TotalScoreByCandidateDepartment { get; set; }
    public List<KeyKpi_SummaryReport_SubmissionDetailDto> SubmissionDetails { get; set; } = [];
}
