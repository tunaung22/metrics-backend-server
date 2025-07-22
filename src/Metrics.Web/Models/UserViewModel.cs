namespace Metrics.Web.Models;

public class UserViewModel
{
    public string Id { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string? PhoneNumber { get; set; } = string.Empty;
    public string? ContactAddress { get; set; } = string.Empty;
    // public long DepartmentId { get; set; }
    // public long UserTitleId { get; set; }
    public DepartmentViewModel Department { get; set; } = null!;
    public UserGroupViewModel UserGroup { get; set; } = null!;

}