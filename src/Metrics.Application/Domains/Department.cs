using Metrics.Application.Interfaces;

namespace Metrics.Application.Domains;

public class Department : IAuditColumn
{
    public long Id { get; set; }

    public Guid DepartmentCode { get; set; }
    public string DepartmentName { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ModifiedAt { get; set; } = DateTimeOffset.UtcNow;

    // Collection Navigational Properties
    public List<Employee> Employees { get; set; } = [];
    public List<KpiSubmission> KpiSubmissions { get; set; } = [];
}
