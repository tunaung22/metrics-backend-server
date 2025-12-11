using Metrics.Application.Domains;
using Metrics.Application.DTOs.KeyKpiSubmissionConstraints;
using Metrics.Application.Results;

namespace Metrics.Application.Interfaces.IServices;

public interface IKeyKpiSubmissionConstraintService
{
    Task<Result> CreateAsync(KeyKpiSubmissionConstraint keyKpiSubmissionConstraint);
    Task<Result> UpdateAsync(Guid lookupId, KeyKpiSubmissionConstraint keyKpiSubmissionConstraint);
    Task<Result> DeleteAsync(Guid lookupId);
    Task<Result> UnDeleteAsync(Guid lookupId);
    Task<ResultT<List<KeyKpiSubmissionConstraintDto>>> FindBySubmitterDepartmentAsync(
        Guid submitterDepartmentCode);
    Task<ResultT<List<KeyKpiSubmissionConstraintDto>>> FindByPeriodBySubmitterDepartmentAsync(
        long periodId, Guid submitterDepartmenCode);
    Task<ResultT<List<KeyKpiSubmissionConstraintDto>>> FindByDepartmentKeyMetricsAsync(
        List<long> departmentKeyMetricIDs);

    // Task<IEnumerable<KeyKpiSubmissionConstraint>> FindAllByPeriodAndDepartmentAsync(
    //     string CurrentPeriodName,
    //     Guid CurrentDepartmentCode);
    Task<ResultT<Dictionary<long, int>>> FindCountsByPeriodBySubmitterDepartmentAsync(
        List<long> periodIds, long submitterDepartmentId);
    Task<ResultT<List<KeyKpiSubmissionConstraintDto>>> FindByPeriodNameAsync(string sourcePeriodName);
    Task<ResultT<List<KeyKpiSubmissionConstraintDto>>> FindByPeriodAsync(long periodId);

}
