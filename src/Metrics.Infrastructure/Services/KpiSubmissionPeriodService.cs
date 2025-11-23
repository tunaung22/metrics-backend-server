using Metrics.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Metrics.Application.Interfaces.IServices;
using Metrics.Application.Interfaces.IRepositories;
using Metrics.Application.Domains;
using Metrics.Application.Exceptions;
using Metrics.Application.Results;
using Metrics.Application.DTOs.KpiPeriod;
using Metrics.Application.Common.Mappers;

namespace Metrics.Infrastructure.Services;

public class KpiSubmissionPeriodService : IKpiSubmissionPeriodService
{
    private readonly MetricsDbContext _context;
    private readonly ILogger<KpiSubmissionPeriodService> _logger;
    private readonly IKpiSubmissionPeriodRepository _kpiSubmissionPeriodRepository;

    public KpiSubmissionPeriodService(
        MetricsDbContext context,
        ILogger<KpiSubmissionPeriodService> logger,
        IKpiSubmissionPeriodRepository kpiSubmissionPeriodRepository)
    {
        _context = context;
        _logger = logger;
        _kpiSubmissionPeriodRepository = kpiSubmissionPeriodRepository;
    }


    // ========== Return Entity ================================================
    // public async Task<KpiSubmissionPeriod> CreateAsync(KpiSubmissionPeriod entity)
    // {
    //     try
    //     {
    //         _kpiSubmissionPeriodRepository.Create(entity);
    //         await _context.SaveChangesAsync();

    //         return entity;
    //     }
    //     catch (DbUpdateException ex)
    //     {
    //         if (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
    //         {
    //             _logger.LogError(ex, pgEx.MessageText);
    //             throw new DuplicateContentException("KPI Period already exist.");
    //         }
    //         else
    //         {
    //             _logger.LogError(ex, "Database error while creating kpi period.");
    //             throw new Exception("A database error occurred.");
    //         }
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Unexpected error while creating KPI Period.");
    //         throw new Exception("An unexpected error occurred. Please try again later.");
    //     }
    // }

    // public async Task<KpiSubmissionPeriod> UpdateAsync(string periodName, KpiSubmissionPeriod entity)
    // {
    //     try
    //     {
    //         if (string.IsNullOrEmpty(periodName) || string.IsNullOrWhiteSpace(periodName))
    //             throw new ArgumentNullException("Period name is required.");

    //         var targetPeriod = await _kpiSubmissionPeriodRepository.FindByPeriodNameAsync(periodName);
    //         if (targetPeriod == null)
    //             throw new NotFoundException("KPI Period not found.");
    //         // Handle concurrency (example using row version)
    //         // if (existing.RowVersion != department.RowVersion)
    //         //     return Result<Department>.Fail("Concurrency conflict.");
    //         _kpiSubmissionPeriodRepository.Update(targetPeriod);
    //         targetPeriod.PeriodName = entity.PeriodName;
    //         targetPeriod.SubmissionStartDate = entity.SubmissionStartDate;
    //         targetPeriod.SubmissionEndDate = entity.SubmissionEndDate;

    //         await _context.SaveChangesAsync();
    //         var updatedEntity = await _kpiSubmissionPeriodRepository.FindByPeriodNameAsync(entity.PeriodName);
    //         if (updatedEntity == null)
    //             throw new MetricsNotFoundException("Submission not found");

    //         return updatedEntity;
    //     }
    //     catch (MetricsNotFoundException) { throw; }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Unexpected error while updating KPI Period.");
    //         throw new Exception("An unexpected error occurred. Please try again later.");
    //     }
    // }

    // public async Task<bool> DeleteAsync(string periodName)
    // {
    //     try
    //     {
    //         if (string.IsNullOrEmpty(periodName) || string.IsNullOrWhiteSpace(periodName))
    //             throw new ArgumentNullException("Period name is required.");

    //         var targetPeriod = await _kpiSubmissionPeriodRepository.FindByPeriodNameAsync(periodName);
    //         if (targetPeriod == null)
    //             return true; // Idempotent: Treat as success

    //         _kpiSubmissionPeriodRepository.Delete(targetPeriod);

    //         return await _context.SaveChangesAsync() > 0;
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Unexpected error while deleting KPI Period.");
    //         throw new Exception("An unexpected error occurred. Please try again later.");
    //     }
    // }

    // public async Task<KpiSubmissionPeriod> FindByIdAsync(long id)
    // {
    //     try
    //     {
    //         var kpiPeriod = await _kpiSubmissionPeriodRepository.FindByIdAsync(id);
    //         if (kpiPeriod == null)
    //             throw new MetricsNotFoundException($"KPI Period with id {id} not found.");

    //         return kpiPeriod;
    //     }
    //     catch (MetricsNotFoundException) { throw; }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Unexpected error while querying KPI Period.");
    //         throw new Exception("An unexpected error occurred. Please try again later.");
    //     }
    // }

    public async Task<KpiSubmissionPeriod?> FindByKpiPeriodNameAsync(string periodName)
    {
        try
        {
            if (string.IsNullOrEmpty(periodName) || string.IsNullOrWhiteSpace(periodName))
                throw new ArgumentNullException("Parameter periodName is required.");

            var kpiPeriod = await _kpiSubmissionPeriodRepository.FindByPeriodNameAsync(periodName);
            // var kpiPeriod = await _context.KpiSubmissionPeriods
            //     .Where(e => e.PeriodName.Trim().ToLower() == periodName.Trim().ToLower())
            //     .OrderBy(e => e.PeriodName)
            //     // .Include(e => e.KpiSubmissions)
            //     .FirstOrDefaultAsync();

            return kpiPeriod;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while querying KPI Period by period name.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    // public async Task<long> FindIdByKpiPeriodNameAsync(string periodName)
    // {
    //     try
    //     {
    //         if (string.IsNullOrEmpty(periodName) || string.IsNullOrWhiteSpace(periodName))
    //             throw new ArgumentNullException("Parameter periodName is required.");

    //         var kpiPeriod = await _kpiSubmissionPeriodRepository.FindByPeriodNameAsync(periodName);
    //         if (kpiPeriod == null)
    //             throw new MetricsNotFoundException($"KPI Period with period name {periodName} not found.");

    //         return kpiPeriod.Id;
    //     }
    //     catch (MetricsNotFoundException) { throw; }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Unexpected error while querying KPI Period by id.");
    //         throw new Exception("An unexpected error occurred. Please try again later.");
    //     }
    // }

    public async Task<IEnumerable<KpiSubmissionPeriod>> FindAllAsync()
    {
        try
        {
            var kpiPeriods = await _kpiSubmissionPeriodRepository.FindAllAsQueryable()
                .ToListAsync();

            return kpiPeriods;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while querying KPI Periods.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<IEnumerable<KpiSubmissionPeriod>> FindAllAsync(int pageNumber, int pageSize)
    {
        try
        {
            var kpiPeriods = await _kpiSubmissionPeriodRepository.FindAllAsQueryable()
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return kpiPeriods;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while querying KPI Periods.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<IEnumerable<KpiSubmissionPeriod>> FindAllByDateAsync(DateTimeOffset todayDate)
    {
        try
        {
            var kpiPeriods = await _kpiSubmissionPeriodRepository.FindAllAsQueryable()
                // .Skip((pageNumber - 1) * pageSize)
                // .Take(pageSize)
                .Where(e =>
                    e.SubmissionStartDate <= todayDate
                    && e.SubmissionEndDate >= todayDate)
                .ToListAsync();

            return kpiPeriods;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while querying KPI Periods by Date.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<bool> KpiPeriodNameExistsAsync(string kpiName)
    {
        try
        {
            return await _kpiSubmissionPeriodRepository.KpiPeriodExistsAsync(kpiName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while checking kpi period name exist.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }


    public async Task<long> FindCountAsync()
    {
        try
        {
            return await _kpiSubmissionPeriodRepository.FindCountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while counting KPI Periods.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    // ========== Result Pattern =====================================
    public async Task<Result> CreateAsync(KpiPeriodCreateDto createDto)
    {
        try
        {
            _kpiSubmissionPeriodRepository.Create(createDto.MapToEntity());
            await _context.SaveChangesAsync();

            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "DB Update error while creating new kpi period. {e}", ex.Message);
            return Result.Fail("Failed to create new kpi period.", ErrorType.DatabaseError);
        }
        catch (Exception ex)
        {
            _logger.LogError("Unexpected error while creating new kpi period. {e}", ex.Message);
            return Result.Fail("Failed to create new period", ErrorType.UnexpectedError);
        }
    }

    public async Task<Result> UpdateAsync(string periodName, KpiPeriodUpdateDto updateDto)
    {
        try
        {
            if (string.IsNullOrEmpty(periodName) || string.IsNullOrWhiteSpace(periodName))
                return Result.Fail("Period name is required.", ErrorType.InvalidArgument);

            var targetPeriod = await _kpiSubmissionPeriodRepository.FindByPeriodNameAsync(periodName);
            if (targetPeriod == null)
                return Result.Fail("Period name to update was not found.", ErrorType.NotFound);

            // Handle concurrency (example using row version)
            // if (existing.RowVersion != department.RowVersion)
            //     return Result<Department>.Fail("Concurrency conflict.");

            _kpiSubmissionPeriodRepository.Update(targetPeriod);
            targetPeriod.PeriodName = updateDto.PeriodName;
            targetPeriod.SubmissionStartDate = updateDto.SubmissionStartDate;
            targetPeriod.SubmissionEndDate = updateDto.SubmissionEndDate;
            await _context.SaveChangesAsync();

            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "DB Update error while updating kpi period. {e}", ex.Message);
            return Result.Fail("Failed to update kpi period.", ErrorType.DatabaseError);
        }
        catch (Exception ex)
        {
            _logger.LogError("Unexpected error while updating kpi period. {msg}", ex);
            return Result.Fail("Failed to update period", ErrorType.UnexpectedError);
        }
    }

    public async Task<Result> DeleteAsync(string periodName)
    {
        try
        {
            if (string.IsNullOrEmpty(periodName) || string.IsNullOrWhiteSpace(periodName))
                throw new ArgumentNullException("Period name is required.");

            var targetPeriod = await _kpiSubmissionPeriodRepository.FindByPeriodNameAsync(periodName);
            if (targetPeriod == null)
                return Result.Success(); // Idempotent: Treat as success

            _kpiSubmissionPeriodRepository.Delete(targetPeriod);
            await _context.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError("Unexpected error while deleting KPI Period. {e}", ex);
            return Result.Fail("An unexpected error occurred deleting Kpi Period.", ErrorType.UnexpectedError);
        }
    }

    public async Task<ResultT<List<KpiPeriodDto>>> FindAll_Async()
    {
        try
        {
            var data = await _kpiSubmissionPeriodRepository.FindAllAsync();
            var result = data.Select(e => e.MapToDto()).ToList();

            return ResultT<List<KpiPeriodDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError("Unexpected error while fetching all KPI Period. {e}", ex);
            return ResultT<List<KpiPeriodDto>>.Fail("An unexpected error occurred fetching all Kpi Periods.", ErrorType.UnexpectedError);
        }
    }
}