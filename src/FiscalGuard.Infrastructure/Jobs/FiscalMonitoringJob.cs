using FiscalGuard.Application;
using FiscalGuard.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;

namespace FiscalGuard.Infrastructure.Jobs;

[DisallowConcurrentExecution]
public sealed class FiscalMonitoringJob(
    FiscalGuardDbContext db,
    IFiscalMonitoringProcessor processor,
    ILogger<FiscalMonitoringJob> logger) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        var startedAt = DateTimeOffset.UtcNow;
        var dueCompanies = await db.Companies
            .AsNoTracking()
            .Where(x =>
                x.Status == CompanyStatus.Active &&
                x.FiscalStatus != FiscalStatus.Processing &&
                x.NextConsultationAt != null &&
                x.NextConsultationAt <= startedAt &&
                db.Subscriptions.Any(subscription =>
                    subscription.OrganizationId == x.OrganizationId &&
                    subscription.Plan != null &&
                    subscription.Plan.AutomatedMonitoring))
            .OrderBy(x => x.NextConsultationAt)
            .Select(x => new DueCompany(x.OrganizationId, x.Id))
            .Take(50)
            .ToListAsync(context.CancellationToken);

        var executions = new Dictionary<Guid, ExecutionSummary>();
        foreach (var company in dueCompanies)
        {
            var summary = executions.GetValueOrDefault(company.OrganizationId) ?? new ExecutionSummary();
            executions[company.OrganizationId] = summary;

            try
            {
                await processor.ProcessAsync(company.OrganizationId, company.CompanyId, null, context.CancellationToken);
                summary.ProcessedItems++;
            }
            catch (Exception ex)
            {
                summary.Errors.Add(ex.Message);
                logger.LogWarning(ex, "Scheduled fiscal monitoring failed for company {CompanyId}", company.CompanyId);
            }
        }

        foreach (var execution in executions)
        {
            db.JobExecutions.Add(new JobExecution
            {
                OrganizationId = execution.Key,
                JobName = nameof(FiscalMonitoringJob),
                StartedAt = startedAt,
                FinishedAt = DateTimeOffset.UtcNow,
                Success = execution.Value.Errors.Count == 0,
                ProcessedItems = execution.Value.ProcessedItems,
                Error = execution.Value.Errors.Count == 0
                    ? null
                    : string.Join(" | ", execution.Value.Errors.Take(5))
            });
        }

        logger.LogInformation("Fiscal monitoring processed {Count} due companies.", dueCompanies.Count);
        await db.SaveChangesAsync(context.CancellationToken);
    }

    private sealed record DueCompany(Guid OrganizationId, Guid CompanyId);

    private sealed class ExecutionSummary
    {
        public int ProcessedItems { get; set; }
        public List<string> Errors { get; } = [];
    }
}
