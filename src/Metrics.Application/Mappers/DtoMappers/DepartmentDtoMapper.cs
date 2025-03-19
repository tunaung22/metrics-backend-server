using Metrics.Application.DTOs.DepartmentDtos;
using Metrics.Domain.Entities;
using System;

namespace Metrics.Application.Mappers.DtoMappers;

public static class DepartmentDtoMapper
{
    // ========== Entity to DTO ==========
    // public static List<DepartmentGetDto> ToGetDto(this List<Department> entities) { }
    public static List<DepartmentGetDto> ToGetDto(this IEnumerable<Department> entities)
    {
        if (entities == null) return [];

        return entities.Select(entity => new DepartmentGetDto
        {
            DepartmentCode = entity.DepartmentCode,
            DepartmentName = entity.DepartmentName,
        }).ToList();
    }

    public static DepartmentGetDto ToGetDto(this Department entity)
    {
        return new DepartmentGetDto
        {
            DepartmentCode = entity.DepartmentCode,
            DepartmentName = entity.DepartmentName,
        };
    }

    // ========== DTO to Entity ==========
    public static Department ToEntity(this DepartmentCreateDto dto)
    {
        return new Department
        {
            // DepartmentCode = dto.DepartmentCode,
            DepartmentName = dto.DepartmentName,
        };
    }

    public static Department ToEntity(this DepartmentUpdateDto dto)
    {
        return new Department
        {
            // DepartmentCode = dto.DepartmentCode,
            DepartmentName = dto.DepartmentName,
        };
    }

    // ========== DTO to DTO ==========
    public static DepartmentGetDto ToGetDto(this DepartmentCreateDto dto)
    {
        return new DepartmentGetDto
        {
            // DepartmentCode = dto.DepartmentCode,
            DepartmentName = dto.DepartmentName,
        };
    }
}
