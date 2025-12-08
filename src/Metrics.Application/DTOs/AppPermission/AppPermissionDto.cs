using Metrics.Application.DTOs.Department;
using Metrics.Application.DTOs.User;
using Metrics.Application.DTOs.UserGroup;

namespace Metrics.Application.DTOs.AppPermission;

public record AppPermissionDto
{
    public long Id { get; set; }
    public string LastModifiedById { get; set; } = null!;
    public string TaskName { get; set; } = null!;
    public long? UserDepartmentId { get; set; }
    public long? UserGroupId { get; set; }
    public DepartmentDto? UserDepartment { get; set; }
    public UserGroupDto? UserGroup { get; set; }
    public UserDto? LastModifiedBy { get; set; }
    // public DateTimeOffset CreatedAt { get; set; }
    // public DateTimeOffset ModifiedAt { get; set; }
}