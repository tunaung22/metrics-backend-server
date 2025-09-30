using Metrics.Application.Interfaces.IRepositories;

namespace Metrics.Application.Common;

public interface IUnitOfWork
{
    IDepartmentRepository Departments { get; }
    IKpiSubmissionPeriodRepository KpiPeriods { get; }
    IUserRepository Users { get; }
    IKpiSubmissionRepository KpiSubmissions { get; }

    Task CompleteAsync();
}
