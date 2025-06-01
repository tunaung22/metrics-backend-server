using Metrics.Application.Domains;
using Metrics.Application.DTOs.AccountDtos;
using Metrics.Application.DTOs.UserAccountDtos;
using Metrics.Application.Exceptions;
using Metrics.Application.Interfaces.IRepositories;
using Metrics.Application.Interfaces.IServices;
using Metrics.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Metrics.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly MetricsDbContext _context;
    private readonly ILogger<UserService> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly IUserRepository _userRepository;

    public UserService(
        MetricsDbContext context,
        ILogger<UserService> logger,
                UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IUserRepository userRepository)
    {
        _context = context;
        _logger = logger;
        _userManager = userManager;
        _roleManager = roleManager;
        _userRepository = userRepository;
    }

    public async Task<IdentityResult> RegisterUserAsync(UserAccountCreateDto createDto)
    {
        // using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // ========== STEP 1 - ApplicationUser =============================
            // var userInstance = Activator.CreateInstance<ApplicationUser>();

            // ----- CHECK username, email duplication -----
            var errors = new List<IdentityError>();

            var accountWithUsernameExists = await _userManager.FindByNameAsync(createDto.UserName);
            if (accountWithUsernameExists != null)
                // throw new MetricsDuplicateContentException("Username is already taken.");
                // newUserIdentityResult.Errors.Append(new IdentityError { Code = "", Description = "Username is already taken." });
                errors.Add(new IdentityError { Code = "DuplicateUserName", Description = "Username is already taken." });

            var accountWithEmailExists = await _userManager.FindByEmailAsync(createDto.Email);
            if (accountWithEmailExists != null)
                // throw new MetricsDuplicateContentException("Email address is already taken.");
                // newUserIdentityResult.Errors.Append(new IdentityError { Code = "", Description = "Email address is already taken." });
                errors.Add(new IdentityError { Code = "DuplicateEmail", Description = "Email address is already taken." });

            var accountWithUserCodeExists = await FindByUserCodeAsync(createDto.UserCode);
            if (accountWithUserCodeExists != null)
                // throw new MetricsDuplicateContentException("Email address is already taken.");
                // newUserIdentityResult.Errors.Append(new IdentityError { Code = "", Description = "Email address is already taken." });
                errors.Add(new IdentityError { Code = "DuplicateUserCode", Description = "User code is already taken." });


            if (errors.Any())
            {
                return IdentityResult.Failed(errors.ToArray());
            }
            // -----------------------------------------------------------------


            var userInstance = new ApplicationUser
            {
                UserName = createDto.UserName,
                Email = createDto.Email,
                UserCode = createDto.UserCode,
                FullName = createDto.FullName,
                PhoneNumber = createDto.PhoneNumber ?? string.Empty,
                ContactAddress = createDto.Address ?? string.Empty,
                DepartmentId = createDto.DepartmentId,
                UserTitleId = createDto.UserTitleId
            };
            // await _userStore.SetUserNameAsync(userInstance, dto.UserName, CancellationToken.None);
            // await _emailStore.SetEmailAsync(userInstance, dto.Email, CancellationToken.None);

            var newUserIdentityResult = await _userManager.CreateAsync(userInstance, createDto.Password);

            if (!newUserIdentityResult.Succeeded)
                return newUserIdentityResult;
            // throw new Exception("User account creation failed.");

            var newUser = await _userManager.FindByNameAsync(createDto.UserName);

            // --- Assign Role SINGLE ---
            // var roleToAssign = await _roleManager.FindByIdAsync(dto.RoleId);
            // if (roleToAssign == null)
            //     throw new MetricsNotFoundException("Role to assign does not exist.");

            // if (roleToAssign != null && newUser != null && !string.IsNullOrEmpty(roleToAssign.Name))
            //     await _userManager.AddToRoleAsync(newUser, roleToAssign.Name);

            // --- Assign Role MULTIPLE ---
            if (newUser != null && createDto.RoleIds.Count > 0)
            {
                for (int i = 0; i < createDto.RoleIds.Count; i++) // foreach cannot execute async operation
                {
                    var roleToAssign = await _roleManager.FindByIdAsync(createDto.RoleIds[i]);
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
        catch (DbUpdateException ex)
        {
            // when (ex.InnerException is PostgresException postgresEx && postgresEx.SqlState == "23505")
            if (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
            {
                _logger.LogError(ex, pgEx.MessageText);
                // TODO: DuplicateEntityException
                throw new DuplicateContentException("Duplicate content exception.", ex.InnerException);
            }
            else if (ex.InnerException is PostgresException foreignKeyViolation && foreignKeyViolation.SqlState == "23503")
            {
                _logger.LogError(ex, foreignKeyViolation.MessageText);
                throw new InvalidOperationException("Foreign Key violation exception.", ex.InnerException);
            }
            else
            {
                // Handle database-specific errors
                _logger.LogError(ex, "Database error while creating user.");
                // TODO: DatabaseException
                throw new Exception("A database error occurred.");
            }
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

    public async Task<ApplicationUser> UpdateAsync(string userId, UserUpdateDto updateDto)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrWhiteSpace(userId))
            throw new ArgumentNullException("User ID is required.");

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var targetUser = await _userManager.FindByIdAsync(userId);
            if (targetUser == null)
                throw new NotFoundException("User not found.");

            // Handle concurrency (example using row version)
            // if (existing.RowVersion != application_user.RowVersion)
            //     return Result<ApplicationUser>.Fail("Concurrency conflict.");

            // Note: This is full update (**not partial update)

            // 1. Update ApplicationUser
            targetUser.UserCode = updateDto.UserCode;
            targetUser.DepartmentId = updateDto.DepartmentId;
            targetUser.UserTitleId = updateDto.UserTitleId;
            var result = await _userManager.UpdateAsync(targetUser);
            if (!result.Succeeded)
                throw new DbUpdateException("User update failed.");
            // 2. Update Role
            var currentRoles = await _userManager.GetRolesAsync(targetUser);
            await _userManager.RemoveFromRolesAsync(targetUser, currentRoles);
            await _userManager.AddToRolesAsync(targetUser, updateDto.RoleNames);
            // foreach (var roleId in updateDto.RoleIds)
            // {
            //     // role.id == roleId ? pass : assign role
            //     //   admin == staff
            //     // existing roles
            //     var existingRoles = await _userManager.GetRolesAsync(targetUser);
            // }

            // refetch updated user
            var updatedUser = await _userManager.FindByIdAsync(userId);
            if (updatedUser == null)
                throw new NotFoundException("Can't load updated user.");

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return updatedUser;
        }
        catch (Exception ex)
        {
            // transaction.Dispose();
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Unexpected error while updating user.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<ApplicationUser> UpdateProfileAsync(string userId, UserProfileUpdateDto updateDto)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrWhiteSpace(userId))
            throw new ArgumentNullException("User ID is required.");

        try
        {
            var targetUser = await _userManager.FindByIdAsync(userId);
            if (targetUser == null)
                throw new NotFoundException("User not found.");

            // Handle concurrency (example using row version)
            // if (existing.RowVersion != application_user.RowVersion)
            //     return Result<ApplicationUser>.Fail("Concurrency conflict.");

            // Note: This is full update (**not partial update)
            // 1. Update ApplicationUser
            targetUser.UserCode = updateDto.UserCode;
            targetUser.Email = updateDto.Email;
            targetUser.FullName = updateDto.FullName;
            targetUser.ContactAddress = updateDto.ContactAddress ?? string.Empty;
            targetUser.PhoneNumber = updateDto.PhoneNumber ?? string.Empty;
            var result = await _userManager.UpdateAsync(targetUser);
            if (!result.Succeeded)
                throw new DbUpdateException("User update failed.");

            // refetch updated user
            var updatedUser = await _userManager.FindByIdAsync(userId);
            if (updatedUser == null)
                throw new NotFoundException("Can't load updated user.");

            await _context.SaveChangesAsync();

            return updatedUser;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while updating user.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }


    public async Task<ApplicationUser> FindByIdAsync(string userId)
    {
        try
        {
            if (string.IsNullOrEmpty(userId))
                throw new Exception("User ID is required.");

            // var user = await _userManager.FindByIdAsync(userId);
            var user = await _userManager.Users
                .Include(u => u.Department)
                .Include(u => u.UserTitle)
                .FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                throw new Exception($"User not found.");

            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while querying user.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<ApplicationUser> FindByUsernameAsync(string username)
    {
        try
        {
            if (string.IsNullOrEmpty(username))
                throw new Exception("Username is required.");

            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
                throw new Exception($"User not found.");

            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while querying user.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<ApplicationUser?> FindByUserCodeAsync(string userCode)
    {
        try
        {
            if (string.IsNullOrEmpty(userCode))
                throw new Exception("Parameter usercode is required.");

            var user = await _userRepository.FindByUserCodeAsync(userCode);
            // if (employee == null)
            //     throw new Exception($"User with code {userCode} not found.");

            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while querying user by user code.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<IEnumerable<ApplicationUser>> FindAllActiveAsync()
    {
        try
        {
            var users = await _userRepository.FindAllAsQueryable()
                .Include(u => u.Department)
                .Include(u => u.UserTitle)
                // .Where(u => )
                .Where(u =>
                    u.UserName != "sysadmin" &&
                    u.LockoutEnabled == true &&
                    (u.LockoutEnd == null || u.LockoutEnd <= DateTimeOffset.UtcNow))
                .ToListAsync();

            return users;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while querying active users.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<IEnumerable<ApplicationUser>> FindAllAsync()
    {
        try
        {
            var users = await _userRepository.FindAllAsQueryable()
                .Include(u => u.Department)
                .Include(u => u.UserTitle)
                .Where(u => u.UserName != "sysadmin")
                .ToListAsync();

            return users;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while querying all users.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }



    /*
    public async Task<ApplicationUser> CreateAsync(ApplicationUser user)
    {
        // if (await _employeeRepository.EmployeeExistsAsync(employee.EmployeeCode.ToString()))
        //     throw new DuplicateContentException("Employee code already exist.");

        try
        {
            _userRepository.Create(user);
            await _context.SaveChangesAsync();

            return user;
        }
        // catch (DbUpdateException ex)
        // {
        //     // when (ex.InnerException is PostgresException postgresEx && postgresEx.SqlState == "23505")
        //     if (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
        //     {
        //         _logger.LogError(ex, pgEx.MessageText);
        //         // TODO: DuplicateEntityException
        //         throw new DuplicateContentException("Employee already exist.", ex.InnerException);
        //     }
        //     else
        //     {
        //         // Handle database-specific errors
        //         _logger.LogError(ex, "Database error while creating department.");
        //         // TODO: DatabaseException
        //         throw new Exception("A database error occurred.");
        //     }
        // }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating department.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<ApplicationUser> UpdateAsync(string username, ApplicationUser user)
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrWhiteSpace(username))
            throw new ArgumentNullException("Username is required.");

        // TODO: does this check necessary?
        // if (username != user.UserName)
        //     throw new DbUpdateException("Update failed. Hint: employeeCode to update and existing employeeCode does not match.");

        try
        {
            var targetUser = await _userRepository.FindByUserCodeAsync(username);
            if (targetUser == null)
                throw new NotFoundException($"Username {username} not found.");

            // Handle concurrency (example using row version)
            // if (existing.RowVersion != department.RowVersion)
            //     return Result<Department>.Fail("Concurrency conflict.");

            // Note: This is full update (**not partial update)
            _userRepository.Update(targetUser);
            targetUser.UserName = user.UserName;
            targetUser.FullName = user.FullName;
            targetUser.PhoneNumber = user.PhoneNumber;
            targetUser.ContactAddress = user.ContactAddress;
            targetUser.DepartmentId = user.DepartmentId;
            // targetUser.ApplicationUserId = user.ApplicationUserId;
            await _context.SaveChangesAsync();

            // refetch updated entity
            var updatedEntity = await _userRepository.FindByUserCodeAsync(username);
            return updatedEntity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while updating employee.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<bool> DeleteAsync(string username)
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrWhiteSpace(username))
            throw new ArgumentNullException("Username is required.");

        try
        {
            var user = await _userManager.FindByNameAsync(username);

            if (user == null)
                throw new NotFoundException($"Username {username} not found.");
            // or return true; // Idempotent: Treat as success

            _userRepository.Delete(user);
            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting employee.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    public async Task<ApplicationUser> FindByIdAsync(long id)
    {
        try
        {
            if (id <= 0)
                throw new Exception("Parameter id is required.");

            var employee = await _userRepository.FindByIdAsync(id);
            if (employee == null)
                throw new Exception($"Employee with id {id} not found.");

            return employee;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while querying employee by employee id.");
            throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }

    */

    // public async Task<long> FindByUserIdAsync(string userId)
    // {
    //     if (string.IsNullOrEmpty(userId) || string.IsNullOrWhiteSpace(userId))
    //         throw new Exception("Parameter userId is required.");

    //     try
    //     {
    //         var employeeId = await _userRepository.FindAllAsQueryable()
    //             .Where(e => e.ApplicationUserId == userId)
    //             .Select(e => e.Id)
    //             .FirstOrDefaultAsync();

    //         if (employeeId <= 0)
    //             throw new NotFoundException("Employee not found.");

    //         return employeeId;
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Unexpected error while querying employee id by user id.");
    //         throw new Exception("An unexpected error occurred. Please try again later.");
    //     }
    // }


    // ========== Return DTO ===================================================
    // public async Task<EmployeeGetDto> CreateAsync(EmployeeCreateDto createDto)
    // {
    //     try
    //     {
    //         if (await _employeeRepository.EmployeeExistsAsync(createDto.EmployeeCode.ToString()))
    //             // TODO: DuplicateEntityException
    //             throw new DuplicateContentException("Employee code already exist.");

    //         var entity = createDto.ToEntity();
    //         _employeeRepository.Create(entity);
    //         await _context.SaveChangesAsync();

    //         return createDto.ToGetDto();
    //     }
    //     catch (DbUpdateException ex)
    //     {
    //         // when (ex.InnerException is PostgresException postgresEx && postgresEx.SqlState == "23505")
    //         if (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
    //         {
    //             _logger.LogError(ex, pgEx.MessageText);
    //             // TODO: DuplicateEntityException
    //             throw new DuplicateContentException("Employee already exist.", ex.InnerException);
    //         }
    //         else
    //         {
    //             // Handle database-specific errors
    //             _logger.LogError(ex, "Database error while creating department.");
    //             // TODO: DatabaseException
    //             throw new Exception("A database error occurred.");
    //         }
    //     }
    //     catch (Exception ex)
    //     {
    //         // Log unexpected errors
    //         _logger.LogError(ex, "Unexpected error while creating department.");
    //         throw new Exception("An unexpected error occurred. Please try again later.");
    //     }
    // }

    // public async Task<EmployeeGetDto> UpdateAsync(string employeeCode, EmployeeUpdateDto updateDto)
    // {
    //     try
    //     {
    //         // Validate existence
    //         var targetEmployee = await _employeeRepository.FindByEmployeeCodeAsync(employeeCode);
    //         if (targetEmployee == null)
    //             // TODO: NotFoundException
    //             throw new NotFoundException("Employee not found.");

    //         // Handle concurrency (example using row version)
    //         // if (existing.RowVersion != department.RowVersion)
    //         //     return Result<Department>.Fail("Concurrency conflict.");

    //         // Track changes

    //         _employeeRepository.Update(targetEmployee);
    //         targetEmployee.FullName = updateDto.FullName;
    //         targetEmployee.PhoneNumber = updateDto.PhoneNumber;
    //         targetEmployee.Address = updateDto.Address;
    //         targetEmployee.DepartmentId = updateDto.DepartmentId;

    //         await _context.SaveChangesAsync();

    //         return targetEmployee.ToGetDto();
    //     }
    //     catch (DbUpdateException ex)
    //     {
    //         if (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
    //         {
    //             _logger.LogError(ex, pgEx.MessageText);
    //             // TODO: DuplicateEntityException
    //             throw new Exception("Duplicate entry exception occurred.");
    //         }
    //         else
    //         {
    //             // Handle database-specific errors
    //             _logger.LogError(ex, "Database error while updating employee.");
    //             // TODO: DatabaseException
    //             throw new Exception("A database error occurred.");
    //         }
    //         // Handle database-specific errors
    //     }
    //     catch (Exception ex)
    //     {
    //         // Log unexpected errors
    //         _logger.LogError(ex, "Unexpected error while updating employee.");
    //         throw new Exception("An unexpected error occurred. Please try again later.");

    //     }
    // }

    // public async Task<bool> DeleteAsync(string employeeCode)
    // {
    //     try
    //     {
    //         var employee = await _employeeRepository.FindByEmployeeCodeAsync(employeeCode);
    //         if (employee == null)
    //             return true; // Idempotent: Treat as success

    //         if (employee == null)
    //             // TODO: NotFoundException
    //             throw new Exception("Employee not found.");

    //         _employeeRepository.Delete(employee);
    //         await _context.SaveChangesAsync();

    //         return true;
    //     }
    //     catch (DbUpdateException ex)
    //     {
    //         // Handle database-specific errors
    //         _logger.LogError(ex, "Database error while deleting employee.");
    //         // TODO: DatabaseException
    //         throw new Exception("A database error occurred.");
    //     }
    //     catch (Exception ex)
    //     {
    //         // Log unexpected errors
    //         _logger.LogError(ex, "Unexpected error while deleting employee.");
    //         // throw; // Propagate to global exception handler
    //         throw new Exception("An unexpected error occurred. Please try again later.");
    //     }
    // }

    // public async Task<EmployeeGetDto> FindByIdAsync(long id)
    // {
    //     try
    //     {
    //         if (id <= 0)
    //             // TODO: ValidationException
    //             throw new Exception("Parameter id is required.");

    //         var employee = await _employeeRepository.FindByIdAsync(id);
    //         if (employee == null)
    //             // TODO: NotFoundException
    //             throw new Exception($"Employee with id {id} not found.");

    //         return employee.ToGetDto();
    //     }
    //     catch (Exception ex)
    //     {
    //         // Log unexpected errors
    //         _logger.LogError(ex, "Unexpected error while querying employee by employee id.");
    //         // throw; // Propagate to global exception handler
    //         throw new Exception("An unexpected error occurred. Please try again later.");
    //     }
    // }

    // public async Task<EmployeeGetDto> FindByEmployeeCodeAsync(string employeeCode)
    // {
    //     try
    //     {
    //         if (string.IsNullOrEmpty(employeeCode))
    //             // TODO: ValidationException
    //             throw new Exception("Parameter employeeCode is required.");

    //         var employee = await _employeeRepository.FindByEmployeeCodeAsync(employeeCode);
    //         if (employee == null)
    //             // TODO: NotFoundException
    //             throw new Exception($"Employee with code {employeeCode} not found.");

    //         return employee.ToGetDto();
    //     }
    //     catch (Exception ex)
    //     {
    //         // Log unexpected errors
    //         _logger.LogError(ex, "Unexpected error while querying department by department code.");
    //         // throw; // Propagate to global exception handler
    //         throw new Exception("An unexpected error occurred. Please try again later.");
    //     }
    // }


    // // public async Task<IEnumerable<EmployeeGetDto>> FindAllAsync()
    // // {
    // //     try
    // //     {
    // //         var employees = await _employeeRepository.FindAllAsync();

    // //         return employees.ToGetDto();
    // //     }
    // //     catch (Exception ex)
    // //     {
    // //         // Log unexpected errors
    // //         _logger.LogError(ex, "Unexpected error while querying employees.");
    // //         // throw; // Propagate to global exception handler
    // //         throw new Exception("An unexpected error occurred. Please try again later.");
    // //     }
    // // }

    // public async Task<IEnumerable<EmployeeGetAllDto>> FindAllAsync()
    // {
    //     try
    //     {
    //         var employees = await _employeeRepository.FindAllAsQueryable()
    //             .Include(e => e.CurrentDepartment)
    //             // .Include(e => e.UserAccount)
    //             .Include(e => e.KpiSubmissions)
    //             .ToListAsync();

    //         return employees.Select(e => new EmployeeGetAllDto
    //         {
    //             EmployeeCode = e.EmployeeCode,
    //             FullName = e.FullName,
    //             Address = e.Address,
    //             PhoneNumber = e.PhoneNumber,
    //             DepartmentId = e.DepartmentId,
    //             CurrentDepartment = e.CurrentDepartment,
    //             ApplicationUserId = e.ApplicationUserId
    //         }).ToList();
    //     }
    //     catch (Exception ex)
    //     {
    //         // Log unexpected errors
    //         _logger.LogError(ex, "Unexpected error while querying employees.");
    //         // throw; // Propagate to global exception handler
    //         throw new Exception("An unexpected error occurred. Please try again later.");
    //     }
    // }

    // public async Task<long> FindByUserIdAsync(string userId)
    // {
    //     try
    //     {
    //         if (string.IsNullOrEmpty(userId) || string.IsNullOrWhiteSpace(userId))
    //             throw new Exception("Parameter userId is required.");

    //         var employeeId = await _employeeRepository.FindAllAsQueryable()
    //             .Where(e => e.ApplicationUserId == userId)
    //             .Select(e => e.Id)
    //             .FirstOrDefaultAsync();

    //         if (employeeId <= 0)
    //             throw new NotFoundException("Employee not found.");

    //         return employeeId;
    //     }
    //     catch (Exception ex)
    //     {
    //         _logger.LogError(ex, "Unexpected error while querying employee id by user id.");
    //         throw new Exception("An unexpected error occurred. Please try again later.");
    //     }
    // }
}
