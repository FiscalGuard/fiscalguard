using FiscalGuard.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;

namespace FiscalGuard.Infrastructure.Jobs;

[DisallowConcurrentExecution]
public sealed class FiscalMonitoringJob(FiscalGuardDbContext db, ILogger<FiscalMonitoringJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var startedAt = DateTimeOffset.UtcNow;
        var dueCompanies = await db.Companies
            .Where(x => x.Status == CompanyStatus.Active && x.NextConsultationAt <= startedAt)
            .OrderBy(x => x.NextConsultationAt)
            .Take(50)
            .ToListAsync(context.CancellationToken);

        var processedByOrganization = dueCompanies.GroupBy(x => x.OrganizationId).ToDictionary(x => x.Key, x => x.Count());
        foreach (var item in processedByOrganization)
        {
            db.JobExecutions.Add(new JobExecution
            {
                OrganizationId = item.Key,
                JobName = nameof(FiscalMonitoringJob),
                StartedAt = startedAt,
                FinishedAt = DateTimeOffset.UtcNow,
                Success = true,
                ProcessedItems = item.Value
            });
        }

        logger.LogInformation("Fiscal monitoring selected {Count} due companies. Processing is delegated to manual service in MVP.", dueCompanies.Count);
        await db.SaveChangesAsync(context.CancellationToken);
    }
}
