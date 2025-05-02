namespace Metrics.Application.Domains;

public partial class KpiPeriod
{
    public long Id { get; set; }
    public string PeriodName { get; set; } = null!;
    public DateTimeOffset SubmissionStartDate { get; set; }
    public DateTimeOffset SubmissionEndDate { get; set; }

    // Collection Navigational Properties
    public List<KpiSubmission> KpiSubmissions { get; set; } = [];

}
