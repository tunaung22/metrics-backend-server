using Metrics.Application.Domains;

namespace Metrics.Application.Interfaces.IServices;

public interface IDepartmentKeyMetricService
{
    Task<DepartmentKeyMetric> CreateAsync(DepartmentKeyMetric entity);
    Task<DepartmentKeyMetric> UpdateAsync(Guid code, DepartmentKeyMetric entity);
    Task<bool> DeleteAsync(Guid departmentKeyMetricCode);
    Task<DepartmentKeyMetric?> FindByIdAsync(long id);
    Task<DepartmentKeyMetric?> FindByCodeAsync(Guid departmentKeyMetricCode);
    // Task<DepartmentKeyMetric?> FindByMetricTitleAsync(string metricTitle);
    Task<IEnumerable<DepartmentKeyMetric>> FindByPeriodIdAsync(long periodId);
    Task<IEnumerable<DepartmentKeyMetric>> FindAllAsync();
    Task<IEnumerable<DepartmentKeyMetric>> FindAllAsync(int pageNumber = 1, int pageSize = 20);
    Task<long> FindCountAsync();
}
