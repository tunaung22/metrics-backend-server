using Metrics.Application.Domains;
using Metrics.Application.DTOs.KeyMetric;

namespace Metrics.Application.Common.Mappers;

public static class KeyMetricMapper
{
    public static KeyMetricDto MapToDto(this KeyMetric e)
    {
        return new KeyMetricDto
        {
            Id = e.Id,
            MetricCode = e.MetricCode,
            MetricTitle = e.MetricTitle,
            Description = e.Description,
            IsDeleted = e.IsDeleted,
        };
    }
}
