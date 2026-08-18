namespace FiscalGuard.Domain;

public sealed class FiscalConsultationItem : Entity, IOrganizationScoped
{
    public Guid OrganizationId { get; set; }
    public Guid FiscalConsultationId { get; set; }
    public FiscalConsultation? FiscalConsultation { get; set; }
    public required string ExternalKey { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public IssueSeverity Severity { get; set; }
}
