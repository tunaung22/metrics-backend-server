using Metrics.Application.Common.Mappers;
using Metrics.Application.Domains;
using Metrics.Application.DTOs.KeyKpiSubmissionConstraints;
using Metrics.Application.Interfaces.IRepositories;
using Metrics.Application.Interfaces.IServices;
using Metrics.Application.Results;
using Metrics.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Metrics.Infrastructure.Services;

public class KeyKpiAssignmentImportService(
    MetricsDbContext context,
    ILogger<KeyKpiAssignmentImportService> logger,
    IDepartmentKeyMetricRepository dkmRepo,
    IKeyKpiSubmissionConstraintRepository keyKpiAssignmentRepo) : IKeyKpiAssignmentImportService
{
    private readonly MetricsDbContext _context = context;
    private readonly ILogger<KeyKpiAssignmentImportService> _logger = logger;
    private readonly IDepartmentKeyMetricRepository _dkmRepo = dkmRepo;
    private readonly IKeyKpiSubmissionConstraintRepository _keyKpiAssignmentRepo = keyKpiAssignmentRepo;

    public async Task<Result> ImportAsync(long periodId, List<KeyKpiSubmissionConstraintImportDto> sourceImportDto)
    // public async Task<Result> ImportAsync(List<KeyKpiSubmissionConstraintImportDto> importDto)
    {
        try
        {
            List<KeyKpiSubmissionConstraint> importList = [];

            // **NOTE** for each sourceImportDto 
            // -> get Target period's dkmId,
            // -> where source's Assignemtns.DKM.IssuerId and Assignemtns.DKM.KeyId 
            // -> matched with target's IssuerId and KeyId 
            foreach (var i in sourceImportDto)
            {
                // construct candidate id + dkm id
                var dkm = await _context.DepartmentKeyMetrics
                    .Where(k => k.IsDeleted == false &&
                        k.KpiSubmissionPeriodId == periodId &&
                        k.DepartmentId == i.KeyIssueDepartmentId &&
                        k.KeyMetricId == i.KeyMetricId)
                    .FirstOrDefaultAsync();
                if (dkm != null && dkm.Id > 0)
                {
                    importList.Add(new KeyKpiSubmissionConstraint
                    {
                        CandidateDepartmentId = i.CandidateDepartmentId,
                        DepartmentKeyMetricId = dkm.Id,
                        IsDeleted = false,
                    });
                }
            }

            // get existing assignments (both active and deleted)
            var existingAssignments = await _context.KeyKpiSubmissionConstraints
                .Where(assignment => assignment.DepartmentKeyMetric.KpiSubmissionPeriodId == periodId)
                .ToListAsync();

            // ==========to INSERT========================================
            // insert: importList - existingAssignment
            var insertEntities = importList.Where(item =>
                !existingAssignments.Any(existing =>
                    existing.CandidateDepartmentId == item.CandidateDepartmentId &&
                    existing.DepartmentKeyMetricId == item.DepartmentKeyMetricId
                )).ToList();
            if (insertEntities.Count > 0)
                _context.KeyKpiSubmissionConstraints.AddRange(insertEntities);

            // ==========to UPDATE========================================
            // DELETE
            // active assignments not in importList
            var toDelete = existingAssignments.Where(existing => existing.IsDeleted == false &&
                !importList.Any(importItem =>
                    importItem.CandidateDepartmentId == existing.CandidateDepartmentId &&
                    importItem.DepartmentKeyMetricId == existing.DepartmentKeyMetricId)
                ).ToList();
            foreach (var item in toDelete)
            {
                item.IsDeleted = true;
            }

            // Un-DELETE the deleted records from target found in source
            // to undelete: find deleted assignments in importList
            var toUndelete = existingAssignments.Where(existing => existing.IsDeleted == true &&
                importList.Any(importItem =>
                    importItem.CandidateDepartmentId == existing.CandidateDepartmentId &&
                    importItem.DepartmentKeyMetricId == existing.DepartmentKeyMetricId)
                ).ToList();
            foreach (var item in toUndelete)
            {
                item.IsDeleted = false;
            }

            // SAVE
            await _context.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError("Unexpected error occured at importing department key assignments. {msg}", ex.Message);
            return Result.Fail("Failed to import department key assignments.", ErrorType.UnexpectedError);
        }
    }

    // Compare Source's assignments with Target's DKMs
    // 1. GET Source Assignment's Issuer ID + Key ID              as SourceAssignmentKeys/A
    // 2. GET Target DepartmentKeyMetrics's Issuer ID + Key ID    as TargetDKMs/B
    // 3. GET SourceAssignmentKeys in TargetDKMs                  as KeysToImport
    // 4. INSERT KeysToImport into Target Assignments

    public async Task<ResultT<List<KeyKpiSubmissionConstraintDto>>> FindAssignmentsToImportByPeriod(
        long sourcePeriodId,
        long targetPeriodId)
    {
        try
        {
            // Source => KeyAssignments 
            // | Candidate ID | Department Key Metric ID |
            var sourceAssignments = await _keyKpiAssignmentRepo.FindByPeriodAsync(sourcePeriodId);

            // Target => DepartmentKeys 
            // | Issuer ID | Key ID |
            var targetDkms = await _dkmRepo.FindByPeriodAsync(targetPeriodId);

            // Target.DKM.DepartmentId == Source.DKM.DepartmentId
            var assignements = sourceAssignments
                .Where(source => targetDkms.Any(target =>
                    target.DepartmentId == source.DepartmentKeyMetric.DepartmentId &&
                    target.KeyMetricId == source.DepartmentKeyMetric.KeyMetricId)
                ).ToList();

            var data = assignements.Select(a => a.MapToDto()).ToList();
            return ResultT<List<KeyKpiSubmissionConstraintDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError("Unexpected error occured while preparing for Import Department Keys Assignments By Period. {msg}", ex.Message);
            return ResultT<List<KeyKpiSubmissionConstraintDto>>.Fail("Unexpected error occured while preparing for Import Department Keys Assignments By Period.", ErrorType.UnexpectedError);
        }
    }
}
