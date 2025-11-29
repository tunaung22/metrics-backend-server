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
    Task<Result> Create_Async(DepartmentKeyMetricCreateDto createDto);
    Task<Result> CreateAsync(List<DepartmentKeyMetricCreateDto> createDTOs);
    /// <summary>
    /// Copy Department Keys from Source List<DepartmentKeyMetricDto> to Target List<DepartmentKeyMetricDto>
    /// </summary>
    /// <param name="sourceDTOs">List<DepartmentKeyMetricDto></param>
    /// <param name="targetDTOs">List<DepartmentKeyMetricDto></param>
    /// <returns>Result</returns>
    Task<Result> ReplicateAsync(List<DepartmentKeyMetricDto> sourceDTOs, List<DepartmentKeyMetricDto> targetDTOs);
    /// <summary>
    /// Copy Department Keys from Source Period to Target Period
    /// </summary>
    /// <param name="sourcePeriodID"></param>
    /// <param name="targetPeriodID"></param>
    /// <returns></returns>
    Task<Result> CopyAsync(long sourcePeriodID, long targetPeriodID);


    Task<DepartmentKeyMetric> CreateAsync(DepartmentKeyMetric entity);
    Task<DepartmentKeyMetric> UpdateAsync(Guid code, DepartmentKeyMetric entity);

    /// <summary>
    /// DeleteAsync (soft delete)
    /// set the IsDeleted to true
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    /// <exception cref="NotFoundException"></exception>
    /// <exception cref="Exception"></exception>
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
    Task<ResultT<long>> FindCountByPeriodAsync(long periodId);
    Task<ResultT<Dictionary<long, int>>> FindCountsByPeriodAsync(List<long> periodIds);
}
