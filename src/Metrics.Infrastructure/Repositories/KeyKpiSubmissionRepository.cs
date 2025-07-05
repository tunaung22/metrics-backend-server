using Metrics.Application.Domains;
using Metrics.Application.Interfaces.IRepositories;
using Metrics.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Metrics.Infrastructure.Repositories;

public class KeyKpiSubmissionRepository : IKeyKpiSubmissionRepository
{
    private readonly MetricsDbContext _context;

    public KeyKpiSubmissionRepository(
        MetricsDbContext context)
    {
        _context = context;
    }


    public async Task<long> FindCountByUserByPeriodAsync(string currentUserId, long kpiPeriodId)
    {
        return await _context.KeyKpiSubmissions
            .Where(s =>
                s.ApplicationUserId == currentUserId
                && s.ScoreSubmissionPeriodId == kpiPeriodId)
            .CountAsync();
    }

    public IQueryable<KeyKpiSubmission> FindAllAsQueryable()
    {
        return _context.KeyKpiSubmissions
            .OrderBy(e => e.SubmittedAt);
    }
}
