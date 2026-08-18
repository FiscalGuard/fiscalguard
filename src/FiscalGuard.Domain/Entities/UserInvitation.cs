namespace FiscalGuard.Domain;

public sealed class UserInvitation : Entity, IOrganizationScoped
{
    public Guid OrganizationId { get; set; }
    public required string Email { get; set; }
    public UserRole Role { get; set; }
    public required string TokenHash { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? AcceptedAt { get; set; }
}
