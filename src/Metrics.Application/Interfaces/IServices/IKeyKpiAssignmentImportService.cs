using Metrics.Application.DTOs.KeyKpiSubmissionConstraints;
using Metrics.Application.Results;

namespace Metrics.Application.Interfaces.IServices;

/// <summary>
/// Import service for Department Key Metrics and Department Key Assignments
/// </summary>
public interface IKeyKpiAssignmentImportService
{
    Task<ResultT<List<KeyKpiSubmissionConstraintDto>>> FindAssignmentsToImportByPeriod(
        long sourcePeriodId,
        long targetPeriodId);
    // Task<ResultT<List<KeyKpiSubmissionConstraintDto>>> PrepareImportAssignmentsByPeriod(string sourcePeriodName, string targetPeriodName);
    // Task<Result> ImportAsync(
    //     long sourcePeriodId,
    //     long targetPeriodId);
    Task<Result> ImportAsync(
        // long periodId, -> get period from assignements.DKM.period
        long periodId,
        List<KeyKpiSubmissionConstraintImportDto> sourceImportDto);
    // Task<Result> ImportAsync(
    //     long periodId,
    //     long candidateDepartmentId,
    //     long keyIssueDepartmentId,
    //     long keyId);
}
