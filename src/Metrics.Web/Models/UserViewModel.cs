using Metrics.Web.Models.UserRoles;

namespace Metrics.Web.Models;

public class UserViewModel
{
    public string Id { get; set; } = null!;
    public required string UserCode { get; set; }
    public required string UserName { get; set; }
    public required string FullName { get; set; }
    public string? PhoneNumber { get; set; } = string.Empty;
    public string? ContactAddress { get; set; } = string.Empty;
    public required long DepartmentId { get; set; }
    // public long UserTitleId { get; set; }
    public RoleViewModel UserRole { get; set; } = null!;
    public List<RoleViewModel> UserRoles { get; set; } = [];
    public DepartmentViewModel Department { get; set; } = null!;
    public UserGroupViewModel UserGroup { get; set; } = null!;
    public bool LockoutEnabled { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
}