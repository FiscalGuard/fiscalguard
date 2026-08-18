namespace FiscalGuard.Application;

public interface IPlanService
{
    Task<IReadOnlyList<PlanDto>> ListAsync(CancellationToken cancellationToken);
}
