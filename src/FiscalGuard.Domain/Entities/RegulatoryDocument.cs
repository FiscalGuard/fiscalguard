namespace FiscalGuard.Domain;

public sealed class RegulatoryDocument : Entity, IOrganizationScoped
{
    public Guid OrganizationId { get; set; }
    public required string ExternalId { get; set; }
    public required string SourceKey { get; set; }
    public required string SourceName { get; set; }
    public required string SourceType { get; set; }
    public required string Title { get; set; }
    public required string Summary { get; set; }
    public required string SourceUrl { get; set; }
    public required string Theme { get; set; }
    public required string ImpactLevel { get; set; }
    public required string ImpactReason { get; set; }
    public required string BusinessImpact { get; set; }
    public required string SuggestedAction { get; set; }
    public string? RawText { get; set; }
    public string? SearchText { get; set; }
    public DateTimeOffset? PresentedAt { get; set; }
    public DateTimeOffset CapturedAt { get; set; }
    public int RelevanceScore { get; set; }
}
