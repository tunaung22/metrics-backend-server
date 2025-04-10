using Metrics.Application.Entities;

namespace Metrics.Application.Interfaces.IRepositories;

public interface IKpiSubmissionRepository
{
    void Create(KpiSubmission entity);
    void CreateRange(List<KpiSubmission> entities);
    void Update(KpiSubmission entity);
    void Delete(KpiSubmission entity);
    Task<KpiSubmission> FindBySubmissionDateAsync(DateOnly submissionDate);
    Task<KpiSubmission> FindByIdAsync(long id);
    Task<IEnumerable<KpiSubmission>> FindAllAsync();
    Task<bool> KpiSubmissionExistsAsync(long employeeId, long kpiPeriodId, long departmentId);


    // ========== Queryable ====================================================
    IQueryable<KpiSubmission> FindByIdAsQueryable(long id);
    IQueryable<KpiSubmission> FindAsQueryable(long employeeId, long kpiPeriodId, long departmentId);
    IQueryable<KpiSubmission> FindAllAsQueryable();
}
