using FiscalGuard.Domain;

namespace FiscalGuard.Application;

public sealed record FiscalIssueDto(
    Guid Id,
    Guid CompanyId,
    string CompanyName,
    string Title,
    IssueSeverity Severity,
    IssueStatus Status,
    DateTimeOffset DetectedAt);

public sealed record FiscalConsultationResult(
    string Provider,
    FiscalStatus Status,
    bool Success,
    string RawPayloadProtected,
    string? Error,
    IReadOnlyList<FiscalFinding> Findings);

public sealed record FiscalFinding(
    string ExternalKey,
    IssueType Type,
    string Title,
    string Description,
    IssueSeverity Severity);
