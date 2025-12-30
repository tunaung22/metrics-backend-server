using System;

namespace Metrics.Web.Models.ReportViewModels.KeyKpiSummaryReports;

public class KeyKpi_SummaryReport_ViewModel
{
    public List<KeyKpi_SummaryReportItem_ViewModel> SummaryReportItems { get; set; } = [];
    public decimal FinalScore { get; set; }

}
