namespace FiscalGuard.Domain;

public sealed class JobExecution : Entity, IOrganizationScoped
{
    public Guid OrganizationId { get; set; }
    public required string JobName { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? FinishedAt { get; set; }
    public bool Success { get; set; }
    public int ProcessedItems { get; set; }
    public string? Error { get; set; }
}
