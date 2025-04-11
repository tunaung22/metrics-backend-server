using Metrics.Application.Exceptions;
using Metrics.Application.Interfaces.IRepositories;
using Metrics.Application.Interfaces.IUnitOfWork;
using Metrics.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Metrics.Infrastructure;

public class UnitOfWork : IUnitOfWork
{
    private readonly MetricsDbContext _context;

    public IDepartmentRepository DepartmentRepository { get; }
    public IKpiPeriodRepository KpiPeriodRepository { get; }
    public IEmployeeRepository EmployeeRepository { get; }
    public IKpiSubmissionRepository KpiSubmissionRepository { get; }

    public UnitOfWork(
        MetricsDbContext context,
        IDepartmentRepository departmentRepository,
        IKpiPeriodRepository kpiPeriodRepository,
        IEmployeeRepository employeeRepository,
        IKpiSubmissionRepository kpiSubmissionRepository)
    {
        _context = context;
        DepartmentRepository = departmentRepository;
        KpiPeriodRepository = kpiPeriodRepository;
        EmployeeRepository = employeeRepository;
        KpiSubmissionRepository = kpiSubmissionRepository;
        // _departmentRepository = new DepartmentRepository(context);
        // _kpiPeriodRepository = new KpiPeriodRepository(context);
        // _employeeRepository = new EmployeeRepository(context);
    }

    // public IDepartmentRepository DepartmentRepository => _departmentRepository;
    // public IKpiPeriodRepository KpiPeriodRepository => _kpiPeriodRepository;
    // public IEmployeeRepository EmployeeRepository => _employeeRepository;


    public async Task<int> SaveChangesAsync()
    {
        try
        {
            return await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
            {
                // TODO: DuplicateEntityException
                throw new DuplicateContentException(ex.Message, ex.InnerException);
            }
            else
            {
                // Handle database-specific errors
                // TODO: DatabaseException
                throw new Exception("A database error occurred.");
            }
        }
        catch (Exception ex)
        {
            // Log unexpected errors
            throw new Exception("An unexpected error occurred. Please try again later.", ex);
        }
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }


}
