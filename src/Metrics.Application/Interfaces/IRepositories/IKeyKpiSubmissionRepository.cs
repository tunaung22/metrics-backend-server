using Metrics.Application.Domains;

namespace Metrics.Application.Interfaces.IRepositories;

public interface IKeyKpiSubmissionRepository
{
    // void Create(KeyKpiSubmission entity);
    // void CreateRange(List<KpiSubmission> entities);
    // void Update(KpiSubmission entity);
    // void Delete(KpiSubmission entity);
    // Task<KpiSubmission> FindBySubmissionDateAsync(DateOnly submissionDate);
    // Task<KpiSubmission> FindByIdAsync(long id);
    // Task<IEnumerable<KpiSubmission>> FindAllAsync();
    // Task<bool> KpiSubmissionExistsAsync(long kpiPeriodId, long departmentId, string candidateId);
    Task<long> FindCountByUserByPeriodAsync(string currentUserId, long kpiPeriodId);

    // ========== Queryable ====================================================
    // IQueryable<KpiSubmission> FindByIdAsQueryable(long id);
    // IQueryable<KpiSubmission> FindAsQueryable(long kpiPeriodId, long departmentId, string candidateId);
    IQueryable<KeyKpiSubmission> FindAllAsQueryable();
}
