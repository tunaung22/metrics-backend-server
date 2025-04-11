using Metrics.Application.Entities;
using Metrics.Application.Results;

namespace Metrics.Application.Interfaces.IServices;

public interface IDepartmentService
{
    // ========== Return Entity =====================================
    Task<Department> CreateAsync(Department entity);
    Task<Department> UpdateAsync(string departmentCode, Department entity);
    Task<bool> DeleteAsync(string departmentCode);
    Task<Department> FindByIdAsync(long id);
    Task<Department> FindByDepartmentCodeAsync(string departmentCode);
    Task<IEnumerable<Department>> FindAllAsync();
    Task<IEnumerable<Department>> FindAllAsync(int pageNumber = 1, int pageSize = 20);
    Task<long> FindCountAsync();


    // ========== Return DTO =====================================
    // Task<DepartmentGetDto> FindByDepartmentCodeAsync(string departmentCode);
    // // Task<IEnumerable<Department>> FindAsync(string keyword); // Search feature
    // Task<DepartmentGetDto> FindByIdAsync(long id);
    // Task<DepartmentGetDto> CreateAsync(DepartmentCreateDto createDto);
    // Task<DepartmentGetDto> UpdateAsync(string departmentCode, DepartmentUpdateDto updateDto);
    // Task<bool> DeleteAsync(string departmentCode);
    // Task<IEnumerable<DepartmentGetDto>> FindAllAsync();
    // Task<IEnumerable<DepartmentGetDto>> FindAllPaginatedAsync();
    // Task<IEnumerable<DepartmentDto>> FindAllInsecureAsync();
    // Task<IEnumerable<DepartmentDto>> FindAllInsecureAsync(int pageNumber = 1, int pageSize = 20);
    // Task<int> FindCountAsync();
    // ========== Operation Result Pattern =====================================
    // Task<Result<Department>> FindById_ResultAsync(long id);
    // Task<Result<Department>> FindByDepartmentCode_ResultAsync(string departmentCode);
    // Task<Result<Department>> Create_ResultAsync(Department entity);
    // Task<Result<Department>> Update_ResultAsync(string departmentCode, Department entity);
    // Task<Result<bool>> Delete_ResultAsync(string departmentCode);
    // Task<Result<IEnumerable<Department>>> FindAll_ResultAsync();
    // Task<Result<IEnumerable<Department>>> FindAll_ResultAsync(int pageNumber = 1, int pageSize = 20);
    // Task<Result<long>> FindCount_ResultAsync();
}
