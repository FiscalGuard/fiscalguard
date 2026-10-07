namespace FiscalGuard.Domain;

public sealed class RegulatorySourceRun : Entity, IOrganizationScoped
{
    public Guid OrganizationId { get; set; }
    public required string SourceKey { get; set; }
    public required string SourceName { get; set; }
    public string? RequestUrl { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? FinishedAt { get; set; }
    public bool Success { get; set; }
    public int DocumentsFound { get; set; }
    public int DocumentsAccepted { get; set; }
    public long DurationMs { get; set; }
    public string? Error { get; set; }
    public string? StatusMessage { get; set; }
}
