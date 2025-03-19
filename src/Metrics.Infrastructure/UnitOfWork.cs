using Metrics.Infrastructure.Data;
using Metrics.Infrastructure.Repositories;
using Metrics.Infrastructure.Repositories.IRepositories;

namespace Metrics.Infrastructure;

public class UnitOfWork : IUnitOfWork
{
    private readonly MetricsDbContext _context;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IKpiPeriodRepository _kpiPeriodRepository;
    private readonly IEmployeeRepository _employeeRepository;

    public UnitOfWork(MetricsDbContext context)
    {
        _context = context;
        _departmentRepository = new DepartmentRepository(context);
        _kpiPeriodRepository = new KpiPeriodRepository(context);
        _employeeRepository = new EmployeeRepository(context);
    }

    public IDepartmentRepository DepartmentRepository => _departmentRepository;
    public IKpiPeriodRepository KpiPeriodRepository => _kpiPeriodRepository;
    public IEmployeeRepository EmployeeRepository => _employeeRepository;


    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }


}
