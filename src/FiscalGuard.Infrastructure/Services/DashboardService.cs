using FiscalGuard.Application;
using FiscalGuard.Domain;
using Microsoft.EntityFrameworkCore;

namespace FiscalGuard.Infrastructure.Services;

public sealed class DashboardService(FiscalGuardDbContext db, ICurrentUserContext currentUser) : IDashboardService
{
    public async Task<DashboardSummary> GetAsync(CancellationToken cancellationToken)
    {
        var organizationId = currentUser.OrganizationId;
        var subscription = await db.Subscriptions
            .Include(x => x.Plan)
            .SingleAsync(x => x.OrganizationId == organizationId, cancellationToken);

        return new DashboardSummary(
            await CountCompaniesAsync(organizationId, null, cancellationToken),
            await CountCompaniesAsync(organizationId, FiscalStatus.Regular, cancellationToken),
            await CountCompaniesAsync(organizationId, FiscalStatus.Attention, cancellationToken),
            await CountCompaniesAsync(organizationId, FiscalStatus.Irregular, cancellationToken),
            await db.FiscalIssues.CountAsync(x => x.OrganizationId == organizationId && x.Status == IssueStatus.Open, cancellationToken),
            await db.Alerts.CountAsync(x => x.OrganizationId == organizationId && !x.IsRead, cancellationToken),
            await db.FiscalConsultations.CountAsync(x => x.OrganizationId == organizationId, cancellationToken),
            subscription.Plan!.Name,
            subscription.Plan.CnpjLimit,
            CalculateDaysRemaining(subscription.TrialEndsAt));
    }

    private Task<int> CountCompaniesAsync(Guid organizationId, FiscalStatus? fiscalStatus, CancellationToken cancellationToken)
    {
        var query = db.Companies.Where(x => x.OrganizationId == organizationId && x.Status == CompanyStatus.Active);
        if (fiscalStatus.HasValue)
        {
            query = query.Where(x => x.FiscalStatus == fiscalStatus.Value);
        }

        return query.CountAsync(cancellationToken);
    }

    private static int CalculateDaysRemaining(DateTimeOffset? trialEnd) =>
        trialEnd is null ? 0 : Math.Max(0, (int)Math.Ceiling((trialEnd.Value - DateTimeOffset.UtcNow).TotalDays));
}
