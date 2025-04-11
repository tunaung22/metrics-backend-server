using Metrics.Application.Entities;

namespace Metrics.Application.Interfaces.IRepositories;

public interface IKpiPeriodRepository
{
    void Create(KpiPeriod entity);
    void Update(KpiPeriod entity);
    void Delete(KpiPeriod entities);
    Task<KpiPeriod> FindByIdAsync(long id);
    Task<KpiPeriod> FindByPeriodNameAsync(string periodName);
    Task<IEnumerable<KpiPeriod>> FindAllAsync();
    Task<bool> KpiPeriodExistsAsync(string periodName);
    Task<long> FindCountAsync();



    // Task<KpiPeriod?> FindByPeriodName(string periodname);
    // Task<bool> IsPeriodNameExist(string periodName);

    // ========== Queryable ====================================================
    IQueryable<KpiPeriod> FindAllAsQueryable();

}
