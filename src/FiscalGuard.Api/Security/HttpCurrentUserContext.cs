using System.Security.Claims;
using FiscalGuard.Application;
using FiscalGuard.Domain;

namespace FiscalGuard.Api.Security;

public sealed class HttpCurrentUserContext(IHttpContextAccessor accessor) : ICurrentUserContext
{
    public Guid OrganizationId => Guid.Parse(
        accessor.HttpContext?.User.FindFirstValue("organization_id")
        ?? throw new UnauthorizedAccessException());

    public Guid UserId => Guid.Parse(
        accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException());

    public UserRole Role => Enum.Parse<UserRole>(
        accessor.HttpContext?.User.FindFirstValue(ClaimTypes.Role)
        ?? nameof(UserRole.ReadOnly));
}
