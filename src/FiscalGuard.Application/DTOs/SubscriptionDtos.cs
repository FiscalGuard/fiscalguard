using FiscalGuard.Domain;

namespace FiscalGuard.Application;

public sealed record SubscriptionDto(
    Guid Id,
    string PlanCode,
    string PlanName,
    SubscriptionStatus Status,
    DateTimeOffset StartedAt,
    DateTimeOffset? TrialEndsAt,
    DateTimeOffset? EndsAt,
    int CnpjLimit,
    int UserLimit,
    int ActiveCompanies,
    int ActiveUsers,
    int DaysRemaining);

public sealed record ChangeSubscriptionPlanRequest(string PlanCode);
