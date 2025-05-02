using Metrics.Application.Domains;
using Metrics.Application.DTOs.SeedingDtos;
using Metrics.Application.Interfaces.IRepositories;
using Metrics.Application.Interfaces.IServices;
using Metrics.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;

namespace Metrics.Infrastructure.Services;

public class SeedingService : ISeedingService
{
    private readonly MetricsDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    // private readonly IDepartmentRepository _departmentRepository;
    // private readonly IEmployeeRepository _employeeRepository;

    public SeedingService(MetricsDbContext context,
                            UserManager<ApplicationUser> userManager,
                            RoleManager<ApplicationRole> roleManager)
    // IDepartmentRepository departmentRepository,
    // IEmployeeRepository employeeRepository)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        // _departmentRepository = departmentRepository;
        // _employeeRepository = employeeRepository;
    }

    public async Task SeedInitialUser(DefaultUserCreateDto createDto)
    {
        /* === logic ===
            1. CHECK department exist
                IF NOT exist:
                    - CREATE new {department}
            2. CHECK admin user exist
                IF NOT exist:
                    - CREATE new {admin user}
            3. CHECK role admin exist?
                IF NOT exist:
                    - CREATE new {role admin}
                - ASSIGN {role admin}
            4. CHECK employee exist
                IF NOT exist:
                    - CREATE new {employee}
                    - ASSIGN {admin user}
                    - ASSIGN {department}

        */

        // using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // ========== Department ===============
            // 1. CHECK department exist
            // if (string.IsNullOrEmpty(createDto.DefaultDepartmentName) || string.IsNullOrWhiteSpace(createDto.DefaultDepartmentName))
            //     throw new ArgumentNullException("Parameter departmentCode is required.");

            // var adminDepartment = await _departmentRepository.FindByDepartmentNameAsync(createDto.DefaultDepartmentName);

            // if (adminDepartment == null)
            // {
            //     var newDepartment = new Department { DepartmentName = createDto.DefaultDepartmentName };
            //     _departmentRepository.Create(newDepartment);
            //     await _context.SaveChangesAsync();
            //     adminDepartment = newDepartment;
            // }

            // ========== Role ===============
            // 3. CHECK role admin exist?
            var adminRole = await _roleManager
                        .FindByNameAsync(createDto.DefaultRoleName);

            if (adminRole == null)
            {
                // - CREATE new { role admin }
                var roleInstance = new ApplicationRole
                {
                    Name = createDto.DefaultRoleName,
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                };
                var newRoleResult = await _roleManager.CreateAsync(roleInstance);
                if (newRoleResult.Succeeded)
                    adminRole = await _roleManager.FindByNameAsync(createDto.DefaultRoleName);
            }

            // 3.2 CHECK role employee 
            //     CREATE employee role
            var employeeRoleName = "Employee";
            var employeeRole = await _roleManager
                        .FindByNameAsync(employeeRoleName);

            if (employeeRole == null)
            {
                // - CREATE new { role admin }
                var roleInstance = new ApplicationRole
                {
                    Name = employeeRoleName,
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                };
                var newRoleResult = await _roleManager.CreateAsync(roleInstance);
                if (newRoleResult.Succeeded)
                    employeeRole = await _roleManager.FindByNameAsync(employeeRoleName);
            }

            // ========== User =================
            // 2. CHECK admin user exist
            var adminUser = await _userManager
                        .FindByNameAsync(createDto.DefaultUsername);
            if (adminUser == null)
            {
                // - CREATE new { admin user }
                var userInstance = new ApplicationUser
                {
                    UserName = createDto.DefaultUsername,
                    Email = createDto.DefaultEmail,
                };
                var newUser = await _userManager.CreateAsync(userInstance, createDto.DefaultPassword);

                if (!newUser.Succeeded)
                    throw new Exception("User Account creation failed.");

                adminUser = userInstance;
            }

            // - ASSIGN {role admin}
            if (adminRole != null && adminUser != null && !string.IsNullOrEmpty(adminRole.Name))
                await _userManager.AddToRoleAsync(adminUser, adminRole.Name);


            // ========== Employee ==========
            // 4.CHECK employee exist 
            // admin user exist, then check employee of that admin
            // **Note: (indended to use if) to check again if admin user is not null
            // condition:   1. already exist
            //              2. or just now created
            // if (adminUser != null)
            // {
            //     if (string.IsNullOrEmpty(createDto.EmployeeCode))
            //         throw new Exception("Parameter employeeCode is required.");

            //     var existingEmployee = await _employeeRepository.FindByEmployeeCodeAsync(createDto.EmployeeCode);
            //     if (existingEmployee == null)
            //     {
            //         // - CREATE new { employee }
            //         // - ASSIGN { admin user}
            //         // - ASSIGN { department}
            //         var employee = new Employee
            //         {
            //             EmployeeCode = createDto.EmployeeCode,
            //             FullName = createDto.EmployeeFullName,
            //             Address = createDto.EmployeeAddress,
            //             PhoneNumber = createDto.EmployeePhoneNumber,
            //             DepartmentId = adminDepartment.Id,
            //             ApplicationUserId = adminUser.Id
            //         };
            //         _employeeRepository.Create(employee);
            //     }
            // }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                                                $"Error: {ex.Message}");
            // _logger.LogError(ex, "Unexpected error while querying department by department code.");
            // throw new Exception("An unexpected error occurred. Please try again later.");
        }
    }
}
