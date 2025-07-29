using Metrics.Application.Domains;
using Metrics.Application.Exceptions;
using Metrics.Application.Interfaces.IRepositories;
using Metrics.Application.Interfaces.IServices;
using Metrics.Infrastructure.Data;
using Metrics.Infrastructure.Data.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Metrics.Infrastructure.Services;

public class CaseFeedbackSubmissionService : ICaseFeedbackSubmissionService
{
    private readonly MetricsDbContext _context;
    private readonly ILogger<CaseFeedbackSubmissionService> _logger;
    private readonly ICaseFeedbackSubmissionRepository _caseFeedbackRepo;

    public CaseFeedbackSubmissionService(
        MetricsDbContext context,
        ILogger<CaseFeedbackSubmissionService> logger,
        ICaseFeedbackSubmissionRepository caseFeedbackSubmissionRepository
        )
    {
        _context = context;
        _logger = logger;
        _caseFeedbackRepo = caseFeedbackSubmissionRepository;
    }

    public async Task SaveAsync(CaseFeedbackSubmission submission)
    {
        try
        {
            _caseFeedbackRepo.Add(submission);
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

    public async Task UpdateAsync(string lookupId, CaseFeedbackSubmission entity)
    {
        try
        {
            await _caseFeedbackRepo.UpdateAsync(lookupId, entity);
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

    public async Task<CaseFeedbackSubmission?> FindByLookupIdAsync(string lookupId)
    {
        if (string.IsNullOrWhiteSpace(lookupId))
            throw new ArgumentException("Lookup ID cannot be empty", nameof(lookupId));

        return await _caseFeedbackRepo.FindByLookupIdAsync(lookupId);
    }

    public async Task<List<CaseFeedbackSubmission>> FindByKpiPeriodAndSubmitterAsync(
        long periodId,
        string userId)
    {
        return await _caseFeedbackRepo.FindByKpiPeriodAndSubmitterAsync(periodId, userId);
    }

    public async Task<List<CaseFeedbackSubmission>> FindByKpiPeriodAsync(long periodId)
    {
        return await _caseFeedbackRepo.FindAllByKpiPeriodAsync(periodId);
    }

    public async Task<List<CaseFeedbackSubmission>> FindAllAsync()
    {
        return await _caseFeedbackRepo.FindAllAsync();
    }

}
