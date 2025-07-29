using Metrics.Application.Domains;
using Metrics.Application.Exceptions;
using Metrics.Application.Interfaces.IRepositories;
using Metrics.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Metrics.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly MetricsDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserRepository(MetricsDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public void SetPasswordChangeRequirementStatus(ApplicationUser user, bool requireChange)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }
        user.IsPasswordChangeRequired = requireChange;
        _context.Entry(user).State = EntityState.Modified;
    }

    public async Task<ApplicationUser?> FindByUserCodeAsync(string userCode)
    {
        var user = await _userManager.Users
            .Where(e => e.UserCode.ToLower() == userCode.ToLower())
            .FirstOrDefaultAsync();

        return user;
    }

    public async Task<IEnumerable<ApplicationUser>> FindAllAsync()
    {
        return await _userManager.Users
            .OrderBy(e => e.UserName)
            .ToListAsync();
    }

    public IQueryable<ApplicationUser> FindAllAsQueryable()
    {
        return _userManager.Users
            .OrderBy(e => e.UserName)
            .AsQueryable();
    }


}


// public void Create(ApplicationUser entity)
// {
//     _context.ApplicationUsers.Add(entity);
// }

// public void Delete(ApplicationUser entity)
// {
//     _context.Employees.Remove(entity);
// }

// public async Task<bool> EmployeeExistsAsync(string userCode)
// {
//     return await _context.Employees
//         .AnyAsync(e => e.UserCode.ToLower() == userCode.ToLower());
// }






// public async Task<ApplicationUser> FindByIdAsync(long id)
// {
//     var employee = await _context.Employees
//         .Where(e => e.Id == id)
//         .FirstOrDefaultAsync();

//     if (employee == null)
//         throw new NotFoundException("Employee not found.");

//     return employee;
// }

// public void Update(ApplicationUser entity)
// {
//     _context.Entry(entity).State = EntityState.Modified;
//     _context.Employees.Update(entity);
// }
