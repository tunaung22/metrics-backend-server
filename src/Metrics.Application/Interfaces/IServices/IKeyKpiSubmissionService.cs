using Metrics.Application.DTOs.KeyKpiSubmissions;
using Metrics.Application.Results;

namespace Metrics.Application.Interfaces.IServices;

public interface IKeyKpiSubmissionService
{
    // =====Create=====
    Task<Result> SubmitSubmissionsAsync(List<CreateKeyKpiSubmissionDto> createDtos);
    // Update
    // Delete

    // =====Retrieve=====
    Task<ResultT<List<KeyKpiSubmissionDto>>> FindByPeriodAsync(long periodId);
    Task<ResultT<List<KeyKpiSubmissionDto>>> FindByPeriodBySubmitterAsync(long periodId, string userId);
    Task<ResultT<List<KeyKpiSubmissionDto>>> FindByPeriodBySubmitterAsync(List<long> periodIds, string userId);
    Task<ResultT<List<KeyKpiSubmissionDto>>> FindByDepartmentKeyMetricsAsync(List<long> departmentKeyMetricIDs);
    // Task<ResultT<List<KpiSubmission>>> FindBySubmitterByPeriodByDepartmentListAsync(
    //     ApplicationUser candidate,
    //     long kpiPeriodId,
    //     List<long> departmentIdList); // find by Submitter & KpiPeriod & Department ID list

    // =====COUNT=====
    // Task<ResultT<long>> FindCountByUserByPeriodAsync(string currentUserId, long kpiPeriodId);
    Task<ResultT<long>> FindCountByPeriodBySubmitterAsync(long kpiPeriodId, string currentUserId);
    Task<ResultT<long>> FindCountByPeriodBySubmitterAsync(List<long> kpiPeriodId, string currentUserId);
    Task<ResultT<Dictionary<long, int>>> FindSubmissionsCountDictByPeriodBySubmitterAsync(List<long> kpiPeriodIds, string currentUserId);
}

