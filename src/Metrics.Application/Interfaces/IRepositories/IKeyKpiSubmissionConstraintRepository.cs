using Metrics.Application.Domains;

namespace Metrics.Application.Interfaces.IRepositories;

public interface IKeyKpiSubmissionConstraintRepository
{
    void Create(KeyKpiSubmissionConstraint entity);
    void Update(KeyKpiSubmissionConstraint entity);
    Task<KeyKpiSubmissionConstraint?> FindByLookupIdAsync(Guid lookupId);

    Task<IEnumerable<KeyKpiSubmissionConstraint>> FindAllByDepartmentAsync(Guid departmentCode);
    Task<IEnumerable<KeyKpiSubmissionConstraint>> FindAllByPeriodAndDepartmentAsync(
        string periodName,
        Guid departmentCode);

    // ========== Queryable ====================================================
    IQueryable<KeyKpiSubmissionConstraint> FindAllAsQueryable();
}
