using Metrics.Application.DTOs.KpiSubmissionDtos;
using Metrics.Application.Domains;

namespace Metrics.Application.Interfaces.IServices;

public interface IKpiSubmissionService
{
    // ========== Return Entity ================================================
    Task<KpiSubmission> CreateAsync(KpiSubmission submission);
    Task<int> CreateRangeAsync(List<KpiSubmission> submissions);
    Task<KpiSubmission> UpdateAsync(DateOnly submissionDate, KpiSubmission submission);
    Task<bool> DeleteBySubmissionDateAsync(DateOnly submissionDate);
    Task<KpiSubmission> FindByIdAsync(long id);
    Task<KpiSubmission> FindBySubmissionDateAsync(DateOnly submissionDate);
    Task<List<KpiSubmission>> FindBySubmitterAndKpiPeriodAndDepartmentListAsync(ApplicationUser candidate, long kpiPeriodId, List<long> departmentIdList); // find by Employee & KpiPeriod & Department ID list
    Task<List<KpiSubmission>> FindByKpiPeriodAndDepartmentListAsync(long kpiPeriodId, List<long> departmentIdList); // find by KpiPeriod & Department ID List
    Task<List<KpiSubmission>> FindByKpiPeriodAndDepartmentAsync(long kpiPeriodId, long departmentId); // find by KpiPeriod+Department
    // Task<List<KpiSubmission>> FindByKpiPeriodByDepartmentByUserGroupAsync(long kpiPeriodId, long departmentId, long userTitleId); // find by KpiPeriod+Department+UserGroup
    Task<IEnumerable<KpiSubmission>> FindAllAsync();
    // Task<IEnumerable<Department>> FindAsync(string keyword); // Search feature
    Task<long> FindCountByUserIdByKpiPeriodIdAsync(string currentUserId, long kpiPeriodId);

    // ========== Return DTO ===================================================
    // Task<KpiSubmissionGetDto> Create_Async(KpiSubmissionCreateDto createDto);
    // Task<int> CreateRange_Async(List<KpiSubmissionCreateDto> createDtos);
    // Task<KpiSubmissionGetDto> Update_Async(DateOnly submissionDate, KpiSubmissionUpdateDto updateDto);
    // Task<bool> Delete_Async(DateOnly submissionDate);
    // Task<KpiSubmissionGetDto> FindById_Async(long id);
    // Task<KpiSubmissionGetDto> FindBySubmissionDate_Async(DateOnly submissionDate);
    // Task<IEnumerable<KpiSubmissionDto>> Find_Async(long employeeId, long kpiPeriodId, List<long> departmentIds);
    // Task<KpiSubmissionGetDto?> Find_Async(long employeeId, long kpiPeriodId, long departmentId);
    // Task<IEnumerable<KpiSubmissionGetDto>> FindAll_Async();
    // Task<IEnumerable<KpiSubmissionDto>> FindAllInsecure_Async(long employeeId, long kpiPeriodId, long departmentId);
    // Task<IEnumerable<KpiSubmissionDto>> FindAllInsecure_Async();
    // Task<IEnumerable<Department>> FindAsync(string keyword); // Search feature
}
