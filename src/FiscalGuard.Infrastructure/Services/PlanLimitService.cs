using FiscalGuard.Application;
using FiscalGuard.Domain;
using Microsoft.EntityFrameworkCore;

namespace FiscalGuard.Infrastructure.Services;

public sealed class PlanLimitService(FiscalGuardDbContext db) : IPlanLimitService
{
    public async Task EnsureCanAddCompanyAsync(Guid organizationId, CancellationToken cancellationToken)
    {
        var subscription = await db.Subscriptions
            .Include(x => x.Plan)
            .SingleAsync(x => x.OrganizationId == organizationId, cancellationToken);

        if (subscription.Status == SubscriptionStatus.Blocked || subscription.TrialEndsAt < DateTimeOffset.UtcNow)
        {
            throw new InvalidOperationException("Assinatura bloqueada ou trial encerrado.");
        }

        var activeCompanies = await db.Companies.CountAsync(
            x => x.OrganizationId == organizationId && x.Status == CompanyStatus.Active,
            cancellationToken);

        if (activeCompanies >= subscription.Plan!.CnpjLimit)
        {
            throw new InvalidOperationException("Limite de CNPJs do plano atingido.");
        }
    }
}
