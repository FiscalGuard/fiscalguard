using FiscalGuard.Application;
using FiscalGuard.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FiscalGuard.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/issues")]
public sealed class IssuesController(IIssueService issueService) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<FiscalIssueDto>> List(
        [FromQuery] IssueStatus? status,
        CancellationToken cancellationToken) =>
        issueService.ListAsync(status, cancellationToken);
}
