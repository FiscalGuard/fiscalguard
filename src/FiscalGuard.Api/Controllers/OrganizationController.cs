using FiscalGuard.Application;
using FiscalGuard.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FiscalGuard.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/organization")]
public sealed class OrganizationController(IOrganizationService organizationService) : ControllerBase
{
    [HttpGet]
    public Task<OrganizationDto> Get(CancellationToken cancellationToken) =>
        organizationService.GetAsync(cancellationToken);

    [HttpPut]
    [Authorize(Roles = $"{nameof(UserRole.Owner)},{nameof(UserRole.Administrator)}")]
    public Task<OrganizationDto> Update(UpdateOrganizationRequest request, CancellationToken cancellationToken) =>
        organizationService.UpdateAsync(request, cancellationToken);
}
