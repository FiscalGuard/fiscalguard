namespace FiscalGuard.Application;

public sealed record RegulatoryAlertDto(
    string Id,
    string Title,
    string Summary,
    string Source,
    string SourceUrl,
    string SourceType,
    string Theme,
    string ImpactLevel,
    string ImpactReason,
    string BusinessImpact,
    string SuggestedAction,
    DateTimeOffset? PresentedAt,
    int AffectedCompaniesCount,
    IReadOnlyList<RegulatoryAffectedCompanyDto> AffectedCompanies);

public sealed record RegulatoryAffectedCompanyDto(
    Guid Id,
    string LegalName,
    string Cnpj,
    string Reason,
    int Score,
    string MatchedTerms);

public sealed record RegulatorySourceDiagnosticDto(
    string SourceKey,
    string SourceName,
    bool Success,
    int DocumentsFound,
    int DocumentsAccepted,
    long DurationMs,
    string? Error,
    string? StatusMessage,
    DateTimeOffset StartedAt,
    DateTimeOffset? FinishedAt);

public sealed record RegulatoryRadarSummary(
    DateTimeOffset GeneratedAt,
    string Source,
    bool FromCache,
    DateTimeOffset? CachedUntil,
    IReadOnlyList<string> MonitoredThemes,
    IReadOnlyList<RegulatorySourceDiagnosticDto> Diagnostics,
    int DocumentsStored,
    int MatchesStored,
    string MatchingModel,
    IReadOnlyList<RegulatoryAlertDto> Alerts);
