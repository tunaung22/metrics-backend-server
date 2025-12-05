using Metrics.Application.DTOs.DepartmentKeyMetric;
using Metrics.Application.Results;

namespace Metrics.Application.Interfaces.IServices;

public interface IDepartmentKeyImportService
{
    /// <summary>
    /// Copy Department Keys from Source Period to Target Period
    /// </summary>
    /// <param name="sourcePeriodID"></param>
    /// <param name="targetPeriodID"></param>
    /// <returns></returns>
    Task<Result> CopyAsync(long sourcePeriodID, long targetPeriodID);
}
