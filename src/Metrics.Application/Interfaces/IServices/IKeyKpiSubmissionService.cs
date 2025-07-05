using Metrics.Application.Domains;

namespace Metrics.Application.Interfaces.IServices;

public interface IKeyKpiSubmissionService
{

    Task<List<KeyKpiSubmission>> FindBySubmitterByPeriodByDepartmentListAsync(
        ApplicationUser candidate,
        long kpiPeriodId,
        List<long> departmentIdList); // find by Employee & KpiPeriod & Department ID list
    Task<long> FindCountByUserByPeriodAsync(string currentUserId, long kpiPeriodId);
}
