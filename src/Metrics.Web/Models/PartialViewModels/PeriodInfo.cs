namespace Metrics.Web.Models.PartialViewModels;

public class PeriodInfo
{
    public string PeriodName { get; set; } = string.Empty;
    public DateTime SubmissionStartDate { get; set; }
    public DateTime SubmissionEndDate { get; set; }
}
