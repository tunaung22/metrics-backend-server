using Metrics.Application.DTOs.DepartmentDtos;
using Metrics.Application.DTOs.KpiSubmissionDtos;
using System;

namespace Metrics.Application.Services.IServices;

public interface IKpiSubmissionService
{
    Task<KpiSubmissionGetDto> FindBySubmissionDate_Async(DateOnly submissionDate);
    // Task<IEnumerable<Department>> FindAsync(string keyword); // Search feature
    Task<KpiSubmissionGetDto> FindById_Async(long id);
    Task<KpiSubmissionGetDto> Create_Async(KpiSubmissionCreateDto createDto);
    Task<int> CreateRange_Async(List<KpiSubmissionCreateDto> createDtos);
    Task<KpiSubmissionGetDto> Update_Async(DateOnly submissionDate, KpiSubmissionUpdateDto updateDto);
    Task<bool> Delete_Async(DateOnly submissionDate);
    Task<IEnumerable<KpiSubmissionGetDto>> FindAll_Async();
    Task<KpiSubmissionGetDto?> Find_Async(long employeeId, long kpiPeriodId, long departmentId);
    Task<IEnumerable<KpiSubmissionDto>> Find_Async(long employeeId, long kpiPeriodId, List<long> departmentIds);
    Task<IEnumerable<KpiSubmissionDto>> FindAllInsecure_Async(long employeeId, long kpiPeriodId, long departmentId);
    Task<IEnumerable<KpiSubmissionDto>> FindAllInsecure_Async();

    // Task<IEnumerable<DepartmentDto>> FindAllInsecure_Async();
}
