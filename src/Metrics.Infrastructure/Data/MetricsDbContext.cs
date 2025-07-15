using Metrics.Application.Domains;
using Metrics.Application.Interfaces;
using Metrics.Infrastructure.Data.EntityConfig;
using Metrics.Shared.Configurations;
using Metrics.Shared.Utils;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;

namespace Metrics.Infrastructure.Data;

public class MetricsDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    private readonly PostgresDbConfig _pgConfig;

    public MetricsDbContext(
        DbContextOptions<MetricsDbContext> options,
        IOptions<PostgresDbConfig> pgConfig) : base(options)
    {
        _pgConfig = pgConfig.Value;
    }

    // ========== DbSet ========================================================
    // public DbSet<ApplicationUser> ApplicationUsers { get; set; }
    // public DbSet<ApplicationRole> ApplicationRoles { get; set; }
    public DbSet<UserTitle> UserTitles { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<KpiSubmissionPeriod> KpiSubmissionPeriods { get; set; }
    public DbSet<KpiSubmission> KpiSubmissions { get; set; }
    public DbSet<KeyMetric> KeyMetrics { get; set; }
    public DbSet<KeyKpiSubmissionConstraint> KeyKpiSubmissionConstraints { get; set; }
    public DbSet<DepartmentKeyMetric> DepartmentKeyMetrics { get; set; }
    public DbSet<KeyKpiSubmission> KeyKpiSubmissions { get; set; }
    public DbSet<KeyKpiSubmissionItem> KeyKpiSubmissionItems { get; set; }
    public DbSet<CaseFeedbackSubmission> CaseFeedbackSubmissions { get; set; }


    public override int SaveChanges()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is IAuditColumn &&
                        (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var entity = (IAuditColumn)entry.Entity;
            if (entry.State == EntityState.Added)
            {
                entity.CreatedAt = DateTimeOffset.UtcNow; // Set created date
            }
            entity.ModifiedAt = DateTimeOffset.UtcNow; // Always set modified date
        }

        return base.SaveChanges();
    }

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

        // for Case Insensitive Collation
        // builder.HasCollation("metric_ci_collation",
        //     locale: "en-u-ks-primary",
        //     provider: "icu",
        //     deterministic: false
        // );
        builder.HasPostgresExtension("citext");

        // ** EFCore.NamingConventions package doesn't change the table name
        //    to snakecase properly.
        foreach (var entity in builder.Model.GetEntityTypes())
        {
            // Replace table names
            entity.SetTableName(entity.GetTableName()?.ToSnakeCase());
        }

        // https://www.npgsql.org/efcore/modeling/generated-properties.html?tabs=13%2Cefcore5#hilo-autoincrement-generation
        // sequence id
        // builder.UseHiLo("entityframeworkcore_hilo_sequence");
        builder.HasDefaultSchema(_pgConfig.PgSchema);

        // ------------ Entity Configurations ------------
        /* https://learn.microsoft.com/en-us/ef/core/modeling/#grouping-configuration
            Method 1: new ItemHeaderConfig().Configure(builder.Entity<ItemHeader>());
            Method 2: builder.ApplyConfiguration(new ItemHeaderConfig());
        */
        builder.ApplyConfiguration(new ApplicationUserConfig());
        builder.ApplyConfiguration(new ApplicationRoleConfig());
        builder.ApplyConfiguration(new UserTitleConfig()); // -------------- User Group

        builder.ApplyConfiguration(new DepartmentConfig());
        builder.ApplyConfiguration(new KpiSubmissionPeriodConfig()); // ---- KPI Period

        builder.ApplyConfiguration(new KpiSubmissionConfig()); // ---------- KPI Submission

        builder
            .ApplyConfiguration(new KeyMetricConfig()) // ---------------- Key Metric
            .ApplyConfiguration(new DepartmentKeyMetricConfig()) // ------ Key KPI
            .ApplyConfiguration(new KeyKpiSubmissionConstraintConfig()) // --- Key KPI Submission Constraint 
            .ApplyConfiguration(new KeyKpiSubmissionConfig()) // --------- Key KPI
            .ApplyConfiguration(new KeyKpiSubmissionItemConfig()); // ---- Key KPI

        builder
            .ApplyConfiguration(new CaseFeedbackSubmissionConfig()); // -------- Case Feedback

        // ----------- Seeding -----------
        // builder.Entity<...>().HasData(...);
        // builder.Entity<Department>().HasData(
        //     new Department { DepartmentName = "Clinical Department" },
        //     new Department { DepartmentName = "Nursing Department (OPD)" }
        // );
    }

}
