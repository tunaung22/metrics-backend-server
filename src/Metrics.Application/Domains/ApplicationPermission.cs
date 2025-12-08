using Metrics.Application.Interfaces;

namespace Metrics.Application.Domains;

public class ApplicationPermission : IAuditColumn
{
    public long Id { get; set; }

    // Audit
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ModifiedAt { get; set; }
    public string LastModifiedById { get; set; } = null!;

    public string TaskName { get; set; } = null!;
    public long? UserDepartmentId { get; set; }
    public long? UserGroupId { get; set; }

    // Navigational
    public Department? UserDepartment { get; set; }
    public UserTitle? UserGroup { get; set; }
    public ApplicationUser? LastModifiedBy { get; set; }
}
