using Metrics.Application.Common.Mappers;
using Metrics.Application.Domains;
using Metrics.Application.DTOs.KeyKpiSubmissionConstraints;
using Metrics.Application.Interfaces.IRepositories;
using Metrics.Application.Interfaces.IServices;
using Metrics.Application.Results;
using Metrics.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Metrics.Infrastructure.Services;

public class KeyKpiSubmissionConstraintService(
    MetricsDbContext context,
    ILogger<KeyKpiSubmissionConstraintService> logger,
    IKeyKpiSubmissionConstraintRepository keyKpiSubmissionConstraintRepository) : IKeyKpiSubmissionConstraintService
{
    private readonly MetricsDbContext _context = context;
    private readonly ILogger<KeyKpiSubmissionConstraintService> _logger = logger;
    private readonly IKeyKpiSubmissionConstraintRepository _keyKpiSubmissionConstraintRepository = keyKpiSubmissionConstraintRepository;


    public async Task<Result> CreateAsync(
        KeyKpiSubmissionConstraint keyKpiSubmissionConstraint)
    {
        try
        {
            _keyKpiSubmissionConstraintRepository.Create(keyKpiSubmissionConstraint);
            await _context.SaveChangesAsync();

            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
            {
                _logger.LogError(ex, "Department Key KPI Constraint already exist. {msg}", pgEx.MessageText);
                return Result.Fail("Department Key KPI Constraint already exist.", ErrorType.DuplicateKey);
            }
            else
            {
                _logger.LogError(ex, "Database error occured while creating department key kpi constraint. {msg}", ex.Message);
                return Result.Fail("A database error occurred while creating department key kpi constraint.", ErrorType.DatabaseError);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating department key kpi constriant. {msg}", ex.Message);
            return Result.Fail("An unexpected error occurred. Please try again later.", ErrorType.UnexpectedError);
        }
    }

    public Task<Result> UpdateAsync(
        Guid lookupId, KeyKpiSubmissionConstraint keyKpiSubmissionConstraint)
    {
        throw new NotImplementedException();
    }

    public async Task<Result> DeleteAsync(Guid lookupId)
    {
        try
        {
            var targetConstraint = await _keyKpiSubmissionConstraintRepository.FindByLookupIdAsync(lookupId);
            if (targetConstraint == null)
                return Result.Fail("Department key kpi constraint to delete was not found.", ErrorType.NotFound);
            targetConstraint.IsDeleted = true;

            await _context.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting Key Kpi Submission Constraint. {msg}", ex.Message);
            return Result.Fail("Failed to delete department key kpi constraint.", ErrorType.UnexpectedError);
        }
    }

    public async Task<Result> UnDeleteAsync(Guid lookupId)
    {
        try
        {
            var targetConstraint = await _keyKpiSubmissionConstraintRepository
                            .FindByLookupIdAsync(lookupId);
            if (targetConstraint == null)
                return Result.Fail("Department key kpi constraint to undelete was not found.", ErrorType.NotFound);

            targetConstraint.IsDeleted = false;
            await _context.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while undeleting Key Kpi Submission Constraint.");
            return Result.Fail("Failed to undelet department key kpi constraint.", ErrorType.UnexpectedError);
        }
    }

    // public async Task<IEnumerable<KeyKpiSubmissionConstraint>> FindAllByPeriodAndDepartmentAsync(
    //     string CurrentPeriodName,
    //     Guid CurrentDepartmentCode)
    // {
    //     try
    //     {
    //         // var result = await _keyKpiSubmissionConstraintRepository
    //         //     .FindAllByPeriodAndDepartmentAsync(CurrentPeriodName, CurrentDepartmentCode);
    //         var result = await _keyKpiSubmissionConstraintRepository
    //             .FindAllByDepartmentAsync(CurrentDepartmentCode);

    //         return result;
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Unexpected error while querying Department Key Metric Submission Constraints by period, department.");
    //         throw new Exception("An unexpected error occurred. Please try again later.");
    //     }
    // }
    public async Task<ResultT<List<KeyKpiSubmissionConstraintDto>>> FindBySubmitterDepartmentAsync(Guid departmentCode)
    {
        try
        {
            var data = await _keyKpiSubmissionConstraintRepository
                .FindBySubmitterDepartmentAsync(departmentCode);
            var result = data.Select(e => e.MapToDto()).ToList();

            return ResultT<List<KeyKpiSubmissionConstraintDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while undeleting Key Kpi Submission Constraint.");
            return ResultT<List<KeyKpiSubmissionConstraintDto>>.Fail("Failed to undelete department key kpi constraint.", ErrorType.UnexpectedError);
        }
    }

    public async Task<ResultT<List<KeyKpiSubmissionConstraintDto>>> FindByPeriodBySubmitterDepartmentAsync(
        long periodId, Guid departmenCode)
    {
        // find all submission constraints// by department (submitter's)// by period
        try
        {
            var data = await _keyKpiSubmissionConstraintRepository
                .FindByPeriodAndSubmitterDepartmentAsync(periodId, departmenCode);
            var result = data.Select(e => e.MapToDto()).ToList();

            // var result = await _context.KeyKpiSubmissionConstraints
            //     .Where(cx =>
            //         cx.DepartmentId == departmentId
            //         && cx.DepartmentKeyMetric.KpiSubmissionPeriod.Id == periodId)
            //     .OrderBy(cx => cx.Department.DepartmentName)
            //     .Include(cx => cx.Department)
            //     .Include(cx => cx.DepartmentKeyMetric)
            //         .ThenInclude(cx => cx.KeyIssueDepartment)
            //     .Include(cx => cx.DepartmentKeyMetric)
            //         .ThenInclude(cx => cx.KpiSubmissionPeriod)
            //     .Include(cx => cx.DepartmentKeyMetric)
            //         .ThenInclude(cx => cx.KeyMetric)
            //     .ToListAsync();

            return ResultT<List<KeyKpiSubmissionConstraintDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occured while fetching " +
                "department key metric submission constraints by period id by department code. {msg}", ex.Message);
            return ResultT<List<KeyKpiSubmissionConstraintDto>>.Fail("Failed to " +
                "fetch department key kpi submission constraints by period by submitter department.", ErrorType.UnexpectedError);
        }
    }

    public async Task<ResultT<List<KeyKpiSubmissionConstraintDto>>> FindByDepartmentKeyMetricsAsync(List<long> departmentKeyMetricIDs)
    {
        try
        {
            if (departmentKeyMetricIDs == null)
                return ResultT<List<KeyKpiSubmissionConstraintDto>>.Fail("Argument cannot be null.", ErrorType.InvalidArgument);


            if (departmentKeyMetricIDs.Count == 0)
                return ResultT<List<KeyKpiSubmissionConstraintDto>>.Success([]);

            var data = await _keyKpiSubmissionConstraintRepository.FindByDepartmentKeyMetricsAsync(departmentKeyMetricIDs);
            var result = data.Select(e => e.MapToDto()).ToList();

            return ResultT<List<KeyKpiSubmissionConstraintDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch department key metric count by Periods by Submitter.");
            return ResultT<List<KeyKpiSubmissionConstraintDto>>.Fail("Argument cannot be null.", ErrorType.InvalidArgument);
        }
    }

    public async Task<ResultT<Dictionary<long, int>>> FindCountsByPeriodBySubmitterDepartmentAsync(
        List<long> periodIds, long submitterDepartmentId)
    {
        try
        {
            var countResult = await _keyKpiSubmissionConstraintRepository
                .FindCountsByPeriodBySubmitterDepartmentAsync(periodIds, submitterDepartmentId);

            return ResultT<Dictionary<long, int>>.Success(countResult);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to count submission constraint by periods by submitter department. {msg}", ex.Message);
            return ResultT<Dictionary<long, int>>.Fail("Failed to count submission constraint by periods by submitter department.", ErrorType.UnexpectedError);
        }
    }

    public async Task<ResultT<List<KeyKpiSubmissionConstraintDto>>> FindByPeriodNameAsync(string sourcePeriodName)
    {
        try
        {
            var data = await _context.KeyKpiSubmissionConstraints
                .AsNoTracking()
                .Where(c => c.DepartmentKeyMetric.KpiSubmissionPeriod.PeriodName == sourcePeriodName)
                .Include(c => c.DepartmentKeyMetric)
                .Include(c => c.DepartmentKeyMetric).ThenInclude(dkm => dkm.KpiSubmissionPeriod)
                .Include(c => c.DepartmentKeyMetric).ThenInclude(dkm => dkm.KeyIssueDepartment)
                .Include(c => c.DepartmentKeyMetric).ThenInclude(dkm => dkm.KeyMetric)
                .Include(c => c.SubmitterDepartment)
                .OrderBy(c => c.SubmitterDepartment.DepartmentName)
                .ToListAsync();

            return ResultT<List<KeyKpiSubmissionConstraintDto>>.Success(data.Select(d => d.MapToDto()).ToList());
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to fetch submission constraint by period name. {msg}", ex.Message);
            return ResultT<List<KeyKpiSubmissionConstraintDto>>.Fail("Failed to fetch submission constraint by period name.", ErrorType.UnexpectedError);
        }
    }

    public async Task<Result> CopyAsync(long sourcePeriodId, long targetPeriodId)
    {
        // =====TO INSERT=====
        // 1. take out non existing entries -> insert
        // =====TO UPDATE=====
        // 2. take out deleted      entries -> do undelete
        // 3. take out undeleted    entries -> do delete
        try
        {
            var sourceData = await _context.KeyKpiSubmissionConstraints
                .Where(s => s.DepartmentKeyMetric.KpiSubmissionPeriodId == sourcePeriodId
                    && s.IsDeleted == false)
                .ToListAsync();
            var targetData = await _context.KeyKpiSubmissionConstraints
                .Where(t => t.DepartmentKeyMetric.KpiSubmissionPeriodId == targetPeriodId)
                .ToListAsync();

            var sourceIDs = new HashSet<Guid>(sourceData.Select(s => s.LookupId));
            var targetIDs = new HashSet<Guid>(targetData.Select(t => t.LookupId));

            if (sourceData == null)
                return Result.Fail("Source period not found.", ErrorType.NotFound);

            // ==========to INSERT========================================
            var sourceToInsert = sourceData
                .Where(source =>
                    !targetData.Any(target =>
                        target.DepartmentId == source.DepartmentId && // submitter
                        target.DepartmentKeyMetricId == source.DepartmentKeyMetricId)
                // **note: sourceData has includes/filtered by Period
                // target.DepartmentKeyMetric.KpiSubmissionPeriodId == source.DepartmentKeyMetric.KpiSubmissionPeriodId)
                ).ToList();

            var entitiesToInsert = sourceToInsert
               .Select(e => new KeyKpiSubmissionConstraint
               {
                   DepartmentId = e.DepartmentId,
                   //    DepartmentKeyMetricId = e.DepartmentKeyMetricId,
                   //    dkm with target period
                   IsDeleted = false,
               }).ToList();

            if (entitiesToInsert.Count > 0) _context.AddRange(entitiesToInsert);

            // ==========to UPDATE========================================
            // DELETE records from target not found in source
            var targetToDelete = targetData
                .Where(target =>
                    !sourceData.Any(source =>
                        source.DepartmentId == target.DepartmentId &&
                        source.DepartmentKeyMetricId == target.DepartmentKeyMetricId &&
                        source.IsDeleted == false
                )).ToList();

            foreach (var entry in targetToDelete)
            {
                entry.IsDeleted = true;
            }

            // Un-DELETE the deleted records from target found in source
            var targetToUpdate = targetData
                .Where(target =>
                    sourceData.Any(source =>
                        source.DepartmentId == target.DepartmentId &&
                        source.DepartmentKeyMetricId == target.DepartmentKeyMetricId &&
                        target.IsDeleted == true
                )).ToList();

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
            _logger.LogError("Unexpected error while replicating Key KPI Submission Constraints. {msg}", ex.Message);
            return Result.Fail("An unexpected error while replicating key kpi submission constraints. Please try again later.", ErrorType.UnexpectedError);
        }
    }


    // public async Task<IEnumerable<KeyKpiSubmissionConstraint>> FindByDepartmentAsync(
    // Guid departmentCode)
    // {
    //     try
    //     {
    //         // var result = await _keyKpiSubmissionConstraintRepository
    //         //     .FindAllByPeriodAndDepartmentAsync(CurrentPeriodName, CurrentDepartmentCode);
    //         // var result = await _keyKpiSubmissionConstraintRepository
    //         //     .FindAllByDepartmentAsync(departmentCode);

    //         // No Repository
    //         // var result = await _context.KeyKpiSubmissionConstraints
    //         //     .Where(k => k.Department.DepartmentCode == departmentCode)
    //         //     .OrderBy(k => k.Department.DepartmentName)
    //         //     .Include(k => k.Department)
    //         //     .Include(k => k.DepartmentKeyMetric)
    //         //         .ThenInclude(k => k.KpiSubmissionPeriod)
    //         //     // .Include(k => k.DepartmentKeyMetric.KpiSubmissionPeriod)
    //         //     .Include(k => k.DepartmentKeyMetric)
    //         //         .ThenInclude(k => k.KeyMetric)
    //         //     // .Include(k => k.DepartmentKeyMetric.KeyMetric)
    //         //     .ToListAsync();

    //         var departmentId = await _context.Departments
    //             .Where(d => d.DepartmentCode == departmentCode)
    //             .Select(d => d.Id)
    //             .FirstOrDefaultAsync();

    //         var result = await _context.KeyKpiSubmissionConstraints
    //             .Where(k => k.DepartmentId == departmentId)
    //             .OrderBy(k => k.Department.DepartmentName)
    //             .Include(k => k.Department)
    //             .Include(k => k.DepartmentKeyMetric)
    //                 .ThenInclude(k => k.KpiSubmissionPeriod)
    //             .Include(k => k.DepartmentKeyMetric)
    //                 .ThenInclude(k => k.KeyMetric)
    //             .ToListAsync();

    //         return result;
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Unexpected error while querying Department Key Metric Submission Constraints by department.");
    //         throw new Exception("An unexpected error occurred. Please try again later.");
    //     }
    // }

}
