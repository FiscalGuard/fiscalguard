namespace FiscalGuard.Domain;

public sealed class Company : Entity, IOrganizationScoped
{
    public Guid OrganizationId { get; set; }
    public Guid CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public required string LegalName { get; set; }
    public string? TradeName { get; set; }
    public required string Cnpj { get; set; }
    public string? StateRegistration { get; set; }
    public TaxRegime TaxRegime { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? ResponsibleName { get; set; }
    public string? Notes { get; set; }
    public string? Tags { get; set; }
    public MonitoringFrequency MonitoringFrequency { get; set; } = MonitoringFrequency.Weekly;
    public CompanyStatus Status { get; set; } = CompanyStatus.Active;
    public FiscalStatus FiscalStatus { get; set; } = FiscalStatus.NotConsulted;
    public string? RegistrationStatus { get; set; }
    public DateOnly? RegistrationStatusDate { get; set; }
    public string? MainCnaeCode { get; set; }
    public string? MainCnaeDescription { get; set; }
    public string? PublicAddress { get; set; }
    public string? PublicDataSource { get; set; }
    public DateTimeOffset? PublicDataUpdatedAt { get; set; }
    public bool? IsSimplesOption { get; set; }
    public bool? IsMeiOption { get; set; }
    public int RiskScore { get; set; }
    public string RiskLevel { get; set; } = "Nao avaliado";
    public string? RiskSummary { get; set; }
    public DateTimeOffset? LastConsultedAt { get; set; }
    public DateTimeOffset? NextConsultationAt { get; set; }
}
