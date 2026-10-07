namespace FiscalGuard.Domain;

public sealed class RegulatoryMatch : Entity, IOrganizationScoped
{
    public Guid OrganizationId { get; set; }
    public Guid RegulatoryDocumentId { get; set; }
    public RegulatoryDocument? RegulatoryDocument { get; set; }
    public Guid CompanyId { get; set; }
    public Company? Company { get; set; }
    public required string Reason { get; set; }
    public required string MatchedTerms { get; set; }
    public int Score { get; set; }
}
