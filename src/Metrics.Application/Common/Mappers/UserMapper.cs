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
            // ...
        };
    }

    public static UserDto MapToDto(this ApplicationUser e)
    {
        return new UserDto(
            Id: e.Id,
            UserName: e.UserName ?? string.Empty,
            FullName: e.FullName,
            PhoneNumber: e.PhoneNumber ?? string.Empty,
            ContactAddress: e.ContactAddress ?? string.Empty,
            UserGroup: e.UserTitle.MapToDto(),
            Department: e.Department.MapToDto()
        );
    }
}
