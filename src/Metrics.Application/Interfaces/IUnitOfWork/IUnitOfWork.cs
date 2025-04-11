using Metrics.Application.Interfaces.IRepositories;

namespace Metrics.Application.Interfaces.IUnitOfWork;

public interface IUnitOfWork : IDisposable
{
    IDepartmentRepository DepartmentRepository { get; }
    IKpiPeriodRepository KpiPeriodRepository { get; }
    IEmployeeRepository EmployeeRepository { get; }
    IKpiSubmissionRepository KpiSubmissionRepository { get; }

    Task<int> SaveChangesAsync();
}
