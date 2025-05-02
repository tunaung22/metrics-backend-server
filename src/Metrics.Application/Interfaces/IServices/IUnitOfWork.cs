using Metrics.Application.Interfaces.IRepositories;

namespace Metrics.Application.Interfaces.IServices;

public interface IUnitOfWork
{
    IDepartmentRepository Departments { get; }
    IKpiPeriodRepository KpiPeriods { get; }
    IEmployeeRepository Employees { get; }
    IKpiSubmissionRepository KpiSubmissions { get; }

    Task CompleteAsync();
}
