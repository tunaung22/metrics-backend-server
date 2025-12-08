using Metrics.Application.Domains;
using Metrics.Application.DTOs.AppPermission;

namespace Metrics.Application.Common.Mappers;

public static class AppPermissionMapper
{
    public static AppPermissionDto MapToDto(this ApplicationPermission e)

    {
        return new AppPermissionDto
        {
            Id = e.Id,
            TaskName = e.TaskName,
            LastModifiedById = e.LastModifiedById,
            UserDepartmentId = e.UserDepartmentId,
            UserGroupId = e.UserGroupId,
            LastModifiedBy = e.LastModifiedBy?.MapToDto(),
            UserDepartment = e.UserDepartment?.MapToDto(),
            UserGroup = e.UserGroup?.MapToDto(),
        };
    }

}
