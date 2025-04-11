using Metrics.Application.DTOs.UserAccountDtos;
using Metrics.Application.Entities;

namespace Metrics.Application.Mappers.DtoMappers;

public static class UserAccountDtoMapper
{
    // UserAccountCreateDto <-> EmployeeCreateDto
    public static Employee ToEntity(this UserAccountCreateDto createDto)
    {
        return new Employee
        {
            EmployeeCode = createDto.EmployeeCode,
            FullName = createDto.FullName,
            Address = createDto.Address ?? string.Empty,
            PhoneNumber = createDto.PhoneNumber ?? string.Empty,
            DepartmentId = createDto.DepartmentId,
            ApplicationUserId = createDto.ApplicationUserId
        };
    }
}
