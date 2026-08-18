namespace FiscalGuard.Application;

public sealed record RegisterOrganizationRequest(
    string ResponsibleName,
    string OrganizationName,
    string Email,
    string Password,
    string? Phone,
    string? OfficeDocument,
    bool AcceptedTerms);

public sealed record LoginRequest(string Email, string Password);

public sealed record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAt,
    Guid OrganizationId,
    string Name,
    string Role);

public sealed record RefreshRequest(string RefreshToken);
