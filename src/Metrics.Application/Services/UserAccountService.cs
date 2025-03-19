using Metrics.Application.DTOs;
using Metrics.Application.DTOs.EmployeeDtos;
using Metrics.Application.Mappers.DtoMappers;
using Metrics.Application.Services.IServices;
using Metrics.Domain.Entities;
using Metrics.Infrastructure;
using Metrics.Infrastructure.Data;
using Metrics.Infrastructure.Repositories.IRepositories;
using Microsoft.AspNetCore.Identity;
using System;
using System.Transactions;

namespace Metrics.Application.Services;

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
        catch (System.Exception)
        {
            transaction.Dispose();

            throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                   $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                   $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
        }
    }
}




// private readonly IUnitOfWork _unitOfWork;
// private readonly UserManager<ApplicationUser> _userManager;
// // private readonly IUserStore<ApplicationUser> _userStore;
// // private readonly IUserEmailStore<ApplicationUser> _emailStore;

// public UserAccountService(
//     IUnitOfWork unitOfWork,
//     UserManager<ApplicationUser> userManager)
// {
//     _unitOfWork = unitOfWork;
//     _userManager = userManager;
//     // _userStore = userStore;
//     // _emailStore = emailStore;
// }


// public async Task<IdentityResult> RegisterUserAsync(UserAccountCreateDto dto)
// {
//     using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

//     try
//     {
//         // STEP 1 - ApplicationUser
//         // var userInstance = Activator.CreateInstance<ApplicationUser>();
//         var userInstance = new ApplicationUser
//         {
//             UserName = dto.UserName,
//             Email = dto.Email
//         };
//         // await _userStore.SetUserNameAsync(userInstance, dto.UserName, CancellationToken.None);
//         // await _emailStore.SetEmailAsync(userInstance, dto.Email, CancellationToken.None);

//         var newUser = await _userManager.CreateAsync(userInstance, dto.Password);

//         if (!newUser.Succeeded)
//             return newUser;

//         // STEP 2 - Employee (User Profile)
//         // ** UserAccountCreateDto to Employee
//         var entity = dto.ToEntity();
//         // dto.ApplicationUserId = userInstance.Id;
//         entity.ApplicationUserId = userInstance.Id;

//         await _unitOfWork.EmployeeRepository.CreateAsync(entity);
//         await _unitOfWork.SaveChangesAsync();

//         transaction.Complete();

//         return IdentityResult.Success;
//     }
//     catch (System.Exception)
//     {
//         transaction.Dispose();

//         throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
//                $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
//                $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
//     }
// }
// }
