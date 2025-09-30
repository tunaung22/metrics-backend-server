using Metrics.Application.DTOs.Department;
using Metrics.Web.Models;

namespace Metrics.Web.Common.Mappers;

public static class DepartmentMapper
{
    public static DepartmentDto MapToDto(this DepartmentViewModel model)
    {
        return new DepartmentDto
        {
            Id = model.Id,
            DepartmentCode = model.DepartmentCode,
            DepartmentName = model.DepartmentName
        };
    }

    public static DepartmentViewModel MapToViewModel(this DepartmentDto dto)
    {
        return new DepartmentViewModel
        {
            Id = dto.Id,
            DepartmentCode = dto.DepartmentCode,
            DepartmentName = dto.DepartmentName,
            StaffCount = dto.DepartmentStaffs.Count,
            DepartmentStaffs = dto.DepartmentStaffs.Select(u => u.MapToViewModel(false)).ToList()
        };
    }
}
