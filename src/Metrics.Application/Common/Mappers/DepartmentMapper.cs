using Metrics.Application.Domains;
using Metrics.Application.DTOs.Department;
using Metrics.Application.DTOs.User;

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
}
