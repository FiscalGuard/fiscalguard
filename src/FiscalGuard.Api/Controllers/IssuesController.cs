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

    [HttpGet("{id:guid}")]
    public Task<FiscalIssueDetailsDto> Get(Guid id, CancellationToken cancellationToken) =>
        issueService.GetAsync(id, cancellationToken);

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = $"{nameof(UserRole.Owner)},{nameof(UserRole.Administrator)},{nameof(UserRole.Accountant)},{nameof(UserRole.Assistant)}")]
    public Task<FiscalIssueDetailsDto> UpdateStatus(Guid id, UpdateIssueStatusRequest request, CancellationToken cancellationToken) =>
        issueService.UpdateStatusAsync(id, request, cancellationToken);
}
