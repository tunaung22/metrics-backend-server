using Metrics.Application.Domains;
using Metrics.Application.Exceptions;
using Metrics.Application.Interfaces.IRepositories;
using Metrics.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Metrics.Infrastructure.Repositories;

public class EmployeeRepository : IEmployeeRepository
{
    private readonly MetricsDbContext _context;

    public EmployeeRepository(MetricsDbContext context)
    {
        _context = context;
    }

    public void Create(Employee entity)
    {
        _context.Employees.Add(entity);
    }

    public void Delete(Employee entity)
    {
        _context.Employees.Remove(entity);
    }

    public async Task<bool> EmployeeExistsAsync(string employeeCode)
    {
        return await _context.Employees
            .AnyAsync(e => e.EmployeeCode.ToLower() == employeeCode.ToLower());
    }

    public async Task<IEnumerable<Employee>> FindAllAsync()
    {
        return await _context.Employees
            .OrderBy(e => e.EmployeeCode)
            .ToListAsync();
    }

    public IQueryable<Employee> FindAllAsQueryable()
    {
        return _context.Employees
            .OrderBy(e => e.EmployeeCode);
    }

    public async Task<Employee> FindByEmployeeCodeAsync(string employeeCode)
    {
        var employee = await _context.Employees
            .Where(e => e.EmployeeCode.ToLower() == employeeCode.ToLower())
            .FirstOrDefaultAsync();

        if (employee == null)
            throw new NotFoundException("Employee not found.");

        return employee;
    }

    public async Task<Employee> FindByIdAsync(long id)
    {
        var employee = await _context.Employees
            .Where(e => e.Id == id)
            .FirstOrDefaultAsync();

        if (employee == null)
            throw new NotFoundException("Employee not found.");

        return employee;
    }

    public void Update(Employee entity)
    {
        _context.Entry(entity).State = EntityState.Modified;
        _context.Employees.Update(entity);
    }

}
