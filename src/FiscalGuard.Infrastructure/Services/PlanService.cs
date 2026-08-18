using FiscalGuard.Application;
using Microsoft.EntityFrameworkCore;

namespace FiscalGuard.Infrastructure.Services;

public sealed class PlanService(FiscalGuardDbContext db) : IPlanService
{
    public async Task<IReadOnlyList<PlanDto>> ListAsync(CancellationToken cancellationToken) =>
        await db.Plans
            .OrderBy(x => x.CnpjLimit)
            .Select(x => new PlanDto(
                x.Code,
                x.Name,
                x.CnpjLimit,
                x.UserLimit,
                x.AutomatedMonitoring,
                x.EmailAlerts,
                x.FullHistory))
            .ToListAsync(cancellationToken);
}
