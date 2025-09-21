using Metrics.Application.Domains;
using Metrics.Application.DTOs.User;

namespace Metrics.Application.Common.Mappers;

public static class UserMapper
{
    // TODO: ???
    public static ApplicationUser MapToEntity(this UserCreateDto dto)
    {
        return new ApplicationUser
        {
            UserName = dto.UserName,
            Email = dto.Email,
            FullName = dto.FullName
        };
    }

    public static UserDto MapToDto(this ApplicationUser e)
    {
        return new UserDto(
            Id: e.Id,
            UserCode: e.UserCode,
            UserName: e.UserName ?? string.Empty,
            FullName: e.FullName,
            PhoneNumber: e.PhoneNumber ?? string.Empty,
            ContactAddress: e.ContactAddress ?? string.Empty,
            UserGroup: e.UserTitle.MapToDto(),
            Department: e.Department.MapToDto(),
            LockoutEnabled: e.LockoutEnabled,
            LockoutEnd: e.LockoutEnd
        );
    }
}
