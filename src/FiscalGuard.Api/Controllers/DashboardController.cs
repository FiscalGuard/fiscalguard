using FiscalGuard.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FiscalGuard.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/dashboard")]
public sealed class DashboardController(IDashboardService dashboardService) : ControllerBase
{
    [HttpGet]
    public Task<DashboardSummary> Get(CancellationToken cancellationToken) =>
        dashboardService.GetAsync(cancellationToken);
}
