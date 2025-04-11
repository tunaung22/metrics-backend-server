using Metrics.Application.DTOs.UserAccountDtos;
using Metrics.Application.Entities;
using Metrics.Application.Interfaces.IRepositories;
using Metrics.Application.Interfaces.IServices;
using Metrics.Application.Mappers.DtoMappers;
using Metrics.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using System.Transactions;

namespace Metrics.Infrastructure.Services;

public class UserAccountService : IUserAccountService
{
    private readonly MetricsDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmployeeRepository _employeeRepository;

    public UserAccountService(
        MetricsDbContext context,
        UserManager<ApplicationUser> userManager,
        IEmployeeRepository employeeRepository)
    {
        _context = context;
        _userManager = userManager;
        _employeeRepository = employeeRepository;
    }

    public async Task<IdentityResult> RegisterUserAsync(UserAccountCreateDto dto)
    {
        using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        try
        {
            // ========== STEP 1 - ApplicationUser =============================
            // var userInstance = Activator.CreateInstance<ApplicationUser>();
            var userInstance = new ApplicationUser
            {
                UserName = dto.UserName,
                Email = dto.Email
            };
            // await _userStore.SetUserNameAsync(userInstance, dto.UserName, CancellationToken.None);
            // await _emailStore.SetEmailAsync(userInstance, dto.Email, CancellationToken.None);

            var newUser = await _userManager.CreateAsync(userInstance, dto.Password);

            if (!newUser.Succeeded)
                return newUser;

            // ========== STEP 2 - Employee (User Profile) =====================
            // ** UserAccountCreateDto to Employee
            var entity = dto.ToEntity();
            // dto.ApplicationUserId = userInstance.Id;
            entity.ApplicationUserId = userInstance.Id;

            _employeeRepository.Create(entity);
            await _context.SaveChangesAsync();

            transaction.Complete();

            return IdentityResult.Success;
        }
        catch (Exception)
        {
            transaction.Dispose();

            throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                   $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                   $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
        }
    }
}
