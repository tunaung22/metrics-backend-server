namespace Metrics.Web.Models.ReportViewModels.KeyKpi;

// public class KeyKpi_AllUserGroup_ReportDetailViewModel
// {
//     public string? PeriodName { get; set; }
//     public UserViewModel SubmittedBy { get; set; } = null!;
//     public DateTimeOffset SubmittedAt { get; set; }
//     // flatten KeyKPISubmisionItems: key, issuer, score, comments
//     public DepartmentKeyMetricViewModel DepartmentKeyMetric { get; set; } = null!;
//     public string? DepartmentName { get; set; }
//     public decimal ScoreValue { get; set; }
//     public string? Comments { get; set; } = string.Empty;
// }

// public class DepartmentScore
// {
//     public string? DepartmentName { get; set; }
//     public DepartmentKeyMetricViewModel DepartmentKeyMetric { get; set; } = null!;
//     public decimal ScoreValue { get; set; }
//     public string? Comments { get; set; } = string.Empty;
// }


// public class KeyKpi_AllUserGroup_ReportDetailViewModel
// {
//     public string? PeriodName { get; set; }
//     public UserViewModel SubmittedBy { get; set; } = null!;
//     // public DateTimeOffset SubmittedAt { get; set; }
//     // flatten KeyKPISubmisionItems: key, issuer, score, comments
//     public List<KeyKpi_DepartmentScoreDetail> DepartmentScoreDetails { get; set; } = [];
// }

public class KeyKpi_AllUserGroup_ReportDetailViewModel
{
    public string? PeriodName { get; set; }
    public UserViewModel SubmittedBy { get; set; } = null!;
    // public DateTimeOffset SubmittedAt { get; set; }
    // flatten KeyKPISubmisionItems: key, issuer, score, comments
    // public List<KeyKpi_DepartmentScoreSummary> KeyKpi_DepartmentScoreSummary { get; set; } = [];
    public List<KeyKpi_DepartmentScoreDetail> KeyKpi_DepartmentScoreDetails { get; set; } = [];
}