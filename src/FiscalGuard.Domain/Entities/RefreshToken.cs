namespace FiscalGuard.Domain;

public sealed class RefreshToken : Entity
{
    public Guid UserId { get; set; }
    public Guid OrganizationId { get; set; }
    public required string TokenHash { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
}
