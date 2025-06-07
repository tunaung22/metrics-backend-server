using Metrics.Application.Domains;
using Metrics.Application.Interfaces.IRepositories;
using Metrics.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Metrics.Infrastructure.Repositories;

public class DepartmentKeyMetricRepository : IDepartmentKeyMetricRepository
{
    private readonly MetricsDbContext _context;

    public DepartmentKeyMetricRepository(MetricsDbContext context)
    {
        _context = context;
    }

    public void Create(DepartmentKeyMetric entity)
    {
        _context.DepartmentKeyMetrics.Add(entity);
    }

    public void Delete(DepartmentKeyMetric entity)
    {
        _context.DepartmentKeyMetrics.Remove(entity);
    }

    public IQueryable<DepartmentKeyMetric> FindAllAsQueryable()
    {
        return _context.DepartmentKeyMetrics
            .OrderBy(k => k.KpiSubmissionPeriod.PeriodName)
            .ThenBy(k => k.TargetDepartment.DepartmentName)
            .ThenBy(k => k.KeyMetric.MetricTitle);
    }

    public async Task<IEnumerable<DepartmentKeyMetric>> FindAllAsync()
    {
        return await _context.DepartmentKeyMetrics
            .OrderBy(k => k.KpiSubmissionPeriod.PeriodName)
            .ThenBy(k => k.TargetDepartment.DepartmentName)
            .ThenBy(k => k.KeyMetric.MetricTitle)
            .ToListAsync();
    }

    public async Task<IEnumerable<DepartmentKeyMetric>> FindAllAsync(int pageNumber, int pageSize)
    {
        return await _context.DepartmentKeyMetrics
            .Include(k => k.KpiSubmissionPeriod)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<DepartmentKeyMetric?> FindByIdAsync(long id)
    {
        return await _context.DepartmentKeyMetrics.FindAsync(id);
    }

    // public async Task<DepartmentKeyMetric?> FindByMetricTitleAsync(string metricTitle)
    // {
    //     return await _context.DepartmentKeyMetrics
    //         .Where(k => k.MetricTitle.ToLower() == metricTitle.ToLower())
    //         .FirstOrDefaultAsync();
    // }

    // public async Task<DepartmentKeyMetric?> FindByMetricCodeAsync(Guid metricCode)
    // {
    //     return await _context.DepartmentKeyMetrics
    //         .Where(k => k.MetricCode == metricCode)
    //         .FirstOrDefaultAsync();
    // }

    public async Task<long> FindCountAsync()
    {
        return await _context.DepartmentKeyMetrics.CountAsync();
    }

    public async Task<bool> KeyKpiExistsAsync(Guid code)
    {
        return await _context.DepartmentKeyMetrics
            .AnyAsync(k => k.DepartmentKeyMetricCode == code);
    }

    public void Update(DepartmentKeyMetric entity)
    {
        _context.Entry(entity).State = EntityState.Modified;
    }

    public async Task<IEnumerable<DepartmentKeyMetric>> FindByPeriodIdAsync(long periodId)
    {
        return await _context.DepartmentKeyMetrics
            .OrderBy(k => k.TargetDepartment.DepartmentName)
            .Where(k => k.KpiSubmissionPeriodId == periodId)
            .ToListAsync();
    }

    public async Task<DepartmentKeyMetric?> FindByCodeAsync(Guid departmentKeyMetricCode)
    {
        return await _context.DepartmentKeyMetrics
            .Where(k => k.DepartmentKeyMetricCode == departmentKeyMetricCode)
            .FirstOrDefaultAsync();
    }
}
