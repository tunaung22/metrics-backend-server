namespace Metrics.Web.Models;

public class KeyMetricViewModel
{
    public long Id { get; set; }
    public Guid LookupId { get; set; }
    public string MetricTitle { get; set; } = null!;
    public string? Description { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
}
