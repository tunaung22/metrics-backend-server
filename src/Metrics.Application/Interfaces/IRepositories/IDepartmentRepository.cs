using Metrics.Application.Entities;

namespace Metrics.Application.Interfaces.IRepositories;

public interface IDepartmentRepository
{
    void Create(Department entity);
    void Update(Department entity);
    void Delete(Department entity);
    Task<IEnumerable<Department>> FindAllAsync();
    Task<Department> FindByIdAsync(long id);
    Task<Department> FindByDepartmentCodeAsync(string departmentCode);
    Task<bool> DepartmentExistsAsync(string departmentCode);
    Task<long> FindCountAsync();

    // ========== Queryable ====================================================
    IQueryable<Department> FindAllAsQueryable();
}
