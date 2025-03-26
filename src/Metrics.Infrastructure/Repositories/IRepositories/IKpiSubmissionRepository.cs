using Metrics.Domain.Entities;
using System;

namespace Metrics.Infrastructure.Repositories.IRepositories;

public interface IKpiSubmissionRepository
{
    Task<KpiSubmission> FindBySubmissionDateAsync(DateOnly submissionDate);
    Task<KpiSubmission> FindByIdAsync(long id);
    void Create(KpiSubmission entity);
    void CreateRange(List<KpiSubmission> entities);
    void Update(KpiSubmission entity);
    void Delete(KpiSubmission entity);
    Task<IEnumerable<KpiSubmission>> FindAllAsync();
    Task<bool> KpiSubmissionExistsAsync(DateOnly submissionDate);
    Task<bool> KpiSubmissionExists2Async(long kpiPeriodId, long departmentId, long employeeId);

    // ========== Use IEnumerable types in most cases ==================
    IQueryable<KpiSubmission> FindByIdAsQueryable(long id);
    IQueryable<KpiSubmission> FindAsQueryable(long kpiPeriodId, long departmentId, long employeeId);
    IQueryable<KpiSubmission> FindAllAsQueryable();
}
