using Metrics.Application.Domains;
using Metrics.Application.Exceptions;
using Metrics.Application.Interfaces.IRepositories;
using Metrics.Application.Interfaces.IServices;
using Metrics.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Metrics.Infrastructure.Services;

public class KeyMetricService : IKeyMetricService
{
    private readonly MetricsDbContext _context;
    private readonly ILogger<KeyMetricService> _logger;
    private readonly IKeyMetricRepository _keyMetricRepository;

    public KeyMetricService(
        MetricsDbContext context,
        ILogger<KeyMetricService> logger,
        IKeyMetricRepository keyMetricRepository)
    {
        _context = context;
        _logger = logger;
        _keyMetricRepository = keyMetricRepository;
    }


    public async Task<KeyMetric> CreateAsync(KeyMetric entity)
    {
        try
        {
            _keyMetricRepository.Create(entity);
            await _context.SaveChangesAsync();

            return entity;
        }
        catch (DbUpdateException ex)
        {
            // when (ex.InnerException is PostgresException postgresEx && postgresEx.SqlState == "23505")
            if (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
            {
                _logger.LogError(ex, pgEx.MessageText);
                // TODO: DuplicateEntityException
                throw new MetricsDuplicateContentException($"Title already exist.", ex.InnerException);
            }
            else
            {
                // Handle database-specific errors
                _logger.LogError(ex, "Database error while creating department.");
                // TODO: DatabaseException
                throw new Exception("A database error occurred.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating key metric.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public Task<bool> DeleteAsync(Guid metricCode)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<KeyMetric>> FindAllAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<KeyMetric>> FindAllAsync(int pageNumber = 1, int pageSize = 20)
    {
        try
        {
            var keyKpis = await _keyMetricRepository
                .FindAllAsync(pageNumber, pageSize);

            return keyKpis;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while querying Key KPIs.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<KeyMetric?> FindByCodeAsync(Guid metricCode)
    {
        try
        {
            if (metricCode == Guid.Empty)
                throw new ArgumentNullException("Parameter metricCode is required.");

            var metric = await _keyMetricRepository.FindByMetricCodeAsync(metricCode);
            if (metric == null)
                throw new NotFoundException($"Key Metric with code {metricCode} not found.");

            return metric;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while querying key metric by code.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public Task<KeyMetric?> FindByIdAsync(long id)
    {
        throw new NotImplementedException();
    }

    public Task<KeyMetric?> FindByMetricTitleAsync(string metricTitle)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> MetricTitleExistsAsync(string title)
    {
        try
        {
            return await _keyMetricRepository.MetricTitleExistsAsync(title);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while checking key metric name exist.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<IEnumerable<KeyMetric>> SearchByMetricTitleAsync(string inputKeyword)
    {
        try
        {
            var keyKpis = await _keyMetricRepository
               .SearchByMetricTitleAsync(inputKeyword);

            return keyKpis;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while querying Key Metrics.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public Task<long> FindCountAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<KeyMetric> UpdateAsync(Guid code, KeyMetric entity)
    {
        // TODO: does this check necessary?
        // less likely to cause error
        // if (code != entity.MetricCode)
        // throw new DbUpdateException("Update failed. Hint: departmentCode to update and existing departmentCode does not match.");

        try
        {
            var targetKeyMetric = await _keyMetricRepository.FindByMetricCodeAsync(code);
            if (targetKeyMetric == null)
                throw new NotFoundException("Key Metric not found.");

            // Handle concurrency (example using row version)
            // if (existing.RowVersion != department.RowVersion)
            //     return Result<Department>.Fail("Concurrency conflict.");

            // Note: This is full update (**not partial update)
            // **** so need to assign all the fields
            _keyMetricRepository.Update(targetKeyMetric);
            targetKeyMetric.MetricCode = entity.MetricCode;
            targetKeyMetric.MetricTitle = entity.MetricTitle;
            targetKeyMetric.Description = entity.Description;
            await _context.SaveChangesAsync();

            // refetch updated entity
            var updatedEntity = await _keyMetricRepository.FindByMetricCodeAsync(code);
            if (updatedEntity == null)
                throw new NotFoundException("Key Metric not found.");

            return updatedEntity;
        }
        catch (DbUpdateException ex)
        {
            // when (ex.InnerException is PostgresException postgresEx && postgresEx.SqlState == "23505")
            if (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
            {
                _logger.LogError(ex, pgEx.MessageText);
                // TODO: DuplicateEntityException
                throw new MetricsDuplicateContentException($"Key Metric Title already exist.", ex.InnerException);
            }
            else
            {
                // Handle database-specific errors
                _logger.LogError(ex, "Database error while updating department.");
                // TODO: DatabaseException
                throw new Exception("A database error occurred.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while updating key metric.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }
}
