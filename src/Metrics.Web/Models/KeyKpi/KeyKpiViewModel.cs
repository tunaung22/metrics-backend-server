namespace Metrics.Web.Models.KeyKpi;

public class KeyKpiViewModel
{
    public long Id { get; set; }
    public Guid MetricCode { get; set; }
    public string MetricTitle { get; set; } = null!;
    public string? Description { get; set; } = string.Empty;
    public bool IsDeleted { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ModifiedAt { get; set; }
}
