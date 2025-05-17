using Metrics.Application.Exceptions;
using Metrics.Application.Interfaces.IRepositories;
using Metrics.Application.Interfaces.IServices;
using Metrics.Infrastructure.Data;

namespace Metrics.Infrastructure.Services;

public class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly MetricsDbContext _context;
    // private readonly Ilogger _logger;
    public IDepartmentRepository Departments { get; }
    public IKpiPeriodRepository KpiPeriods { get; }
    public IUserRepository Users { get; }
    public IKpiSubmissionRepository KpiSubmissions { get; }

    public UnitOfWork(
        MetricsDbContext context,
        // Ilogger logger,
        IDepartmentRepository departmentRepository,
        IKpiPeriodRepository kpiPeriodRepository,
        IUserRepository employeeRepository,
        IKpiSubmissionRepository kpiSubmissionRepository)
    {
        _context = context;
        // _logger = Logger;
        Departments = departmentRepository;
        KpiPeriods = kpiPeriodRepository;
        Users = employeeRepository;
        KpiSubmissions = kpiSubmissionRepository;
        // // 
        // // _departmentRepository = new DepartmentRepository(context);
        // // _kpiPeriodRepository = new KpiPeriodRepository(context);
        // // _employeeRepository = new EmployeeRepository(context);
    }

    public async Task CompleteAsync()
    {
        await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

}


// public IDepartmentRepository DepartmentRepository => _departmentRepository;
// public IKpiPeriodRepository KpiPeriodRepository => _kpiPeriodRepository;
// public IEmployeeRepository EmployeeRepository => _employeeRepository;


// public async Task<int> SaveChangesAsync()
// {
//     try
//     {
//         return await _context.SaveChangesAsync();
//     }
//     catch (DbUpdateException ex)
//     {
//         if (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
//         {
//             // TODO: DuplicateEntityException
//             throw new DuplicateContentException(ex.Message, ex.InnerException);
//         }
//         else
//         {
//             // Handle database-specific errors
//             // TODO: DatabaseException
//             throw new Exception("A database error occurred.");
//         }
//     }
//     catch (Exception ex)
//     {
//         // Log unexpected errors
//         throw new Exception("An unexpected error occurred. Please try again later.", ex);
//     }
// }