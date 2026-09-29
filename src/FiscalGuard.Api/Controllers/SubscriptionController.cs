using FiscalGuard.Application;
using FiscalGuard.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FiscalGuard.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/subscription")]
public sealed class SubscriptionController(ISubscriptionService subscriptionService) : ControllerBase
{
    [HttpGet]
    public Task<SubscriptionDto> Get(CancellationToken cancellationToken) =>
        subscriptionService.GetAsync(cancellationToken);

    [HttpPatch("plan")]
    [Authorize(Roles = $"{nameof(UserRole.Owner)},{nameof(UserRole.Administrator)}")]
    public Task<SubscriptionDto> ChangePlan(ChangeSubscriptionPlanRequest request, CancellationToken cancellationToken) =>
        subscriptionService.ChangePlanAsync(request, cancellationToken);
}
