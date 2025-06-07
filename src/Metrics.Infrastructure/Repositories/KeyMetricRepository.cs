using Metrics.Application.Domains;
using Metrics.Application.Interfaces.IRepositories;
using Metrics.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Metrics.Infrastructure.Repositories;

public class KeyMetricRepository : IKeyMetricRepository
{

    private readonly MetricsDbContext _context;

    public KeyMetricRepository(MetricsDbContext context)
    {
        _context = context;
    }

    public void Create(KeyMetric entity)
    {
        _context.KeyMetrics.Add(entity);
    }

    public void Delete(KeyMetric entity)
    {
        _context.KeyMetrics.Remove(entity);
    }

    public IQueryable<KeyMetric> FindAllAsQueryable()
    {
        return _context.KeyMetrics
            .OrderBy(k => k.MetricTitle);
    }

    public async Task<IEnumerable<KeyMetric>> FindAllAsync()
    {
        return await _context.KeyMetrics
            .OrderBy(k => k.MetricTitle)
            .ToListAsync();
    }

    public async Task<IEnumerable<KeyMetric>> FindAllAsync(int pageNumber, int pageSize)
    {
        return await _context.KeyMetrics
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<KeyMetric?> FindByIdAsync(long id)
    {
        return await _context.KeyMetrics.FindAsync(id);
    }

    public async Task<IEnumerable<KeyMetric>> FindByMetricTitleAsync(string metricTitle)
    {
        return await _context.KeyMetrics
            .Where(k => k.MetricTitle.ToLower().Contains(metricTitle.ToLower()))
            // .Where(k => k.MetricTitle.Contains(metricTitle, StringComparison.OrdinalIgnoreCase))
            .ToListAsync();
    }

    public async Task<KeyMetric?> FindByMetricCodeAsync(Guid metricCode)
    {
        return await _context.KeyMetrics
            .Where(k => k.MetricCode == metricCode)
            .FirstOrDefaultAsync();
    }

    public async Task<long> FindCountAsync()
    {
        return await _context.KeyMetrics.CountAsync();
    }

    public async Task<IEnumerable<KeyMetric>> SearchByMetricTitleAsync(string metricTitle)
    {
        return await _context.KeyMetrics
            .Where(k => k.MetricTitle.ToLower().Contains(metricTitle.ToLower()))
            // .OrderBy(k=> k.MetricTitle.IndexOf(metricTitle, StringComparison.OrdinalIgnoreCase))
            // .Where(k => EF.Functions.Like(k.MetricTitle.ToLower(), $"%{metricTitle.ToLower()}%"))
            // .OrderBy(k => k.MetricTitle.IndexOf(metricTitle, StringComparison.OrdinalIgnoreCase))
            .ToListAsync();
    }

    public async Task<bool> MetricTitleExistsAsync(string metricTitle)
    {
        return await _context.KeyMetrics
            .AnyAsync(k => k.MetricTitle == metricTitle);
    }

    public void Update(KeyMetric entity)
    {
        _context.Entry(entity).State = EntityState.Modified;
    }


}
