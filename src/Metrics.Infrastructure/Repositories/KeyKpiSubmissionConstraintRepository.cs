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

    public async Task<IEnumerable<KeyKpiSubmissionConstraint>> FindBySubmitterDepartmentAsync(Guid departmentCode)
    {
        return await _context.KeyKpiSubmissionConstraints
            .Where(k => k.SubmitterDepartment.DepartmentCode == departmentCode)
            .Include(k => k.SubmitterDepartment)
            .Include(k => k.DepartmentKeyMetric).ThenInclude(dkm => dkm.KpiSubmissionPeriod)
            .Include(k => k.DepartmentKeyMetric).ThenInclude(dkm => dkm.KeyMetric)
            .Include(k => k.DepartmentKeyMetric).ThenInclude(dkm => dkm.KeyIssueDepartment)
            .OrderBy(k => k.SubmitterDepartment.DepartmentName)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<KeyKpiSubmissionConstraint>> FindByPeriodAndSubmitterDepartmentAsync(long periodId, Guid departmentCode)
    {
        return await _context.KeyKpiSubmissionConstraints
            .Where(k => k.DepartmentKeyMetric.KpiSubmissionPeriod.Id == periodId
                && k.SubmitterDepartment.DepartmentCode == departmentCode)
            .Include(k => k.SubmitterDepartment)
            .Include(k => k.DepartmentKeyMetric).ThenInclude(dkm => dkm.KpiSubmissionPeriod)
            .Include(k => k.DepartmentKeyMetric).ThenInclude(dkm => dkm.KeyMetric)
            .Include(k => k.DepartmentKeyMetric).ThenInclude(dkm => dkm.KeyIssueDepartment)
            .OrderBy(k => k.SubmitterDepartment.DepartmentName)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<KeyKpiSubmissionConstraint>> FindByPeriodAndSubmitterDepartmentAsync(string periodName, Guid departmentCode)
    {
        return await _context.KeyKpiSubmissionConstraints
            .Where(k => k.DepartmentKeyMetric.KpiSubmissionPeriod.PeriodName == periodName
                && k.SubmitterDepartment.DepartmentCode == departmentCode)
            .Include(k => k.SubmitterDepartment)
            .Include(k => k.DepartmentKeyMetric).ThenInclude(dkm => dkm.KpiSubmissionPeriod)
            .Include(k => k.DepartmentKeyMetric).ThenInclude(dkm => dkm.KeyMetric)
            .Include(k => k.DepartmentKeyMetric).ThenInclude(dkm => dkm.KeyIssueDepartment)
            .OrderBy(k => k.SubmitterDepartment.DepartmentName)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<KeyKpiSubmissionConstraint>> FindByDepartmentKeyMetricsAsync(List<long> departmentKeyMetricIDs)
    {
        return await _context.KeyKpiSubmissionConstraints
            .Where(k => departmentKeyMetricIDs.Contains(k.DepartmentKeyMetricId))
            .Include(k => k.SubmitterDepartment)
            .Include(k => k.DepartmentKeyMetric).ThenInclude(dkm => dkm.KpiSubmissionPeriod)
            .Include(k => k.DepartmentKeyMetric).ThenInclude(dkm => dkm.KeyMetric)
            .Include(k => k.DepartmentKeyMetric).ThenInclude(dkm => dkm.KeyIssueDepartment)
            .OrderBy(k => k.SubmitterDepartment.DepartmentName)
            .AsNoTracking()
            .ToListAsync();
    }

    public IQueryable<KeyKpiSubmissionConstraint> FindAllAsQueryable()
    {
        return _context.KeyKpiSubmissionConstraints
            .OrderBy(k => k.SubmitterDepartment.DepartmentName)
                .ThenBy(k => k.DepartmentKeyMetric.KeyMetric.MetricTitle);
    }

    public async Task<Dictionary<long, int>> FindCountsByPeriodBySubmitterDepartmentAsync(List<long> periodIds, long submitterDepartmentId)
    {
        return await _context.KeyKpiSubmissionConstraints
            .Where(e => periodIds.Contains(e.DepartmentKeyMetric.KpiSubmissionPeriodId)
                && e.SubmitterDepartment.Id == submitterDepartmentId)
            .GroupBy(e => e.DepartmentKeyMetric.KpiSubmissionPeriodId)
            .Select(e => new { PeriodId = e.Key, Count = e.Count() })
            .ToDictionaryAsync(e => e.PeriodId, e => e.Count);
    }

}
