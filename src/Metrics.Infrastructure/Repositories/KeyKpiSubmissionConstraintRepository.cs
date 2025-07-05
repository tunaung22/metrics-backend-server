using Metrics.Application.Domains;
using Metrics.Application.Interfaces.IRepositories;
using Metrics.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Metrics.Infrastructure.Repositories;

public class KeyKpiSubmissionConstraintRepository : IKeyKpiSubmissionConstraintRepository
{
    private readonly MetricsDbContext _context;

    public KeyKpiSubmissionConstraintRepository(MetricsDbContext context)
    {
        _context = context;
    }

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

    public async Task<IEnumerable<KeyKpiSubmissionConstraint>> FindAllByDepartmentAsync(Guid departmentCode)
    {
        return await _context.KeyKpiSubmissionConstraints
            .Include(k => k.Department)
            .Include(k => k.DepartmentKeyMetric)
            .OrderBy(k => k.Department.DepartmentName)
            .Where(k => k.Department.DepartmentCode == departmentCode)
            .ToListAsync();
    }

    public async Task<IEnumerable<KeyKpiSubmissionConstraint>> FindAllByPeriodAndDepartmentAsync(string periodName, Guid departmentCode)
    {
        var query = _context.KeyKpiSubmissionConstraints
            .Include(k => k.Department)
            .Include(k => k.DepartmentKeyMetric)
            .OrderBy(k => k.Department.DepartmentName)
            .Where(k => k.DepartmentKeyMetric.KpiSubmissionPeriod.PeriodName == periodName
                && k.Department.DepartmentCode == departmentCode)
            .ToQueryString();
        Console.WriteLine(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>. ", query);

        return await _context.KeyKpiSubmissionConstraints
            .Include(k => k.Department)
            .Include(k => k.DepartmentKeyMetric)
            .OrderBy(k => k.Department.DepartmentName)
            .Where(k => k.DepartmentKeyMetric.KpiSubmissionPeriod.PeriodName == periodName
                && k.Department.DepartmentCode == departmentCode)
            .ToListAsync();
    }

    public IQueryable<KeyKpiSubmissionConstraint> FindAllAsQueryable()
    {
        return _context.KeyKpiSubmissionConstraints
            .OrderBy(k => k.Department.DepartmentName)
            .ThenBy(k => k.DepartmentKeyMetric.KeyMetric.MetricTitle);
    }




}
