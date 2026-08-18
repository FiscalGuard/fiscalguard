namespace FiscalGuard.Domain;

public interface IOrganizationScoped
{
    Guid OrganizationId { get; set; }
}
