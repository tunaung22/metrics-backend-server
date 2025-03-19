using Metrics.Application.DTOs.KpiSubmissionDtos;
using Metrics.Application.Services.IServices;
using Metrics.Common.Utils;
using Metrics.Domain.Exceptions;
using Metrics.Domain.Entities;
using Metrics.Infrastructure.Data;
using Metrics.Infrastructure.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;

namespace Metrics.Application.Services;

public class KpiSubmissionService : IKpiSubmissionService
{
    private readonly MetricsDbContext _context;
    private readonly ILogger<KpiSubmissionService> _logger;
    private readonly IKpiSubmissionRepository _kpiSubmissionRepository;

    public KpiSubmissionService(
        MetricsDbContext context,
        ILogger<KpiSubmissionService> logger,
        IKpiSubmissionRepository kpiSubmissionRepository)
    {
        _context = context;
        _logger = logger;
        _kpiSubmissionRepository = kpiSubmissionRepository;
    }

    public async Task<KpiSubmissionGetDto> Create_Async(KpiSubmissionCreateDto createDto)
    {
        try
        {
            var submissionDate = DateOnly.FromDateTime(createDto.SubmissionTime.LocalDateTime.ToLocalTime());
            var departmentId = createDto.DepartmentId;
            if (await _kpiSubmissionRepository.KpiSubmissionExists2Async(createDto.KpiPeriodId, createDto.DepartmentId, createDto.EmployeeId))
                throw new DuplicateContentException("Submission already exist.");

            // var entity = createDto.ToEntity();
            var entity = new KpiSubmission
            {
                SubmissionTime = createDto.SubmissionTime,
                KpiScore = createDto.KpiScore,
                Comments = createDto.Comments,
                KpiPeriodId = createDto.KpiPeriodId,
                DepartmentId = createDto.DepartmentId,
                EmployeeId = createDto.EmployeeId
            };
            _kpiSubmissionRepository.Create(entity);
            await _context.SaveChangesAsync();

            var newSubmission = await FindById_Async(entity.Id);

            return newSubmission;
            // return new KpiSubmissionGetDto
            // {
            //     SubmissionTime = createDto.SubmissionTime,
            //     KpiScore = createDto.KpiScore,
            //     Comments = createDto.Comments,
            //     KpiPeriodId = entity.id
            //     // DepartmentId = createDto.DepartmentId,
            //     // EmployeeId = createDto.EmployeeId
            // };
        }
        catch (DbUpdateException ex)
        {
            // when (ex.InnerException is PostgresException postgresEx && postgresEx.SqlState == "23505")
            if (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
            {
                _logger.LogError(ex, pgEx.MessageText);
                // TODO: DuplicateEntityException
                throw new DuplicateContentException("Submission already exist.", ex.InnerException);
            }
            else
            {
                // Handle database-specific errors
                _logger.LogError(ex, "Database error while creating submission.");
                // TODO: DatabaseException
                throw new Exception("A database error occurred.");
            }
        }
        catch (Exception ex)
        {
            // Log unexpected errors
            _logger.LogError(ex, "Unexpected error while creating submission.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<List<KpiSubmissionDto>> CreateRange_Async(List<KpiSubmissionCreateDto> createDtos)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            List<KpiSubmission> entities = createDtos.Select(dto => new KpiSubmission
            {
                SubmissionTime = dto.SubmissionTime,
                KpiScore = dto.KpiScore,
                Comments = dto.Comments,
                KpiPeriodId = dto.KpiPeriodId,
                DepartmentId = dto.DepartmentId,
                EmployeeId = dto.EmployeeId
            }).ToList();

            _kpiSubmissionRepository.CreateRange(entities);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            var result = createDtos.Select(dto => new KpiSubmissionDto
            {
                SubmissionTime = dto.SubmissionTime,
                KpiScore = dto.KpiScore,
                Comments = dto.Comments,
                KpiPeriodId = dto.KpiPeriodId,
                DepartmentId = dto.DepartmentId,
                EmployeeId = dto.EmployeeId
            }).ToList();

            return result;
        }
        catch (DbUpdateException ex)
        {
            // when (ex.InnerException is PostgresException postgresEx && postgresEx.SqlState == "23505")
            if (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
            {
                _logger.LogError(ex, pgEx.MessageText);
                // TODO: DuplicateEntityException
                throw new DuplicateContentException("Submission already exist.", ex.InnerException);
            }
            else
            {
                // Handle database-specific errors
                _logger.LogError(ex, "Database error while creating submission.");
                // TODO: DatabaseException
                throw new Exception("A database error occurred.");
            }
        }
        catch (Exception ex)
        {
            // Log unexpected errors
            _logger.LogError(ex, "Unexpected error while creating submission.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<bool> Delete_Async(DateOnly submissionDate)
    {
        try
        {
            var kpiSubmission = await _kpiSubmissionRepository.FindBySubmissionDateAsync(submissionDate);
            if (kpiSubmission == null)
                return true; // Idempotent: Treat as success

            if (kpiSubmission == null)
                // TODO: NotFoundException
                throw new Exception("KPI Submission not found.");

            _kpiSubmissionRepository.Delete(kpiSubmission);
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

    public async Task<IEnumerable<KpiSubmissionGetDto>> FindAll_Async()
    {
        try
        {
            var departments = _kpiSubmissionRepository.FindAllAsQueryable()
                .Include(e => e.KpiPeriod)
                .Include(e => e.TargetDepartment)
                .Include(e => e.Candidate)
                .OrderBy(e => e.SubmissionDate);

            // return departments.ToGetDto();
            return await departments.Select(e => new KpiSubmissionGetDto
            {
                SubmissionTime = e.SubmissionTime,
                SubmissionDate = e.SubmissionDate,
                KpiScore = e.KpiScore,
                KpiPeriod = e.KpiPeriod.PeriodName,
                Department = e.TargetDepartment.DepartmentName,
                Candidate = e.Candidate.FullName
            }).ToListAsync();
        }
        catch (Exception ex)
        {
            // Log unexpected errors
            _logger.LogError(ex, "Unexpected error while querying departments.");
            // throw; // Propagate to global exception handler
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<KpiSubmissionGetDto> FindById_Async(long id)
    {
        try
        {
            if (id <= 0)
                // TODO: ValidationException
                throw new Exception("Parameter id is required.");

            var kpiSubmission = await _kpiSubmissionRepository.FindByIdAsQueryable(id)
                .Include(e => e.KpiPeriod)
                .Include(e => e.TargetDepartment)
                .Include(e => e.Candidate)
                .FirstOrDefaultAsync();

            if (kpiSubmission == null)
                // TODO: NotFoundException
                throw new NotFoundException($"Department with id {id} not found.");

            return new KpiSubmissionGetDto
            {
                SubmissionTime = kpiSubmission.SubmissionTime,
                SubmissionDate = kpiSubmission.SubmissionDate,
                KpiScore = kpiSubmission.KpiScore,
                KpiPeriod = kpiSubmission.KpiPeriod.PeriodName,
                Department = kpiSubmission.TargetDepartment.DepartmentName,
                Candidate = kpiSubmission.Candidate.FullName
            };
        }
        catch (Exception ex)
        {
            // Log unexpected errors
            _logger.LogError(ex, "Unexpected error while querying submission by id.");
            // throw; // Propagate to global exception handler
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public Task<KpiSubmissionGetDto> FindBySubmissionDate_Async(DateOnly submissionDate)
    {
        throw new NotImplementedException();
    }

    public async Task<KpiSubmissionGetDto> Update_Async(DateOnly submissionDate, KpiSubmissionUpdateDto updateDto)
    {
        try
        {
            // Validate existence
            var targetSubmission = await _kpiSubmissionRepository.FindBySubmissionDateAsync(submissionDate);
            if (targetSubmission == null)
                // TODO: NotFoundException
                throw new NotFoundException("Department not found.");

            // Handle concurrency (example using row version)
            // if (existing.RowVersion != department.RowVersion)
            //     return Result<Department>.Fail("Concurrency conflict.");

            // Track changes

            _kpiSubmissionRepository.Update(targetSubmission);

            // **TODO: SHOULD SUBMISSION EDITABLE??
            // targetSubmission. = updateDto.DepartmentName;

            await _context.SaveChangesAsync();

            // return targetSubmission.ToGetDto();
            throw new NotImplementedException();
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
            {
                _logger.LogError(ex, pgEx.MessageText);
                // TODO: DuplicateEntityException
                throw new Exception("Duplicate entry exception occurred.");
            }
            else
            {
                // Handle database-specific errors
                _logger.LogError(ex, "Database error while updating submission.");
                // TODO: DatabaseException
                throw new Exception("A database error occurred.");
            }
            // Handle database-specific errors
        }
        catch (Exception ex)
        {
            // Log unexpected errors
            _logger.LogError(ex, "Unexpected error while updating submission.");
            throw new Exception("An unexpected error occurred. Please try again later.");

        }
    }
}
