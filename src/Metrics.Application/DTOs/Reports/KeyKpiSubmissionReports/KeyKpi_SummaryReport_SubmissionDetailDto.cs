namespace Metrics.Application.DTOs.Reports.KeyKpiSubmissionReports;

public class KeyKpi_SummaryReport_SubmissionDetailDto
{
    public string CandidateUserCode { get; set; } = string.Empty;
    public string CandidateUserName { get; set; } = string.Empty;
    public string CandidateName { get; set; } = string.Empty;
    public string CandidateGroup { get; set; } = string.Empty;
    public decimal SubmittedScore { get; set; }
}