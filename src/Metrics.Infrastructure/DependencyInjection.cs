using Metrics.Application.Interfaces.IRepositories;
using Metrics.Application.Interfaces.IServices;
using Metrics.Infrastructure.Repositories;
using Metrics.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Collections;

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
        ;


        return services;
    }
}