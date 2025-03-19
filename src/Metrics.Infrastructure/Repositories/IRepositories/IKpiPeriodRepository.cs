using Metrics.Domain.Entities;

namespace Metrics.Infrastructure.Repositories.IRepositories;

public interface IKpiPeriodRepository // : IGenericRepository<KpiPeriod>
{
    Task<KpiPeriod> FindByPeriodNameAsync(string periodName);
    Task<KpiPeriod> FindByIdAsync(long id);
    void Create(KpiPeriod entity);
    void Update(KpiPeriod enitiy);
    void Delete(KpiPeriod enitie);
    Task<IEnumerable<KpiPeriod>> FindAllAsync();
    Task<bool> KpiPeriodExistsAsync(string periodName);



    // Task<KpiPeriod?> FindByPeriodName(string periodname);

    // Task<bool> IsPeriodNameExist(string periodName);
    IQueryable<KpiPeriod> FindAllAsQueryable();

}
