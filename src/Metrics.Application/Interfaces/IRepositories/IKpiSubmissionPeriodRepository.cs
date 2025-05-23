using Metrics.Application.Domains;

namespace Metrics.Application.Interfaces.IRepositories;

public interface IKpiSubmissionPeriodRepository
{
    void Create(KpiSubmissionPeriod entity);
    void Update(KpiSubmissionPeriod entity);
    void Delete(KpiSubmissionPeriod entities);
    Task<KpiSubmissionPeriod> FindByIdAsync(long id);
    Task<KpiSubmissionPeriod> FindByPeriodNameAsync(string periodName);
    Task<IEnumerable<KpiSubmissionPeriod>> FindAllAsync();
    Task<bool> KpiPeriodExistsAsync(string periodName);
    Task<long> FindCountAsync();



    // Task<KpiPeriod?> FindByPeriodName(string periodname);
    // Task<bool> IsPeriodNameExist(string periodName);

    // ========== Queryable ====================================================
    IQueryable<KpiSubmissionPeriod> FindAllAsQueryable();

}
