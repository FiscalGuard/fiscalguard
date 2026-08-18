using FiscalGuard.Application;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FiscalGuard.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/alerts")]
public sealed class AlertsController(IAlertService alertService) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<AlertDto>> List(CancellationToken cancellationToken) =>
        alertService.ListAsync(cancellationToken);

    [HttpPatch("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken cancellationToken)
    {
        await alertService.MarkReadAsync(id, cancellationToken);
        return NoContent();
    }
}
