namespace FiscalGuard.Domain;

public sealed class Subscription : Entity, IOrganizationScoped
{
    public Guid OrganizationId { get; set; }
    public Guid PlanId { get; set; }
    public Plan? Plan { get; set; }
    public SubscriptionStatus Status { get; set; }
    public DateTimeOffset StartedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? TrialEndsAt { get; set; }
    public DateTimeOffset? EndsAt { get; set; }
}
