using Metrics.Application.Domains;
using Metrics.Application.Exceptions;
using Metrics.Application.Interfaces.IServices;
using Metrics.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using Npgsql.Internal;
using System.Data.Common;

namespace Metrics.Infrastructure.Services;

public class CaseFeedbackSubmissionService : ICaseFeedbackSubmissionService
{
    private readonly ILogger<CaseFeedbackSubmissionService> _logger;
    private readonly MetricsDbContext _context;

    public CaseFeedbackSubmissionService(
        ILogger<CaseFeedbackSubmissionService> logger,
        MetricsDbContext context
        )
    {
        _logger = logger;
        _context = context;

    }

    public async Task SaveAsync(CaseFeedbackSubmission submission)
    {
        try
        {
            _context.CaseFeedbackSubmissions.Add(submission);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
            {
                _logger.LogError(ex, pgEx.MessageText);
                throw new MetricsDuplicateContentException("Submission already exist.", ex.InnerException);
            }
            else
            {
                _logger.LogError(ex, "Database error while creating case feedback submission.");
                throw new Exception("A database error occurred.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Error saving CaseFeedbackSubmissions. Error: {ex}", ex);
            throw new Exception("Error saving CaseFeedbackSubmissions");
        }
    }

    public async Task UpdateAsync(string lookupId, CaseFeedbackSubmission entity)
    {
        try
        {
            // find
            var targetSubmission = await FindByLookupIdAsync(lookupId);
            if (targetSubmission == null)
                throw new MetricsNotFoundException("Case feedback submission not found.");
            // update
            targetSubmission.IncidentAt = entity.IncidentAt;
            targetSubmission.CaseDepartmentId = entity.CaseDepartmentId;
            targetSubmission.NegativeScoreValue = entity.NegativeScoreValue;
            targetSubmission.WardName = entity.WardName;
            targetSubmission.CPINumber = entity.CPINumber;
            targetSubmission.PatientName = entity.PatientName;
            targetSubmission.RoomNumber = entity.RoomNumber;
            targetSubmission.Description = entity.Description;
            targetSubmission.Comments = entity.Comments;

            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
            {
                _logger.LogError(ex, pgEx.MessageText);
                throw new MetricsDuplicateContentException("Submission already exist.", ex.InnerException);
            }
            else
            {
                _logger.LogError(ex, "Database error while creating case feedback submission.");
                throw new Exception("A database error occurred.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Error saving CaseFeedbackSubmissions. Error: {ex}", ex);
            throw new Exception("Error saving CaseFeedbackSubmissions");
        }
    }

    public async Task<CaseFeedbackSubmission?> FindByLookupIdAsync(string lookupId)
    {
        try
        {
            var submission = await _context.CaseFeedbackSubmissions
                .Include(e => e.CaseDepartment)
                .Include(e => e.SubmittedBy)
                    .ThenInclude(e => e.Department)
                .Include(e => e.SubmittedBy.UserTitle)
                .Include(e => e.TargetPeriod)
                .Where(e => e.LookupId == Guid.Parse(lookupId))
                .FirstOrDefaultAsync();

            return submission;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while querying case feedback submission by lookup id.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<List<CaseFeedbackSubmission>> FindByKpiPeriodAndSubmitterAsync(
        long periodId,
        string userId)
    {
        try
        {
            var submissions = await _context.CaseFeedbackSubmissions
                .Include(e => e.CaseDepartment)
                .Include(e => e.SubmittedBy)
                    .ThenInclude(e => e.Department)
                .Include(e => e.SubmittedBy.UserTitle)
                .Include(e => e.TargetPeriod)
                .Where(e =>
                    e.KpiSubmissionPeriodId == periodId
                    && e.SubmitterId == userId)
                .ToListAsync();

            return submissions ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while querying case feedback submissions by period by submitter.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<List<CaseFeedbackSubmission>> FindByKpiPeriodAsync(long periodId)
    {
        try
        {
            var submissions = await _context.CaseFeedbackSubmissions
                .Include(e => e.CaseDepartment)
                .Include(e => e.SubmittedBy)
                    .ThenInclude(e => e.Department)
                .Include(e => e.SubmittedBy.UserTitle)
                .Include(e => e.TargetPeriod)
                .Where(e => e.KpiSubmissionPeriodId == periodId)
                .ToListAsync();

            return submissions ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while querying case feedback submissions by period.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<List<CaseFeedbackSubmission>> FindAllAsync()
    {
        try
        {
            var result = await _context.CaseFeedbackSubmissions
                .Include(e => e.CaseDepartment)
                .Include(e => e.SubmittedBy)
                    .ThenInclude(e => e.Department)
                .Include(e => e.SubmittedBy.UserTitle)
                .OrderBy(e => e.SubmissionDate)
                .ThenBy(e => e.SubmittedBy.FullName)
                .ThenBy(e => e.CaseDepartment.DepartmentName)
                .ToListAsync();

            return result ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while querying case feedback submissions.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public IQueryable<CaseFeedbackSubmission> FindAllAsQueryable()
    {
        var q = _context.CaseFeedbackSubmissions
            .OrderBy(e => e.SubmissionDate)
            .ThenBy(e => e.SubmittedBy.FullName)
            .ThenBy(e => e.CaseDepartment.DepartmentName)
            .AsQueryable();

        return q;
    }


}
