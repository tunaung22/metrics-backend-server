using Metrics.Application.Domains;
using Metrics.Application.Results;

namespace Metrics.Application.Interfaces.IRepositories;

public interface IDepartmentKeyMetricRepository
{
    Task<IEnumerable<DepartmentKeyMetric>> FindByPeriodByKeyIssueDepartmentAsync(
        long periodId,
        Guid keyIssueDepartmentCode);

    void Create(DepartmentKeyMetric entity);
    void Update(DepartmentKeyMetric entity);
    // void Delete(DepartmentKeyMetric entity);
    Task<IEnumerable<DepartmentKeyMetric>> FindAllAsync();
    Task<IEnumerable<DepartmentKeyMetric>> FindAllAsync(int pageNumber, int pageSize);
    Task<DepartmentKeyMetric?> FindByIdAsync(long id);
    Task<DepartmentKeyMetric?> FindByCodeAsync(Guid departmentKeyMetricCode);
    // Task<DepartmentKeyMetric?> FindByMetricTitleAsync(string metricTitle);
    Task<DepartmentKeyMetric?> FindByPeriodAndDepartmentAndKeyMetricAsync(
        string periodName,
        Guid departmentCode,
        Guid keyMetricCode);
    Task<IEnumerable<DepartmentKeyMetric>> FindAllByPeriodIdAsync(long periodId);
    Task<IEnumerable<DepartmentKeyMetric>> FindAllByPeriodNameAsync(string periodName);
    Task<IEnumerable<DepartmentKeyMetric>> FindAllByPeriodAndDepartmentAsync(
        string periodName,
        Guid departmentCode);
    Task<bool> KeyKpiExistsAsync(Guid metricCode);
    Task<long> FindCountAsync();

    // ========== Count ====================================================
    Task<Dictionary<long, int>> FindCountsByPeriodAsync(List<long> periodIds);

    // ========== Queryable ====================================================
    IQueryable<DepartmentKeyMetric> FindAllAsQueryable();
}
