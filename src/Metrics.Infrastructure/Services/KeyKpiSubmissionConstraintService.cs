using Metrics.Application.Domains;
using Metrics.Application.Exceptions;
using Metrics.Application.Interfaces.IRepositories;
using Metrics.Application.Interfaces.IServices;
using Metrics.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Metrics.Infrastructure.Services;

public class KeyKpiSubmissionConstraintService : IKeyKpiSubmissionConstraintService
{
    private readonly MetricsDbContext _context;
    private readonly ILogger<KeyKpiSubmissionConstraintService> _logger;
    private readonly IKeyKpiSubmissionConstraintRepository _keyKpiSubmissionConstraintRepository;

    public KeyKpiSubmissionConstraintService(
           MetricsDbContext context,
           ILogger<KeyKpiSubmissionConstraintService> logger,
           IKeyKpiSubmissionConstraintRepository keyKpiSubmissionConstraintRepository)
    {
        _context = context;
        _logger = logger;
        _keyKpiSubmissionConstraintRepository = keyKpiSubmissionConstraintRepository;
    }

    public async Task<KeyKpiSubmissionConstraint> CreateAsync(KeyKpiSubmissionConstraint entity)
    {
        try
        {
            _keyKpiSubmissionConstraintRepository.Create(entity);
            await _context.SaveChangesAsync();

            return entity;
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
            {
                _logger.LogError(ex, pgEx.MessageText);
                throw new MetricsDuplicateContentException("Department Key KPI already exist.", ex.InnerException);
            }
            else
            {
                _logger.LogError(ex, "Database error while creating Department Key KPI.");
                throw new Exception("A database error occurred.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating Department Key KPI.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public Task<KeyKpiSubmissionConstraint> UpdateAsync(Guid lookupId, KeyKpiSubmissionConstraint entity)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> DeleteAsync(Guid lookupId)
    {
        try
        {
            var targetConstraint = await _keyKpiSubmissionConstraintRepository
                .FindByLookupIdAsync(lookupId);
            if (targetConstraint == null)
                throw new MetricsNotFoundException("Key Kpi Submission Constraint not found.");

            targetConstraint.IsDeleted = true;
            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting Key Kpi Submission Constraint.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<bool> UnDeleteAsync(Guid lookupId)
    {
        try
        {
            var targetConstraint = await _keyKpiSubmissionConstraintRepository
                            .FindByLookupIdAsync(lookupId);
            if (targetConstraint == null)
                throw new MetricsNotFoundException("Key Kpi Submission Constraint not found.");

            targetConstraint.IsDeleted = false;
            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while undeleting Key Kpi Submission Constraint.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    // public async Task<IEnumerable<KeyKpiSubmissionConstraint>> FindAllByPeriodAndDepartmentAsync(
    //     string CurrentPeriodName,
    //     Guid CurrentDepartmentCode)
    // {
    //     try
    //     {
    //         // var result = await _keyKpiSubmissionConstraintRepository
    //         //     .FindAllByPeriodAndDepartmentAsync(CurrentPeriodName, CurrentDepartmentCode);
    //         var result = await _keyKpiSubmissionConstraintRepository
    //             .FindAllByDepartmentAsync(CurrentDepartmentCode);

    //         return result;
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Unexpected error while querying Department Key Metric Submission Constraints by period, department.");
    //         throw new Exception("An unexpected error occurred. Please try again later.");
    //     }
    // }
    public async Task<IEnumerable<KeyKpiSubmissionConstraint>> FindAllByPeriodBySubmitterDepartmentAsync(
        long periodId,
        long departmentId)
    {
        // find all submission constraints
        // by department (submitter's)
        // by period
        try
        {
            var result = await _context.KeyKpiSubmissionConstraints
                .Where(cx =>
                    cx.DepartmentId == departmentId
                    && cx.DepartmentKeyMetric.KpiSubmissionPeriod.Id == periodId)
                .OrderBy(cx => cx.Department.DepartmentName)
                .Include(cx => cx.Department)
                .Include(cx => cx.DepartmentKeyMetric)
                    .ThenInclude(cx => cx.KeyIssueDepartment)
                .Include(cx => cx.DepartmentKeyMetric)
                    .ThenInclude(cx => cx.KpiSubmissionPeriod)
                .Include(cx => cx.DepartmentKeyMetric)
                    .ThenInclude(cx => cx.KeyMetric)
                .ToListAsync();

            return result ?? Enumerable.Empty<KeyKpiSubmissionConstraint>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while querying Department Key Metric Submission Constraints by department.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }


    public async Task<IEnumerable<KeyKpiSubmissionConstraint>> FindAllByDepartmentAsync(
    Guid departmentCode)
    {
        try
        {
            // var result = await _keyKpiSubmissionConstraintRepository
            //     .FindAllByPeriodAndDepartmentAsync(CurrentPeriodName, CurrentDepartmentCode);
            // var result = await _keyKpiSubmissionConstraintRepository
            //     .FindAllByDepartmentAsync(departmentCode);

            // No Repository
            // var result = await _context.KeyKpiSubmissionConstraints
            //     .Where(k => k.Department.DepartmentCode == departmentCode)
            //     .OrderBy(k => k.Department.DepartmentName)
            //     .Include(k => k.Department)
            //     .Include(k => k.DepartmentKeyMetric)
            //         .ThenInclude(k => k.KpiSubmissionPeriod)
            //     // .Include(k => k.DepartmentKeyMetric.KpiSubmissionPeriod)
            //     .Include(k => k.DepartmentKeyMetric)
            //         .ThenInclude(k => k.KeyMetric)
            //     // .Include(k => k.DepartmentKeyMetric.KeyMetric)
            //     .ToListAsync();

            var departmentId = await _context.Departments
                .Where(d => d.DepartmentCode == departmentCode)
                .Select(d => d.Id)
                .FirstOrDefaultAsync();

            var result = await _context.KeyKpiSubmissionConstraints
                .Where(k => k.DepartmentId == departmentId)
                .OrderBy(k => k.Department.DepartmentName)
                .Include(k => k.Department)
                .Include(k => k.DepartmentKeyMetric)
                    .ThenInclude(k => k.KpiSubmissionPeriod)
                .Include(k => k.DepartmentKeyMetric)
                    .ThenInclude(k => k.KeyMetric)
                .ToListAsync();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while querying Department Key Metric Submission Constraints by department.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

}
