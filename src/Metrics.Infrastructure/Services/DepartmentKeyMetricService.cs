using Metrics.Application.Common.Mappers;
using Metrics.Application.Domains;
using Metrics.Application.DTOs.DepartmentKeyMetric;
using Metrics.Application.Exceptions;
using Metrics.Application.Interfaces.IRepositories;
using Metrics.Application.Interfaces.IServices;
using Metrics.Application.Results;
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

    public async Task<bool> DeleteAsync(Guid code) // Soft Delete
    {
        try
        {
            var targetKeyKpi = await _departmentKeyMetricRepository.FindByCodeAsync(code);
            if (targetKeyKpi == null)
                throw new NotFoundException("Department Key Metric not found.");

            targetKeyKpi.IsDeleted = true;
            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting Department Key Metric.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<bool> UnDeleteAsync(Guid DepartmentKeyMetricCode) // Soft Delete
    {
        try
        {
            var targetKeyKpi = await _departmentKeyMetricRepository
                .FindByCodeAsync(DepartmentKeyMetricCode);

            if (targetKeyKpi == null)
                throw new NotFoundException("Department Key Metric not found.");

            targetKeyKpi.IsDeleted = false;
            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while undeleting Department Key Metric.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
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

    public async Task<ResultT<List<DepartmentKeyMetricDto>>> FindByPeriodIdAsync(long periodId)
    {
        try
        {
            var data = await _departmentKeyMetricRepository.FindAllByPeriodIdAsync(periodId);
            var result = data.Select(e => e.MapToDto()).ToList();

            return ResultT<List<DepartmentKeyMetricDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occured at fetching department key metrics by period id. {msg}", ex.Message);
            return ResultT<List<DepartmentKeyMetricDto>>.Fail("Failed to fetch department key metrics by period.", ErrorType.UnexpectedError);
        }
    }

    public async Task<ResultT<List<DepartmentKeyMetricDto>>> FindByPeriodByKeyIssueDepartmentAsync(
        long periodId, Guid keyIssueDepartmentCode)
    {
        try
        {
            var data = await _departmentKeyMetricRepository.FindByPeriodByKeyIssueDepartmentAsync(periodId, keyIssueDepartmentCode);
            var result = data.Select(e => e.MapToDto()).ToList();

            return ResultT<List<DepartmentKeyMetricDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occured while fetching department key metrics by period id by key issue department. {msg}", ex.Message);
            return ResultT<List<DepartmentKeyMetricDto>>.Fail("Failed to fetch department key metrics by period by key issue department", ErrorType.UnexpectedError);
        }
    }


    public async Task<IEnumerable<DepartmentKeyMetric>> FindAllByPeriodIdAsync(long periodId)
    {
        try
        {
            var keyKpis = await _departmentKeyMetricRepository.FindAllByPeriodIdAsync(periodId);

            return keyKpis ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while querying Key KPI by period id.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<IEnumerable<DepartmentKeyMetric>> FindAllByPeriodNameAsync(string periodName)
    {
        try
        {
            var keyKpis = await _departmentKeyMetricRepository.FindAllByPeriodNameAsync(periodName);

            return keyKpis ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while querying Key KPI by period id.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<IEnumerable<DepartmentKeyMetric>> FindAllByPeriodAndDepartmentAsync(
        string currentPeriodName,
        Guid currentDepartmentCode)
    {
        try
        {
            // var result = await _departmentKeyMetricRepository
            //     .FindAllByPeriodAndDepartmentAsync(CurrentPeriodName, CurrentDepartmentCode);
            var result = await _context.DepartmentKeyMetrics
                .Where(k => k.KpiSubmissionPeriod.PeriodName == currentPeriodName
                    && k.KeyIssueDepartment.DepartmentCode == currentDepartmentCode)
                .OrderBy(k => k.KeyIssueDepartment.DepartmentName)
                .Include(k => k.KpiSubmissionPeriod)
                .Include(k => k.KeyMetric)
                .Include(k => k.KeyIssueDepartment)
                .ToListAsync();

            return result ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while querying Department Key Metric by period, department.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    // FindBy PeriodName And DepartmentCode And KeyMetricCode
    public async Task<DepartmentKeyMetric?> FindByPeriodAndDepartmentAndKeyMetricAsync(
        string periodName,
        Guid departmentCode,
        Guid keyMetricCode)
    {
        try
        {
            var keyKpi = await _departmentKeyMetricRepository
                .FindByPeriodAndDepartmentAndKeyMetricAsync(periodName, departmentCode, keyMetricCode);

            return keyKpi;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while querying Department Key Metric by period, department, key metric.");
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

    public async Task<ResultT<List<DepartmentKeyMetricDto>>> FindByPeriodNameAsync(string periodName)
    {
        try
        {
            var data = await _departmentKeyMetricRepository.FindAllByPeriodNameAsync(periodName);
            var result = data.Select(e => e.MapToDto()).ToList();

            return ResultT<List<DepartmentKeyMetricDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while fetching department key metrics by period name.");
            return ResultT<List<DepartmentKeyMetricDto>>.Fail("An unexpected error occurred while fetching department key metrics by period name.", ErrorType.UnexpectedError);
        }
    }


    public async Task<ResultT<Dictionary<long, int>>> FindCountsByPeriodAsync(List<long> periodIds)
    {
        try
        {
            var countResult = await _departmentKeyMetricRepository.FindCountsByPeriodAsync(periodIds);

            return ResultT<Dictionary<long, int>>.Success(countResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get department key metric count by Periods.");
            return ResultT<Dictionary<long, int>>.Fail("Failed to get department key metric count by Periods.", ErrorType.UnexpectedError);
        }
    }
}
