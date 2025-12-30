namespace Metrics.Application.DTOs.Reports.KeyKpiSubmissionReports;

public class KeyKpi_SummaryReportDto
{
    public List<KeyKpi_SummaryReportItemDto> SummaryReportItems { get; set; } = [];
    public decimal FinalScore { get; set; }
}
