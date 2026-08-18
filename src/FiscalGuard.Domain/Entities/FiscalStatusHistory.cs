namespace FiscalGuard.Domain;

public sealed class FiscalStatusHistory : Entity, IOrganizationScoped
{
    public Guid OrganizationId { get; set; }
    public Guid CompanyId { get; set; }
    public FiscalStatus PreviousStatus { get; set; }
    public FiscalStatus NewStatus { get; set; }
    public string? Reason { get; set; }
}
