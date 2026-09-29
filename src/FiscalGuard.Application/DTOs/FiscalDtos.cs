using FiscalGuard.Domain;

namespace FiscalGuard.Application;

public sealed record FiscalIssueDto(
    Guid Id,
    Guid CompanyId,
    string CompanyName,
    IssueType Type,
    string Title,
    string Description,
    IssueSeverity Severity,
    IssueStatus Status,
    DateTimeOffset DetectedAt,
    string? Recommendation,
    string EvidenceType);

public sealed record FiscalIssueDetailsDto(
    Guid Id,
    Guid CompanyId,
    string CompanyName,
    IssueType Type,
    string Title,
    string Description,
    IssueSeverity Severity,
    IssueStatus Status,
    IssueOrigin Origin,
    string? ExternalIdentifier,
    DateTimeOffset DetectedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? ResolvedAt,
    string? Notes,
    string? Recommendation,
    string EvidenceType,
    string DeduplicationKey);

public sealed record UpdateIssueStatusRequest(
    IssueStatus Status,
    string? Notes);

public sealed record FiscalConsultationResult(
    string Provider,
    FiscalStatus Status,
    bool Success,
    string RawPayloadProtected,
    string? Error,
    IReadOnlyList<FiscalFinding> Findings,
    FiscalCompanySnapshot? CompanySnapshot = null);

public sealed record FiscalFinding(
    string ExternalKey,
    IssueType Type,
    string Title,
    string Description,
    IssueSeverity Severity,
    string? Recommendation = null,
    string EvidenceType = "Dado derivado");

public sealed record FiscalCompanySnapshot(
    string? LegalName,
    string? TradeName,
    string? RegistrationStatus,
    DateOnly? RegistrationStatusDate,
    string? MainCnaeCode,
    string? MainCnaeDescription,
    string? PublicAddress,
    string? PublicDataSource,
    DateTimeOffset PublicDataUpdatedAt,
    bool? IsSimplesOption,
    bool? IsMeiOption);
