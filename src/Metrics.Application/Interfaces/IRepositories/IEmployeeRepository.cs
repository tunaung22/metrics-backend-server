using Metrics.Application.Domains;

namespace Metrics.Application.Interfaces.IRepositories;

public interface IEmployeeRepository
{
    void Create(Employee entity);
    void Update(Employee enitiy);
    void Delete(Employee enitiy);
    Task<Employee> FindByEmployeeCodeAsync(string employeeCode);
    Task<Employee> FindByIdAsync(long id);
    Task<IEnumerable<Employee>> FindAllAsync();
    Task<bool> EmployeeExistsAsync(string employeeCode);
    // Task<Employee> FindByEmployeeCodeAsync(string employeeCode);
    // Task<bool> EmployeeExistAsync(string employeeCode);

    // ========== Queryable ====================================================
    IQueryable<Employee> FindAllAsQueryable();
}
