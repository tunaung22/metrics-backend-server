using Metrics.Web.Models.DepartmentKeyMetric;

namespace Metrics.Web.Models.ReportViewModels.KeyKpi;

public class KeyKpi_DepartmentScoreDetail
{
    public string KeyIssueDepartmentName { get; set; } = string.Empty;
    public DepartmentKeyMetricViewModel? DepartmentKeyMetric { get; set; }
    public long KeyId { get; set; }
    public long DKMId { get; set; }
    public string? KeyTitle { get; set; } = null!;
    public decimal ScoreValue { get; set; }
    public string? Comments { get; set; } = string.Empty;
}