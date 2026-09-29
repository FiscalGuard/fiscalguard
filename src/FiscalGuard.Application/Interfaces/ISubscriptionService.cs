namespace FiscalGuard.Application;

public interface ISubscriptionService
{
    Task<SubscriptionDto> GetAsync(CancellationToken cancellationToken);
    Task<SubscriptionDto> ChangePlanAsync(ChangeSubscriptionPlanRequest request, CancellationToken cancellationToken);
}
