namespace FiscalGuard.Application;

public sealed record DashboardSummary(
    int TotalCompanies,
    int RegularCompanies,
    int AttentionCompanies,
    int IrregularCompanies,
    int OpenIssues,
    int RecentAlerts,
    int Consultations,
    int HighRiskCompanies,
    int CriticalRiskCompanies,
    int NotConsultedCompanies,
    int QueryErrorCompanies,
    int CompaniesWithPublicData,
    int SimplesOptInCompanies,
    int SimplesNotOptInCompanies,
    int RecommendationsGenerated,
    IReadOnlyList<DashboardRiskCompany> TopRiskCompanies,
    IReadOnlyList<DashboardValueIndicator> ValueIndicators,
    string PlanName,
    int CnpjLimit,
    int DaysRemaining);

public sealed record DashboardRiskCompany(
    Guid Id,
    string LegalName,
    string Cnpj,
    FiscalGuard.Domain.FiscalStatus FiscalStatus,
    int RiskScore,
    string RiskLevel,
    int OpenIssues);

public sealed record DashboardValueIndicator(
    string Title,
    string Value,
    string Description);
