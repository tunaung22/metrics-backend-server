using Metrics.Application.Domains;
using Metrics.Application.Interfaces.IRepositories;
using Metrics.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Metrics.Infrastructure.Repositories;

public class UserTitleRepository : IUserTitleRepository
{
    public readonly MetricsDbContext _context;

    public UserTitleRepository(MetricsDbContext context)
    {
        _context = context;
    }

    public void Create(UserTitle entity)
    {
        _context.UserTitles.Add(entity);
    }

    public async Task<IEnumerable<UserTitle>> FindAllAsync()
    {
        return await _context.UserTitles
            .OrderBy(e => e.TitleName)
            .ToListAsync();
    }

    public async Task<UserTitle?> FindByTitleNameAsync(string titleName)
    {
        var userTitle = await _context.UserTitles
            // .Where(e => string.Equals(departmentName, e.DepartmentName, StringComparison.OrdinalIgnoreCase))
            // .Where(e => e.DepartmentName.Equals(departmentName, StringComparison.CurrentCultureIgnoreCase))
            .Where(e => e.TitleName.ToLower() == titleName.ToLower())
            .FirstOrDefaultAsync();

        // if (department == null)
        //     throw new NotFoundException("Department not found.");

        return userTitle;
    }

    public async Task<UserTitle?> FindByIdAsync(long id)
    {
        var department = await _context.UserTitles
            .Where(e => e.Id == id)
            .FirstOrDefaultAsync();

        return department;
    }
}
