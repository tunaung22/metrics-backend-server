using Metrics.Application.DTOs.KeyMetric;
using Metrics.Web.Models.KeyMetric;

namespace Metrics.Web.Common.Mappers;

public static class KeyMetricMapper
{
    public static KeyMetricViewModel MapToViewModel(this KeyMetricDto dto)
    {
        return new KeyMetricViewModel
        {
            Id = dto.Id,
            LookupId = dto.MetricCode,
            MetricTitle = dto.MetricTitle,
            Description = dto.Description,
            IsDeleted = dto.IsDeleted,
        };
    }
}
