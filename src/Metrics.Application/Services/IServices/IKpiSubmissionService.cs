using Metrics.Application.DTOs.KpiSubmissionDtos;
using System;

namespace Metrics.Application.Services.IServices;

public interface IKpiSubmissionService
{
    Task<KpiSubmissionGetDto> FindBySubmissionDate_Async(DateOnly submissionDate);
    // Task<IEnumerable<Department>> FindAsync(string keyword); // Search feature
    Task<KpiSubmissionGetDto> FindById_Async(long id);
    Task<KpiSubmissionGetDto> Create_Async(KpiSubmissionCreateDto createDto);
    Task<List<KpiSubmissionDto>> CreateRange_Async(List<KpiSubmissionCreateDto> createDtos);
    Task<KpiSubmissionGetDto> Update_Async(DateOnly submissionDate, KpiSubmissionUpdateDto updateDto);
    Task<bool> Delete_Async(DateOnly submissionDate);
    Task<IEnumerable<KpiSubmissionGetDto>> FindAll_Async();
    // Task<IEnumerable<DepartmentDto>> FindAllInsecure_Async();
}
