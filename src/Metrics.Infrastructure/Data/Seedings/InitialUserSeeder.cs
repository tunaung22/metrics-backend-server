
using Metrics.Application.Domains;
using Metrics.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Metrics.Application.DTOs.SeedingDtos;

namespace Metrics.Infrastructure.Data.Seedings;

public static class InitialUserSeeder
{
    public static async Task InitAsync(
        IServiceProvider serviceProvider)
    // InitialSeedingDataConfig initialSeedingData)
    {
        var userManager = serviceProvider
                    .GetRequiredService<UserManager<ApplicationUser>>();
        var seedingService = serviceProvider
                        .GetRequiredService<ISeedingService>();

        // var departmentService = serviceProvider
        //     .GetRequiredService<IDepartmentService>();
        // var roleManager = serviceProvider
        // .GetRequiredService<RoleManager<ApplicationRole>>();
        // var roleService = serviceProvider
        //     .GetRequiredService<IUserRoleService>();
        // var userManager = serviceProvider
        //    .GetRequiredService<UserManager<ApplicationUser>>();
        // var userAccountService = serviceProvider
        //     .GetRequiredService<IUserAccountService>();
        // var employeeService = serviceProvider
        //     .GetRequiredService<IEmployeeService>();        


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

        // sample: seedingService.SeedInitialUser(initialSeedingData.DefaultUserDataConfig);


        // IF NOT EXIST -> user with username "admin" + role "admin" + employee "admin" + department "admin department"
        // EXECUTE -> SeedInitialUser
        // var createDto = new DefaultUserCreateDto()
        // {
        //     DefaultUsername = initialSeedingData.DefaultUserDataConfig.DefaultUsername,
        //     DefaultEmail = initialSeedingData.DefaultUserDataConfig.DefaultEmail,
        //     DefaultPassword = initialSeedingData.DefaultUserDataConfig.DefaultPassword,
        //     DefaultRoleName = initialSeedingData.DefaultUserDataConfig.DefaultRoleName,
        //     DefaultFullName = initialSeedingData.DefaultUserDataConfig.DefaultFullName,
        //     DefaultDepartmentName = initialSeedingData.DefaultUserDataConfig.DefaultDepartmentName,
        //     EmployeeCode = initialSeedingData.DefaultUserDataConfig.EmployeeCode,
        //     EmployeeFullName = initialSeedingData.DefaultUserDataConfig.EmployeeFullName,
        //     EmployeeAddress = initialSeedingData.DefaultUserDataConfig.EmployeeAddress,
        //     EmployeePhoneNumber = initialSeedingData.DefaultUserDataConfig.EmployeePhoneNumber
        // };

        // **Values are hard coded for now
        var createDto = new DefaultUserCreateDto()
        {
            DefaultUsername = "sysadmin",
            DefaultEmail = "sysadmin",
            DefaultPassword = "00000000",
            DefaultRoleName = "admin",
            DefaultFullName = "System Admin",
            DefaultDepartmentName = "System Admin Department",
            EmployeeCode = new Guid().ToString(),
            EmployeeFullName = "System Admin",
            EmployeeAddress = "none",
            EmployeePhoneNumber = "none"
        };

        try
        {
            // if sysadmin user exist, do nothing
            // var adminUser = await userManager
            //                         .FindByNameAsync(createDto.DefaultUsername);
            // if (adminUser == null)
            await seedingService.SeedInitialUser(createDto);
        }
        catch (Exception ex)
        {
            throw new Exception("Seeding initial user failed.", ex);
        }

    }
}
