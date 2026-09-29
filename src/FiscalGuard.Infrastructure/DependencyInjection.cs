using FiscalGuard.Application;
using FiscalGuard.Infrastructure.Jobs;
using FiscalGuard.Infrastructure.Notifications;
using FiscalGuard.Infrastructure.Providers;
using FiscalGuard.Infrastructure.Seed;
using FiscalGuard.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        services.AddScoped<IFiscalMonitoringProcessor, FiscalMonitoringProcessor>();
        services.AddScoped<IFiscalMonitoringService, FiscalMonitoringService>();
        services.AddScoped<IIssueService, IssueService>();
        services.AddScoped<IAlertService, AlertService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IPlanService, PlanService>();
        services.AddScoped<IPlanLimitService, PlanLimitService>();
        services.AddScoped<IOrganizationService, OrganizationService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ISubscriptionService, SubscriptionService>();
        services.AddScoped<IRegulatoryRadarService, RegulatoryRadarService>();
        var fiscalProvider = configuration["FiscalData:Provider"] ?? "BrasilApi";
        if (string.Equals(fiscalProvider, "Mock", StringComparison.OrdinalIgnoreCase))
        {
            services.AddScoped<IFiscalDataProvider, MockFiscalDataProvider>();
        }
        else
        {
            services.AddScoped<IFiscalDataProvider>(provider =>
            {
                var client = new HttpClient
                {
                    BaseAddress = new Uri(configuration["FiscalData:BrasilApiBaseUrl"] ?? "https://brasilapi.com.br/api/"),
                    Timeout = TimeSpan.FromSeconds(12)
                };
                client.DefaultRequestHeaders.UserAgent.ParseAdd("FiscalGuardTech-MVP/1.0");
                return new BrasilApiFiscalDataProvider(
                    client,
                    provider.GetRequiredService<ILogger<BrasilApiFiscalDataProvider>>());
            });
        }
        if (string.IsNullOrWhiteSpace(configuration["Smtp:Host"]))
        {
            services.AddScoped<IEmailService, DevelopmentEmailService>();
        }
        else
        {
            services.AddScoped<IEmailService, SmtpEmailService>();
        }
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
