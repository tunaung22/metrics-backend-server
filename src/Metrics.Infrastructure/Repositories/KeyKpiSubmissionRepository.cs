using Metrics.Application.Domains;
using Metrics.Application.Interfaces.IRepositories;
using Metrics.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Metrics.Infrastructure.Repositories;

public class KeyKpiSubmissionRepository(MetricsDbContext context) : IKeyKpiSubmissionRepository
{
    private readonly MetricsDbContext _context = context;


    public void Add(KeyKpiSubmission submission)
    {
        _context.Add(submission);
    }

    public async Task AddRangeAsync(List<KeyKpiSubmission> submisions)
    {
        await _context.AddRangeAsync(submisions);
    }

    public async Task DeleteByPeriodByCandidateAsync(long periodId, string candidateId)
    {
        var submissions = await FindByPeriodByUserAsync(periodId, candidateId);
        if (submissions.Any())
        {
            _context.KeyKpiSubmissions.RemoveRange(submissions);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<KeyKpiSubmission>> FindByPeriodByUserAsync(long periodId, string userId)
    {
        return await _context.KeyKpiSubmissions.Where(s =>
            s.DepartmentKeyMetric.KpiSubmissionPeriodId == periodId &&
            s.SubmitterId.ToLower() == userId.ToLower()
        ).ToListAsync();
    }


    public async Task<KeyKpiSubmission?> FindByIdAsync(long id)
    {
        return await _context.KeyKpiSubmissions
            .FindAsync(id);
    }

    public async Task<KeyKpiSubmission?> FindBySubmitterByDepartmentKeyMetricAsync(
        string submitterId,
        long departmentKeyMetricId)
    {
        return await _context.KeyKpiSubmissions
            .Where(e =>
                e.SubmitterId == submitterId &&
                e.DepartmentKeyMetricId == departmentKeyMetricId)
            .OrderBy(e => e.SubmittedAt)
            .Include(e => e.SubmittedBy).ThenInclude(submitter => submitter.Department)
            .Include(e => e.SubmittedBy).ThenInclude(group => group.UserTitle)
            .Include(e => e.DepartmentKeyMetric).ThenInclude(e => e.KpiSubmissionPeriod)
            .Include(e => e.DepartmentKeyMetric).ThenInclude(e => e.KeyIssueDepartment)
            .Include(e => e.DepartmentKeyMetric).ThenInclude(e => e.KeyMetric)
            .FirstOrDefaultAsync();
    }

    public async Task<List<KeyKpiSubmission>> FindByPeriodAsync(long periodId, bool includeLockedUser = true)
    {
        var query = _context.KeyKpiSubmissions
            .AsNoTracking()
            .Where(e => e.DepartmentKeyMetric.KpiSubmissionPeriodId == periodId)
            .OrderBy(e => e.SubmittedAt)
            .Include(e => e.SubmittedBy).ThenInclude(submitter => submitter.Department)
            .Include(e => e.SubmittedBy).ThenInclude(group => group.UserTitle)
            .Include(e => e.DepartmentKeyMetric).ThenInclude(e => e.KpiSubmissionPeriod)
            .Include(e => e.DepartmentKeyMetric).ThenInclude(e => e.KeyIssueDepartment)
            .Include(e => e.DepartmentKeyMetric).ThenInclude(e => e.KeyMetric);
        if (!includeLockedUser)
        {
            return await query.Where(s =>
                    s.SubmittedBy.LockoutEnd == null ||
                    s.SubmittedBy.LockoutEnd <= DateTimeOffset.UtcNow)
                .ToListAsync();
        }
        else
        {
            return await query.ToListAsync();
        }
    }

    public async Task<List<KeyKpiSubmission>> FindBySubmitterAsync(string submitterId)
    {
        return await _context.KeyKpiSubmissions
            .Where(e => e.SubmitterId == submitterId)
            .OrderBy(e => e.SubmittedAt)
            .Include(e => e.SubmittedBy).ThenInclude(submitter => submitter.Department)
            .Include(e => e.SubmittedBy).ThenInclude(group => group.UserTitle)
            .Include(e => e.DepartmentKeyMetric).ThenInclude(e => e.KpiSubmissionPeriod)
            .Include(e => e.DepartmentKeyMetric).ThenInclude(e => e.KeyIssueDepartment)
            .Include(e => e.DepartmentKeyMetric).ThenInclude(e => e.KeyMetric)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<KeyKpiSubmission>> FindByDepartmentKeyMetricListAsync(List<long> departmentKeyMetricIDs)
    {
        return await _context.KeyKpiSubmissions
            .Where(e => departmentKeyMetricIDs.Contains(e.DepartmentKeyMetricId))
            .OrderBy(e => e.SubmittedAt)
            .Include(e => e.SubmittedBy).ThenInclude(submitter => submitter.Department)
            .Include(e => e.SubmittedBy).ThenInclude(group => group.UserTitle)
            .Include(e => e.DepartmentKeyMetric).ThenInclude(dkm => dkm.KpiSubmissionPeriod)
            .Include(e => e.DepartmentKeyMetric).ThenInclude(dkm => dkm.KeyIssueDepartment)
            .Include(e => e.DepartmentKeyMetric).ThenInclude(dkm => dkm.KeyMetric)
            .AsNoTracking()
            .ToListAsync();
    }


    public async Task<List<KeyKpiSubmission>> FindByPeriodBySubmitterAsync(long periodId, string submitterId)
    {
        return await _context.KeyKpiSubmissions
            .Where(e => e.DepartmentKeyMetric.KpiSubmissionPeriodId == periodId
                && e.SubmitterId == submitterId)
            .OrderBy(e => e.SubmittedAt)
            .Include(e => e.SubmittedBy).ThenInclude(submitter => submitter.Department)
            .Include(e => e.SubmittedBy).ThenInclude(group => group.UserTitle)
            .Include(e => e.DepartmentKeyMetric).ThenInclude(dkm => dkm.KpiSubmissionPeriod)
            .Include(e => e.DepartmentKeyMetric).ThenInclude(dkm => dkm.KeyIssueDepartment)
            .Include(e => e.DepartmentKeyMetric).ThenInclude(dkm => dkm.KeyMetric)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<KeyKpiSubmission>> FindByPeriodBySubmitterAsync(List<long> periodIds, string submitterId)
    {
        return await _context.KeyKpiSubmissions
            .Where(e => periodIds.Contains(e.DepartmentKeyMetric.KpiSubmissionPeriodId)
                && e.SubmitterId == submitterId
                && e.IsDeleted != false)
            .OrderBy(e => e.SubmittedAt)
            .Include(e => e.SubmittedBy).ThenInclude(submitter => submitter.Department)
            .Include(e => e.SubmittedBy).ThenInclude(group => group.UserTitle)
            .Include(e => e.DepartmentKeyMetric).ThenInclude(e => e.KpiSubmissionPeriod)
            .Include(e => e.DepartmentKeyMetric).ThenInclude(e => e.KeyIssueDepartment)
            .Include(e => e.DepartmentKeyMetric).ThenInclude(e => e.KeyMetric)
            .AsNoTracking()
            .ToListAsync();
    }


    public async Task<List<KeyKpiSubmission>> FindByPeriodBySubmitterByDepartmentAsync(
        long periodId,
        string submitterId,
        long departmentId)
    {
        return await _context.KeyKpiSubmissions
            .Where(e =>
                e.DepartmentKeyMetric.KpiSubmissionPeriodId == periodId
                && e.SubmitterId == submitterId
                && e.DepartmentKeyMetric.DepartmentId == departmentId)
            .OrderBy(e => e.SubmittedAt)
            .Include(e => e.SubmittedBy).ThenInclude(submitter => submitter.Department)
            .Include(e => e.SubmittedBy).ThenInclude(group => group.UserTitle)
            .Include(e => e.DepartmentKeyMetric)
                .ThenInclude(e => e.KpiSubmissionPeriod)
            .Include(e => e.DepartmentKeyMetric)
                .ThenInclude(e => e.KeyIssueDepartment)
            .Include(e => e.DepartmentKeyMetric)
                .ThenInclude(e => e.KeyMetric)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<long> FindCountAsync()
    {
        return await _context.KeyKpiSubmissions.CountAsync();
    }

    public async Task<long> FindCountBySubmitterAsync(string submitterId)
    {
        return await _context.KeyKpiSubmissions
            .Where(e => e.SubmitterId == submitterId)
            .CountAsync();
    }

    public async Task<long> FindCountByDepartmentAsync(long departmentId)
    {
        return await _context.KeyKpiSubmissions
            .Where(e => e.DepartmentKeyMetric.DepartmentId == departmentId)
            .CountAsync();
    }

    public async Task<long> FindCountByPeriodAsync(long periodId)
    {
        return await _context.KeyKpiSubmissions
             .Where(e => e.DepartmentKeyMetric.KpiSubmissionPeriodId == periodId)
             .CountAsync();
    }

    public async Task<long> FindCountByPeriodBySubmitterAsync(long periodId, string submitterId)
    {
        return await _context.KeyKpiSubmissions
            .Where(e =>
                e.SubmitterId == submitterId
                && e.DepartmentKeyMetric.KpiSubmissionPeriodId == periodId)
            .CountAsync();
    }

    public async Task<long> FindCountByPeriodBySubmitterAsync(List<long> periodIds, string submitterId)
    {
        return await _context.KeyKpiSubmissions
            .Where(e => e.SubmitterId == submitterId
                && periodIds.Contains(e.DepartmentKeyMetric.KpiSubmissionPeriodId))
            .CountAsync();
    }

    public async Task<Dictionary<long, int>> FindSubmissionCountsByPeriodBySubmitterAsync(List<long> periodIds, string submitterId)
    {
        return await _context.KeyKpiSubmissions
            .Where(e => e.SubmitterId == submitterId
                && periodIds.Contains(e.DepartmentKeyMetric.KpiSubmissionPeriodId))
            .GroupBy(e => e.DepartmentKeyMetric.KpiSubmissionPeriodId)
            .Select(e => new { PeriodId = e.Key, Count = e.Count() })
            .ToDictionaryAsync(e => e.PeriodId, e => e.Count);
    }


}
