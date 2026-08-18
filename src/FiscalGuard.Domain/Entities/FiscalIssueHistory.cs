namespace FiscalGuard.Domain;

public sealed class FiscalIssueHistory : Entity, IOrganizationScoped
{
    public Guid OrganizationId { get; set; }
    public Guid FiscalIssueId { get; set; }
    public IssueStatus PreviousStatus { get; set; }
    public IssueStatus NewStatus { get; set; }
    public string? Notes { get; set; }
}
