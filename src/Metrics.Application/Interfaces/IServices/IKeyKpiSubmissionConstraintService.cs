using Metrics.Application.Domains;

namespace Metrics.Application.Interfaces.IServices;

public interface IKeyKpiSubmissionConstraintService
{
    Task<KeyKpiSubmissionConstraint> CreateAsync(KeyKpiSubmissionConstraint entity);
    Task<KeyKpiSubmissionConstraint> UpdateAsync(Guid lookupId, KeyKpiSubmissionConstraint entity);
    Task<bool> DeleteAsync(Guid lookupId);
    Task<bool> UnDeleteAsync(Guid lookupId);
    Task<IEnumerable<KeyKpiSubmissionConstraint>> FindAllByDepartmentAsync(
        Guid departmentCode);
    // Task<IEnumerable<KeyKpiSubmissionConstraint>> FindAllByPeriodAndDepartmentAsync(
    //     string CurrentPeriodName,
    //     Guid CurrentDepartmentCode);
}
