using Metrics.Application.Domains;

namespace Metrics.Application.Interfaces.IServices;

public interface IKeyMetricService
{
    Task<KeyMetric> CreateAsync(KeyMetric entity);
    Task<IEnumerable<KeyMetric>> CreateRangeAsync(IEnumerable<KeyMetric> entities);
    Task<KeyMetric> UpdateAsync(Guid code, KeyMetric entity);
    Task<bool> DeleteAsync(Guid metricCode);
    Task<KeyMetric?> FindByIdAsync(long id);
    Task<KeyMetric?> FindByCodeAsync(Guid metricCode);
    Task<KeyMetric?> FindByMetricTitleAsync(string metricTitle);
    Task<IEnumerable<KeyMetric>> FindAllAsync();
    Task<IEnumerable<KeyMetric>> FindAllAsync(int pageNumber = 1, int pageSize = 20);
    Task<long> FindCountAsync();
    Task<bool> MetricTitleExistsAsync(string title);
    Task<IEnumerable<KeyMetric>> SearchByMetricTitleAsync(string inputKeyword);
}
