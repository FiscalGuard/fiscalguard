using FiscalGuard.Domain;

namespace FiscalGuard.Application;

public sealed record OrganizationUserDto(
    Guid Id,
    Guid UserId,
    string Name,
    string Email,
    string? Phone,
    UserRole Role,
    bool IsActive,
    DateTimeOffset CreatedAt);

public sealed record UserInvitationDto(
    Guid Id,
    string Email,
    UserRole Role,
    DateTimeOffset ExpiresAt,
    DateTimeOffset? AcceptedAt);

public sealed record InviteUserRequest(
    string Email,
    UserRole Role);

public sealed record UpdateUserRoleRequest(UserRole Role);

public sealed record UpdateUserStatusRequest(bool IsActive);
