using System;

namespace Metrics.Web.Models.ReportViewModels;

public class AllUserScoreSubmissionsReport
{
    public string? PeriodName { get; set; }
    public string? DepartmentName { get; set; }
    public List<UserGroupSubmissionInfo> UserGroupSubmissionInfos { get; set; } = [];
    public long TotalSubmissions { get; set; }
    public decimal TotalScore { get; set; }
    public decimal KpiScore { get; set; }
}

