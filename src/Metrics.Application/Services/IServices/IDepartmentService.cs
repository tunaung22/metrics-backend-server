using Metrics.Application.DTOs.DepartmentDtos;
using Metrics.Application.Results;
using Metrics.Domain.Entities;

namespace Metrics.Application.Services.IServices;

public interface IDepartmentService
{
    Task<Result<Department>> FindByDepartmentCodeAsync(string departmentCode);
    // Task<IEnumerable<Department>> FindAsync(string keyword); // Search feature
    Task<Result<Department>> FindByIdAsync(long id);
    Task<Result<Department>> CreateAsync(Department entity);
    Task<Result<Department>> UpdateAsync(Department entity);
    Task<Result<bool>> DeleteAsync(string departmentCode);
    Task<Result<IEnumerable<Department>>> FindAllAsync();

    Task<DepartmentGetDto> FindByDepartmentCode_Async(string departmentCode);
    // Task<IEnumerable<Department>> FindAsync(string keyword); // Search feature
    Task<DepartmentGetDto> FindById_Async(long id);
    Task<DepartmentGetDto> Create_Async(DepartmentCreateDto createDto);
    Task<DepartmentGetDto> Update_Async(string departmentCode, DepartmentUpdateDto updateDto);
    Task<bool> Delete_Async(string departmentCode);
    Task<IEnumerable<DepartmentGetDto>> FindAll_Async();
    Task<IEnumerable<DepartmentDto>> FindAllInsecure_Async();
    // Task<Department?> GetBy_DepartmentCode_Async(string departmentCode);
    // Task<IEnumerable<Department>> Search_Async(string keyword);
    // Task<Department?> GetBy_IdAsync(long id);
    // Task<Department> Create_Async(Department entity);
    // Task<Department> Update_Async(Department entity);
    // Task<bool> Delete_Async(string departmentCode);
    // Task<IEnumerable<Department>> GetAll_Async();


    // Task<Department?> GetBy_DepartmentCode_Async(string departmentCode);
    // Task<IEnumerable<Department>> Search_Async(string keyword);
    // Task<Department?> GetBy_IdAsync(long id);
    // Task<Department> Create_Async(Department entity);
    // Task<Department> Update_Async(Department entity);
    // Task<bool> Delete_Async(string departmentCode);
    // Task<IEnumerable<Department>> GetAll_Async();

    // ====== Return DTO ======
    // Task<DepartmentGetDto?> GetByDepartmentCodeAsync(string departmentCode);
    // Task<DepartmentGetDto?> GetByIdAsync(long id);
    // Task<IEnumerable<DepartmentGetDto>> GetAllAsync();
    // Task<DepartmentGetDto> CreateAsync(DepartmentCreateDto createDto);
    // Task<DepartmentGetDto> UpdateAsync(DepartmentUpdateDto updateDto);
    // Task<bool> DeleteAsync(string departmentCode);
    // Task<IEnumerable<DepartmentGetDto>> SearchAsync(string keyword);
    // ======  ======

    // Task<Department> FindByIdAsync(long id);
    // Task<Department?> FindByDepartmentCodeAsync(string departmentCode);
    // Task<Department> CreateAsync(Department entity);
    // Task<Department> UpdateAsync(long id, Department entity);
    // Task<Department> UpdateByDepartmentCodeAsync(string departmentCode, Department entity);
    // Task<bool> Delete(string departmentCode);
    // Task<IEnumerable<Department>> FindAllAsync();

}
