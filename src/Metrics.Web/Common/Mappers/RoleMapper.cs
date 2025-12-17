using Metrics.Application.DTOs.Role;
using Metrics.Web.Models.UserRoles;

namespace Metrics.Web.Common.Mappers;

public static class RoleMapper
{
    public static RoleDto MapToDto(this RoleViewModel model)
    {
        return new RoleDto(Id: model.Id, RoleName: model.RoleName);
    }

    public static RoleViewModel MapToViewModel(this RoleDto dto)
    {
        return new RoleViewModel
        {
            Id = dto.Id,
            RoleName = dto.RoleName
        };
    }
}
