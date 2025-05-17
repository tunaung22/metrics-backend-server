using Metrics.Application.DTOs.EmployeeDtos;
using Metrics.Application.Domains;

namespace Metrics.Application.Mappers.DtoMappers;

public static class EmployeeDtoMapper
{
    /*
    // ========== Entity to DTO ==========
    // public static List<EmployeeGetDto> ToGetDto(this List<Employee> entities) { }
    public static List<EmployeeGetDto> ToGetDto(this IEnumerable<ApplicationUser> entities)
    {
        if (entities == null) return [];

        return entities.Select(e => new EmployeeGetDto
        {
            EmployeeCode = e.EmployeeCode,
            FullName = e.FullName,
            Address = e.Address,
            PhoneNumber = e.PhoneNumber,
            DepartmentId = e.DepartmentId,
            ApplicationUserId = e.ApplicationUserId
        }).ToList();
    }

    public static EmployeeGetDto ToGetDto(this ApplicationUser entity)
    {
        return new EmployeeGetDto
        {
            EmployeeCode = entity.EmployeeCode,
            FullName = entity.FullName,
            Address = entity.Address,
            PhoneNumber = entity.PhoneNumber,
            DepartmentId = entity.DepartmentId,
            ApplicationUserId = entity.ApplicationUserId
        };
    }

    // ========== DTO to Entity ==========
    public static ApplicationUser ToEntity(this EmployeeCreateDto createDto)
    {
        return new ApplicationUser
        {
            EmployeeCode = createDto.EmployeeCode,
            FullName = createDto.FullName,
            Address = createDto.Address,
            PhoneNumber = createDto.PhoneNumber,
            DepartmentId = createDto.DepartmentId,
            ApplicationUserId = createDto.ApplicationUserId
        };
    }

    public static ApplicationUser ToEntity(EmployeeUpdateDto updateDto)
    {
        return new ApplicationUser
        {
            FullName = updateDto.FullName,
            Address = updateDto.Address,
            PhoneNumber = updateDto.PhoneNumber,
            DepartmentId = updateDto.DepartmentId
        };
    }

    // ========== DTO to DTO ==========
    public static EmployeeGetDto ToGetDto(this EmployeeCreateDto createDto)
    {
        return new EmployeeGetDto
        {
            EmployeeCode = createDto.EmployeeCode,
            FullName = createDto.FullName,
            Address = createDto.Address,
            PhoneNumber = createDto.PhoneNumber,
            DepartmentId = createDto.DepartmentId,
            ApplicationUserId = createDto.ApplicationUserId
        };
    }
    */
}
