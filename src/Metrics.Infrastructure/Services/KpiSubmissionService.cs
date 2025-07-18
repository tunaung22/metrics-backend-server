using Metrics.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Metrics.Application.Interfaces.IServices;
using Metrics.Application.Interfaces.IRepositories;
using Metrics.Application.Exceptions;
using Metrics.Application.Domains;

namespace Metrics.Infrastructure.Services;

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


    // ========== Return Entity ================================================
    public async Task<KpiSubmission> CreateAsync(KpiSubmission submission)
    {
        try
        {
            // **NOTE: submission time use from input or system time?? 
            var submissionDate = DateOnly.FromDateTime(submission.SubmittedAt.LocalDateTime.ToLocalTime());

            // var departmentId = submission.DepartmentId;
            var submissionsExist = await _kpiSubmissionRepository
                .KpiSubmissionExistsAsync(
                    submission.KpiSubmissionPeriodId,
                    submission.DepartmentId,
                    submission.ApplicationUserId);
            if (submissionsExist)
                throw new DuplicateContentException("Submission already exist.");

            var submissionEntity = new KpiSubmission
            {
                SubmittedAt = submission.SubmittedAt,
                ScoreValue = submission.ScoreValue,
                PositiveAspects = submission.PositiveAspects,
                NegativeAspects = submission.NegativeAspects,
                Comments = submission.Comments,
                KpiSubmissionPeriodId = submission.KpiSubmissionPeriodId,
                DepartmentId = submission.DepartmentId,
                ApplicationUserId = submission.ApplicationUserId
            };
            _kpiSubmissionRepository.Create(submissionEntity);
            var created = await _context.SaveChangesAsync();

            if (created <= 0)
                throw new DbUpdateException("Database error while creating submission.");

            var newSubmission = await FindByIdAsync(submissionEntity.Id);

            return newSubmission;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating submission.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<int> CreateRangeAsync(List<KpiSubmission> submissions)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            List<KpiSubmission> entities = submissions.Select(s => new KpiSubmission
            {
                SubmittedAt = s.SubmittedAt,
                ScoreValue = s.ScoreValue,
                PositiveAspects = s.PositiveAspects,
                NegativeAspects = s.NegativeAspects,
                Comments = s.Comments,
                KpiSubmissionPeriodId = s.KpiSubmissionPeriodId,
                DepartmentId = s.DepartmentId,
                ApplicationUserId = s.ApplicationUserId
            }).ToList();

            _kpiSubmissionRepository.CreateRange(entities);
            var affectedRows = await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return affectedRows;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Unexpected error while creating submission.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<KpiSubmission> UpdateAsync(DateOnly submissionDate, KpiSubmission submission)
    {
        try
        {
            var targetSubmission = await _kpiSubmissionRepository.FindBySubmissionDateAsync(submissionDate);
            if (targetSubmission == null)
                throw new NotFoundException($"Submissions not found for {submissionDate}.");

            // Handle concurrency (example using row version)
            // if (existing.RowVersion != department.RowVersion)
            //     return Result<Department>.Fail("Concurrency conflict.");

            _kpiSubmissionRepository.Update(targetSubmission);

            // **TODO: SHOULD SUBMISSION EDITABLE??
            // CURRENTLY DO NOTHING
            // targetSubmission. = updateDto.DepartmentName;

            await _context.SaveChangesAsync();

            throw new NotImplementedException();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while updating submission.");
            throw new Exception("An unexpected error occurred. Please try again later.");

        }
    }

    public async Task<bool> DeleteBySubmissionDateAsync(DateOnly submissionDate)
    {
        try
        {
            var kpiSubmission = await _kpiSubmissionRepository.FindBySubmissionDateAsync(submissionDate);
            if (kpiSubmission == null)
                throw new NotFoundException($"Submissions not found on {submissionDate}.");
            // or return true; // Idempotent: Treat as success

            _kpiSubmissionRepository.Delete(kpiSubmission);

            return await _context.SaveChangesAsync() > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting department.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<KpiSubmission> FindByIdAsync(long id)
    {
        if (id <= 0)
            throw new ArgumentNullException("Parameter id is required.");

        try
        {
            var kpiSubmission = await _kpiSubmissionRepository.FindByIdAsQueryable(id)
                .Include(e => e.TargetPeriod)
                .Include(e => e.TargetDepartment)
                .Include(e => e.SubmittedBy)
                .FirstOrDefaultAsync();

            if (kpiSubmission == null)
                throw new NotFoundException($"Submission with id {id} not found.");

            return new KpiSubmission
            {
                SubmittedAt = kpiSubmission.SubmittedAt,
                SubmissionDate = kpiSubmission.SubmissionDate,
                ScoreValue = kpiSubmission.ScoreValue,
                KpiSubmissionPeriodId = kpiSubmission.TargetPeriod.Id,
                DepartmentId = kpiSubmission.TargetDepartment.Id,
                ApplicationUserId = kpiSubmission.SubmittedBy.Id
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while querying submission by id.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public Task<KpiSubmission> FindBySubmissionDateAsync(DateOnly submissionDate)
    {
        throw new NotImplementedException();
    }

    public async Task<List<KpiSubmission>> FindByKpiPeriodAsync(long kpiPeriodId)
    {
        try
        {
            var foundSubmissions = await _kpiSubmissionRepository.FindAllAsQueryable()
                .Include(e => e.TargetPeriod)
                .Include(e => e.TargetDepartment)
                .Include(e => e.SubmittedBy)
                .OrderBy(e => e.SubmissionDate)
                // .Where(e => e.ApplicationUserId == candidateId
                //     && e.KpiSubmissionPeriodId == kpiPeriodId)
                .Where(e => e.KpiSubmissionPeriodId == kpiPeriodId)
                .ToListAsync();

            if (foundSubmissions != null)
            {
                return foundSubmissions;
                // return foundSubmissions.Select(s => new KpiSubmission
                // {
                //     SubmittedAt = s.SubmittedAt,
                //     SubmissionDate = s.SubmissionDate,
                //     ScoreValue = s.ScoreValue,
                //     PositiveAspects = s.PositiveAspects,
                //     NegativeAspects = s.NegativeAspects,
                //     Comments = s.Comments,
                //     KpiSubmissionPeriodId = s.KpiSubmissionPeriodId,
                //     DepartmentId = s.DepartmentId,
                //     TargetDepartment = s.TargetDepartment,
                //     ApplicationUserId = s.ApplicationUserId
                //     // KpiPeriod = s.KpiPeriod.PeriodName,
                //     // Department = s.TargetDepartment.DepartmentName,
                //     // Candidate = s.Candidate.FullName
                // }).ToList();
            }
            return [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while querying submissions by period.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<List<KpiSubmission>> FindByKpiPeriodAndSubmitterAsync(long kpiPeriodId, string candidateId)
    {
        try
        {
            var foundSubmissions = await _kpiSubmissionRepository.FindAllAsQueryable()
                .Include(e => e.TargetPeriod)
                .Include(e => e.TargetDepartment)
                .Include(e => e.SubmittedBy)
                .OrderBy(e => e.SubmissionDate)
                .Where(e => e.ApplicationUserId == candidateId
                    && e.KpiSubmissionPeriodId == kpiPeriodId)
                .ToListAsync();

            if (foundSubmissions != null)
            {
                return foundSubmissions;
                // return foundSubmissions.Select(s => new KpiSubmission
                // {
                //     SubmittedAt = s.SubmittedAt,
                //     SubmissionDate = s.SubmissionDate,
                //     ScoreValue = s.ScoreValue,
                //     PositiveAspects = s.PositiveAspects,
                //     NegativeAspects = s.NegativeAspects,
                //     Comments = s.Comments,
                //     KpiSubmissionPeriodId = s.KpiSubmissionPeriodId,
                //     DepartmentId = s.DepartmentId,
                //     TargetDepartment = s.TargetDepartment,
                //     ApplicationUserId = s.ApplicationUserId
                //     // KpiPeriod = s.KpiPeriod.PeriodName,
                //     // Department = s.TargetDepartment.DepartmentName,
                //     // Candidate = s.Candidate.FullName
                // }).ToList();
            }
            return [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while querying submissions by candidate and period.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<List<KpiSubmission>> FindBySubmitterAndKpiPeriodAndDepartmentListAsync(ApplicationUser candidate, long kpiPeriodId, List<long> departmentIds)
    {
        try
        {
            var foundSubmissions = await _kpiSubmissionRepository.FindAllAsQueryable()
                .Include(e => e.TargetPeriod)
                .Include(e => e.TargetDepartment)
                .Include(e => e.SubmittedBy)
                .OrderBy(e => e.SubmissionDate)
                .Where(e => e.ApplicationUserId == candidate.Id
                    && e.KpiSubmissionPeriodId == kpiPeriodId
                    && departmentIds.Any(d => e.DepartmentId == d))
                .ToListAsync();

            if (foundSubmissions != null)
            {
                return foundSubmissions.Select(s => new KpiSubmission
                {
                    SubmittedAt = s.SubmittedAt,
                    SubmissionDate = s.SubmissionDate,
                    ScoreValue = s.ScoreValue,
                    KpiSubmissionPeriodId = s.KpiSubmissionPeriodId,
                    DepartmentId = s.DepartmentId,
                    ApplicationUserId = s.ApplicationUserId
                    // KpiPeriod = s.KpiPeriod.PeriodName,
                    // Department = s.TargetDepartment.DepartmentName,
                    // Candidate = s.Candidate.FullName
                }).ToList();
            }
            return [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while querying score submission by submitter by period by department list.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }
    public async Task<List<KpiSubmission>> FindByKpiPeriodAndDepartmentListAsync(long kpiPeriodId, List<long> departmentIds)
    {
        try
        {
            var foundSubmissions = await _kpiSubmissionRepository.FindAllAsQueryable()
                .Include(e => e.TargetPeriod)
                .Include(e => e.TargetDepartment)
                .Include(e => e.SubmittedBy)
                .OrderBy(e => e.SubmissionDate)
                .Where(e => e.KpiSubmissionPeriodId == kpiPeriodId
                    && departmentIds.Any(d => e.DepartmentId == d))
                .ToListAsync();

            if (foundSubmissions != null)
            {
                return foundSubmissions.Select(s => new KpiSubmission
                {
                    SubmittedAt = s.SubmittedAt,
                    SubmissionDate = s.SubmissionDate,
                    ScoreValue = s.ScoreValue,
                    KpiSubmissionPeriodId = s.KpiSubmissionPeriodId,
                    DepartmentId = s.DepartmentId,
                    ApplicationUserId = s.ApplicationUserId
                    // KpiPeriod = s.KpiPeriod.PeriodName,
                    // Department = s.TargetDepartment.DepartmentName,
                    // Candidate = s.Candidate.FullName
                }).ToList();
            }
            return [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while querying department.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }


    public async Task<List<KpiSubmission>> FindByKpiPeriodAndDepartmentAsync(long kpiPeriodId, long departmentId)
    {
        try
        {
            var foundedSubmissions = await _kpiSubmissionRepository.FindAllAsQueryable()
                .Include(e => e.TargetPeriod)
                .Include(e => e.TargetDepartment)
                .Include(e => e.SubmittedBy)
                .OrderBy(e => e.SubmissionDate)
                .Where(e => e.KpiSubmissionPeriodId == kpiPeriodId
                    && e.DepartmentId == departmentId)
                .ToListAsync();

            if (foundedSubmissions != null)
            {
                return foundedSubmissions.Select(s => new KpiSubmission
                {
                    SubmittedAt = s.SubmittedAt,
                    SubmissionDate = s.SubmissionDate,
                    ScoreValue = s.ScoreValue,
                    KpiSubmissionPeriodId = s.KpiSubmissionPeriodId,
                    DepartmentId = s.DepartmentId,
                    ApplicationUserId = s.ApplicationUserId,
                    TargetPeriod = s.TargetPeriod,
                    TargetDepartment = s.TargetDepartment,
                    SubmittedBy = s.SubmittedBy
                }).ToList();
            }
            return [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while querying department.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<IEnumerable<KpiSubmission>> FindAllAsync()
    {
        try
        {
            var submissions = await _kpiSubmissionRepository.FindAllAsQueryable()
                .Include(e => e.TargetPeriod)
                .Include(e => e.TargetDepartment)
                .Include(e => e.SubmittedBy)
                .OrderBy(e => e.SubmissionDate)
                .AsNoTracking()
                .ToListAsync();

            if (submissions.Count > 0)
                return submissions;
            // return submissions.Select(e => new KpiSubmissionDto
            // {
            //     SubmissionTime = e.SubmissionTime,
            //     SubmissionDate = e.SubmissionDate,
            //     KpiScore = e.KpiScore,
            //     KpiPeriodId = e.KpiPeriodId,
            //     // KpiPeriod = e.KpiPeriod.PeriodName,
            //     DepartmentId = e.DepartmentId,
            //     // Department = e.TargetDepartment.DepartmentName,
            //     EmployeeId = e.EmployeeId
            //     // Candidate = e.Candidate.FullName
            // }).ToList();

            return [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while querying departments.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }


    public async Task<long> FindCountByUserIdByKpiPeriodIdAsync(string currentUserId, long kpiPeriodId)
    {
        try
        {
            return await _kpiSubmissionRepository
                .FindCountByUserIdByKpiPeriodIdAsync(currentUserId, kpiPeriodId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while counting submissions.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }


    // ========== Return DTO ===================================================
    // public async Task<KpiSubmissionGetDto> Create_Async(KpiSubmissionCreateDto createDto)
    // {
    //     try
    //     {
    //         var submissionDate = DateOnly.FromDateTime(createDto.SubmissionTime.LocalDateTime.ToLocalTime());
    //         var departmentId = createDto.DepartmentId;
    //         if (await _kpiSubmissionRepository.KpiSubmissionExistsAsync(createDto.KpiPeriodId, createDto.DepartmentId, createDto.EmployeeId))
    //             throw new DuplicateContentException("Submission already exist.");

    //         // var entity = createDto.ToEntity();
    //         var entity = new KpiSubmission
    //         {
    //             SubmissionTime = createDto.SubmissionTime,
    //             KpiScore = createDto.KpiScore,
    //             Comments = createDto.Comments,
    //             KpiPeriodId = createDto.KpiPeriodId,
    //             DepartmentId = createDto.DepartmentId,
    //             EmployeeId = createDto.EmployeeId
    //         };
    //         _kpiSubmissionRepository.Create(entity);
    //         await _context.SaveChangesAsync();

    //         var newSubmission = await FindById_Async(entity.Id);

    //         return newSubmission;
    //         // return new KpiSubmissionGetDto
    //         // {
    //         //     SubmissionTime = createDto.SubmissionTime,
    //         //     KpiScore = createDto.KpiScore,
    //         //     Comments = createDto.Comments,
    //         //     KpiPeriodId = entity.id
    //         //     // DepartmentId = createDto.DepartmentId,
    //         //     // EmployeeId = createDto.EmployeeId
    //         // };
    //     }
    //     catch (DbUpdateException ex)
    //     {
    //         // when (ex.InnerException is PostgresException postgresEx && postgresEx.SqlState == "23505")
    //         if (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
    //         {
    //             _logger.LogError(ex, pgEx.MessageText);
    //             // TODO: DuplicateEntityException
    //             throw new DuplicateContentException("Submission already exist.", ex.InnerException);
    //         }
    //         else
    //         {
    //             // Handle database-specific errors
    //             _logger.LogError(ex, "Database error while creating submission.");
    //             // TODO: DatabaseException
    //             throw new Exception("A database error occurred.");
    //         }
    //     }
    //     catch (Exception ex)
    //     {
    //         // Log unexpected errors
    //         _logger.LogError(ex, "Unexpected error while creating submission.");
    //         throw new Exception("An unexpected error occurred. Please try again later.");
    //     }
    // }

    // public async Task<int> CreateRange_Async(List<KpiSubmissionCreateDto> createDtos)
    // {
    //     using var transaction = await _context.Database.BeginTransactionAsync();

    //     try
    //     {
    //         List<KpiSubmission> entities = createDtos.Select(dto => new KpiSubmission
    //         {
    //             SubmissionTime = dto.SubmissionTime,
    //             KpiScore = dto.KpiScore,
    //             Comments = dto.Comments,
    //             KpiPeriodId = dto.KpiPeriodId,
    //             DepartmentId = dto.DepartmentId,
    //             EmployeeId = dto.EmployeeId
    //         }).ToList();

    //         _kpiSubmissionRepository.CreateRange(entities);
    //         var affectedRows = await _context.SaveChangesAsync();
    //         await transaction.CommitAsync();

    //         // var result = createDtos.Select(dto => new KpiSubmissionDto
    //         // {
    //         //     SubmissionTime = dto.SubmissionTime,
    //         //     KpiScore = dto.KpiScore,
    //         //     Comments = dto.Comments,
    //         //     KpiPeriodId = dto.KpiPeriodId,
    //         //     DepartmentId = dto.DepartmentId,
    //         //     EmployeeId = dto.EmployeeId
    //         // }).ToList();

    //         return affectedRows;
    //     }
    //     catch (DbUpdateException ex)
    //     {
    //         // when (ex.InnerException is PostgresException postgresEx && postgresEx.SqlState == "23505")
    //         if (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
    //         {
    //             _logger.LogError(ex, pgEx.MessageText);
    //             // TODO: DuplicateEntityException
    //             throw new DuplicateContentException("Submission already exist.", ex.InnerException);
    //         }
    //         else
    //         {
    //             // Handle database-specific errors
    //             _logger.LogError(ex, "Database error while creating submission.");
    //             // TODO: DatabaseException
    //             throw new Exception("A database error occurred.");
    //         }
    //     }
    //     catch (Exception ex)
    //     {
    //         // Log unexpected errors
    //         _logger.LogError(ex, "Unexpected error while creating submission.");
    //         throw new Exception("An unexpected error occurred. Please try again later.");
    //     }
    // }

    // public async Task<KpiSubmissionGetDto> Update_Async(DateOnly submissionDate, KpiSubmissionUpdateDto updateDto)
    // {
    //     try
    //     {
    //         // Validate existence
    //         var targetSubmission = await _kpiSubmissionRepository.FindBySubmissionDateAsync(submissionDate);
    //         if (targetSubmission == null)
    //             throw new NotFoundException($"Submissions not found for {submissionDate}.");

    //         // Handle concurrency (example using row version)
    //         // if (existing.RowVersion != department.RowVersion)
    //         //     return Result<Department>.Fail("Concurrency conflict.");

    //         _kpiSubmissionRepository.Update(targetSubmission);

    //         // **TODO: SHOULD SUBMISSION EDITABLE??
    //         // CURRENTLY DO NOTHING
    //         // targetSubmission. = updateDto.DepartmentName;

    //         await _context.SaveChangesAsync();

    //         throw new NotImplementedException();
    //     }
    //     catch (DbUpdateException ex)
    //     {
    //         if (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
    //         {
    //             _logger.LogError(ex, pgEx.MessageText);
    //             throw new Exception("Duplicate entry exception occurred.");
    //         }
    //         else
    //         {
    //             _logger.LogError(ex, "Database error while updating submission.");
    //             throw new Exception("A database error occurred.");
    //         }
    //     }
    //     catch (Exception ex)
    //     {
    //         // Log unexpected errors
    //         _logger.LogError(ex, "Unexpected error while updating submission.");
    //         throw new Exception("An unexpected error occurred. Please try again later.");

    //     }
    // }

    // public async Task<bool> Delete_Async(DateOnly submissionDate)
    // {
    //     try
    //     {
    //         var kpiSubmission = await _kpiSubmissionRepository.FindBySubmissionDateAsync(submissionDate);
    //         if (kpiSubmission == null)
    //             return true; // Idempotent: Treat as success

    //         if (kpiSubmission == null)
    //             // TODO: NotFoundException
    //             throw new NotFoundException("KPI Submission not found.");

    //         _kpiSubmissionRepository.Delete(kpiSubmission);
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

    // public async Task<KpiSubmissionGetDto> FindById_Async(long id)
    // {
    //     try
    //     {
    //         if (id <= 0)
    //             // TODO: ValidationException
    //             throw new Exception("Parameter id is required.");

    //         var kpiSubmission = await _kpiSubmissionRepository.FindByIdAsQueryable(id)
    //             .Include(e => e.KpiPeriod)
    //             .Include(e => e.TargetDepartment)
    //             .Include(e => e.Candidate)
    //             .FirstOrDefaultAsync();

    //         if (kpiSubmission == null)
    //             // TODO: NotFoundException
    //             throw new NotFoundException($"Submission with id {id} not found.");

    //         return new KpiSubmissionGetDto
    //         {
    //             SubmissionTime = kpiSubmission.SubmissionTime,
    //             SubmissionDate = kpiSubmission.SubmissionDate,
    //             KpiScore = kpiSubmission.KpiScore,
    //             KpiPeriod = kpiSubmission.KpiPeriod.PeriodName,
    //             Department = kpiSubmission.TargetDepartment.DepartmentName,
    //             Candidate = kpiSubmission.Candidate.FullName
    //         };
    //     }
    //     catch (Exception ex)
    //     {
    //         // Log unexpected errors
    //         _logger.LogError(ex, "Unexpected error while querying submission by id.");
    //         // throw; // Propagate to global exception handler
    //         throw new Exception("An unexpected error occurred. Please try again later.");
    //     }
    // }

    // public Task<KpiSubmissionGetDto> FindBySubmissionDate_Async(DateOnly submissionDate)
    // {
    //     throw new NotImplementedException();
    // }

    // public async Task<IEnumerable<KpiSubmissionDto>> Find_Async(long employeeId, long kpiPeriodId, List<long> departmentIds)
    // {
    //     try
    //     {
    //         var submissions = await _kpiSubmissionRepository.FindAllAsQueryable()
    //             .Include(e => e.KpiPeriod)
    //             .Include(e => e.TargetDepartment)
    //             .Include(e => e.Candidate)
    //             .OrderBy(e => e.SubmissionDate)
    //             .Where(e => e.EmployeeId == employeeId
    //                 && e.KpiPeriodId == kpiPeriodId
    //                 && departmentIds.Any(d => e.DepartmentId == d))
    //             .ToListAsync();

    //         if (submissions != null)
    //         {
    //             return submissions.Select(s => new KpiSubmissionDto
    //             {
    //                 SubmissionTime = s.SubmissionTime,
    //                 SubmissionDate = s.SubmissionDate,
    //                 KpiScore = s.KpiScore,
    //                 KpiPeriodId = s.KpiPeriodId,
    //                 DepartmentId = s.DepartmentId,
    //                 EmployeeId = s.EmployeeId
    //                 // KpiPeriod = s.KpiPeriod.PeriodName,
    //                 // Department = s.TargetDepartment.DepartmentName,
    //                 // Candidate = s.Candidate.FullName
    //             }).ToList();
    //         }
    //         return [];
    //     }
    //     catch (NotFoundException)
    //     {
    //         // throw new NotFoundException("Submission does not exist.");
    //         return [];
    //     }
    //     catch (Exception ex)
    //     {
    //         // Log unexpected errors
    //         _logger.LogError(ex, "Unexpected error while querying department.");
    //         // throw; // Propagate to global exception handler
    //         throw new Exception("An unexpected error occurred. Please try again later.");
    //     }
    // }

    // public async Task<KpiSubmissionGetDto?> Find_Async(long employeeId, long kpiPeriodId, long departmentId)
    // {
    //     try
    //     {
    //         var submission = await _kpiSubmissionRepository.FindAsQueryable(kpiPeriodId, departmentId, employeeId)
    //             .Include(e => e.KpiPeriod)
    //             .Include(e => e.TargetDepartment)
    //             .Include(e => e.Candidate)
    //             .OrderBy(e => e.SubmissionDate)
    //             .FirstOrDefaultAsync();

    //         if (submission != null)
    //         {
    //             return new KpiSubmissionGetDto
    //             {
    //                 SubmissionTime = submission.SubmissionTime,
    //                 SubmissionDate = submission.SubmissionDate,
    //                 KpiScore = submission.KpiScore,
    //                 KpiPeriod = submission.KpiPeriod.PeriodName,
    //                 Department = submission.TargetDepartment.DepartmentName,
    //                 Candidate = submission.Candidate.FullName
    //             };
    //         }
    //         return null;
    //     }
    //     catch (NotFoundException)
    //     {
    //         // throw new NotFoundException("Submission does not exist.");
    //         return null;
    //     }
    //     catch (Exception ex)
    //     {
    //         // Log unexpected errors
    //         _logger.LogError(ex, "Unexpected error while querying department.");
    //         // throw; // Propagate to global exception handler
    //         throw new Exception("An unexpected error occurred. Please try again later.");
    //     }
    // }

    // public async Task<IEnumerable<KpiSubmissionGetDto>> FindAll_Async()
    // {
    //     try
    //     {
    //         var submissions = _kpiSubmissionRepository.FindAllAsQueryable()
    //             .Include(e => e.KpiPeriod)
    //             .Include(e => e.TargetDepartment)
    //             .Include(e => e.Candidate)
    //             .OrderBy(e => e.SubmissionDate);

    //         // return departments.ToGetDto();
    //         return await submissions.Select(e => new KpiSubmissionGetDto
    //         {
    //             SubmissionTime = e.SubmissionTime,
    //             SubmissionDate = e.SubmissionDate,
    //             KpiScore = e.KpiScore,
    //             KpiPeriod = e.KpiPeriod.PeriodName,
    //             Department = e.TargetDepartment.DepartmentName,
    //             Candidate = e.Candidate.FullName
    //         }).ToListAsync();
    //     }
    //     catch (Exception ex)
    //     {
    //         // Log unexpected errors
    //         _logger.LogError(ex, "Unexpected error while querying departments.");
    //         // throw; // Propagate to global exception handler
    //         throw new Exception("An unexpected error occurred. Please try again later.");
    //     }
    // }

    // TODO: remove this
    // public async Task<IEnumerable<KpiSubmissionDto>> FindAllInsecure_Async(long employeeId, long kpiPeriodId, long departmentId)
    // {
    //     try
    //     {
    //         var departments = await _kpiSubmissionRepository.FindAllAsQueryable()
    //             .Include(e => e.KpiPeriod)
    //             .Include(e => e.TargetDepartment)
    //             .Include(e => e.Candidate)
    //             .OrderBy(e => e.SubmissionDate)
    //             .Where(e => e.EmployeeId == employeeId
    //                 && e.KpiPeriodId == kpiPeriodId
    //                 && e.DepartmentId == departmentId)
    //             .ToListAsync();
    //         if (departments.Count > 0)
    //         {
    //             return departments.Select(e => new KpiSubmissionDto
    //             {
    //                 SubmissionTime = e.SubmissionTime,
    //                 SubmissionDate = e.SubmissionDate,
    //                 KpiScore = e.KpiScore,
    //                 KpiPeriodId = e.KpiPeriodId,
    //                 // KpiPeriod = e.KpiPeriod.PeriodName,
    //                 DepartmentId = e.DepartmentId,
    //                 // Department = e.TargetDepartment.DepartmentName,
    //                 EmployeeId = e.EmployeeId
    //                 // Candidate = e.Candidate.FullName
    //             }).ToList();
    //         }

    //         return [];
    //     }
    //     catch (Exception ex)
    //     {
    //         // Log unexpected errors
    //         _logger.LogError(ex, "Unexpected error while querying departments.");
    //         // throw; // Propagate to global exception handler
    //         throw new Exception("An unexpected error occurred. Please try again later.");
    //     }
    // }

    // public async Task<IEnumerable<KpiSubmissionDto>> FindAllInsecure_Async()
    // {
    //     try
    //     {
    //         var departments = await _kpiSubmissionRepository.FindAllAsQueryable()
    //             .Include(e => e.KpiPeriod)
    //             .Include(e => e.TargetDepartment)
    //             .Include(e => e.Candidate)
    //             .OrderBy(e => e.SubmissionDate)
    //             // .Where(e => e.EmployeeId == employeeId
    //             // && e.KpiPeriodId == kpiPeriodId
    //             // && e.DepartmentId == departmentId)
    //             .AsNoTracking()
    //             .ToListAsync();

    //         if (departments.Count > 0)
    //         {
    //             return departments.Select(e => new KpiSubmissionDto
    //             {
    //                 SubmissionTime = e.SubmissionTime,
    //                 SubmissionDate = e.SubmissionDate,
    //                 KpiScore = e.KpiScore,
    //                 KpiPeriodId = e.KpiPeriodId,
    //                 // KpiPeriod = e.KpiPeriod.PeriodName,
    //                 DepartmentId = e.DepartmentId,
    //                 // Department = e.TargetDepartment.DepartmentName,
    //                 EmployeeId = e.EmployeeId
    //                 // Candidate = e.Candidate.FullName
    //             }).ToList();
    //         }

    //         return [];
    //     }
    //     catch (Exception ex)
    //     {
    //         // Log unexpected errors
    //         _logger.LogError(ex, "Unexpected error while querying departments.");
    //         // throw; // Propagate to global exception handler
    //         throw new Exception("An unexpected error occurred. Please try again later.");
    //     }
    // }


}
