using Metrics.Application.Domains;

namespace Metrics.Application.Interfaces.IRepositories;

public interface IDepartmentRepository
{
    void Create(Department entity);
    void Update(Department entity);
    void Delete(Department entity);
    Task<IEnumerable<Department>> FindAllAsync();
    Task<IEnumerable<Department>> FindAllAsync(int pageNumber, int pageSize);
    Task<Department?> FindByIdAsync(long id);
    Task<Department?> FindByDepartmentCodeAsync(string departmentCode);
    Task<Department?> FindByDepartmentNameAsync(string departmentName);
    Task<bool> DepartmentExistsAsync(string departmentCode);
    Task<long> FindCountAsync();

    // ========== Queryable ====================================================
    IQueryable<Department> FindAllAsQueryable();
}
