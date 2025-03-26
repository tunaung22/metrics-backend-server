using Metrics.Application.Mappers.DtoMappers;
using Metrics.Application.DTOs.KpiPeriodDtos;
using Metrics.Application.Services.IServices;
using Metrics.Infrastructure;
using Metrics.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using Metrics.Infrastructure.Repositories.IRepositories;
using Metrics.Application.Results;
using Metrics.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Metrics.Domain.Exceptions;
using Npgsql;

namespace Metrics.Application.Services;

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

    public async Task<Result<KpiPeriod>> CreateAsync(KpiPeriod entity)
    {
        try
        {
            if (await _kpiPeriodRepository.KpiPeriodExistsAsync(entity.PeriodName))
                return Result<KpiPeriod>.Fail("KPI Period already exist.", ErrorType.DuplicateKey);

            _kpiPeriodRepository.Create(entity);
            await _context.SaveChangesAsync();

            return Result<KpiPeriod>.Ok(entity);
        }
        catch (DbUpdateException ex)
        {
            // Handle database-specific errors
            _logger.LogError(ex, "Database error while creating department.");
            return Result<KpiPeriod>.Fail("A database error occurred.", ErrorType.DatabaseError);
        }
        catch (Exception ex)
        {
            // Log unexpected errors
            _logger.LogError(ex, "Unexpected error while creating department.");
            return Result<KpiPeriod>.Fail("An unexpected error occurred. Please try again later.", ErrorType.UnexpectedError);
        }
    }


    public async Task<Result<bool>> DeleteAsync(string periodName)
    {
        try
        {
            var kpiPeriod = await _kpiPeriodRepository.FindByPeriodNameAsync(periodName);
            if (kpiPeriod == null)
                return Result<bool>.Ok(true);

            _kpiPeriodRepository.Delete(kpiPeriod);
            await _context.SaveChangesAsync();

            return Result<bool>.Ok(true);
        }
        catch (DbUpdateException ex)
        {
            // Handle database-specific errors
            _logger.LogError(ex, "Database error while creating department.");
            return Result<bool>.Fail("A database error occurred.", ErrorType.DatabaseError);
        }
        catch (Exception ex)
        {
            // Log unexpected errors
            _logger.LogError(ex, "Unexpected error while creating department.");
            return Result<bool>.Fail("An unexpected error occurred. Please try again later.", ErrorType.UnexpectedError);
        }
    }



    public async Task<Result<IEnumerable<KpiPeriod>>> FindAllAsync()
    {
        try
        {
            var kpiPeriods = await _kpiPeriodRepository.FindAllAsync();

            return Result<IEnumerable<KpiPeriod>>.Ok(kpiPeriods);
        }
        catch (Exception ex)
        {
            // Log unexpected errors
            _logger.LogError(ex, "Unexpected error while creating department.");
            return Result<IEnumerable<KpiPeriod>>.Fail("An unexpected error occurred. Please try again later.", ErrorType.UnexpectedError);
        }
    }



    public async Task<Result<KpiPeriod>> FindByIdAsync(long id)
    {
        try
        {
            var kpiPeriod = await _kpiPeriodRepository.FindByIdAsync(id);
            if (kpiPeriod == null)
                return Result<KpiPeriod>.Fail("Kpi Period not found.", ErrorType.NotFound);

            return Result<KpiPeriod>.Ok(kpiPeriod);
        }
        catch (Exception ex)
        {
            // Log unexpected errors
            _logger.LogError(ex, "Unexpected error while creating department.");
            return Result<KpiPeriod>.Fail("An unexpected error occurred. Please try again later.", ErrorType.UnexpectedError);
        }
    }


    public async Task<Result<KpiPeriod>> FindByKpiPeriodNameAsync(string periodName)
    {
        try
        {
            var kpiPeriod = await _kpiPeriodRepository.FindByPeriodNameAsync(periodName);
            if (kpiPeriod == null)
                return Result<KpiPeriod>.Fail("Kpi Period not found.", ErrorType.NotFound);

            return Result<KpiPeriod>.Ok(kpiPeriod);
        }
        catch (Exception ex)
        {
            // Log unexpected errors
            _logger.LogError(ex, "Unexpected error while creating department.");
            return Result<KpiPeriod>.Fail("An unexpected error occurred. Please try again later.", ErrorType.UnexpectedError);
        }
    }



    public async Task<Result<KpiPeriod>> UpdateAsync(KpiPeriod entity)
    {
        try
        {
            var kpiPeriod = await _kpiPeriodRepository
                .FindByPeriodNameAsync(entity.PeriodName);
            if (kpiPeriod == null)
                return Result<KpiPeriod>.Fail("Kpi Period does not exist.", ErrorType.NotFound);

            _kpiPeriodRepository.Update(entity);
            await _context.SaveChangesAsync();

            return Result<KpiPeriod>.Ok(entity);
        }
        catch (DbUpdateException ex)
        {
            // Handle database-specific errors
            _logger.LogError(ex, "Database error while creating department.");
            return Result<KpiPeriod>.Fail("A database error occurred.", ErrorType.DatabaseError);
        }
        catch (Exception ex)
        {
            // Log unexpected errors
            _logger.LogError(ex, "Unexpected error while creating department.");
            return Result<KpiPeriod>.Fail("An unexpected error occurred. Please try again later.", ErrorType.UnexpectedError);
        }
    }



    // ================ DTO ===============================================
    public async Task<long> FindIdByKpiPeriodName_Async(string periodName)
    {
        try
        {
            if (string.IsNullOrEmpty(periodName))
                // TODO: ValidationException
                throw new Exception("Parameter periodName is required.");

            var kpiPeriod = await _kpiPeriodRepository.FindByPeriodNameAsync(periodName);

            if (kpiPeriod == null)
                // TODO: NotFoundException
                // throw new NotFoundException($"KPI Period with name {periodName} not found.");
                return 0;

            return kpiPeriod.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while querying department by department code.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<KpiPeriodGetDto> FindByKpiPeriodName_Async(string periodName)
    {
        try
        {
            if (string.IsNullOrEmpty(periodName))
                // TODO: ValidationException
                throw new Exception("Parameter periodName is required.");

            var kpiPeriod = await _kpiPeriodRepository.FindByPeriodNameAsync(periodName);
            if (kpiPeriod == null)
                // TODO: NotFoundException
                throw new Exception($"KPI Period with name {periodName} not found.");

            return kpiPeriod.ToGetDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while querying department by department code.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }
    public async Task<KpiPeriodGetDto> Update_Async(KpiPeriodUpdateDto updateDto)
    {
        try
        {
            // Validate existence
            var targetKpiPeriod = await _kpiPeriodRepository.FindByPeriodNameAsync(updateDto.PeriodName.ToString());
            if (targetKpiPeriod == null)
                // TODO: NotFoundException
                throw new NotFoundException("KPI Period not found.");

            // Handle concurrency (example using row version)
            // if (existing.RowVersion != department.RowVersion)
            //     return Result<Department>.Fail("Concurrency conflict.");

            // Track changes

            _kpiPeriodRepository.Update(targetKpiPeriod);

            // DTOs to Entity
            // targetKpiPeriod = updateDto.ToEntity(); // **will it work???
            targetKpiPeriod.SubmissionStartDate = updateDto.SubmissionStartDate;
            targetKpiPeriod.SubmissionEndDate = updateDto.SubmissionEndDate;

            await _context.SaveChangesAsync();

            return targetKpiPeriod.ToGetDto();
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
            {
                _logger.LogError(ex, pgEx.MessageText);
                throw new DuplicateContentException("Duplicate entry exception occurred.");
            }
            else
            {
                _logger.LogError(ex, "Database error while updating department.");
                throw new Exception("A database error occurred.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while updating departments.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<KpiPeriodGetDto> FindById_Async(long id)
    {
        try
        {
            if (id <= 0)
                // TODO: ValidationException
                throw new ValidationException("Parameter periodName is required.");

            var kpiPeriod = await _kpiPeriodRepository.FindByIdAsync(id);
            if (kpiPeriod == null)
                // TODO: NotFoundException
                throw new NotFoundException($"KPI Period with id {id} not found.");

            return kpiPeriod.ToGetDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while querying department by department code.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<IEnumerable<KpiPeriodGetDto>> FindAll_Async()
    {
        try
        {
            var kpiPeriods = await _kpiPeriodRepository.FindAllAsync();

            return kpiPeriods.ToGetDto();
        }
        catch (Exception ex)
        {
            // Log unexpected errors
            _logger.LogError(ex, "Unexpected error while querying KPI Periods.");
            // throw; // Propagate to global exception handler
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<IEnumerable<KpiPeriodDto>> FindAllInsecure_Async()
    {
        try
        {
            var kpiPeriods = await _kpiPeriodRepository.FindAllAsync();

            return kpiPeriods.Select(e => new KpiPeriodDto
            {
                Id = e.Id,
                PeriodName = e.PeriodName,
                SubmissionStartDate = e.SubmissionStartDate,
                SubmissionEndDate = e.SubmissionEndDate
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while querying KPI Periods.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<IEnumerable<KpiPeriodDto>> FindAllByValidDate_Async(DateTimeOffset todayDate)
    {
        try
        {

            var kpiPeriods = await _kpiPeriodRepository.FindAllAsQueryable()
                // startDate >= today && endDate <= today
                // startDate <= today >= endDate
                // today >= startDate && today <= endDate
                .Where(e => e.SubmissionStartDate <= todayDate && e.SubmissionEndDate >= todayDate)
                .ToListAsync();

            return kpiPeriods.Select(e => new KpiPeriodDto
            {
                Id = e.Id,
                PeriodName = e.PeriodName,
                SubmissionStartDate = e.SubmissionStartDate,
                SubmissionEndDate = e.SubmissionEndDate
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while querying KPI Periods.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<bool> Delete_Async(string periodName)
    {
        try
        {
            var kpiPeriod = await _kpiPeriodRepository.FindByPeriodNameAsync(periodName);
            if (kpiPeriod == null)
                return true; // Idempotent: Treat as success

            if (kpiPeriod == null)
                // TODO: NotFoundException
                throw new Exception("KPI Period not found.");

            _kpiPeriodRepository.Delete(kpiPeriod);
            await _context.SaveChangesAsync();

            return true;
        }
        catch (DbUpdateException ex)
        {
            // Handle database-specific errors
            _logger.LogError(ex, "Database error while deleting department.");
            // TODO: DatabaseException
            throw new Exception("A database error occurred.");
        }
        catch (Exception ex)
        {
            // Log unexpected errors
            _logger.LogError(ex, "Unexpected error while deleting department.");
            // throw; // Propagate to global exception handler
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<KpiPeriodGetDto> Create_Async(KpiPeriodCreateDto createDto)
    {
        try
        {
            if (await _kpiPeriodRepository.KpiPeriodExistsAsync(createDto.PeriodName.ToString()))
                // TODO: DuplicateEntityException
                throw new DuplicateContentException("KPI Period name already exist.");

            var entity = createDto.ToEntity();
            _kpiPeriodRepository.Create(entity);
            await _context.SaveChangesAsync();

            return createDto.ToGetDto();
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
        {
            _logger.LogError(ex, pgEx.MessageText);
            throw new DuplicateContentException("KPI Period already exist.", ex.InnerException);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error while creating department.");
            // TODO: DatabaseException
            throw new Exception("A database error occurred.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating department.");
            // throw new Exception("An unexpected error occurred. Please try again later.");
            throw;
        }
    }
    // private readonly IUnitOfWork _unitOfWork;

    // public KpiPeriodService(IUnitOfWork unitOfWork)
    // {
    //     _unitOfWork = unitOfWork;
    // }

    // // ===========================================================================
    // public async Task<KpiPeriodGetDto> CreateAsync(KpiPeriodCreateDto dto)
    // {
    //     if (await _unitOfWork.KpiPeriodRepository.IsPeriodNameExist(dto.PeriodName))
    //         throw new Exception($"KPI Peroid name {dto.PeriodName} already exist.");

    //     var entity = dto.ToEntity();
    //     await _unitOfWork.KpiPeriodRepository.CreateAsync(entity);
    //     if (await _unitOfWork.SaveChangesAsync() > 0)
    //     {
    //         return dto.ToGetDto();
    //     }

    //     throw new Exception("Create new KPI Period failed.");
    // }

    // public async Task<bool> DeleteAsync(string periodName)
    // {
    //     var kpiPeriod = await _unitOfWork.KpiPeriodRepository.FindByPeriodName(periodName);
    //     if (kpiPeriod == null)
    //         throw new Exception($"KPI Period name {periodName} does not exist.");

    //     _unitOfWork.KpiPeriodRepository.Delete(kpiPeriod);

    //     return await _unitOfWork.SaveChangesAsync() > 0;
    // }

    // public async Task<IEnumerable<KpiPeriodGetDto>> GetAllAsync()
    // {
    //     var kpiPeriod = await _unitOfWork.KpiPeriodRepository.FindAllAsync();
    //     if (kpiPeriod == null)
    //         return [];

    //     return kpiPeriod.ToGetDto();
    // }

    // public async Task<KpiPeriodGetDto> GetByKpiPeriodNameAsync(string periodName)
    // {
    //     var kpiPeriod = await _unitOfWork.KpiPeriodRepository.FindByPeriodName(periodName);
    //     if (kpiPeriod == null)
    //         throw new Exception("KPI period not found.");

    //     return kpiPeriod.ToGetDto()
    //         ?? throw new Exception("mapping to DTO failed.");
    // }

    // public Task<KpiPeriodGetDto> UpdateAsync(KpiPeriodUpdateDto dto)
    // {
    //     // Raise
    //     // not found
    //     // update fail
    //     throw new NotImplementedException();
    // }

    // public Task<bool> UpdatePeriodNameAsync(string periodName, string newPeriodName)
    // {
    //     throw new NotImplementedException();

    // }
}
