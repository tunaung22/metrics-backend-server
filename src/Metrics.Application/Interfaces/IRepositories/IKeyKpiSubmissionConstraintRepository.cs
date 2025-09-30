using Metrics.Application.Domains;

namespace Metrics.Application.Interfaces.IRepositories;

public interface IKeyKpiSubmissionConstraintRepository
{
    void Create(KeyKpiSubmissionConstraint entity);
    void Update(KeyKpiSubmissionConstraint entity);
    Task<KeyKpiSubmissionConstraint?> FindByLookupIdAsync(
        Guid lookupId);

    Task<IEnumerable<KeyKpiSubmissionConstraint>> FindBySubmitterDepartmentAsync(
        Guid departmentCode);
    Task<IEnumerable<KeyKpiSubmissionConstraint>> FindByPeriodAndSubmitterDepartmentAsync(
        long periodId,
        Guid departmentCode);

    Task<IEnumerable<KeyKpiSubmissionConstraint>> FindByDepartmentKeyMetricsAsync(List<long> departmentKeyMetricIDs);


    // ========== Queryable ====================================================
    IQueryable<KeyKpiSubmissionConstraint> FindAllAsQueryable();


    Task<Dictionary<long, int>> FindCountsByPeriodBySubmitterDepartmentAsync(
        List<long> periodIds, long submitterDepartmentId);
}
