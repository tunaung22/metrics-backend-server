using Metrics.Application.Domains;

namespace Metrics.Application.Interfaces.IRepositories;

public interface IUserRepository
{
    // void Create(ApplicationUser entity);
    // void Update(ApplicationUser enitiy);
    // void Delete(ApplicationUser enitiy);
    Task<ApplicationUser?> FindByUserCodeAsync(string userCode);
    // Task<ApplicationUser> FindByIdAsync(long id);
    // Task<IEnumerable<ApplicationUser>> FindAllAsync();
    // Task<bool> EmployeeExistsAsync(string userCode);
    // Task<Employee> FindByUserCodeAsync(string userCode);
    // Task<bool> EmployeeExistAsync(string userCode);

    // ========== Queryable ====================================================
    // IQueryable<ApplicationUser> FindAllAsQueryable();
}
