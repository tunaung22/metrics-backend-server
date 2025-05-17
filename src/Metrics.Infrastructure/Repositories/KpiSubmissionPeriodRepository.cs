using Metrics.Application.Domains;
using Metrics.Application.Interfaces.IRepositories;
using Metrics.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Metrics.Infrastructure.Repositories;

public class KpiSubmissionPeriodRepository : IKpiPeriodRepository
{
    private readonly MetricsDbContext _context;

    public KpiSubmissionPeriodRepository(MetricsDbContext context)
    {
        _context = context;
    }

    public void Create(KpiSubmissionPeriod entity)
    {
        _context.KpiSubmissionPeriods.Add(entity);
    }

    public void Update(KpiSubmissionPeriod enitiy)
    {
        _context.Entry(enitiy).State = EntityState.Modified;
        _context.KpiSubmissionPeriods.Update(enitiy);
    }

    public void Delete(KpiSubmissionPeriod entity)
    {
        _context.KpiSubmissionPeriods.Remove(entity);
    }

    public async Task<KpiSubmissionPeriod> FindByIdAsync(long id)
    {
        var kpiPeriod = await _context.KpiSubmissionPeriods
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

    public async Task<KpiSubmissionPeriod> FindByPeriodNameAsync(string periodName)
    {
        var kpiPeriod = await _context.KpiSubmissionPeriods
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

    public async Task<IEnumerable<KpiSubmissionPeriod>> FindAllAsync()
    {
        return await _context.KpiSubmissionPeriods
            .OrderBy(e => e.PeriodName)
            .ToListAsync();
    }

    public async Task<bool> KpiPeriodExistsAsync(string periodName)
    {
        return await _context.KpiSubmissionPeriods
            .AnyAsync(e => e.PeriodName == periodName);
    }

    public async Task<long> FindCountAsync()
    {
        return await _context.KpiSubmissionPeriods.CountAsync();
    }

    public IQueryable<KpiSubmissionPeriod> FindAllAsQueryable()
    {
        return _context.KpiSubmissionPeriods
            .OrderBy(e => e.PeriodName);
    }

}
