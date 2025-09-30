using Metrics.Application.Domains;
using Metrics.Application.DTOs.DepartmentKeyMetric;
using Metrics.Application.Results;

namespace Metrics.Application.Interfaces.IServices;

public interface IDepartmentKeyMetricService
{
    Task<ResultT<List<DepartmentKeyMetricDto>>> FindByPeriodIdAsync(long periodId);
    Task<ResultT<List<DepartmentKeyMetricDto>>> FindByPeriodByKeyIssueDepartmentAsync(
        long periodId,
        Guid keyIssueDepartmentCode);




    Task<DepartmentKeyMetric> CreateAsync(DepartmentKeyMetric entity);
    Task<DepartmentKeyMetric> UpdateAsync(Guid code, DepartmentKeyMetric entity);
    Task<bool> DeleteAsync(Guid departmentKeyMetricCode);
    Task<bool> UnDeleteAsync(Guid departmentKeyMetricCode);
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
        string CurrentPeriodName,
        Guid CurrentDepartmentCode);
    Task<IEnumerable<DepartmentKeyMetric>> FindAllAsync();
    Task<IEnumerable<DepartmentKeyMetric>> FindAllAsync(int pageNumber = 1, int pageSize = 20);
    Task<long> FindCountAsync();

    Task<ResultT<List<DepartmentKeyMetricDto>>> FindByPeriodNameAsync(string periodName);

    Task<ResultT<Dictionary<long, int>>> FindCountsByPeriodAsync(List<long> periodIds);
}
