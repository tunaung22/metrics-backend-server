using Metrics.Application.Domains;
using Metrics.Application.Interfaces.IRepositories;
using Metrics.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Metrics.Infrastructure.Repositories;

public class KeyKpiSubmissionConstraintRepository(
    MetricsDbContext context) : IKeyKpiSubmissionConstraintRepository
{
    private readonly MetricsDbContext _context = context;


    public void Create(KeyKpiSubmissionConstraint entity)
    {
        _context.KeyKpiSubmissionConstraints.Add(entity);
    }

    public void Update(KeyKpiSubmissionConstraint entity)
    {
        throw new NotImplementedException();
    }

    public void Update(DepartmentKeyMetric entity)
    {
        _context.Entry(entity).State = EntityState.Modified;
    }

    public async Task<KeyKpiSubmissionConstraint?> FindByLookupIdAsync(Guid lookupId)
    {
        return await _context.KeyKpiSubmissionConstraints
           .Where(k => k.LookupId == lookupId)
           .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Find all active by period id
    /// </summary>
    /// <param name="sourcePeriodId"></param>
    /// <returns></returns>
    public async Task<List<KeyKpiSubmissionConstraint>> FindByPeriodAsync(long sourcePeriodId)
    {
        return await _context.KeyKpiSubmissionConstraints
            .AsNoTracking()
            .Where(c =>
                c.IsDeleted == false &&
                c.DepartmentKeyMetric.KpiSubmissionPeriodId == sourcePeriodId)
            .Include(c => c.DepartmentKeyMetric).ThenInclude(dkm => dkm.KpiSubmissionPeriod)
            .Include(c => c.DepartmentKeyMetric).ThenInclude(dkm => dkm.KeyIssueDepartment)
            .Include(c => c.DepartmentKeyMetric).ThenInclude(dkm => dkm.KeyMetric)
            .Include(c => c.CandidateDepartment)
            .OrderBy(c => c.CandidateDepartment.DepartmentName)
            .ToListAsync();
    }

    public async Task<IEnumerable<KeyKpiSubmissionConstraint>> FindBySubmitterDepartmentAsync(Guid departmentCode)
    {
        return await _context.KeyKpiSubmissionConstraints
            .AsNoTracking()
            .Where(k => k.CandidateDepartment.DepartmentCode == departmentCode)
            .Include(k => k.CandidateDepartment)
            .Include(k => k.DepartmentKeyMetric).ThenInclude(dkm => dkm.KpiSubmissionPeriod)
            .Include(k => k.DepartmentKeyMetric).ThenInclude(dkm => dkm.KeyMetric)
            .Include(k => k.DepartmentKeyMetric).ThenInclude(dkm => dkm.KeyIssueDepartment)
            .OrderBy(k => k.CandidateDepartment.DepartmentName)
            .ToListAsync();
    }

    public async Task<IEnumerable<KeyKpiSubmissionConstraint>> FindByPeriodAndSubmitterDepartmentAsync(long periodId, Guid departmentCode)
    {
        return await _context.KeyKpiSubmissionConstraints
            .Where(k => k.DepartmentKeyMetric.KpiSubmissionPeriod.Id == periodId
                && k.CandidateDepartment.DepartmentCode == departmentCode)
            .Include(k => k.CandidateDepartment)
            .Include(k => k.DepartmentKeyMetric).ThenInclude(dkm => dkm.KpiSubmissionPeriod)
            .Include(k => k.DepartmentKeyMetric).ThenInclude(dkm => dkm.KeyMetric)
            .Include(k => k.DepartmentKeyMetric).ThenInclude(dkm => dkm.KeyIssueDepartment)
            .OrderBy(k => k.CandidateDepartment.DepartmentName)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<KeyKpiSubmissionConstraint>> FindByPeriodAndSubmitterDepartmentAsync(string periodName, Guid departmentCode)
    {
        return await _context.KeyKpiSubmissionConstraints
            .Where(k => k.DepartmentKeyMetric.KpiSubmissionPeriod.PeriodName == periodName
                && k.CandidateDepartment.DepartmentCode == departmentCode)
            .Include(k => k.CandidateDepartment)
            .Include(k => k.DepartmentKeyMetric).ThenInclude(dkm => dkm.KpiSubmissionPeriod)
            .Include(k => k.DepartmentKeyMetric).ThenInclude(dkm => dkm.KeyMetric)
            .Include(k => k.DepartmentKeyMetric).ThenInclude(dkm => dkm.KeyIssueDepartment)
            .OrderBy(k => k.CandidateDepartment.DepartmentName)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<KeyKpiSubmissionConstraint>> FindByDepartmentKeyMetricsAsync(List<long> departmentKeyMetricIDs)
    {
        return await _context.KeyKpiSubmissionConstraints
            .Where(k => departmentKeyMetricIDs.Contains(k.DepartmentKeyMetricId))
            .Include(k => k.CandidateDepartment)
            .Include(k => k.DepartmentKeyMetric).ThenInclude(dkm => dkm.KpiSubmissionPeriod)
            .Include(k => k.DepartmentKeyMetric).ThenInclude(dkm => dkm.KeyMetric)
            .Include(k => k.DepartmentKeyMetric).ThenInclude(dkm => dkm.KeyIssueDepartment)
            .OrderBy(k => k.CandidateDepartment.DepartmentName)
            .AsNoTracking()
            .ToListAsync();
    }

    public IQueryable<KeyKpiSubmissionConstraint> FindAllAsQueryable()
    {
        return _context.KeyKpiSubmissionConstraints
            .OrderBy(k => k.CandidateDepartment.DepartmentName)
                .ThenBy(k => k.DepartmentKeyMetric.KeyMetric.MetricTitle);
    }

    public async Task<Dictionary<long, int>> FindCountsByPeriodBySubmitterDepartmentAsync(List<long> periodIds, long submitterDepartmentId)
    {
        return await _context.KeyKpiSubmissionConstraints
            .Where(e => periodIds.Contains(e.DepartmentKeyMetric.KpiSubmissionPeriodId)
                && e.CandidateDepartment.Id == submitterDepartmentId)
            .GroupBy(e => e.DepartmentKeyMetric.KpiSubmissionPeriodId)
            .Select(e => new { PeriodId = e.Key, Count = e.Count() })
            .ToDictionaryAsync(e => e.PeriodId, e => e.Count);
    }


}
