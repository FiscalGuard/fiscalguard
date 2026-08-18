namespace FiscalGuard.Domain;

public sealed class SubscriptionUsage : Entity, IOrganizationScoped
{
    public Guid OrganizationId { get; set; }
    public int CompaniesCount { get; set; }
    public int ActiveUsersCount { get; set; }
    public int ConsultationsThisMonth { get; set; }
}
