using Metrics.Application.DTOs.User;
using Metrics.Application.Domains;
using Metrics.Application.Interfaces.IRepositories;
using Metrics.Application.Interfaces.IServices;
using Metrics.Application.Mappers.DtoMappers;
using Metrics.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Metrics.Application.Exceptions;

namespace Metrics.Infrastructure.Services;

public class UserAccountService : IUserAccountService
{
    private readonly MetricsDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IUserRepository _employeeRepository;

    public UserAccountService(
        MetricsDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IUserRepository employeeRepository)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _employeeRepository = employeeRepository;
    }

    public async Task<IdentityResult> RegisterUserAsync(UserCreateDto dto)
    {
        // using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // ========== STEP 1 - ApplicationUser =============================
            // var userInstance = Activator.CreateInstance<ApplicationUser>();

            // ----- CHECK username, email duplication -----
            var errors = new List<IdentityError>();

            var accountWithUsernameExists = await _userManager.FindByNameAsync(dto.UserName);
            if (accountWithUsernameExists != null)
                // throw new MetricsDuplicateContentException("Username is already taken.");
                // newUserIdentityResult.Errors.Append(new IdentityError { Code = "", Description = "Username is already taken." });
                errors.Add(new IdentityError { Code = "DuplicateUserName", Description = "Username is already taken." });

            var accountWithEmailExists = await _userManager.FindByEmailAsync(dto.Email);
            if (accountWithEmailExists != null)
                // throw new MetricsDuplicateContentException("Email address is already taken.");
                // newUserIdentityResult.Errors.Append(new IdentityError { Code = "", Description = "Email address is already taken." });
                errors.Add(new IdentityError { Code = "DuplicateEmail", Description = "Email address is already taken." });

            if (errors.Any())
            {
                return IdentityResult.Failed(errors.ToArray());
            }
            // -----------------------------------------------------------------


            var userInstance = new ApplicationUser
            {
                UserName = dto.UserName,
                Email = dto.Email
            };
            // await _userStore.SetUserNameAsync(userInstance, dto.UserName, CancellationToken.None);
            // await _emailStore.SetEmailAsync(userInstance, dto.Email, CancellationToken.None);

            var newUserIdentityResult = await _userManager.CreateAsync(userInstance, dto.Password);

            if (!newUserIdentityResult.Succeeded)
                return newUserIdentityResult;
            // throw new Exception("User account creation failed.");

            var newUser = await _userManager.FindByNameAsync(dto.UserName);

            // --- Assign Role SINGLE ---
            // var roleToAssign = await _roleManager.FindByIdAsync(dto.RoleId);
            // if (roleToAssign == null)
            //     throw new MetricsNotFoundException("Role to assign does not exist.");

            // if (roleToAssign != null && newUser != null && !string.IsNullOrEmpty(roleToAssign.Name))
            //     await _userManager.AddToRoleAsync(newUser, roleToAssign.Name);

            // --- Assign Role MULTIPLE ---
            if (newUser != null && dto.RoleIds.Count > 0)
            {
                for (int i = 0; i < dto.RoleIds.Count; i++) // foreach cannot execute async operation
                {
                    var roleToAssign = await _roleManager.FindByIdAsync(dto.RoleIds[i]);
                    if (roleToAssign == null)
                        throw new MetricsNotFoundException("Role to assign does not exist.");

                    if (roleToAssign != null && !string.IsNullOrEmpty(roleToAssign.Name))
                        await _userManager.AddToRoleAsync(newUser, roleToAssign.Name);
                }
            }


            // var employeeRole = await _roleManager.FindByNameAsync("Employee");
            // if (employeeRole == null)
            //     throw new Exception("No such role, Employee.");
            // if (employeeRole != null && newUser != null && !string.IsNullOrEmpty(employeeRole.Name))
            //     await _userManager.AddToRoleAsync(newUser, employeeRole.Name);


            // ========== STEP 2 - Employee (User Profile) =====================
            // ** UserAccountCreateDto to Employee
            // var employeeEntity = dto.ToEntity();
            // employeeEntity.ApplicationUserId = userInstance.Id;
            // _employeeRepository.Create(employeeEntity);

            // ========== STEP 3 - Save All Changes =====================
            await _context.SaveChangesAsync();

            // transaction.Complete();
            await transaction.CommitAsync();

            return IdentityResult.Success;
        }
        catch (MetricsDuplicateContentException)
        {
            // transaction.Dispose();
            await transaction.RollbackAsync();
            throw;
        }
        catch (MetricsNotFoundException)
        {
            // transaction.Dispose();
            await transaction.RollbackAsync();
            throw;
        }
        catch (Exception)
        {
            // transaction.Dispose();
            await transaction.RollbackAsync();

            throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                   $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                   $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
        }
    }
}
