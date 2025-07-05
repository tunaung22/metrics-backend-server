using Metrics.Application.Domains;
using Metrics.Application.Interfaces.IRepositories;
using Metrics.Application.Interfaces.IServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Metrics.Infrastructure.Services;

public class KeyKpiSubmissionService : IKeyKpiSubmissionService
{

    private readonly ILogger<KeyKpiSubmissionService> _logger;
    private readonly IKeyKpiSubmissionRepository _keyMetricSubmissionRepository;

    public KeyKpiSubmissionService(
        ILogger<KeyKpiSubmissionService> logger,
        IKeyKpiSubmissionRepository keyMetricSubmissionRepository
        )
    {
        _logger = logger;
        _keyMetricSubmissionRepository = keyMetricSubmissionRepository;
    }

    public async Task<List<KeyKpiSubmission>> FindBySubmitterByPeriodByDepartmentListAsync(
        ApplicationUser candidate,
        long kpiPeriodId,
        List<long> departmentIdList)
    {
        try
        {
            var foundSubmissions = await _keyMetricSubmissionRepository.FindAllAsQueryable()
                .Include(e => e.TargetPeriod)
                .Include(e => e.TargetDepartment)
                .Include(e => e.SubmittedBy)
                .Include(e => e.KeyKpiSubmissionItems)
                .Where(e => e.SubmittedBy.Id == candidate.Id
                    && e.ScoreSubmissionPeriodId == kpiPeriodId
                    && departmentIdList.Any(d => d == e.DepartmentId))
                .ToListAsync();

            if (foundSubmissions.Count > 0)
            {
                return foundSubmissions.Select(s => new KeyKpiSubmission
                {
                    SubmittedAt = s.SubmittedAt,
                    SubmissionDate = s.SubmissionDate,
                    ScoreSubmissionPeriodId = s.ScoreSubmissionPeriodId,
                    DepartmentId = s.DepartmentId,
                    ApplicationUserId = s.ApplicationUserId,
                    KeyKpiSubmissionItems = s.KeyKpiSubmissionItems
                }).ToList();
            }

            return [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while querying key metric submissions by submittter by period by department list.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<long> FindCountByUserByPeriodAsync(
        string currentUserId,
        long kpiPeriodId)
    {
        try
        {
            return await _keyMetricSubmissionRepository
                .FindCountByUserByPeriodAsync(currentUserId, kpiPeriodId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while counting submissions.");
            throw new Exception("An unexpected error occurred. Please try again later.");

        }
    }
}
