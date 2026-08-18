namespace FiscalGuard.Domain;

public sealed class Notification : Entity, IOrganizationScoped
{
    public Guid OrganizationId { get; set; }
    public Guid? AlertId { get; set; }
    public Guid? FiscalIssueId { get; set; }
    public required string Recipient { get; set; }
    public NotificationChannel Channel { get; set; }
    public required string Content { get; set; }
    public NotificationStatus Status { get; set; } = NotificationStatus.Pending;
    public int Attempts { get; set; }
    public DateTimeOffset? SentAt { get; set; }
    public string? Error { get; set; }
}
