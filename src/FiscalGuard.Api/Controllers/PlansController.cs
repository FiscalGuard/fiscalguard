using FiscalGuard.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FiscalGuard.Api.Controllers;

[ApiController]
[Route("api/plans")]
public sealed class PlansController(IPlanService planService) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public Task<IReadOnlyList<PlanDto>> List(CancellationToken cancellationToken) =>
        planService.ListAsync(cancellationToken);
}
