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
    string Reason);

public sealed record RegulatoryRadarSummary(
    DateTimeOffset GeneratedAt,
    string Source,
    bool FromCache,
    DateTimeOffset? CachedUntil,
    IReadOnlyList<string> MonitoredThemes,
    IReadOnlyList<RegulatoryAlertDto> Alerts);
