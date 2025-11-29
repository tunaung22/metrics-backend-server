using Metrics.Application.Domains;
using Metrics.Application.DTOs.Department;

namespace Metrics.Application.Common.Mappers;

public static class DepartmentMapper
{
    public static DepartmentDto MapToDto(this Department e)
    {
        return new DepartmentDto
        {
            Id = e.Id,
            DepartmentCode = e.DepartmentCode,
            DepartmentName = e.DepartmentName,
            // DepartmentStaffs = e.ApplicationUsers.Select(u => u.MapToDto(false)).ToList(),
            StaffCount = e.ApplicationUsers.Count,
        };
    }

    public static Department MapToEntity(this DepartmentDto dto)
    {
        return new Department
        {
            DepartmentCode = dto.DepartmentCode,
            DepartmentName = dto.DepartmentName,
        };
    }

    public static Department MapToEntity(this DepartmentCreateDto dto)
    {
        return new Department
        {
            // DepartmentCode = dto.DepartmentCode,
            DepartmentName = dto.DepartmentName,
        };
    }

    public static Department MapToEntity(this DepartmentUpdateDto dto)
    {
        return new Department
        {
            // DepartmentCode = dto.DepartmentCode,
            DepartmentName = dto.DepartmentName,
        };
    }
}
