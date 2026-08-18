namespace FiscalGuard.Application;

public sealed record DashboardSummary(
    int TotalCompanies,
    int RegularCompanies,
    int AttentionCompanies,
    int IrregularCompanies,
    int OpenIssues,
    int RecentAlerts,
    int Consultations,
    string PlanName,
    int CnpjLimit,
    int DaysRemaining);
