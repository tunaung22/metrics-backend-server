using Metrics.Domain.Entities;
using Metrics.Infrastructure.Data;
using Metrics.Infrastructure.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;
using System;

namespace Metrics.Infrastructure.Repositories;

public class KpiPeriodRepository : IKpiPeriodRepository //: GenericRepository<KpiPeriod>, IKpiPeriodRepository
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

    public void Delete(KpiPeriod entity)
    {
        _context.KpiPeriods.Remove(entity);
    }

    public async Task<IEnumerable<KpiPeriod>> FindAllAsync()
    {
        return await _context.KpiPeriods
            .OrderBy(e => e.PeriodName)
            .ToListAsync();
    }

    public IQueryable<KpiPeriod> FindAllAsQueryable()
    {
        return _context.KpiPeriods
            .OrderBy(e => e.PeriodName);
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
            .Where(e => e.PeriodName == periodName)
            .FirstOrDefaultAsync();

        if (kpiPeriod == null)
            // TODO
            // throw new EntityNotFoundException();
            throw new Exception("Kpi Period not found.");

        return kpiPeriod;
    }

    public async Task<bool> KpiPeriodExistsAsync(string periodName)
    {
        return await _context.KpiPeriods
            .AnyAsync(e => e.PeriodName == periodName);
    }

    public void Update(KpiPeriod enitiy)
    {
        _context.Entry(enitiy).State = EntityState.Modified;
    }


    // public KpiPeriodRepository(MetricsDbContext context) : base(context)
    // { }

    // public async Task<KpiPeriod?> FindByPeriodName(string periodName)
    // {
    //     return await _dbSet.FirstOrDefaultAsync(e => e.PeriodName == periodName);
    // }

    // public async Task<bool> IsPeriodNameExist(string periodName)
    // {
    //     return await _dbSet.AnyAsync(e => e.PeriodName == periodName);
    // }

}
