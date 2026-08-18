namespace FiscalGuard.Domain;

public sealed class FiscalConsultation : Entity, IOrganizationScoped
{
    public Guid OrganizationId { get; set; }
    public Guid CompanyId { get; set; }
    public Company? Company { get; set; }
    public required string Provider { get; set; }
    public bool Success { get; set; }
    public FiscalStatus NormalizedStatus { get; set; }
    public required string RawPayloadProtected { get; set; }
    public string NormalizerVersion { get; set; } = "mock-v1";
    public string? Error { get; set; }
    public long DurationMs { get; set; }
}
