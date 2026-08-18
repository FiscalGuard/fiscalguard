using FiscalGuard.Application;
using FiscalGuard.Infrastructure.Jobs;
using FiscalGuard.Infrastructure.Notifications;
using FiscalGuard.Infrastructure.Providers;
using FiscalGuard.Infrastructure.Seed;
using FiscalGuard.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace FiscalGuard.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddFiscalGuardInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<FiscalGuardDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<PasswordHasher>();
        services.AddScoped<TokenFactory>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICompanyService, CompanyService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IFiscalMonitoringService, FiscalMonitoringService>();
        services.AddScoped<IIssueService, IssueService>();
        services.AddScoped<IAlertService, AlertService>();
        services.AddScoped<IPlanService, PlanService>();
        services.AddScoped<IPlanLimitService, PlanLimitService>();
        services.AddScoped<IFiscalDataProvider, MockFiscalDataProvider>();
        services.AddScoped<IEmailService, DevelopmentEmailService>();
        services.AddScoped<DevelopmentSeeder>();

        var intervalMinutes = int.TryParse(configuration["Monitoring:IntervalMinutes"], out var configuredInterval)
            ? configuredInterval
            : 60;

        services.AddQuartz(q =>
        {
            var jobKey = new JobKey(nameof(FiscalMonitoringJob));
            q.AddJob<FiscalMonitoringJob>(opts => opts.WithIdentity(jobKey));
            q.AddTrigger(opts => opts
                .ForJob(jobKey)
                .WithIdentity($"{nameof(FiscalMonitoringJob)}-trigger")
                .WithSimpleSchedule(x => x.WithIntervalInMinutes(intervalMinutes).RepeatForever()));
        });
        services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

        return services;
    }
}
