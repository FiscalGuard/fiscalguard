namespace FiscalGuard.Domain;

public sealed class OrganizationUser : Entity, IOrganizationScoped
{
    public Guid OrganizationId { get; set; }
    public Organization? Organization { get; set; }
    public Guid UserId { get; set; }
    public AppUser? User { get; set; }
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;
}
