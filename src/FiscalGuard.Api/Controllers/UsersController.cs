using FiscalGuard.Application;
using FiscalGuard.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FiscalGuard.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/users")]
public sealed class UsersController(IUserService userService) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<OrganizationUserDto>> List(CancellationToken cancellationToken) =>
        userService.ListAsync(cancellationToken);

    [HttpGet("invitations")]
    public Task<IReadOnlyList<UserInvitationDto>> ListInvitations(CancellationToken cancellationToken) =>
        userService.ListInvitationsAsync(cancellationToken);

    [HttpPost("invitations")]
    [Authorize(Roles = $"{nameof(UserRole.Owner)},{nameof(UserRole.Administrator)}")]
    public Task<UserInvitationDto> Invite(InviteUserRequest request, CancellationToken cancellationToken) =>
        userService.InviteAsync(request, cancellationToken);

    [HttpPatch("{id:guid}/role")]
    [Authorize(Roles = $"{nameof(UserRole.Owner)},{nameof(UserRole.Administrator)}")]
    public Task<OrganizationUserDto> UpdateRole(Guid id, UpdateUserRoleRequest request, CancellationToken cancellationToken) =>
        userService.UpdateRoleAsync(id, request, cancellationToken);

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = $"{nameof(UserRole.Owner)},{nameof(UserRole.Administrator)}")]
    public Task<OrganizationUserDto> UpdateStatus(Guid id, UpdateUserStatusRequest request, CancellationToken cancellationToken) =>
        userService.UpdateStatusAsync(id, request, cancellationToken);
}
