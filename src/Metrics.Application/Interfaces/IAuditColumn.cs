namespace Metrics.Application.Interfaces;

public interface IAuditColumn
{
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ModifiedAt { get; set; }
}
