using Metrics.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Metrics.Application.Interfaces.IServices;
using Metrics.Application.Interfaces.IRepositories;
using Metrics.Application.Domains;
using Metrics.Application.Exceptions;

namespace Metrics.Infrastructure.Services;

public class KpiPeriodService : IKpiPeriodService
{
    private readonly MetricsDbContext _context;
    private readonly ILogger<KpiPeriodService> _logger;
    private readonly IKpiPeriodRepository _kpiPeriodRepository;

    public KpiPeriodService(
        MetricsDbContext context,
        ILogger<KpiPeriodService> logger,
        IKpiPeriodRepository kpiPeriodRepository)
    {
        _context = context;
        _logger = logger;
        _kpiPeriodRepository = kpiPeriodRepository;
    }


    // ========== Return Entity ================================================
    public async Task<KpiPeriod> CreateAsync(KpiPeriod entity)
    {
        try
        {
            _kpiPeriodRepository.Create(entity);
            await _context.SaveChangesAsync();

            return entity;
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
            {
                _logger.LogError(ex, pgEx.MessageText);
                throw new DuplicateContentException("KPI Period already exist.");
            }
            else
            {
                _logger.LogError(ex, "Database error while creating kpi period.");
                throw new Exception("A database error occurred.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating KPI Period.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<KpiPeriod> UpdateAsync(string periodName, KpiPeriod entity)
    {
        try
        {
            if (string.IsNullOrEmpty(periodName) || string.IsNullOrWhiteSpace(periodName))
                throw new ArgumentNullException("Period name is required.");

            var targetPeriod = await _kpiPeriodRepository.FindByPeriodNameAsync(periodName);
            if (targetPeriod == null)
                throw new NotFoundException("KPI Period not found.");
            // Handle concurrency (example using row version)
            // if (existing.RowVersion != department.RowVersion)
            //     return Result<Department>.Fail("Concurrency conflict.");
            _kpiPeriodRepository.Update(targetPeriod);
            targetPeriod.PeriodName = entity.PeriodName;
            targetPeriod.SubmissionStartDate = entity.SubmissionStartDate;
            targetPeriod.SubmissionEndDate = entity.SubmissionEndDate;

            await _context.SaveChangesAsync();
            var updatedEntity = await _kpiPeriodRepository.FindByPeriodNameAsync(periodName);

            return updatedEntity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while updating KPI Period.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<bool> DeleteAsync(string periodName)
    {
        try
        {
            if (string.IsNullOrEmpty(periodName) || string.IsNullOrWhiteSpace(periodName))
                throw new ArgumentNullException("Period name is required.");

            var targetPeriod = await _kpiPeriodRepository.FindByPeriodNameAsync(periodName);
            if (targetPeriod == null)
                return true; // Idempotent: Treat as success

            _kpiPeriodRepository.Delete(targetPeriod);

            return await _context.SaveChangesAsync() > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting KPI Period.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<KpiPeriod> FindByIdAsync(long id)
    {
        try
        {
            var kpiPeriod = await _kpiPeriodRepository.FindByIdAsync(id);
            if (kpiPeriod == null)
                throw new NotFoundException($"KPI Period with id {id} not found.");

            return kpiPeriod;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while querying KPI Period.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<KpiPeriod> FindByKpiPeriodNameAsync(string periodName)
    {
        try
        {
            if (string.IsNullOrEmpty(periodName) || string.IsNullOrWhiteSpace(periodName))
                throw new ArgumentNullException("Parameter periodName is required.");

            var kpiPeriod = await _kpiPeriodRepository.FindByPeriodNameAsync(periodName);
            if (kpiPeriod == null)
                throw new NotFoundException($"KPI Period with period name {periodName} not found.");

            return kpiPeriod;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while querying KPI Period by period name.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<long> FindIdByKpiPeriodNameAsync(string periodName)
    {
        try
        {
            if (string.IsNullOrEmpty(periodName) || string.IsNullOrWhiteSpace(periodName))
                throw new ArgumentNullException("Parameter periodName is required.");

            var kpiPeriod = await _kpiPeriodRepository.FindByPeriodNameAsync(periodName);
            if (kpiPeriod == null)
                throw new NotFoundException($"KPI Period with period name {periodName} not found.");

            return kpiPeriod.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while querying KPI Period by id.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<IEnumerable<KpiPeriod>> FindAllAsync()
    {
        try
        {
            var kpiPeriods = await _kpiPeriodRepository.FindAllAsQueryable()
                .ToListAsync();

            return kpiPeriods;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while querying KPI Periods.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<IEnumerable<KpiPeriod>> FindAllAsync(int pageNumber, int pageSize)
    {
        try
        {
            var kpiPeriods = await _kpiPeriodRepository.FindAllAsQueryable()
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

    public async Task<IEnumerable<KpiPeriod>> FindAllByDateAsync(DateTimeOffset todayDate)
    {
        try
        {
            var kpiPeriods = await _kpiPeriodRepository.FindAllAsQueryable()
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
            return await _kpiPeriodRepository.KpiPeriodExistsAsync(kpiName);
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
            return await _kpiPeriodRepository.FindCountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while counting KPI Periods.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }
}

// ========== Operation Result Pattern =====================================
// public async Task<Result<KpiPeriod>> Create_ResultAsync(KpiPeriod entity)
// {
//     try
//     {
//         if (await _kpiPeriodRepository.KpiPeriodExistsAsync(entity.PeriodName))
//             return Result<KpiPeriod>.Fail("KPI Period already exist.", ErrorType.DuplicateKey);

//         _kpiPeriodRepository.Create(entity);
//         await _context.SaveChangesAsync();

//         return Result<KpiPeriod>.Ok(entity);
//     }
//     catch (DbUpdateException ex)
//     {
//         // Handle database-specific errors
//         _logger.LogError(ex, "Database error while creating department.");
//         return Result<KpiPeriod>.Fail("A database error occurred.", ErrorType.DatabaseError);
//     }
//     catch (Exception ex)
//     {
//         // Log unexpected errors
//         _logger.LogError(ex, "Unexpected error while creating department.");
//         return Result<KpiPeriod>.Fail("An unexpected error occurred. Please try again later.", ErrorType.UnexpectedError);
//     }
// }

// public async Task<Result<bool>> Delete_ResultAsync(string periodName)
// {
//     try
//     {
//         var kpiPeriod = await _kpiPeriodRepository.FindByPeriodNameAsync(periodName);
//         if (kpiPeriod == null)
//             return Result<bool>.Ok(true);

//         _kpiPeriodRepository.Delete(kpiPeriod);
//         await _context.SaveChangesAsync();

//         return Result<bool>.Ok(true);
//     }
//     catch (DbUpdateException ex)
//     {
//         // Handle database-specific errors
//         _logger.LogError(ex, "Database error while creating department.");
//         return Result<bool>.Fail("A database error occurred.", ErrorType.DatabaseError);
//     }
//     catch (Exception ex)
//     {
//         // Log unexpected errors
//         _logger.LogError(ex, "Unexpected error while creating department.");
//         return Result<bool>.Fail("An unexpected error occurred. Please try again later.", ErrorType.UnexpectedError);
//     }
// }



// public async Task<Result<IEnumerable<KpiPeriod>>> FindAll_ResultAsync()
// {
//     try
//     {
//         var kpiPeriods = await _kpiPeriodRepository.FindAllAsync();

//         return Result<IEnumerable<KpiPeriod>>.Ok(kpiPeriods);
//     }
//     catch (Exception ex)
//     {
//         // Log unexpected errors
//         _logger.LogError(ex, "Unexpected error while creating department.");
//         return Result<IEnumerable<KpiPeriod>>.Fail("An unexpected error occurred. Please try again later.", ErrorType.UnexpectedError);
//     }
// }



// public async Task<Result<KpiPeriod>> FindById_ResultAsync(long id)
// {
//     try
//     {
//         var kpiPeriod = await _kpiPeriodRepository.FindByIdAsync(id);
//         if (kpiPeriod == null)
//             return Result<KpiPeriod>.Fail("Kpi Period not found.", ErrorType.NotFound);

//         return Result<KpiPeriod>.Ok(kpiPeriod);
//     }
//     catch (Exception ex)
//     {
//         // Log unexpected errors
//         _logger.LogError(ex, "Unexpected error while creating department.");
//         return Result<KpiPeriod>.Fail("An unexpected error occurred. Please try again later.", ErrorType.UnexpectedError);
//     }
// }


// public async Task<Result<KpiPeriod>> FindByKpiPeriodName_ResultAsync(string periodName)
// {
//     try
//     {
//         var kpiPeriod = await _kpiPeriodRepository.FindByPeriodNameAsync(periodName);
//         if (kpiPeriod == null)
//             return Result<KpiPeriod>.Fail("Kpi Period not found.", ErrorType.NotFound);

//         return Result<KpiPeriod>.Ok(kpiPeriod);
//     }
//     catch (Exception ex)
//     {
//         // Log unexpected errors
//         _logger.LogError(ex, "Unexpected error while creating department.");
//         return Result<KpiPeriod>.Fail("An unexpected error occurred. Please try again later.", ErrorType.UnexpectedError);
//     }
// }

// public async Task<Result<KpiPeriod>> Update_ResultAsync(KpiPeriod entity)
// {
//     try
//     {
//         var kpiPeriod = await _kpiPeriodRepository
//             .FindByPeriodNameAsync(entity.PeriodName);
//         if (kpiPeriod == null)
//             return Result<KpiPeriod>.Fail("Kpi Period does not exist.", ErrorType.NotFound);

//         _kpiPeriodRepository.Update(entity);
//         await _context.SaveChangesAsync();

//         return Result<KpiPeriod>.Ok(entity);
//     }
//     catch (DbUpdateException ex)
//     {
//         // Handle database-specific errors
//         _logger.LogError(ex, "Database error while creating department.");
//         return Result<KpiPeriod>.Fail("A database error occurred.", ErrorType.DatabaseError);
//     }
//     catch (Exception ex)
//     {
//         // Log unexpected errors
//         _logger.LogError(ex, "Unexpected error while creating department.");
//         return Result<KpiPeriod>.Fail("An unexpected error occurred. Please try again later.", ErrorType.UnexpectedError);
//     }
// }



// ========== Return DTO ===================================================
// public async Task<long> FindIdByKpiPeriodName_Async(string periodName)
// {
//     try
//     {
//         if (string.IsNullOrEmpty(periodName))
//             // TODO: ValidationException
//             throw new Exception("Parameter periodName is required.");

//         var kpiPeriod = await _kpiPeriodRepository.FindByPeriodNameAsync(periodName);

//         if (kpiPeriod == null)
//             // TODO: NotFoundException
//             // throw new NotFoundException($"KPI Period with name {periodName} not found.");
//             return 0;

//         return kpiPeriod.Id;
//     }
//     catch (Exception ex)
//     {
//         _logger.LogError(ex, "Unexpected error while querying department by department code.");
//         throw new Exception("An unexpected error occurred. Please try again later.");
//     }
// }

// public async Task<KpiPeriodGetDto> FindByKpiPeriodName_Async(string periodName)
// {
//     try
//     {
//         if (string.IsNullOrEmpty(periodName))
//             // TODO: ValidationException
//             throw new Exception("Parameter periodName is required.");

//         var kpiPeriod = await _kpiPeriodRepository.FindByPeriodNameAsync(periodName);
//         if (kpiPeriod == null)
//             // TODO: NotFoundException
//             throw new Exception($"KPI Period with name {periodName} not found.");

//         return kpiPeriod.ToGetDto();
//     }
//     catch (Exception ex)
//     {
//         _logger.LogError(ex, "Unexpected error while querying department by department code.");
//         throw new Exception("An unexpected error occurred. Please try again later.");
//     }
// }
// public async Task<KpiPeriodGetDto> Update_Async(KpiPeriodUpdateDto updateDto)
// {
//     try
//     {
//         // Validate existence
//         var targetKpiPeriod = await _kpiPeriodRepository.FindByPeriodNameAsync(updateDto.PeriodName.ToString());
//         if (targetKpiPeriod == null)
//             // TODO: NotFoundException
//             throw new NotFoundException("KPI Period not found.");

//         // Handle concurrency (example using row version)
//         // if (existing.RowVersion != department.RowVersion)
//         //     return Result<Department>.Fail("Concurrency conflict.");

//         // Track changes

//         _kpiPeriodRepository.Update(targetKpiPeriod);

//         // DTOs to Entity
//         // targetKpiPeriod = updateDto.ToEntity(); // **will it work???
//         targetKpiPeriod.SubmissionStartDate = updateDto.SubmissionStartDate;
//         targetKpiPeriod.SubmissionEndDate = updateDto.SubmissionEndDate;

//         await _context.SaveChangesAsync();

//         return targetKpiPeriod.ToGetDto();
//     }
//     catch (DbUpdateException ex)
//     {
//         if (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
//         {
//             _logger.LogError(ex, pgEx.MessageText);
//             throw new DuplicateContentException("Duplicate entry exception occurred.");
//         }
//         else
//         {
//             _logger.LogError(ex, "Database error while updating department.");
//             throw new Exception("A database error occurred.");
//         }
//     }
//     catch (Exception ex)
//     {
//         _logger.LogError(ex, "Unexpected error while updating departments.");
//         throw new Exception("An unexpected error occurred. Please try again later.");
//     }
// }

// public async Task<KpiPeriodGetDto> FindById_Async(long id)
// {
//     try
//     {
//         if (id <= 0)
//             // TODO: ValidationException
//             throw new ValidationException("Parameter periodName is required.");

//         var kpiPeriod = await _kpiPeriodRepository.FindByIdAsync(id);
//         if (kpiPeriod == null)
//             // TODO: NotFoundException
//             throw new NotFoundException($"KPI Period with id {id} not found.");

//         return kpiPeriod.ToGetDto();
//     }
//     catch (Exception ex)
//     {
//         _logger.LogError(ex, "Unexpected error while querying department by department code.");
//         throw new Exception("An unexpected error occurred. Please try again later.");
//     }
// }

// public async Task<IEnumerable<KpiPeriodGetDto>> FindAll_Async()
// {
//     try
//     {
//         var kpiPeriods = await _kpiPeriodRepository.FindAllAsync();

//         return kpiPeriods.ToGetDto();
//     }
//     catch (Exception ex)
//     {
//         // Log unexpected errors
//         _logger.LogError(ex, "Unexpected error while querying KPI Periods.");
//         // throw; // Propagate to global exception handler
//         throw new Exception("An unexpected error occurred. Please try again later.");
//     }
// }

// public async Task<IEnumerable<KpiPeriodDto>> FindAllInsecure_Async()
// {
//     try
//     {
//         var kpiPeriods = await _kpiPeriodRepository.FindAllAsync();

//         return kpiPeriods.Select(e => new KpiPeriodDto
//         {
//             Id = e.Id,
//             PeriodName = e.PeriodName,
//             SubmissionStartDate = e.SubmissionStartDate,
//             SubmissionEndDate = e.SubmissionEndDate
//         }).ToList();
//     }
//     catch (Exception ex)
//     {
//         _logger.LogError(ex, "Unexpected error while querying KPI Periods.");
//         throw new Exception("An unexpected error occurred. Please try again later.");
//     }
// }

// public async Task<IEnumerable<KpiPeriodDto>> FindAllByValidDate_Async(DateTimeOffset todayDate)
// {
//     try
//     {

//         var kpiPeriods = await _kpiPeriodRepository.FindAllAsQueryable()
//             // startDate >= today && endDate <= today
//             // startDate <= today >= endDate
//             // today >= startDate && today <= endDate
//             .Where(e => e.SubmissionStartDate <= todayDate && e.SubmissionEndDate >= todayDate)
//             .ToListAsync();

//         return kpiPeriods.Select(e => new KpiPeriodDto
//         {
//             Id = e.Id,
//             PeriodName = e.PeriodName,
//             SubmissionStartDate = e.SubmissionStartDate,
//             SubmissionEndDate = e.SubmissionEndDate
//         }).ToList();
//     }
//     catch (Exception ex)
//     {
//         _logger.LogError(ex, "Unexpected error while querying KPI Periods.");
//         throw new Exception("An unexpected error occurred. Please try again later.");
//     }
// }

// public async Task<bool> Delete_Async(string periodName)
// {
//     try
//     {
//         var kpiPeriod = await _kpiPeriodRepository.FindByPeriodNameAsync(periodName);
//         if (kpiPeriod == null)
//             return true; // Idempotent: Treat as success

//         if (kpiPeriod == null)
//             // TODO: NotFoundException
//             throw new Exception("KPI Period not found.");

//         _kpiPeriodRepository.Delete(kpiPeriod);
//         await _context.SaveChangesAsync();

//         return true;
//     }
//     catch (DbUpdateException ex)
//     {
//         // Handle database-specific errors
//         _logger.LogError(ex, "Database error while deleting department.");
//         // TODO: DatabaseException
//         throw new Exception("A database error occurred.");
//     }
//     catch (Exception ex)
//     {
//         // Log unexpected errors
//         _logger.LogError(ex, "Unexpected error while deleting department.");
//         // throw; // Propagate to global exception handler
//         throw new Exception("An unexpected error occurred. Please try again later.");
//     }
// }

// public async Task<KpiPeriodGetDto> Create_Async(KpiPeriodCreateDto createDto)
// {
//     try
//     {
//         if (await _kpiPeriodRepository.KpiPeriodExistsAsync(createDto.PeriodName.ToString()))
//             // TODO: DuplicateEntityException
//             throw new DuplicateContentException("KPI Period name already exist.");

//         var entity = createDto.ToEntity();
//         _kpiPeriodRepository.Create(entity);
//         await _context.SaveChangesAsync();

//         return createDto.ToGetDto();
//     }
//     catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
//     {
//         _logger.LogError(ex, pgEx.MessageText);
//         throw new DuplicateContentException("KPI Period already exist.", ex.InnerException);
//     }
//     catch (DbUpdateException ex)
//     {
//         _logger.LogError(ex, "Database error while creating department.");
//         // TODO: DatabaseException
//         throw new Exception("A database error occurred.");
//     }
//     catch (Exception ex)
//     {
//         _logger.LogError(ex, "Unexpected error while creating department.");
//         // throw new Exception("An unexpected error occurred. Please try again later.");
//         throw;
//     }
// }