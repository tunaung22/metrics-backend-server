using Metrics.Application.Domains;
using Metrics.Application.DTOs.Role;

namespace Metrics.Application.Common.Mappers;

public static class RoleDtoMapper
{
    public static RoleDto MapToDto(this ApplicationRole e)
    {
        return new RoleDto(e.Id, e.Name!);
    }
    public static ApplicationRole MapToEntity(this RoleDto dto)
    {
        return new ApplicationRole
        {
            Name = dto.RoleName,
        };
    }

}
