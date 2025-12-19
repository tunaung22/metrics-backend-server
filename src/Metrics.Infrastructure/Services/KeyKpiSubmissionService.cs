using Metrics.Application.Common.Mappers;
using Metrics.Application.DTOs.KeyKpiSubmissions;
using Metrics.Application.Interfaces.IRepositories;
using Metrics.Application.Interfaces.IServices;
using Metrics.Application.Results;
using Metrics.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Metrics.Infrastructure.Services;

public class KeyKpiSubmissionService(
    ILogger<KeyKpiSubmissionService> logger,
    MetricsDbContext context,
    IKeyKpiSubmissionRepository keyKpiSubmissionRepo) : IKeyKpiSubmissionService
{
    private readonly ILogger<KeyKpiSubmissionService> _logger = logger;
    private readonly MetricsDbContext _context = context;
    private readonly IKeyKpiSubmissionRepository _keyKpiSubmissionRepo = keyKpiSubmissionRepo;


    public async Task<Result> SubmitSubmissionsAsync(List<CreateKeyKpiSubmissionDto> createDtos)
    {
        try
        {
            var submittedAt = DateTimeOffset.UtcNow;
            foreach (var dto in createDtos)
            {
                // check existing
                // by submitter, period, key id
                var existingEntry = await _keyKpiSubmissionRepo
                    .FindBySubmitterByDepartmentKeyMetricAsync(
                        submitterId: dto.SubmitterId,
                        departmentKeyMetricId: dto.DepartmentKeyMetricId);
                //
                if (existingEntry != null)
                {
                    // update

                }
                else
                {
                    // insert
                    _keyKpiSubmissionRepo.Add(dto.MapToEntity());
                }
            }
            await _context.SaveChangesAsync();

            return Result.Success();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "DB Update error while creating submission. {e}", ex.Message);
            return Result.Fail("Failed to submit key kpi submissions.", ErrorType.DatabaseError);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to submit key kpi submissions. {msg}", ex.Message);
            return Result.Fail("Failed to submit key kpi submissions.", ErrorType.UnexpectedError);
        }
    }

    public async Task<Result> DeleteByPeriodByCandidateAsync(long periodId, string candidateId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            await _keyKpiSubmissionRepo.DeleteByPeriodByCandidateAsync(periodId, candidateId);
            await transaction.CommitAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError("Failed to delete key kpi submissions by period id by user id. {msg}", ex.Message);
            return Result.Fail("Failed to delete key kpi submissions by period by candidate.", ErrorType.UnexpectedError);
        }
    }


    // public async Task<ResultT<List<KeyKpiSubmissionDto>>> FindByPeriodAsync(long periodId)
    // {
    //     try
    //     {
    //         var data = await _keyKpiSubmissionRepo.findbyperio
    //         return ResultT<List<KeyKpiSubmissionDto>>.Success();

    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Unexpected error while querying key metric submissions by period");
    //         return ResultT<List<KeyKpiSubmissionDto>>.Fail("Failed to find submissions by period.", ErrorType.UnexpectedError);
    //     }
    // }

    public async Task<ResultT<List<KeyKpiSubmissionDto>>> FindByPeriodAsync(long periodId, bool includeLockedUser = true)
    {
        try
        {
            var data = await _keyKpiSubmissionRepo.FindByPeriodAsync(periodId, includeLockedUser);
            var result = data.Select(submission => submission.MapToDto()).ToList();

            return ResultT<List<KeyKpiSubmissionDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed fetching submissions by period id. {msg}", ex.Message);
            return ResultT<List<KeyKpiSubmissionDto>>.Fail("Failed to fetch submissions by period", ErrorType.UnexpectedError);
        }
    }

    public async Task<ResultT<List<KeyKpiSubmissionDto>>> FindByPeriodBySubmitterAsync(long periodId, string userId)
    {
        try
        {
            var data = await _keyKpiSubmissionRepo.FindByPeriodBySubmitterAsync(periodId, userId);
            var result = data.Select(submission => submission.MapToDto()).ToList();

            return ResultT<List<KeyKpiSubmissionDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed fetching submissions by period id by submitter id. {msg}", ex.Message);
            return ResultT<List<KeyKpiSubmissionDto>>.Fail("Failed to fetch submissions by period by submitter.", ErrorType.UnexpectedError);
        }
    }

    public async Task<ResultT<List<KeyKpiSubmissionDto>>> FindByPeriodBySubmitterAsync(List<long> periodIds, string userId)
    {
        try
        {
            var data = await _keyKpiSubmissionRepo.FindByPeriodBySubmitterAsync(periodIds, userId);
            var result = data.Select(submission => submission.MapToDto()).ToList();

            return ResultT<List<KeyKpiSubmissionDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed fetching submissions by period id by submitter id. {msg}", ex.Message);
            return ResultT<List<KeyKpiSubmissionDto>>.Fail("Failed to fetch submissions by period by submitter.", ErrorType.UnexpectedError);
        }
    }



    public async Task<ResultT<List<KeyKpiSubmissionDto>>> FindByDepartmentKeyMetricsAsync(List<long> departmentKeyMetricIDs)
    {
        try
        {
            var data = await _keyKpiSubmissionRepo.FindByDepartmentKeyMetricListAsync(departmentKeyMetricIDs);
            var result = data.Select(submission => submission.MapToDto()).ToList();

            return ResultT<List<KeyKpiSubmissionDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed fetching submissions by department key metric list. {msg}", ex.Message);
            return ResultT<List<KeyKpiSubmissionDto>>.Fail("Failed to fetch submissions by department key metric list.", ErrorType.UnexpectedError);
        }
    }

    // public async Task<ResultT<long>> FindCountByUserByPeriodAsync(
    //     string currentUserId, long kpiPeriodId)
    // {
    //     try
    //     {
    //         var countResult = await _keyKpiSubmissionRepo.FindCountByPeriodBySubmitterAsync(
    //             submitterId: currentUserId,
    //             periodId: kpiPeriodId);

    //         return ResultT<long>.Success(countResult);
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Failed to get submission count by User by Period.");
    //         return ResultT<long>.Fail("Failed to get submission count by User by Period.", ErrorType.UnexpectedError);
    //     }
    // }

    public async Task<ResultT<long>> FindCountByPeriodBySubmitterAsync(long kpiPeriodId, string currentUserId)
    {
        try
        {
            var countResult = await _keyKpiSubmissionRepo.FindCountByPeriodBySubmitterAsync(
                submitterId: currentUserId,
                periodId: kpiPeriodId);

            return ResultT<long>.Success(countResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get submission count by User by Period.");
            return ResultT<long>.Fail("Failed to get submission count by User by Period.", ErrorType.UnexpectedError);
        }
    }

    public async Task<ResultT<long>> FindCountByPeriodBySubmitterAsync(List<long> kpiPeriodIds, string currentUserId)
    {
        try
        {
            var countResult = await _keyKpiSubmissionRepo.FindCountByPeriodBySubmitterAsync(
                submitterId: currentUserId,
                periodIds: kpiPeriodIds);

            return ResultT<long>.Success(countResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get submission count by User by Period.");
            return ResultT<long>.Fail("Failed to get submission count by User by Period.", ErrorType.UnexpectedError);
        }
    }

    public async Task<ResultT<Dictionary<long, int>>> FindSubmissionsCountDictByPeriodBySubmitterAsync(List<long> kpiPeriodIds, string currentUserId)
    {
        try
        {
            var countResult = await _keyKpiSubmissionRepo.FindSubmissionCountsByPeriodBySubmitterAsync(
                submitterId: currentUserId,
                periodIds: kpiPeriodIds);

            return ResultT<Dictionary<long, int>>.Success(countResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get submission count by User by Period.");
            return ResultT<Dictionary<long, int>>.Fail("Failed to get submission count by User by Period.", ErrorType.UnexpectedError);
        }
    }






    // public async Task<List<KpiSubmission>> FindBySubmitterByPeriodByDepartmentListAsync(
    //     ApplicationUser candidate,
    //     long kpiPeriodId,
    //     List<long> departmentIdList)
    // {
    //     try
    //     {
    //         // submissions by Period by User by Department (of DKM)
    //         // var foundSubmissions = await _keyMetricSubmissionRepository.FindAllAsQueryable()
    //         //     .Include(e => e.TargetPeriod)
    //         //     .Include(e => e.TargetDepartment)
    //         //     .Include(e => e.SubmittedBy)
    //         //     .Include(e => e.KeyKpiSubmissionItems)
    //         //     .Where(e => e.SubmittedBy.Id == candidate.Id
    //         //         && e.ScoreSubmissionPeriodId == kpiPeriodId
    //         //         && departmentIdList.Any(d => d == e.DepartmentId))
    //         //     .ToListAsync();
    //         // REMOVE: Repository 
    //         var query = await _keyKpiSubmissionRepo.FindAllByPeriodBySubmitterByDepartmentAsync(
    //             periodId: kpiPeriodId,
    //             submitterId: candidate.Id,
    //             departmentId: depart
    //         )


    //         //  _context.KeyKpiSubmissions
    //         //     .Where(e => e.SubmittedBy.Id == candidate.Id
    //         //         && e.ScoreSubmissionPeriodId == kpiPeriodId
    //         //         // && departmentIdList.Any(departmentId => departmentId == e.DepartmentId))
    //         //         && departmentIdList.Contains(e.TargetDepartmentKeyMetric.DepartmentId))
    //         //     .OrderBy(e => e.SubmittedAt)
    //         //     .Include(e => e.TargetPeriod)
    //         //     .Include(e => e.TargetDepartmentKeyMetric)
    //         //         .ThenInclude(i => i.KeyMetric)
    //         //     .Include(e => e.TargetDepartmentKeyMetric)
    //         //         .ThenInclude(i => i.KeyIssueDepartment)
    //         //     .Include(e => e.SubmittedBy)
    //         //         .ThenInclude(i => i.Department);

    //         }

    //         return [];
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Unexpected error while querying key metric submissions by submittter by period by department list.");
    //         throw new Exception("An unexpected error occurred. Please try again later.");
    //     }
    // }
}
