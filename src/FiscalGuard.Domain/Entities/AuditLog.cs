namespace FiscalGuard.Domain;

public sealed class AuditLog : Entity, IOrganizationScoped
{
    public Guid OrganizationId { get; set; }
    public Guid? UserId { get; set; }
    public required string Action { get; set; }
    public required string EntityName { get; set; }
    public string? EntityId { get; set; }
    public string? BeforeValues { get; set; }
    public string? AfterValues { get; set; }
}
