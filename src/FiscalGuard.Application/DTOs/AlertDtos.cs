using FiscalGuard.Domain;

namespace FiscalGuard.Application;

public sealed record AlertDto(
    Guid Id,
    string Title,
    string Message,
    AlertType Type,
    IssueSeverity Severity,
    bool IsRead,
    DateTimeOffset CreatedAt);
