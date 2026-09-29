using FiscalGuard.Application;
using FiscalGuard.Domain;
using Microsoft.EntityFrameworkCore;

namespace FiscalGuard.Infrastructure.Services;

public sealed class SubscriptionService(
    FiscalGuardDbContext db,
    ICurrentUserContext currentUser) : ISubscriptionService
{
    public async Task<SubscriptionDto> GetAsync(CancellationToken cancellationToken)
    {
        var subscription = await db.Subscriptions
            .Include(x => x.Plan)
            .SingleAsync(x => x.OrganizationId == currentUser.OrganizationId, cancellationToken);

        return await ToDtoAsync(subscription, cancellationToken);
    }

    public async Task<SubscriptionDto> ChangePlanAsync(ChangeSubscriptionPlanRequest request, CancellationToken cancellationToken)
    {
        var plan = await db.Plans.SingleAsync(x => x.Code == request.PlanCode, cancellationToken);
        var subscription = await db.Subscriptions
            .Include(x => x.Plan)
            .SingleAsync(x => x.OrganizationId == currentUser.OrganizationId, cancellationToken);

        await EnsureCurrentUsageFitsPlanAsync(plan, cancellationToken);

        subscription.PlanId = plan.Id;
        subscription.Plan = plan;
        subscription.Status = plan.Code switch
        {
            "free" => SubscriptionStatus.Free,
            "trial" => SubscriptionStatus.Trial,
            _ => SubscriptionStatus.Active
        };
        subscription.TrialEndsAt = plan.Code == "trial"
            ? DateTimeOffset.UtcNow.AddDays(plan.TrialDays)
            : subscription.TrialEndsAt;

        db.AuditLogs.Add(new AuditLog
        {
            OrganizationId = currentUser.OrganizationId,
            UserId = currentUser.UserId,
            Action = "subscription.plan.changed",
            EntityName = nameof(Subscription),
            EntityId = subscription.Id.ToString(),
            AfterValues = plan.Code
        });

        await db.SaveChangesAsync(cancellationToken);
        return await ToDtoAsync(subscription, cancellationToken);
    }

    private async Task EnsureCurrentUsageFitsPlanAsync(Plan plan, CancellationToken cancellationToken)
    {
        var activeCompanies = await db.Companies.CountAsync(
            x => x.OrganizationId == currentUser.OrganizationId && x.Status == CompanyStatus.Active,
            cancellationToken);
        var activeUsers = await db.OrganizationUsers.CountAsync(
            x => x.OrganizationId == currentUser.OrganizationId && x.IsActive,
            cancellationToken);

        if (activeCompanies > plan.CnpjLimit)
        {
            throw new InvalidOperationException("Uso atual de CNPJs excede o limite do plano selecionado.");
        }

        if (activeUsers > plan.UserLimit)
        {
            throw new InvalidOperationException("Uso atual de usuarios excede o limite do plano selecionado.");
        }
    }

    private async Task<SubscriptionDto> ToDtoAsync(Subscription subscription, CancellationToken cancellationToken)
    {
        var activeCompanies = await db.Companies.CountAsync(
            x => x.OrganizationId == currentUser.OrganizationId && x.Status == CompanyStatus.Active,
            cancellationToken);
        var activeUsers = await db.OrganizationUsers.CountAsync(
            x => x.OrganizationId == currentUser.OrganizationId && x.IsActive,
            cancellationToken);

        return new SubscriptionDto(
            subscription.Id,
            subscription.Plan!.Code,
            subscription.Plan.Name,
            subscription.Status,
            subscription.StartedAt,
            subscription.TrialEndsAt,
            subscription.EndsAt,
            subscription.Plan.CnpjLimit,
            subscription.Plan.UserLimit,
            activeCompanies,
            activeUsers,
            CalculateDaysRemaining(subscription.TrialEndsAt));
    }

    private static int CalculateDaysRemaining(DateTimeOffset? trialEnd) =>
        trialEnd is null ? 0 : Math.Max(0, (int)Math.Ceiling((trialEnd.Value - DateTimeOffset.UtcNow).TotalDays));
}
