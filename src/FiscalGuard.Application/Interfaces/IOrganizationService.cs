namespace FiscalGuard.Application;

public interface IOrganizationService
{
    Task<OrganizationDto> GetAsync(CancellationToken cancellationToken);
    Task<OrganizationDto> UpdateAsync(UpdateOrganizationRequest request, CancellationToken cancellationToken);
}
