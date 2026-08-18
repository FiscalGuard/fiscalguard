namespace FiscalGuard.Domain;

public sealed class FiscalIssue : Entity, IOrganizationScoped
{
    public Guid OrganizationId { get; set; }
    public Guid CompanyId { get; set; }
    public Company? Company { get; set; }
    public IssueType Type { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public IssueSeverity Severity { get; set; }
    public IssueStatus Status { get; set; } = IssueStatus.Open;
    public IssueOrigin Origin { get; set; } = IssueOrigin.FiscalConsultation;
    public string? ExternalIdentifier { get; set; }
    public DateTimeOffset DetectedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ResolvedAt { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public string? Notes { get; set; }
    public required string DeduplicationKey { get; set; }
}
