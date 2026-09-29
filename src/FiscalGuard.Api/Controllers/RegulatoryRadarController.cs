using FiscalGuard.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FiscalGuard.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/regulatory-radar")]
public sealed class RegulatoryRadarController(IRegulatoryRadarService regulatoryRadarService) : ControllerBase
{
    [HttpGet]
    public Task<RegulatoryRadarSummary> Get([FromQuery] bool forceRefresh, CancellationToken cancellationToken) =>
        regulatoryRadarService.GetLatestAsync(forceRefresh, cancellationToken);
}
