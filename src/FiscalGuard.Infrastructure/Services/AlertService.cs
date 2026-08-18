using FiscalGuard.Application;
using Microsoft.EntityFrameworkCore;

namespace FiscalGuard.Infrastructure.Services;

public sealed class AlertService(FiscalGuardDbContext db, ICurrentUserContext currentUser) : IAlertService
{
    public async Task<IReadOnlyList<AlertDto>> ListAsync(CancellationToken cancellationToken) =>
        await db.Alerts
            .Where(x => x.OrganizationId == currentUser.OrganizationId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(100)
            .Select(x => new AlertDto(x.Id, x.Title, x.Message, x.Type, x.Severity, x.IsRead, x.CreatedAt))
            .ToListAsync(cancellationToken);

    public async Task MarkReadAsync(Guid alertId, CancellationToken cancellationToken)
    {
        var alert = await db.Alerts.SingleAsync(
            x => x.Id == alertId && x.OrganizationId == currentUser.OrganizationId,
            cancellationToken);

        alert.IsRead = true;
        await db.SaveChangesAsync(cancellationToken);
    }
}
