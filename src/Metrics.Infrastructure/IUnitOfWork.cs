using Metrics.Infrastructure.Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metrics.Infrastructure
{
    public interface IUnitOfWork : IDisposable
    {
        IDepartmentRepository DepartmentRepository { get; }
        IKpiPeriodRepository KpiPeriodRepository { get; }
        IEmployeeRepository EmployeeRepository { get; }
        //...

        Task<int> SaveChangesAsync();
    }
}
