namespace FiscalGuard.Application;

public sealed record PlanDto(
    string Code,
    string Name,
    int CnpjLimit,
    int UserLimit,
    bool AutomatedMonitoring,
    bool EmailAlerts,
    bool FullHistory);
