using Metrics.Application.Common.Mappers;
using Metrics.Application.Domains;
using Metrics.Application.DTOs.DepartmentKeyMetric;
using Metrics.Application.Exceptions;
using Metrics.Application.Interfaces;
using Metrics.Application.Interfaces.IRepositories;
using Metrics.Application.Interfaces.IServices;
using Metrics.Application.Results;
using Metrics.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Metrics.Infrastructure.Services;

public class DepartmentKeyMetricService : IDepartmentKeyMetricService
{
    private readonly MetricsDbContext _context;
    private readonly ILogger<DepartmentKeyMetricService> _logger;

    public DepartmentKeyMetricService(
           MetricsDbContext context,
           ILogger<DepartmentKeyMetricService> logger,
           IDepartmentKeyMetricRepository keyKpiRepository)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<DepartmentKeyMetric> CreateAsync(DepartmentKeyMetric entity)
    {
        try
        {
            _context.DepartmentKeyMetrics.Add(entity);
            await _context.SaveChangesAsync();

            return entity;
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
            {
                _logger.LogError(ex, pgEx.MessageText);
                throw new DuplicateContentException("Department Key already exist.", ex.InnerException);
            }
            else
            {
                _logger.LogError(ex, "Database error while creating Department Key.");
                throw new Exception("A database error occurred.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating Department Key.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    /// <summary>
    /// DeleteAsync (soft delete)
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    /// <exception cref="NotFoundException"></exception>
    /// <exception cref="Exception"></exception>
    public async Task<bool> DeleteAsync(Guid code) // Soft Delete
    {
        try
        {
            var targetKeyKpi = await _context.DepartmentKeyMetrics
                .Where(k => k.DepartmentKeyMetricCode == code)
                .FirstOrDefaultAsync();
            // await _departmentKeyMetricRepository.FindByCodeAsync(code);
            if (targetKeyKpi == null)
                throw new NotFoundException("Department Key Metric not found.");

            targetKeyKpi.IsDeleted = true;
            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting Department Key Metric.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<bool> UnDeleteAsync(Guid DepartmentKeyMetricCode) // Soft Delete
    {
        try
        {
            var targetKeyKpi = await _context.DepartmentKeyMetrics
                .Where(k => k.DepartmentKeyMetricCode == DepartmentKeyMetricCode)
                .FirstOrDefaultAsync();

            if (targetKeyKpi == null)
                throw new NotFoundException("Department Key Metric not found.");

            targetKeyKpi.IsDeleted = false;
            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while undeleting Department Key Metric.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<IEnumerable<DepartmentKeyMetric>> FindAllAsync()
    {
        try
        {
            var result = await _context.DepartmentKeyMetrics
                .OrderBy(k => k.KpiSubmissionPeriod.PeriodName)
                .ThenBy(k => k.KeyIssueDepartment.DepartmentName)
                .ThenBy(k => k.KeyMetric.MetricTitle)
                .ToListAsync();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while querying Key KPIs.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<IEnumerable<DepartmentKeyMetric>> FindAllAsync(int pageNumber = 1, int pageSize = 20)
    {
        try
        {
            // var keyKpis = await _keyKpiRepository.FindAllAsQueryable()
            //     .Skip((pageNumber - 1) * pageSize)
            //     .Take(pageSize)
            //     .ToListAsync();
            var result = await _context.DepartmentKeyMetrics
                .Include(k => k.KpiSubmissionPeriod)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while querying Key KPIs.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<DepartmentKeyMetric?> FindByIdAsync(long id)
    {
        try
        {
            var department = await _context.DepartmentKeyMetrics.FindAsync(id);
            if (department == null)
                throw new NotFoundException($"Key KPI with id {id} not found.");

            return department;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while querying Key KPI by Metric Code.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<DepartmentKeyMetric?> FindByCodeAsync(Guid departmentKeyMetricCode)
    {
        try
        {
            if (string.IsNullOrEmpty(departmentKeyMetricCode.ToString()))
                throw new ArgumentNullException("Parameter metricCode is required.");

            var keyKpi = await _context.DepartmentKeyMetrics
                .Where(k => k.DepartmentKeyMetricCode == departmentKeyMetricCode)
                .FirstOrDefaultAsync();

            if (keyKpi == null)
                throw new NotFoundException($"Department Key Metric with code {departmentKeyMetricCode} not found.");

            return keyKpi;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while querying Department Key Metric by Code.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }


    // public async Task<DepartmentKeyMetric?> FindByMetricTitleAsync(string metricTitle)
    // {
    //     try
    //     {
    //         if (string.IsNullOrEmpty(metricTitle) || string.IsNullOrWhiteSpace(metricTitle))
    //             throw new ArgumentNullException("Parameter metricTitle is required.");

    //         var department = await _keyKpiRepository.FindByMetricTitleAsync(metricTitle);

    //         return department;
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Unexpected error while querying Key KPI by metric title.");
    //         throw new Exception("An unexpected error occurred. Please try again later.");
    //     }
    // }

    public async Task<ResultT<List<DepartmentKeyMetricDto>>> FindByPeriodIdAsync(long periodId)
    {
        try
        {
            var data = await _context.DepartmentKeyMetrics
                .Include(k => k.KpiSubmissionPeriod)
                .Include(k => k.KeyMetric)
                .Include(k => k.KeyIssueDepartment).ThenInclude(u => u.ApplicationUsers)
                .Include(k => k.KeyIssueDepartment)
                    .ThenInclude(u => u.ApplicationUsers)
                .Include(k => k.KeyIssueDepartment.ApplicationUsers)
                    .ThenInclude(g => g.UserTitle)
                .OrderBy(k => k.KeyIssueDepartment.DepartmentName)
                .Where(k => k.KpiSubmissionPeriodId == periodId)
                .ToListAsync();

            var result = data.Select(e => e.MapToDto()).ToList();

            return ResultT<List<DepartmentKeyMetricDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occured at fetching department key metrics by period id. {msg}", ex.Message);
            return ResultT<List<DepartmentKeyMetricDto>>.Fail("Failed to fetch department key metrics by period.", ErrorType.UnexpectedError);
        }
    }

    public async Task<ResultT<List<DepartmentKeyMetricDto>>> FindByPeriodByKeyIssueDepartmentAsync(
        long periodId, Guid keyIssueDepartmentCode)
    {
        try
        {
            var data = await _context.DepartmentKeyMetrics
                .Include(k => k.KpiSubmissionPeriod)
                .Include(k => k.KeyMetric)
                .Include(k => k.KeyIssueDepartment)
                .OrderBy(k => k.KeyIssueDepartment.DepartmentName)
                .Where(k => k.KpiSubmissionPeriod.Id == periodId
                    && k.KeyIssueDepartment.DepartmentCode == keyIssueDepartmentCode)
                .ToListAsync();
            var result = data.Select(e => e.MapToDto()).ToList();

            return ResultT<List<DepartmentKeyMetricDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occured while fetching department key metrics by period id by key issue department. {msg}", ex.Message);
            return ResultT<List<DepartmentKeyMetricDto>>.Fail("Failed to fetch department key metrics by period by key issue department", ErrorType.UnexpectedError);
        }
    }


    public async Task<IEnumerable<DepartmentKeyMetric>> FindAllByPeriodIdAsync(long periodId)
    {
        try
        {
            var keyKpis = await _context.DepartmentKeyMetrics
                .Include(k => k.KpiSubmissionPeriod)
                .Include(k => k.KeyMetric)
                .Include(k => k.KeyIssueDepartment).ThenInclude(u => u.ApplicationUsers)
                .Include(k => k.KeyIssueDepartment)
                    .ThenInclude(u => u.ApplicationUsers)
                .Include(k => k.KeyIssueDepartment.ApplicationUsers)
                    .ThenInclude(g => g.UserTitle)
                .OrderBy(k => k.KeyIssueDepartment.DepartmentName)
                .Where(k => k.KpiSubmissionPeriodId == periodId)
                .ToListAsync();

            return keyKpis;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while querying Key KPI by period id.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<IEnumerable<DepartmentKeyMetric>> FindAllByPeriodNameAsync(string periodName)
    {
        try
        {
            var keyKpis = await _context.DepartmentKeyMetrics
                .Include(k => k.KpiSubmissionPeriod)
                .Include(k => k.KeyMetric)
                .Include(k => k.KeyIssueDepartment)
                .OrderBy(k => k.KeyIssueDepartment.DepartmentName)
                .Where(k => k.KpiSubmissionPeriod.PeriodName == periodName)
                .ToListAsync();

            return keyKpis;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while querying Key KPI by period id.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<IEnumerable<DepartmentKeyMetric>> FindAllByPeriodAndDepartmentAsync(
        string currentPeriodName,
        Guid currentDepartmentCode)
    {
        try
        {
            var result = await _context.DepartmentKeyMetrics
                .Where(k => k.KpiSubmissionPeriod.PeriodName == currentPeriodName
                    && k.KeyIssueDepartment.DepartmentCode == currentDepartmentCode)
                .OrderBy(k => k.KeyIssueDepartment.DepartmentName)
                .Include(k => k.KpiSubmissionPeriod)
                .Include(k => k.KeyMetric)
                .Include(k => k.KeyIssueDepartment)
                .ToListAsync();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while querying Department Key Metric by period, department.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    // FindBy PeriodName And DepartmentCode And KeyMetricCode
    public async Task<DepartmentKeyMetric?> FindByPeriodAndDepartmentAndKeyMetricAsync(
        string periodName,
        Guid departmentCode,
        Guid keyMetricCode)
    {
        try
        {
            var result = await _context.DepartmentKeyMetrics
                .Where(k => k.KpiSubmissionPeriod.PeriodName == periodName
                        && k.KeyIssueDepartment.DepartmentCode == departmentCode
                        && k.KeyMetric.MetricCode == keyMetricCode)
                .FirstOrDefaultAsync();
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while querying Department Key Metric by period, department, key metric.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<long> FindCountAsync()
    {
        try
        {
            return await _context.DepartmentKeyMetrics.CountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while counting Key KPIs.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<DepartmentKeyMetric> UpdateAsync(Guid code, DepartmentKeyMetric entity)
    {
        if (string.IsNullOrEmpty(code.ToString()))
            throw new ArgumentNullException("Department code is required.");

        // TODO: does this check necessary?
        if (code != entity.DepartmentKeyMetricCode)
            throw new DbUpdateException("Update failed. Hint: depatmentKeyMetricCode to update and existing depatmentKeyMetricCode does not match.");

        try
        {
            var targetKeyKpi = await _context.DepartmentKeyMetrics
                .Where(k => k.DepartmentKeyMetricCode == code)
                .FirstOrDefaultAsync();
            if (targetKeyKpi == null)
                throw new NotFoundException("Department not found.");
            // Handle concurrency (example using row version)
            // if (existing.RowVersion != department.RowVersion)
            //     return Result<Department>.Fail("Concurrency conflict.");

            // Note: This is full update (**not partial update, so need to set all field)
            // _departmentKeyMetricRepository.Update(targetKeyKpi);
            _context.Entry(targetKeyKpi).State = EntityState.Modified;
            targetKeyKpi.DepartmentId = entity.DepartmentId;
            targetKeyKpi.KpiSubmissionPeriodId = entity.KpiSubmissionPeriodId;
            targetKeyKpi.KeyMetricId = entity.KeyMetricId;
            // targetKeyKpi.MetricCode = entity.MetricCode;
            // targetKeyKpi.MetricTitle = entity.MetricTitle;
            await _context.SaveChangesAsync();

            // refetch updated entity
            var updatedEntity = await _context.DepartmentKeyMetrics
                .Where(k => k.DepartmentKeyMetricCode == entity.DepartmentKeyMetricCode)
                .FirstOrDefaultAsync();

            if (updatedEntity == null)
                throw new NotFoundException("Key KPI not found.");

            return updatedEntity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while updating Key KPI.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<ResultT<List<DepartmentKeyMetricDto>>> FindByPeriodNameAsync(string periodName)
    {
        try
        {
            var data = await _context.DepartmentKeyMetrics
                .AsNoTracking()
                .Where(k => k.KpiSubmissionPeriod.PeriodName == periodName)
                .Include(k => k.KpiSubmissionPeriod)
                .Include(k => k.KeyMetric)
                .Include(k => k.KeyIssueDepartment)
                .OrderBy(k => k.KeyIssueDepartment.DepartmentName)
                .ToListAsync();
            var result = data
                // .Where(e => e.IsDeleted == false)
                .Select(e => e.MapToDto()).ToList();

            return ResultT<List<DepartmentKeyMetricDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while fetching department key metrics by period name.");
            return ResultT<List<DepartmentKeyMetricDto>>.Fail("An unexpected error occurred while fetching department key metrics by period name.", ErrorType.UnexpectedError);
        }
    }


    public async Task<ResultT<Dictionary<long, int>>> FindCountsByPeriodAsync(List<long> periodIds)
    {
        try
        {
            var countResult = await _context.DepartmentKeyMetrics
                .Where(e => periodIds.Contains(e.KpiSubmissionPeriodId))
                .GroupBy(e => e.KpiSubmissionPeriodId)
                .Select(e => new { PeriodId = e.Key, Count = e.Count() })
                .ToDictionaryAsync(e => e.PeriodId, e => e.Count);

            return ResultT<Dictionary<long, int>>.Success(countResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get department key metric count by Periods.");
            return ResultT<Dictionary<long, int>>.Fail("Failed to get department key metric count by Periods.", ErrorType.UnexpectedError);
        }
    }

    public async Task<Result> Create_Async(DepartmentKeyMetricCreateDto createDto)
    {
        try
        {
            _context.DepartmentKeyMetrics.Add(createDto.MapToEntity());
            await _context.SaveChangesAsync();

            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
            {
                _logger.LogError(ex, pgEx.MessageText);
                return Result.Fail("Department Key already exist.", ErrorType.DuplicateKey);
            }
            else
            {
                _logger.LogError(ex, "Database error while creating Department Key.");
                return Result.Fail("A database error occurred.", ErrorType.DatabaseError);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating Department Key.");
            return Result.Fail("An unexpected error occurred. Please try again later.", ErrorType.UnexpectedError);
        }
    }

    public async Task<Result> CreateAsync(List<DepartmentKeyMetricCreateDto> createDTOs)
    {
        try
        {
            var entities = createDTOs.Select(x => x.MapToEntity()).ToList();
            _context.DepartmentKeyMetrics.AddRange(entities);
            await _context.SaveChangesAsync();

            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
            {
                _logger.LogError(ex, pgEx.MessageText);
                return Result.Fail("Department Key already exist.", ErrorType.DuplicateKey);
            }
            else
            {
                _logger.LogError(ex, "Database error while creating Department Key.");
                return Result.Fail("A database error occurred.", ErrorType.DatabaseError);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating Department Key.");
            return Result.Fail("An unexpected error occurred. Please try again later.", ErrorType.UnexpectedError);
        }
    }

    public async Task<Result> ReplicateAsync(List<DepartmentKeyMetricDto> sourceDTOs, List<DepartmentKeyMetricDto> targetDTOs)
    {
        // =====TO INSERT=====
        // 1. take out non existing entries -> insert
        // =====TO UPDATE=====
        // 2. take out deleted      entries -> do undelete
        // 3. take out undeleted    entries -> do delete
        try
        {
            var targetLookupIDs = new HashSet<Guid>(targetDTOs.Select(t => t.LookupId));

            // =====TO INSERT=====
            // 1. take out non existing entries -> insert
            var toInsert = sourceDTOs
                .Where(source => !targetLookupIDs.Contains(source.LookupId))
                .ToList();
            var entities = toInsert.Select(e => e.MapToEntity()).ToList();
            _context.AddRange(entities);

            // =====TO UPDATE=====
            var toUpdate = sourceDTOs
                .Where(source => targetLookupIDs.Contains(source.LookupId))
                .ToList();
            // 2. take out deleted      entries -> do undelete
            var toUndelete = toUpdate.Where(source => source.IsDeleted == true).ToList();
            foreach (var entity in toUndelete)
            {

            }
            // 3. take out undeleted    entries -> do delete
            var toDelete = toUpdate.Where(source => source.IsDeleted == false).ToList();





            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while replicating Department Keys.");
            return Result.Fail("An unexpected error while replicating Department Keys. Please try again later.", ErrorType.UnexpectedError);
        }
    }

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
}
