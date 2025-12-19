using Metrics.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Metrics.Application.Interfaces.IServices;
using Metrics.Application.Interfaces.IRepositories;
using Metrics.Application.Exceptions;
using Metrics.Application.Domains;
using Metrics.Application.Results;
using Metrics.Application.DTOs.KpiSubmissionDtos;
using Metrics.Application.Common.Mappers;

namespace Metrics.Infrastructure.Services;

public class KpiSubmissionService(
    MetricsDbContext context,
    ILogger<KpiSubmissionService> logger,
    IKpiSubmissionRepository kpiSubmissionRepository) : IKpiSubmissionService
{
    private readonly MetricsDbContext _context = context;
    private readonly ILogger<KpiSubmissionService> _logger = logger;
    private readonly IKpiSubmissionRepository _kpiSubmissionRepository = kpiSubmissionRepository;

    public async Task<ResultT<List<KpiSubmissionDto>>> FindByPeriod_Async(long kpiPeriodId, bool includeLockedUsers = true)
    {
        try
        {
            var query = _context.KpiSubmissions
                .Where(e => e.KpiSubmissionPeriodId == kpiPeriodId)
                .Include(e => e.TargetPeriod)
                .Include(e => e.TargetDepartment)
                .Include(e => e.SubmittedBy).ThenInclude(user => user.UserTitle)
                .Include(e => e.SubmittedBy).ThenInclude(user => user.Department)
                .OrderBy(e => e.SubmissionDate)
                .AsQueryable();
            List<KpiSubmissionDto> data = [];

            if (includeLockedUsers)
            {
                var result = await query.AsNoTracking().ToListAsync();
                data = result.Select(e => e.MapToDto()).ToList(); // entity to dto
            }
            else
            {
                var result = await query.AsNoTracking()
                    .Where(s => s.SubmittedBy.LockoutEnd == null ||
                        s.SubmittedBy.LockoutEnd <= DateTimeOffset.UtcNow)
                    .ToListAsync();
                data = result.Select(e => e.MapToDto()).ToList(); // entity to dto
            }
            return ResultT<List<KpiSubmissionDto>>.Success(data);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to load kpi submissions by period. {msg}", ex.Message);
            return ResultT<List<KpiSubmissionDto>>.Fail("Failed to load kpi submissions by period.", ErrorType.UnexpectedError);
        }
    }

    // ========== Return Entity ================================================
    public async Task<KpiSubmission> CreateAsync(KpiSubmission submission)
    {
        try
        {
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

    // private async Task<KpiSubmission> GetByIdAsync(long id)
    // {
    //     try
    //     {
    //         var kpiSubmission = await _kpiSubmissionRepository.FindByIdAsync(id);

    //         if (kpiSubmission == null)
    //             throw new NotFoundException($"Submission with id {id} not found.");

    //         return new KpiSubmission
    //         {
    //             SubmittedAt = kpiSubmission.SubmittedAt,
    //             SubmissionDate = kpiSubmission.SubmissionDate,
    //             ScoreValue = kpiSubmission.ScoreValue,
    //             KpiSubmissionPeriodId = kpiSubmission.TargetPeriod.Id,
    //             DepartmentId = kpiSubmission.TargetDepartment.Id,
    //             ApplicationUserId = kpiSubmission.SubmittedBy.Id
    //         };
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Unexpected error while querying submission by id.");
    //         throw new Exception("An unexpected error occurred. Please try again later.");
    //     }
    // }

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

    /// <summary>
    /// Delete all submission by period
    /// </summary>
    /// <param name="periodId"></param>
    /// <param name="includeLockedUsers"></param>
    /// <returns></returns>
    public async Task<Result> DeleteByPeriodAsync(long periodId, bool includeLockedUsers = true)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            await _kpiSubmissionRepository.DeleteByPeriodAsync(periodId, includeLockedUsers);
            await transaction.CommitAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError("Failed to delete submissions by period id. {msg}", ex.Message);
            return Result.Fail("Failed to delete submissions by period.", ErrorType.UnexpectedError);
        }
    }

    public async Task<Result> DeleteByPeriodByCandidateAsync(long periodId, string candidateId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            await _kpiSubmissionRepository.DeleteByPeriodByCandidateAsync(periodId, candidateId);
            await transaction.CommitAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError("Failed to delete submissions by period id by user id. {msg}", ex.Message);
            return Result.Fail("Failed to delete submissions by period by candidate.", ErrorType.UnexpectedError);
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



}
