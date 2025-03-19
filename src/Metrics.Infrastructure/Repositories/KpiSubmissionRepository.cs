using Metrics.Domain.Entities;
using Metrics.Domain.Exceptions;
using Metrics.Infrastructure.Data;
using Metrics.Infrastructure.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;
using System;

namespace Metrics.Infrastructure.Repositories;

public class KpiSubmissionRepository : IKpiSubmissionRepository
{
    private readonly MetricsDbContext _context;

    public KpiSubmissionRepository(MetricsDbContext context)
    {
        _context = context;
    }


    public void Create(KpiSubmission entity)
    {
        _context.KpiSubmissions.Add(entity);
    }

    public void CreateRange(List<KpiSubmission> entities)
    {
        _context.KpiSubmissions.AddRange(entities);
    }

    public void Delete(KpiSubmission entity)
    {
        _context.KpiSubmissions.Remove(entity);
    }

    public async Task<bool> KpiSubmissionExistsAsync(DateOnly submissionDate)
    {
        return await _context.KpiSubmissions
            .AnyAsync(e => e.SubmissionDate == submissionDate);
    }

    public async Task<bool> KpiSubmissionExists2Async(long kpiPeriodId, long departmentId, long employeeId)
    {
        return await _context.KpiSubmissions
            .AnyAsync(e => e.KpiPeriodId == kpiPeriodId
                        && e.DepartmentId == departmentId
                        && e.EmployeeId == employeeId);
    }

    public IQueryable<KpiSubmission> FindAllAsQueryable()
    {
        return _context.KpiSubmissions
            .OrderBy(e => e.SubmissionDate);
    }

    public async Task<IEnumerable<KpiSubmission>> FindAllAsync()
    {
        return await _context.KpiSubmissions
            .OrderBy(e => e.SubmissionDate)
            .ToListAsync();
    }

    public async Task<KpiSubmission> FindByIdAsync(long id)
    {
        var submission = await _context.KpiSubmissions
                .Where(e => e.Id == id)
                .FirstOrDefaultAsync();

        if (submission == null)
            throw new NotFoundException("Department not found.");

        return submission;
    }

    public IQueryable<KpiSubmission> FindByIdAsQueryable(long id)
    {
        var submission = _context.KpiSubmissions
                .Where(e => e.Id == id);

        return submission;
    }

    public async Task<KpiSubmission> FindBySubmissionDateAsync(DateOnly submissionDate)
    {
        var submission = await _context.KpiSubmissions
                .Where(e => e.SubmissionDate == submissionDate)
                .FirstOrDefaultAsync();

        if (submission == null)
            throw new NotFoundException("Department not found.");

        return submission;
    }

    public void Update(KpiSubmission entity)
    {
        _context.Entry(entity).State = EntityState.Modified;
    }
}
