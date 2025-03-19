using Metrics.Domain.Entities;
using System;

namespace Metrics.Infrastructure.Repositories.IRepositories;

public interface IEmployeeRepository //: IGenericRepository<Employee>
{
    // Task<Employee> FindByEmployeeCodeAsync(string employeeCode);

    // // Experimental
    // Task<bool> EmployeeExistAsync(string employeeCode);

    Task<Employee> FindByEmployeeCodeAsync(string employeeCode);
    Task<Employee> FindByIdAsync(long id);
    void Create(Employee entity);
    void Update(Employee enitiy);
    void Delete(Employee enitiy);
    Task<IEnumerable<Employee>> FindAllAsync();
    Task<bool> EmployeeExistsAsync(string employeeCode);
    // Experimentals
    IQueryable<Employee> FindAllAsQueryable();
}
