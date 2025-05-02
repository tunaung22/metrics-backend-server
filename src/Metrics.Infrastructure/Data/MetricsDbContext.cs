using Metrics.Application.Domains;
using Metrics.Infrastructure.Data.EntityConfig;
using Metrics.Shared.Configurations;
using Metrics.Shared.Utils;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Metrics.Infrastructure.Data;

public class MetricsDbContext : IdentityDbContext
{
    private readonly PostgresDbConfig _pgConfig;

    public MetricsDbContext(
        DbContextOptions<MetricsDbContext> options,
        IOptions<PostgresDbConfig> pgConfig) : base(options)
    {
        _pgConfig = pgConfig.Value;
    }

    // ========== DbSet ========================================================
    public DbSet<ApplicationUser> ApplicationUsers { get; set; }
    public DbSet<ApplicationRole> ApplicationRoles { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<KpiPeriod> KpiPeriods { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<KpiSubmission> KpiSubmissions { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        // optionsBuilder.UseSeeding(async (context, _) =>
        // {
        // ---------- Role -------------------------------------------------
        // var adminRole = context.Set<ApplicationRole>().FirstOrDefault(role => role.Name == "admin");
        // if (adminRole == null)
        // {
        //     var newRole = new ApplicationRole
        //     {
        //         Name = "admin",
        //     };
        //     var newRole = await _roleManager
        // }

        // ---------- User -------------------------------------------------
        // var adminUser = context.Set<ApplicationUser>().FirstOrDefault(user => user.UserName == "admin");
        // if (adminUser == null)
        // {
        //     // Create new Admin user
        //     var userInstance = new ApplicationUser
        //     {
        //         UserName = "admin",
        //         Email = "admin@metricsbackend.com"
        //     };
        //     var newUser = await _userManager.CreateAsync(userInstance, dto.Password);
        //     context.Set<ApplicationUser>().Add(new ApplicationUser
        //     {
        //         UserName = "admin",
        //         Email = "admin@metricsbackend.com",


        //     });
        //     context.SaveChanges();
        // }
        // });
    }



    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);



        foreach (var entity in builder.Model.GetEntityTypes())
        {
            // Replace table names
            entity.SetTableName(entity.GetTableName()?.ToSnakeCase());
        }
        // ------------ Entity Configurations ------------
        /* https://learn.microsoft.com/en-us/ef/core/modeling/#grouping-configuration
            Method 1: new ItemHeaderConfig().Configure(builder.Entity<ItemHeader>());
            Method 2: builder.ApplyConfiguration(new ItemHeaderConfig());
        */
        Console.WriteLine("Postgres Schema: " + _pgConfig.PgSchema);
        builder.HasDefaultSchema(_pgConfig.PgSchema);
        builder.ApplyConfiguration(new DepartmentConfig());
        builder.ApplyConfiguration(new KpiPeriodConfig());
        builder.ApplyConfiguration(new EmployeeConfig());
        builder.ApplyConfiguration(new KpiSubmissionConfig());

        // ----------- Seeding -----------
        // builder.Entity<...>().HasData(...);
        // builder.Entity<Department>().HasData(
        //     new Department { DepartmentName = "Clinical Department" },
        //     new Department { DepartmentName = "Nursing Department (OPD)" }
        // );
    }

}
