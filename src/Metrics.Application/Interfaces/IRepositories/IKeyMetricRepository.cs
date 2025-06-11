using Metrics.Application.Domains;
using System;

namespace Metrics.Application.Interfaces.IRepositories;

public interface IKeyMetricRepository
{
    void Create(KeyMetric entity);
    void CreateRange(IEnumerable<KeyMetric> entites);
    void Update(KeyMetric entity);
    void Delete(KeyMetric entity);
    Task<IEnumerable<KeyMetric>> FindAllAsync();
    Task<IEnumerable<KeyMetric>> FindAllAsync(int pageNumber, int pageSize);
    Task<KeyMetric?> FindByIdAsync(long id);
    Task<KeyMetric?> FindByMetricCodeAsync(Guid metricCode);
    Task<IEnumerable<KeyMetric>> FindByMetricTitleAsync(string metricTitle);
    Task<IEnumerable<KeyMetric>> SearchByMetricTitleAsync(string metricTitle);
    Task<bool> MetricTitleExistsAsync(string metricTitle);
    Task<long> FindCountAsync();

    // ========== Queryable ====================================================
    IQueryable<KeyMetric> FindAllAsQueryable();
}
