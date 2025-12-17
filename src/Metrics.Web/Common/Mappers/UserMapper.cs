using Metrics.Application.DTOs.User;
using Metrics.Web.Models;

namespace Metrics.Web.Common.Mappers;

public static class UserMapper
{
    public static UserDto MapToDto(this UserViewModel model)
    {
        return new UserDto(
            Id: model.Id,
            UserCode: model.UserCode,
            UserName: model.UserName,
            FullName: model.FullName,
            PhoneNumber: model.PhoneNumber ?? string.Empty,
            ContactAddress: model.ContactAddress ?? string.Empty,
            DepartmentId: model.DepartmentId,
            Department: model.Department.MapToDto(),
            UserGroup: model.UserGroup.MapToDto(),
            LockoutEnabled: model.LockoutEnabled,
            LockoutEnd: model.LockoutEnd
        // UserRole: model.UserRole.MapToDto()
        );
    }

    public static UserViewModel MapToViewModel(this UserDto dto, bool includeDepartment = true)
    {
        return new UserViewModel
        {
            Id = dto.Id,
            UserCode = dto.UserCode,
            UserName = dto.UserName,
            FullName = dto.FullName,
            PhoneNumber = dto.PhoneNumber,
            ContactAddress = dto.ContactAddress,
            DepartmentId = dto.DepartmentId,
            Department = dto.Department.MapToViewModel(),
            UserGroup = dto.UserGroup.MapToViewModel(),
            LockoutEnabled = dto.LockoutEnabled,
            LockoutEnd = dto.LockoutEnd
            // UserRole = dto.UserRole.MapToViewModel(),
        };
    }
}
