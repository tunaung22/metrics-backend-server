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

public class CaseFeedbackService : ICaseFeedbackService
{
    private readonly MetricsDbContext _context;
    private readonly ILogger<CaseFeedbackService> _logger;
    private readonly ICaseFeedbackRepository _caseFeedbackRepo;

    public CaseFeedbackService(
        MetricsDbContext context,
        ILogger<CaseFeedbackService> logger,
        ICaseFeedbackRepository caseFeedbackSubmissionRepository
        )
    {
        _context = context;
        _logger = logger;
        _caseFeedbackRepo = caseFeedbackSubmissionRepository;
    }

    public async Task SaveAsync(CaseFeedback submission)
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

    public async Task UpdateAsync(string lookupId, CaseFeedback entity)
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

    public async Task<CaseFeedback?> FindByLookupIdAsync(string lookupId)
    {
        if (string.IsNullOrWhiteSpace(lookupId))
            throw new ArgumentException("Lookup ID cannot be empty", nameof(lookupId));

        return await _caseFeedbackRepo.FindByLookupIdAsync(lookupId);
    }

    public async Task<List<CaseFeedback>> FindByKpiPeriodAndSubmitterAsync(
        long periodId,
        string userId)
    {
        return await _caseFeedbackRepo.FindByKpiPeriodAndSubmitterAsync(periodId, userId);
    }

    public async Task<List<CaseFeedback>> FindByKpiPeriodAsync(long periodId)
    {
        return await _caseFeedbackRepo.FindAllByKpiPeriodAsync(periodId);
    }

    public async Task<List<CaseFeedback>> FindAllAsync()
    {
        return await _caseFeedbackRepo.FindAllAsync();
    }

}
