using Metrics.Application.Authorization;
using Metrics.Application.Common;
using Metrics.Application.Interfaces.IRepositories;
using Metrics.Application.Interfaces.IServices;
using Metrics.Infrastructure.Identity;
using Metrics.Infrastructure.Identity.RequirementHandlers;
using Metrics.Infrastructure.Identity.Requirements;
using Metrics.Infrastructure.Repositories;
using Metrics.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Metrics.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // ==============Repositories===========================================
        services
            .AddScoped<IDepartmentRepository, DepartmentRepository>()
            .AddScoped<IKpiSubmissionPeriodRepository, KpiSubmissionPeriodRepository>()
            .AddScoped<IUserRepository, UserRepository>()
            .AddScoped<IUserTitleRepository, UserTitleRepository>()
            .AddScoped<IKpiSubmissionRepository, KpiSubmissionRepository>()
            .AddScoped<IKeyKpiSubmissionRepository, KeyKpiSubmissionRepository>()
            .AddScoped<IKeyMetricRepository, KeyMetricRepository>()
            .AddScoped<IDepartmentKeyMetricRepository, DepartmentKeyMetricRepository>()
            .AddScoped<IKeyKpiSubmissionConstraintRepository, KeyKpiSubmissionConstraintRepository>()
            .AddScoped<ICaseFeedbackRepository, CaseFeedbackRepository>()
            .AddScoped<ICaseFeedbackScoreSubmissionRepository, CaseFeedbackScoreSubmissionRepository>()
        ;

        // ==============Services===============================================
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services
            .AddScoped<ISeedingService, SeedingService>()
            .AddScoped<IDepartmentService, DepartmentService>()
            .AddScoped<IKpiSubmissionPeriodService, KpiSubmissionPeriodService>()
            .AddScoped<IUserService, UserService>()
            .AddScoped<IUserTitleService, UserTitleService>()
            .AddScoped<IUserAccountService, UserAccountService>()
            .AddScoped<IUserRoleService, UserRoleService>()
            .AddScoped<IKpiSubmissionService, KpiSubmissionService>()
            .AddScoped<IKeyKpiSubmissionService, KeyKpiSubmissionService>()
            .AddScoped<IKeyMetricService, KeyMetricService>()
            .AddScoped<IDepartmentKeyMetricService, DepartmentKeyMetricService>()
            .AddScoped<IKeyKpiSubmissionConstraintService, KeyKpiSubmissionConstraintService>()
            .AddScoped<ICaseFeedbackService, CaseFeedbackService>()
            .AddScoped<ICaseFeedbackScoreSubmissionService, CaseFeedbackScoreSubmissionService>()
            .AddScoped<IIdentityService, IdentityService>()
        ;

        return services;
    }

    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        // Authorization
        services.AddAuthorizationCore(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();

            // ADMIN FEATURES
            options.AddPolicy(ApplicationPolicies.CanAccessAdminFeaturesPolicy, policy =>
            {
                // policy.RequireRole("Admin");
                policy.AddRequirements(new AdminRoleRequirement());
                policy.AddRequirements(new AccessAdminFeaturesRequirement());
            });

            // SCORE KEY KPI
            options.AddPolicy(ApplicationPolicies.CanSubmitKeyKpiScorePolicy, policy =>
            {
                // policy.RequireRole("Staff");
                policy.AddRequirements(new StaffRoleRequirement());
                policy.AddRequirements(new SubmitKeyKpiScoreRequirement(new List<string>
                {
                    // UserGroups.Staff, // staff not allowed
                    UserGroups.HOD,
                    UserGroups.Management
                }));
            });

            // SCORE FEEDBACK
            options.AddPolicy(ApplicationPolicies.CanSubmitFeedbackScorePolicy, policy =>
            {
                // policy.RequireRole("Staff");
                policy.AddRequirements(new StaffRoleRequirement());
                policy.AddRequirements(new SubmitFeedbackScoreRequirement(new List<string>
                {
                    // UserGroups.Staff, // allows management only
                    // UserGroups.HOD, // allows management only
                    UserGroups.Management
                }));
            });

            // SCORE KPI
            options.AddPolicy(ApplicationPolicies.CanSubmitKpiScorePolicy, policy =>
            {
                // policy.RequireRole("Staff");
                policy.AddRequirements(new StaffRoleRequirement());
                policy.AddRequirements(new SubmitKpiScoreRequirement(new List<string>
                {
                    UserGroups.Staff,
                    UserGroups.HOD,
                    UserGroups.Management
                }));
            });

            // GIVE FEEDBACK
            options.AddPolicy(ApplicationPolicies.CanGiveFeedbackPolicy, policy =>
            {
                // policy.RequireRole("Staff");
                policy.AddRequirements(new StaffRoleRequirement());
                policy.AddRequirements(new GiveFeedbackRequirement(new List<string>
                {
                    UserGroups.Staff,
                    UserGroups.HOD,
                    // UserGroups.Management // management will give score
                }));
            });
        });

        // .AddPolicy("CanAccessAdminFeaturePolicy", policy =>
        // {
        //     policy.RequireRole("Admin");
        // })
        // .AddPolicy("CanSubmitBaseScorePolicy", policy =>
        // {
        //     policy.RequireRole("Staff");
        //     // policy.RequireClaim("UserGroup", ["Staff", "HOD", "Management"]);
        //     policy.RequireAssertion(context =>
        //     {
        //         var userGroupClaims = context.User.FindFirst("UserGroup");
        //         if (userGroupClaims == null) return false;

        //         var allowedGroups = new[] { "staff", "hod", "management" };
        //         return allowedGroups.Any(group =>
        //         {
        //             return string.Equals(userGroupClaims.Value, group, StringComparison.OrdinalIgnoreCase);
        //         });
        //     });
        //     // TODO: Implement Custom Authorization Requirement
        //     // IAuthorizationRequirement
        //     // policy.AddRequirements(new MinimumLevelRequirement(1000));
        // })
        // .AddPolicy("CanSubmitKeyScorePolicy", policy =>
        // {
        //     policy.RequireRole("Staff");
        //     // Requires HOD, Management
        //     // policy.RequireClaim("UserGroup", ["HOD", "Management"]);
        //     policy.RequireAssertion(context =>
        //     {
        //         var userGroupClaims = context.User.FindFirst("UserGroup");
        //         if (userGroupClaims == null) return false;

        //         var allowedGroups = new[] { "staff", "hod", "management" };
        //         return allowedGroups.Any(group =>
        //         {
        //             return string.Equals(userGroupClaims.Value, group, StringComparison.OrdinalIgnoreCase);
        //         });
        //     });
        //     // TODO: Implement Custom Authorization Requirement
        //     // IAuthorizationRequirement
        //     // policy.AddRequirements(new MinimumLevelRequirement(2000));
        // })
        // .AddPolicy("CanSubmitCaseFeedbackPolicy", policy =>
        // {
        //     policy.RequireRole("Staff");
        //     // Requires HOD, Management
        //     policy.RequireClaim("UserGroup", ["Staff", "HOD", "Management"]);
        //     // TODO: Implement Custom Authorization Requirement
        //     // IAuthorizationRequirement
        //     // policy.AddRequirements(new MinimumLevelRequirement(2000));
        // });

        return services;
    }

    public static IServiceCollection AddAuthorizationHandlers(this IServiceCollection services)
    {
        services.AddSingleton<IAuthorizationHandler, AdminRoleRequirementHandler>();
        services.AddSingleton<IAuthorizationHandler, StaffRoleRequirementHandler>();

        services.AddSingleton<IAuthorizationHandler, AllowLockedUserHandler>();
        // builder.Services.AddSingleton<IAuthorizationHandler, MinimumLevelHandler>();
        services.AddScoped<IAuthorizationHandler, AccessAdminFeaturesHandler>();
        services.AddScoped<IAuthorizationHandler, SubmitKeyKpiScoreHandler>();
        services.AddScoped<IAuthorizationHandler, SubmitFeedbackScoreHandler>();
        services.AddScoped<IAuthorizationHandler, SubmitKpiScoreHandler>();
        services.AddScoped<IAuthorizationHandler, GiveFeedbackHandler>();

        return services;
    }

}