using Metrics.Domain.Entities;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrics.Infrastructure.Repositories.IRepositories;

public interface IDepartmentRepository //: IGenericRepository<Department>
{
    Task<Department> FindByDepartmentCodeAsync(string departmentCode);
    Task<Department> FindByIdAsync(long id);
    void Create(Department entity);
    void Update(Department entity);
    void Delete(Department entity);
    Task<IEnumerable<Department>> FindAllAsync();
    Task<bool> DepartmentExistsAsync(string departmentCode);
    Task<int> FindCountAsync();

    // ========== Experimental ========================================
    // [Obsolete("Experimental! Use FindAllAsync() instead.", false)]
    IQueryable<Department> FindAllAsQueryable();
}
