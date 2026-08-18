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
    int OpenIssues);
