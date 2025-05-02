using Metrics.Application.Domains;
using Metrics.Application.Interfaces.IRepositories;
using Metrics.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Metrics.Infrastructure.Repositories;

public class KpiPeriodRepository : IKpiPeriodRepository
{
    private readonly MetricsDbContext _context;

    public KpiPeriodRepository(MetricsDbContext context)
    {
        _context = context;
    }

    public void Create(KpiPeriod entity)
    {
        _context.KpiPeriods.Add(entity);
    }

    public void Update(KpiPeriod enitiy)
    {
        _context.Entry(enitiy).State = EntityState.Modified;
        _context.KpiPeriods.Update(enitiy);
    }

    public void Delete(KpiPeriod entity)
    {
        _context.KpiPeriods.Remove(entity);
    }

    public async Task<KpiPeriod> FindByIdAsync(long id)
    {
        var kpiPeriod = await _context.KpiPeriods
            // .Include(e => e.KpiSubmissions)
            .OrderBy(e => e.PeriodName)
            .Where(e => e.Id == id)
            .FirstOrDefaultAsync();
        if (kpiPeriod == null)
            // TODO
            // throw new EntityNotFoundException();
            throw new Exception("Kpi Period not found.");

        return kpiPeriod;
    }

    public async Task<KpiPeriod> FindByPeriodNameAsync(string periodName)
    {
        var kpiPeriod = await _context.KpiPeriods
            // .Include(e => e.KpiSubmissions)
            .OrderBy(e => e.PeriodName)
            .Where(e => e.PeriodName.Trim().ToLower() == periodName.Trim().ToLower())
            .FirstOrDefaultAsync();

        if (kpiPeriod == null)
            // TODO
            // throw new EntityNotFoundException();
            throw new Exception("Kpi Period not found.");

        return kpiPeriod;
    }

    public async Task<IEnumerable<KpiPeriod>> FindAllAsync()
    {
        return await _context.KpiPeriods
            .OrderBy(e => e.PeriodName)
            .ToListAsync();
    }

    public async Task<bool> KpiPeriodExistsAsync(string periodName)
    {
        return await _context.KpiPeriods
            .AnyAsync(e => e.PeriodName == periodName);
    }

    public async Task<long> FindCountAsync()
    {
        return await _context.KpiPeriods.CountAsync();
    }

    public IQueryable<KpiPeriod> FindAllAsQueryable()
    {
        return _context.KpiPeriods
            .OrderBy(e => e.PeriodName);
    }

}
