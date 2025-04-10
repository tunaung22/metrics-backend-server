using Metrics.Application.DTOs.KpiSubmissionDtos;
using Metrics.Application.Entities;

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
    Task<List<KpiSubmission>> FindByEmployeeAndKpiPeriodAndDepartmentListAsync(long employeeId, long kpiPeriodId, List<long> departmentIds); // find by Employee & KpiPeriod & Department
    Task<IEnumerable<KpiSubmission>> FindAllAsync();
    // Task<IEnumerable<Department>> FindAsync(string keyword); // Search feature


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
