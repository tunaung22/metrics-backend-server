using Metrics.Domain.Entities;
using Metrics.Domain.Exceptions;
using Metrics.Infrastructure.Data;
using Metrics.Infrastructure.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Metrics.Infrastructure.Repositories
{
    public class DepartmentRepository : IDepartmentRepository // : GenericRepository<Department>, IDepartmentRepository
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

        public void Delete(Department entity)
        {
            _context.Departments.Remove(entity);
        }

        public async Task<bool> DepartmentExistsAsync(string departmentCode)
        {
            return await _context.Departments
                .AnyAsync(e => e.DepartmentCode.ToString() == departmentCode);
        }

        public IQueryable<Department> FindAllAsQueryable()
        {
            return _context.Departments
                .OrderBy(e => e.DepartmentName);
        }

        public async Task<IEnumerable<Department>> FindAllAsync()
        {
            return await _context.Departments
                .OrderBy(e => e.DepartmentName)
                .ToListAsync();
        }

        public async Task<Department> FindByDepartmentCodeAsync(string departmentCode)
        {
            var department = await _context.Departments
                // .Include(e => e.Employees)
                .Where(e => e.DepartmentCode.ToString() == departmentCode)
                .FirstOrDefaultAsync();

            if (department == null)
                // TODO
                // throw new EntityNotFoundException();
                throw new NotFoundException("Department not found.");

            return department;
        }

        public async Task<Department> FindByIdAsync(long id)
        {
            var department = await _context.Departments
                // .Include(e => e.Employees)
                .Where(e => e.Id == id)
                .FirstOrDefaultAsync();

            if (department == null)
                // TODO
                // throw new EntityNotFoundException();
                throw new NotFoundException("Department not found.");

            return department;
        }

        public void Update(Department entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
        }





        // public DepartmentRepository(MetricsDbContext context) : base(context)
        // {
        // }

        // public async Task<bool> DepartmentExistAsync(string code)
        // {
        //     return await _dbSet.AnyAsync(e => e.DepartmentCode.ToString() == code);
        // }

        // public async Task<Department?> FindByDepartmentCodeAsync(string code)
        // {
        //     // return await _context.Set<Department>().Where(e => e.DepartmentCode == code).FirstOrDefaultAsync();
        //     return await _dbSet.FirstOrDefaultAsync(e => e.DepartmentCode.ToString() == code);
        // }

        // //public void Update(Department entity)
        // //{

        // //    _context.Set<Department>().Update(entity);
        // //}

        // public void UpdatePartial(Department entity)
        // {
        //     //_context.Set<Department>().Entry 
        //     //entity.DepartmentCode
        //     _context.Set<Department>().Update(entity);
        // }
    }
}
