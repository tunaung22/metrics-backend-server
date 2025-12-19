using Metrics.Application.Domains;

namespace Metrics.Application.Interfaces.IRepositories;

public interface IKeyKpiSubmissionRepository
{
    void Add(KeyKpiSubmission submission);
    Task AddRangeAsync(List<KeyKpiSubmission> submisions);
    // Task UpdateAsync(long id, KeyKpiSubmission submission);
    // void Delete(long id);
    Task DeleteByPeriodByCandidateAsync(long periodId, string candidateId);
    Task<IEnumerable<KeyKpiSubmission>> FindByPeriodByUserAsync(long periodId, string userId);
    // =====Retrieve=====
    Task<KeyKpiSubmission?> FindByIdAsync(long id);
    Task<KeyKpiSubmission?> FindBySubmitterByDepartmentKeyMetricAsync(
        string submitterId, long departmentKeyMetricId);
    Task<List<KeyKpiSubmission>> FindByPeriodAsync(long periodId, bool includeLockedUser = true);
    Task<List<KeyKpiSubmission>> FindBySubmitterAsync(string submitterId); // submissions by user
    Task<List<KeyKpiSubmission>> FindByDepartmentKeyMetricListAsync(List<long> departmentKeyMetricIDs); // submissions by user
    Task<List<KeyKpiSubmission>> FindByPeriodBySubmitterAsync(
        long periodId, string submitterId); // by Period by Submitter
    Task<List<KeyKpiSubmission>> FindByPeriodBySubmitterAsync(
        List<long> periodIds, string submitterId);
    Task<List<KeyKpiSubmission>> FindByPeriodBySubmitterByDepartmentAsync(
        long periodId, string submitterId, long departmentId);

    // =====COUNT=====
    Task<long> FindCountAsync();
    Task<long> FindCountBySubmitterAsync(string submitterId); // by Submitter
    Task<long> FindCountByDepartmentAsync(long departmentId); // by Department
    Task<long> FindCountByPeriodAsync(long periodId); // by Department Key Metric
    Task<long> FindCountByPeriodBySubmitterAsync(long periodId, string submitterId); // by Period by Submitter
    Task<long> FindCountByPeriodBySubmitterAsync(List<long> periodIds, string submitterId);
    Task<Dictionary<long, int>> FindSubmissionCountsByPeriodBySubmitterAsync(List<long> periodIds, string submitterId);

    // void Create(KeyKpiSubmission entity);
    // void CreateRange(List<KpiSubmission> entities);
    // void Update(KpiSubmission entity);
    // void Delete(KpiSubmission entity);
    // Task<KpiSubmission> FindBySubmissionDateAsync(DateOnly submissionDate);
    // Task<KpiSubmission> FindByIdAsync(long id);
    // Task<IEnumerable<KpiSubmission>> FindAllAsync();
    // Task<bool> KpiSubmissionExistsAsync(long kpiPeriodId, long departmentId, string candidateId);

    // ========== Queryable ====================================================
    // IQueryable<KpiSubmission> FindByIdAsQueryable(long id);
    // IQueryable<KpiSubmission> FindAsQueryable(long kpiPeriodId, long departmentId, string candidateId);
    // IQueryable<KeyKpiSubmission> FindAllAsQueryable();
}
