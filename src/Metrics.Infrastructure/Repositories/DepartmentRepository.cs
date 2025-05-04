using Metrics.Application.Domains;
using Metrics.Application.Exceptions;
using Metrics.Application.Interfaces.IRepositories;
using Metrics.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Metrics.Infrastructure.Repositories;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly MetricsDbContext _context;

    public DepartmentRepository(MetricsDbContext context)
    {
        _context = context;
    }

    public void Create(Department entity)
    {
        _context.Departments.Add(entity);
    }

    public void Update(Department entity)
    {
        _context.Entry(entity).State = EntityState.Modified;
    }

    public void Delete(Department entity)
    {
        _context.Departments.Remove(entity);
    }

    public async Task<Department> FindByDepartmentCodeAsync(string departmentCode)
    {
        var department = await _context.Departments
            // .Include(e => e.Employees)
            .Where(e => e.DepartmentCode.ToString().ToLower() == departmentCode.ToLower())
            .FirstOrDefaultAsync();

        if (department == null)
            throw new NotFoundException("Department not found.");

        return department;
    }

    public async Task<Department?> FindByDepartmentNameAsync(string departmentName)
    {
        var department = await _context.Departments
            // .Include(e => e.Employees)
            // .Where(e => string.Equals(departmentName, e.DepartmentName, StringComparison.OrdinalIgnoreCase))
            // .Where(e => e.DepartmentName.Equals(departmentName, StringComparison.CurrentCultureIgnoreCase))
            .Where(e => e.DepartmentName.ToLower() == departmentName.ToLower())
            .FirstOrDefaultAsync();

        // if (department == null)
        //     throw new NotFoundException("Department not found.");

        return department;
    }

    public async Task<Department> FindByIdAsync(long id)
    {
        var department = await _context.Departments
            // .Include(e => e.Employees)
            .Where(e => e.Id == id)
            .FirstOrDefaultAsync();

        if (department == null)
            throw new NotFoundException("Department not found.");

        return department;
    }

    public async Task<bool> DepartmentExistsAsync(string departmentCode)
    {
        return await _context.Departments
            .AnyAsync(e => e.DepartmentCode.ToString() == departmentCode);
    }

    public async Task<IEnumerable<Department>> FindAllAsync()
    {
        return await _context.Departments
            .OrderBy(e => e.DepartmentName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Department>> FindAllAsync(int pageNumber, int pageSize)
    {
        return await _context.Departments
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<long> FindCountAsync()
    {
        return await _context.Departments.CountAsync();
    }

    public IQueryable<Department> FindAllAsQueryable()
    {
        return _context.Departments
            .OrderBy(e => e.DepartmentName);
    }

}
