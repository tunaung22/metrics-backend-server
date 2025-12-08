using Metrics.Application.Interfaces.IServices;
using Metrics.Web.Common.Mappers;
using Metrics.Web.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Metrics.Web.Pages.Manage.Permissions;

public class IndexModel(
    IAppPermissionService appPermissionService) : PageModel
{
    private readonly IAppPermissionService _appPermissionService = appPermissionService;


    // =========================================================================
    public List<PermissionViewModel> Permissions { get; set; } = [];


    // =========================================================================
    public async Task OnGetAsync()
    {
        var perms = await _appPermissionService.FindAllPermissionsAsync();
        if (perms.IsSuccess && perms.Data != null)
        {
            Permissions = perms.Data.Select(d => new PermissionViewModel
            {
                Id = d.Id,
                TaskName = d.TaskName,
                UserDepartmentId = d.UserDepartmentId,
                UserDepartment = d.UserDepartment?.MapToViewModel(),
                UserGroupId = d.UserGroupId,
                UserGroup = d.UserGroup?.MapToViewModel(),
                LastModifiedById = d.LastModifiedById,
                LastModifiedBy = d.LastModifiedBy?.MapToViewModel(),
            }).ToList();
        }
    }


    // =========================================================================
    public class PermissionViewModel
    {
        public long Id { get; set; }
        public string LastModifiedById { get; set; } = null!;
        public string TaskName { get; set; } = null!;
        public long? UserDepartmentId { get; set; }
        public long? UserGroupId { get; set; }
        public DepartmentViewModel? UserDepartment { get; set; }
        public UserGroupViewModel? UserGroup { get; set; }
        public UserViewModel? LastModifiedBy { get; set; }
        // public DateTimeOffset CreatedAt { get; set; }
        // public DateTimeOffset ModifiedAt { get; set; }
    }
}
