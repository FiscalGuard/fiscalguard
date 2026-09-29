using FiscalGuard.Domain;

namespace FiscalGuard.Application;

public sealed record CompanyRequest(
    string LegalName,
    string? TradeName,
    string Cnpj,
    string? StateRegistration,
    TaxRegime TaxRegime,
    string? Email,
    string? Phone,
    string? ResponsibleName,
    string? Notes,
    string? Tags,
    MonitoringFrequency MonitoringFrequency);

public sealed record CompanySummary(
    Guid Id,
    string LegalName,
    string? TradeName,
    string Cnpj,
    FiscalStatus FiscalStatus,
    CompanyStatus Status,
    DateTimeOffset? LastConsultedAt,
    int OpenIssues,
    int RiskScore,
    string RiskLevel);

public sealed record CompanyDetails(
    Guid Id,
    Guid CustomerId,
    string LegalName,
    string? TradeName,
    string Cnpj,
    string? StateRegistration,
    TaxRegime TaxRegime,
    string? Email,
    string? Phone,
    string? ResponsibleName,
    string? Notes,
    string? Tags,
    MonitoringFrequency MonitoringFrequency,
    FiscalStatus FiscalStatus,
    CompanyStatus Status,
    string? RegistrationStatus,
    DateOnly? RegistrationStatusDate,
    string? MainCnaeCode,
    string? MainCnaeDescription,
    string? PublicAddress,
    string? PublicDataSource,
    DateTimeOffset? PublicDataUpdatedAt,
    bool? IsSimplesOption,
    bool? IsMeiOption,
    int RiskScore,
    string RiskLevel,
    string? RiskSummary,
    DateTimeOffset? LastConsultedAt,
    DateTimeOffset? NextConsultationAt,
    int OpenIssues);

public sealed record UpdateCompanyStatusRequest(CompanyStatus Status);

public sealed record FiscalConsultationDto(
    Guid Id,
    Guid CompanyId,
    string Provider,
    bool Success,
    FiscalStatus NormalizedStatus,
    string? Error,
    long DurationMs,
    DateTimeOffset CreatedAt);

public sealed record CompanyImportResult(
    int TotalRows,
    int Imported,
    int Duplicated,
    int Invalid,
    IReadOnlyList<CompanyImportRowResult> Rows);

public sealed record CompanyImportRowResult(
    int RowNumber,
    string? Cnpj,
    string Status,
    string Message,
    Guid? CompanyId);
