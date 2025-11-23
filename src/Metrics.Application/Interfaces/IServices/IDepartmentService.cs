using Metrics.Application.Domains;
using Metrics.Application.DTOs.Department;
using Metrics.Application.Results;

namespace Metrics.Application.Interfaces.IServices;

public interface IDepartmentService
{
    Task<ResultT<List<DepartmentDto>>> FindAllAsync(int pageNumber = 1, int pageSize = 50);
    Task<ResultT<List<DepartmentDto>>> FindAll_R_Async();
    Task<Result> CreateAsync(DepartmentCreateDto createDto);
    Task<ResultT<DepartmentDto>> FindByCodeAsync(string departmentCode);


    // Task<Department> CreateAsync(Department entity);
    Task<Department> UpdateAsync(string departmentCode, Department entity);
    Task<bool> DeleteAsync(string departmentCode);
    Task<Department> FindByIdAsync(long id);
    Task<Department> FindByDepartmentCodeAsync(string departmentCode);
    Task<Department?> FindByDepartmentNameAsync(string departmentName);
    Task<IEnumerable<Department>> FindAllAsync();
    Task<IEnumerable<Department>> FindAll_Async(int pageNumber = 1, int pageSize = 20);
    Task<long> FindCountAsync();
}
