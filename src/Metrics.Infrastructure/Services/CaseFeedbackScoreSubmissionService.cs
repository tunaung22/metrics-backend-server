using Metrics.Application.Common.Mappers;
using Metrics.Application.Domains;
using Metrics.Application.DTOs;
using Metrics.Application.DTOs.CaseFeedbackScoreSubmission;
using Metrics.Application.Exceptions;
using Metrics.Application.Interfaces.IRepositories;
using Metrics.Application.Interfaces.IServices;
using Metrics.Application.Results;
using Metrics.Infrastructure.Data;
using Metrics.Infrastructure.Data.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Microsoft.Extensions.Logging;

namespace Metrics.Infrastructure.Services;

public class CaseFeedbackScoreSubmissionService(
    ILogger<CaseFeedbackScoreSubmissionService> logger,
    MetricsDbContext context,
    ICaseFeedbackScoreSubmissionRepository submissionRepo)
    : ICaseFeedbackScoreSubmissionService
{
    private readonly ILogger<CaseFeedbackScoreSubmissionService> _logger = logger;
    private readonly MetricsDbContext _context = context;
    private readonly ICaseFeedbackScoreSubmissionRepository _submissionRepo = submissionRepo;

    public async Task SaveAsync(CaseFeedbackScoreSubmission submission)
    {
        try
        {
            _submissionRepo.Add(submission);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.IsUniqueConstraintViolation())
        {
            _logger.LogError(ex, "Duplicate key violation.");
            throw new MetricsDuplicateContentException("Submission already exist.", ex);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error during create.");
            throw;
        }
    }

    public async Task SaveRangeAsync(
        List<CaseFeedbackScoreSubmissionCreateDto> createDto)
    {
        try
        {
            // dto to entity
            var entities = createDto.Select(dto => dto.MapToEntity()).ToList();
            // save
            _submissionRepo.AddRange(entities);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database error during create many.");
            throw;
        }
    }

    public async Task<Result> UpsertRangeAsync(List<CaseFeedbackScoreSubmissionUpsertDto> submissions)
    {
        // submission -> existingSubmissions    -> update (existingSubmissions)
        // submission -> newSubmissions         -> insert (newSubmissions)
        try
        {
            // get id list
            var IDs = submissions.Select(s => s.Id).ToList();
            if (IDs.Count == 0)
                return Result.Fail("Submissions list is empty.", ErrorType.InvalidArgument);

            var existingSubmissions = await _submissionRepo.FindByIdAsync(IDs);
            var existingIDs = existingSubmissions.Select(s => s.Id).ToHashSet();

            var toUpdateDto = submissions
                .Where(submission => existingIDs.Contains(submission.Id))
                .Select(submission => submission.MapToUpdateDto())
                .ToList();
            var toUpdate = toUpdateDto
                .Select(updateDto => updateDto.MapToEntity())
                .ToList();

            var toInsertDto = submissions
                .Where(submission => !existingIDs.Contains(submission.Id))
                .Select(submission => submission.MapToCreateDto())
                .ToList();
            var toInsert = toInsertDto
                .Select(insertDto => insertDto.MapToEntity())
                .ToList();

            if (toUpdate.Count > 0)
            {
                await PrepareUpdateAsync(toUpdate);
            }
            if (toInsert.Count > 0)
            {
                _submissionRepo.AddRange(toInsert);
            }

            await _context.SaveChangesAsync();
            return Result.Success();
        }
        catch (DbUpdateException ex) when (ex.IsUniqueConstraintViolation())
        {
            _logger.LogError(ex, "Duplicate key violated on UpsertRangeAsync: {message}", ex.Message);
            return Result.Fail("Duplicate record exists.", ErrorType.DuplicateKey);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database update failed for UpsertRangeAsync: {message}", ex.Message);
            return Result.Fail("Failed to save case feedback submissions.", ErrorType.DatabaseError);
        }
        catch (Exception ex)
        {
            return Result.Fail(ex.Message, ErrorType.UnexpectedError);
        }
    }

    public async Task UpdateAsync(
        string lookupId,
        CaseFeedbackScoreSubmission entity)
    {
        try
        {
            await _submissionRepo.UpdateAsync(lookupId, entity);
            await _context.SaveChangesAsync();
        }
        catch (MetricsNotFoundException)
        {
            _logger.LogError("Submission not found for update.");
            throw;
        }
        catch (DbUpdateException ex) when (ex.IsUniqueConstraintViolation())
        {
            _logger.LogError(ex, "Duplicate key voilation.");
            throw new MetricsDuplicateContentException("Submission already exist.", ex);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database error during update.");
            throw;
        }
    }

    private async Task PrepareUpdateAsync(List<CaseFeedbackScoreSubmission> entities)
    {
        var IDs = entities.Select(s => s.Id).ToList();
        var entitiesToUpdate = await _submissionRepo.FindByIdAsync(IDs);

        var updateLookup = entities.ToDictionary(e => e.Id); // id as dict key

        foreach (var entity in entitiesToUpdate)
        {
            if (updateLookup.TryGetValue(entity.Id, out var updated))
            {
                entity.NegativeScoreValue = updated.NegativeScoreValue;
                entity.Comments = updated.Comments;
                entity.ModifiedAt = DateTimeOffset.UtcNow;
            }
        }
    }

    public async Task<Result> UpdateRangeAsync(List<CaseFeedbackScoreSubmission> entities)
    {
        try
        {
            await PrepareUpdateAsync(entities);
            await _context.SaveChangesAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception at UpsertRangeAsync: {message}", ex.Message);
            return Result.Fail("Failed to update submissions.", ErrorType.UnexpectedError);
        }
    }

    public Task<CaseFeedbackScoreSubmission?> FindByLookupIdAsync(string lookupId)
    {
        if (string.IsNullOrEmpty(lookupId))
            throw new ArgumentException("Lookup ID cannot be empty", nameof(lookupId));

        return _submissionRepo.FindByLookupIdAsync(lookupId);
    }

    // public async Task<List<CaseFeedbackScoreSubmission>> FindByKpiPeriodAndSubmitterAsync(
    //     long periodId,
    //     string userId)
    // {
    //     return await _submissionRepo.FindByKpiPeriodAndSubmitterAsync(
    //         periodId: periodId,
    //         userId: userId);
    // }

    public async Task<ResultT<List<CaseFeedbackScoreSubmissionDto>>> FindByKpiPeriodAndSubmitterAsync(
        long periodId,
        string userId)
    {
        try
        {
            var submissions = await _submissionRepo.FindByKpiPeriodAndSubmitterAsync(
                periodId: periodId,
                userId: userId);
            if (submissions.Count > 0)
            {
                // entity -> dto
                var data = submissions
                    .Select(submission => submission.MapToDto())
                    .ToList();

                return ResultT<List<CaseFeedbackScoreSubmissionDto>>
                    .Success(data);
            }
            return ResultT<List<CaseFeedbackScoreSubmissionDto>>.Success([]);
        }
        catch (Exception ex)
        {
            return ResultT<List<CaseFeedbackScoreSubmissionDto>>.Fail(ex.Message, ErrorType.UnexpectedError);
        }

    }


    // public async Task<List<CaseFeedbackScoreSubmission>> FindByKpiPeriodAsync(long periodId)
    // {
    //     return await _submissionRepo.FindAllByKpiPeriodAsync(periodId);
    // }
    public async Task<ResultT<List<CaseFeedbackScoreSubmissionDto>>> FindByKpiPeriodAsync(long periodId)
    {
        try
        {
            var submissions = await _submissionRepo.FindAllByKpiPeriodAsync(periodId);
            if (submissions.Count > 0)
            {
                var data = submissions
                    .Select(submission => submission.MapToDto())
                    .ToList();

                return ResultT<List<CaseFeedbackScoreSubmissionDto>>.Success(data);
            }
            return ResultT<List<CaseFeedbackScoreSubmissionDto>>.Success([]);
        }
        catch (Exception ex)
        {
            return ResultT<List<CaseFeedbackScoreSubmissionDto>>.Fail(ex.Message, ErrorType.UnexpectedError);
        }
    }


    public Task<List<CaseFeedbackScoreSubmission>> FindAllAsync()
    {
        throw new NotImplementedException();
    }


}
