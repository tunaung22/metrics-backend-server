using Metrics.Application.DTOs.DepartmentDtos;
using Metrics.Common.Utils;
using Metrics.Domain.Entities;
using Metrics.Web.ViewModels.DepartmentViewModels;

namespace Metrics.Web.Mappers.ViewModelMappers;

/**
 * DTO <-to-> ViewModel
 */
public static class DepartmentViewModelMapper
{
    // ========== Entity to ViewModel ==========
    public static IEnumerable<DepartmentGetViewModel> ToGetViewModel(this IEnumerable<Department> entity)
    {
        if (entity.Count() == 0)
            return [];

        return entity.Select(e => new DepartmentGetViewModel
        {
            Id = e.Id,
            DepartmentCode = e.DepartmentCode,
            DepartmentName = e.DepartmentName
        });
    }
    public static DepartmentGetViewModel ToGetViewModel(this Department entity)
    {
        return new DepartmentGetViewModel
        {
            DepartmentCode = entity.DepartmentCode,
            DepartmentName = entity.DepartmentName
        };
    }
    public static DepartmentCreateViewModel ToCreateViewModel(this Department entity)
    {
        return new DepartmentCreateViewModel
        {
            DepartmentName = entity.DepartmentName
        };
    }

    public static DepartmentUpdateViewModel ToUpdateViewModel(this Department entity)
    {
        return new DepartmentUpdateViewModel
        {
            Id = entity.Id,
            DepartmentCode = entity.DepartmentCode,
            DepartmentName = entity.DepartmentName,
        };
    }


    // ========== ViewModel to Entity ==========
    public static Department ToEntity(this DepartmentUpdateViewModel formModel)
    {
        return new Department
        {
            Id = formModel.Id,
            DepartmentCode = formModel.DepartmentCode,
            DepartmentName = formModel.DepartmentName,
        };
    }
    public static Department ToEntity(this DepartmentCreateViewModel model)
    {
        return new Department
        {
            DepartmentName = model.DepartmentName,
        };
    }
    // =========================================================================

    // ========== DTO to ViewModel ==========
    public static IEnumerable<DepartmentGetViewModel>? ToViewModel(this IEnumerable<DepartmentGetDto>? dto)
    {
        if (dto == null)
            return null;

        return dto.Select(e => new DepartmentGetViewModel
        {
            DepartmentCode = e.DepartmentCode,
            DepartmentName = e.DepartmentName,
        });
    }

    public static DepartmentUpdateViewModel? ToUpdateViewModel(this DepartmentGetDto dto)
    {
        if (dto == null) return null;

        return new DepartmentUpdateViewModel
        {
            DepartmentName = dto.DepartmentName,
        };
        //return dto?.ToViewModel();
    }

    public static DepartmentGetViewModel? ToViewModel(this DepartmentGetDto dto)
    {
        return new DepartmentGetViewModel
        {
            DepartmentCode = dto.DepartmentCode,
            DepartmentName = dto.DepartmentName,
        };
        //return dto?.ToViewModel();
    }

    public static DepartmentFormViewModel? ToFormViewModel(this DepartmentGetDto dto)
    {
        return new DepartmentFormViewModel
        {
            // DepartmentCode = dto.DepartmentCode,
            DepartmentName = dto.DepartmentName,
        };
        //return dto?.ToViewModel();
    }

    // ========== ViewModel to DTO ==========
    public static DepartmentCreateDto ToCreateDto(this DepartmentCreateViewModel viewModel)
    {
        return new DepartmentCreateDto
        {
            DepartmentName = viewModel.DepartmentName,
        };
    }

    public static DepartmentUpdateDto ToUpdateDto(this DepartmentUpdateViewModel viewModel)
    {
        return new DepartmentUpdateDto
        {
            DepartmentName = viewModel.DepartmentName,
        };
    }

    public static DepartmentCreateDto ToCreateDto(this DepartmentFormViewModel viewModel)
    {
        return new DepartmentCreateDto
        {
            DepartmentName = viewModel.DepartmentName,
        };
    }

    public static DepartmentUpdateDto ToUpdateDto(this DepartmentFormViewModel viewModel)
    {
        return new DepartmentUpdateDto
        {
            DepartmentName = viewModel.DepartmentName,
        };
    }
    // public static Department ToEntity(DepartmentUpdateDto dto) { }
    // public static Department ToEntity(this DepartmentCreateDto dto) { }

    // ========== DTO to DTO ==========
    // public static DepartmentGetDto ToGetDto(this DepartmentCreateDto dto) { }
}