using FiscalGuard.Application;
using FiscalGuard.Domain;
using Microsoft.EntityFrameworkCore;

namespace FiscalGuard.Infrastructure.Services;

public sealed class IssueService(FiscalGuardDbContext db, ICurrentUserContext currentUser) : IIssueService
{
    public async Task<IReadOnlyList<FiscalIssueDto>> ListAsync(IssueStatus? status, CancellationToken cancellationToken)
    {
        var query = db.FiscalIssues
            .Include(x => x.Company)
            .Where(x => x.OrganizationId == currentUser.OrganizationId);

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        return await query
            .OrderByDescending(x => x.DetectedAt)
            .Take(100)
            .Select(x => new FiscalIssueDto(
                x.Id,
                x.CompanyId,
                x.Company!.LegalName,
                x.Title,
                x.Severity,
                x.Status,
                x.DetectedAt))
            .ToListAsync(cancellationToken);
    }
}
