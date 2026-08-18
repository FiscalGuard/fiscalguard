namespace FiscalGuard.Domain;

public sealed class Alert : Entity, IOrganizationScoped
{
    public Guid OrganizationId { get; set; }
    public Guid? CompanyId { get; set; }
    public Guid? FiscalIssueId { get; set; }
    public AlertType Type { get; set; }
    public IssueSeverity Severity { get; set; }
    public required string Title { get; set; }
    public required string Message { get; set; }
    public bool IsRead { get; set; }
}
