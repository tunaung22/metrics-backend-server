using Metrics.Application.Domains;
using Metrics.Application.Exceptions;
using Metrics.Application.Interfaces.IRepositories;
using Metrics.Application.Interfaces.IServices;
using Metrics.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Metrics.Infrastructure.Services;

public class DepartmentKeyMetricService : IDepartmentKeyMetricService
{
    private readonly MetricsDbContext _context;
    private readonly ILogger<DepartmentKeyMetricService> _logger;
    private readonly IDepartmentKeyMetricRepository _departmentKeyMetricRepository;

    public DepartmentKeyMetricService(
           MetricsDbContext context,
           ILogger<DepartmentKeyMetricService> logger,
           IDepartmentKeyMetricRepository keyKpiRepository)
    {
        _context = context;
        _logger = logger;
        _departmentKeyMetricRepository = keyKpiRepository;
    }

    public async Task<DepartmentKeyMetric> CreateAsync(DepartmentKeyMetric entity)
    {
        try
        {
            _departmentKeyMetricRepository.Create(entity);
            await _context.SaveChangesAsync();

            return entity;
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
            {
                _logger.LogError(ex, pgEx.MessageText);
                throw new DuplicateContentException("Key KPI already exist.", ex.InnerException);
            }
            else
            {
                _logger.LogError(ex, "Database error while creating Key KPI.");
                throw new Exception("A database error occurred.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating Key KPI.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public Task<bool> DeleteAsync(Guid metricCode)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<DepartmentKeyMetric>> FindAllAsync()
    {
        try
        {
            return await _departmentKeyMetricRepository.FindAllAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while querying Key KPIs.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<IEnumerable<DepartmentKeyMetric>> FindAllAsync(int pageNumber = 1, int pageSize = 20)
    {
        try
        {
            // var keyKpis = await _keyKpiRepository.FindAllAsQueryable()
            //     .Skip((pageNumber - 1) * pageSize)
            //     .Take(pageSize)
            //     .ToListAsync();
            var keyKpis = await _departmentKeyMetricRepository
                .FindAllAsync(pageNumber, pageSize);

            return keyKpis;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while querying Key KPIs.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<DepartmentKeyMetric?> FindByIdAsync(long id)
    {
        try
        {
            // var department = await _keyKpiRepository.FindByIdAsync(id);
            var department = await _departmentKeyMetricRepository.FindByIdAsync(id);
            if (department == null)
                throw new NotFoundException($"Key KPI with id {id} not found.");

            return department;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while querying Key KPI by Metric Code.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<DepartmentKeyMetric?> FindByCodeAsync(Guid departmentKeyMetricCode)
    {
        try
        {
            if (string.IsNullOrEmpty(departmentKeyMetricCode.ToString()))
                throw new ArgumentNullException("Parameter metricCode is required.");

            var keyKpi = await _departmentKeyMetricRepository.FindByCodeAsync(departmentKeyMetricCode);
            if (keyKpi == null)
                throw new NotFoundException($"Department Key Metric with code {departmentKeyMetricCode} not found.");

            return keyKpi;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while querying Department Key Metric by Code.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }


    // public async Task<DepartmentKeyMetric?> FindByMetricTitleAsync(string metricTitle)
    // {
    //     try
    //     {
    //         if (string.IsNullOrEmpty(metricTitle) || string.IsNullOrWhiteSpace(metricTitle))
    //             throw new ArgumentNullException("Parameter metricTitle is required.");

    //         var department = await _keyKpiRepository.FindByMetricTitleAsync(metricTitle);

    //         return department;
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Unexpected error while querying Key KPI by metric title.");
    //         throw new Exception("An unexpected error occurred. Please try again later.");
    //     }
    // }

    public async Task<IEnumerable<DepartmentKeyMetric>> FindByPeriodIdAsync(long periodId)
    {
        try
        {
            var keyKpis = await _departmentKeyMetricRepository.FindByPeriodIdAsync(periodId);

            return keyKpis ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while querying Key KPI by period id.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<long> FindCountAsync()
    {
        try
        {
            return await _departmentKeyMetricRepository.FindCountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while counting Key KPIs.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<DepartmentKeyMetric> UpdateAsync(Guid code, DepartmentKeyMetric entity)
    {
        if (string.IsNullOrEmpty(code.ToString()))
            throw new ArgumentNullException("Department code is required.");

        // TODO: does this check necessary?
        if (code != entity.DepartmentKeyMetricCode)
            throw new DbUpdateException("Update failed. Hint: depatmentKeyMetricCode to update and existing depatmentKeyMetricCode does not match.");

        try
        {
            var targetKeyKpi = await _departmentKeyMetricRepository.FindByCodeAsync(code);
            if (targetKeyKpi == null)
                throw new NotFoundException("Department not found.");
            // Handle concurrency (example using row version)
            // if (existing.RowVersion != department.RowVersion)
            //     return Result<Department>.Fail("Concurrency conflict.");

            // Note: This is full update (**not partial update, so need to set all field)
            _departmentKeyMetricRepository.Update(targetKeyKpi);
            targetKeyKpi.DepartmentId = entity.DepartmentId;
            targetKeyKpi.KpiSubmissionPeriodId = entity.KpiSubmissionPeriodId;
            targetKeyKpi.KeyMetricId = entity.KeyMetricId;
            // targetKeyKpi.MetricCode = entity.MetricCode;
            // targetKeyKpi.MetricTitle = entity.MetricTitle;
            await _context.SaveChangesAsync();

            // refetch updated entity
            var updatedEntity = await _departmentKeyMetricRepository.FindByCodeAsync(code);
            if (updatedEntity == null)
                throw new NotFoundException("Key KPI not found.");

            return updatedEntity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while updating Key KPI.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }
}
