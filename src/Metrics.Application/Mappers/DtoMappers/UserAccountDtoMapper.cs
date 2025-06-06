using Metrics.Application.DTOs.UserAccountDtos;
using Metrics.Application.Domains;

namespace Metrics.Application.Mappers.DtoMappers;

public static class UserAccountDtoMapper
{
    // UserAccountCreateDto <-> EmployeeCreateDto
    public static ApplicationUser ToEntity(this UserAccountCreateDto createDto)
    {
        return new ApplicationUser
        {
            UserCode = createDto.UserCode,
            FullName = createDto.FullName,
            ContactAddress = createDto.Address ?? string.Empty,
            ProfilePictureUrl = string.Empty,
            PhoneNumber = createDto.PhoneNumber ?? string.Empty,
            DepartmentId = createDto.DepartmentId,
            // ApplicationUserId = createDto.ApplicationUserId
        };
    }
}
