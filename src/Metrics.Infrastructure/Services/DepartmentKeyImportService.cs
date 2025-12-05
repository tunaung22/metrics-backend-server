using Metrics.Application.Common.Mappers;
using Metrics.Application.Domains;
using Metrics.Application.DTOs.DepartmentKeyMetric;
using Metrics.Application.Interfaces.IServices;
using Metrics.Application.Results;
using Metrics.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Metrics.Infrastructure.Services;

public class DepartmentKeyImportService(
    ILogger<DepartmentKeyImportService> logger,
    MetricsDbContext context) : IDepartmentKeyImportService
{
    private readonly ILogger<DepartmentKeyImportService> _logger = logger;
    private readonly MetricsDbContext _context = context;


    public async Task<Result> CopyAsync(long sourcePeriodID, long targetPeriodID)
    {
        // =====TO INSERT=====
        // 1. take out non existing entries -> insert
        // =====TO UPDATE=====
        // 2. take out deleted      entries -> do undelete
        // 3. take out undeleted    entries -> do delete
        try
        {
            // get dkm by periods
            var sourceDKMs = await _context.DepartmentKeyMetrics.Where(s => s.KpiSubmissionPeriodId == sourcePeriodID && s.IsDeleted == false).ToListAsync();
            var targetDKMs = await _context.DepartmentKeyMetrics.Where(t => t.KpiSubmissionPeriodId == targetPeriodID).ToListAsync();

            var targetIDs = new HashSet<Guid>(targetDKMs.Select(t => t.DepartmentKeyMetricCode));
            var sourceIDs = new HashSet<Guid>(sourceDKMs.Select(s => s.DepartmentKeyMetricCode));

            if (sourceDKMs == null)
                return Result.Fail("Source period not found.", ErrorType.NotFound);

            // ==========to INSERT========================================
            // source data not found in target
            var sourceToInsert = sourceDKMs
                .Where(source =>
                    !targetDKMs.Any(target =>
                        target.DepartmentId == source.DepartmentId &&
                        target.KeyMetricId == source.KeyMetricId)
                ).ToList();

            var entitiesToInsert = sourceToInsert
                .Select(e => new DepartmentKeyMetric
                {
                    KpiSubmissionPeriodId = targetPeriodID, // set Target Period
                    // DepartmentKeyMetricCode // auto-gen
                    DepartmentId = e.DepartmentId,
                    KeyMetricId = e.KeyMetricId,
                    IsDeleted = false,
                }).ToList();

            if (entitiesToInsert.Count > 0) _context.AddRange(entitiesToInsert);

            // ==========to UPDATE========================================
            // source data found in target
            // var sourceToUpdate = sourceDKMs.Where(source =>
            //      targetDKMs.Any(target =>
            //          target.DepartmentId == source.DepartmentId &&
            //          target.KeyMetricId == source.KeyMetricId
            //  ))
            //  .ToList();

            // DELETE records from target not found in source
            var targetToDelete = targetDKMs.Where(target =>
                !sourceDKMs.Any(source =>
                    source.DepartmentId == target.DepartmentId &&
                    source.KeyMetricId == target.KeyMetricId &&
                    source.IsDeleted == false
             ))
            .ToList();
            foreach (var entry in targetToDelete)
            {
                entry.IsDeleted = true;
            }

            // Un-DELETE the deleted records from target found in source
            var targetToUpdate = targetDKMs.Where(target =>
                sourceDKMs.Any(source =>
                    source.DepartmentId == target.DepartmentId &&
                    source.KeyMetricId == target.KeyMetricId &&
                    target.IsDeleted == true
             ))
             .ToList();

            foreach (var entry in targetToUpdate)
            {
                entry.IsDeleted = false;
            }

            // SAVE
            await _context.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while replicating Department Keys.");
            return Result.Fail("An unexpected error while replicating Department Keys. Please try again later.", ErrorType.UnexpectedError);
        }
    }


    // public async Task<Result> ReplicateAsync(List<DepartmentKeyMetricDto> sourceDTOs, List<DepartmentKeyMetricDto> targetDTOs)
    // {
    //     // =====TO INSERT=====
    //     // 1. take out non existing entries -> insert
    //     // =====TO UPDATE=====
    //     // 2. take out deleted      entries -> do undelete
    //     // 3. take out undeleted    entries -> do delete
    //     try
    //     {
    //         var targetLookupIDs = new HashSet<Guid>(targetDTOs.Select(t => t.LookupId));

    //         // =====TO INSERT=====
    //         // 1. take out non existing entries -> insert
    //         var toInsert = sourceDTOs
    //             .Where(source => !targetLookupIDs.Contains(source.LookupId))
    //             .ToList();
    //         var entities = toInsert.Select(e => e.MapToEntity()).ToList();
    //         _context.AddRange(entities);

    //         // =====TO UPDATE=====
    //         var toUpdate = sourceDTOs
    //             .Where(source => targetLookupIDs.Contains(source.LookupId))
    //             .ToList();
    //         // 2. take out deleted      entries -> do undelete
    //         var toUndelete = toUpdate.Where(source => source.IsDeleted == true).ToList();
    //         foreach (var entity in toUndelete)
    //         {

    //         }
    //         // 3. take out undeleted    entries -> do delete
    //         var toDelete = toUpdate.Where(source => source.IsDeleted == false).ToList();





    //         return Result.Success();
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Unexpected error while replicating Department Keys.");
    //         return Result.Fail("An unexpected error while replicating Department Keys. Please try again later.", ErrorType.UnexpectedError);
    //     }
    // }
}

