namespace FiscalGuard.Domain;

public sealed class Customer : Entity, IOrganizationScoped
{
    public Guid OrganizationId { get; set; }
    public required string LegalName { get; set; }
    public string? TradeName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? ResponsibleName { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
}
