namespace FiscalGuard.Application;

public sealed record OrganizationDto(
    Guid Id,
    string Name,
    string? Document,
    string? Phone,
    bool IsActive);

public sealed record UpdateOrganizationRequest(
    string Name,
    string? Document,
    string? Phone);
