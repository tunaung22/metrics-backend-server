namespace Metrics.Web.Models.ReportViewModels.KeyKpi;

public class KeyKpi_UserGroup_SubmissionInfo
{
    public string? GroupName { get; set; }
    public int Keys { get; set; } // numbers of keys
    public int TotalSubmissions { get; set; }
    public decimal TotalScore { get; set; }
}
