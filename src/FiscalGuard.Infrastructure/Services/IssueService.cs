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
                x.Type,
                x.Title,
                x.Description,
                x.Severity,
                x.Status,
                x.DetectedAt,
                x.Recommendation,
                x.EvidenceType))
            .ToListAsync(cancellationToken);
    }

    public async Task<FiscalIssueDetailsDto> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var issue = await GetIssueAsync(id, cancellationToken);
        return ToDetailsDto(issue);
    }

    public async Task<FiscalIssueDetailsDto> UpdateStatusAsync(Guid id, UpdateIssueStatusRequest request, CancellationToken cancellationToken)
    {
        var issue = await GetIssueAsync(id, cancellationToken);
        var previousStatus = issue.Status;
        issue.Status = request.Status;
        issue.Notes = request.Notes;
        issue.ResolvedAt = request.Status == IssueStatus.Resolved ? DateTimeOffset.UtcNow : issue.ResolvedAt;

        db.FiscalIssueHistory.Add(new FiscalIssueHistory
        {
            OrganizationId = currentUser.OrganizationId,
            FiscalIssueId = issue.Id,
            PreviousStatus = previousStatus,
            NewStatus = request.Status,
            Notes = request.Notes
        });
        db.AuditLogs.Add(new AuditLog
        {
            OrganizationId = currentUser.OrganizationId,
            UserId = currentUser.UserId,
            Action = "issue.status.updated",
            EntityName = nameof(FiscalIssue),
            EntityId = issue.Id.ToString(),
            BeforeValues = previousStatus.ToString(),
            AfterValues = request.Status.ToString()
        });

        await db.SaveChangesAsync(cancellationToken);
        return ToDetailsDto(issue);
    }

    private async Task<FiscalIssue> GetIssueAsync(Guid id, CancellationToken cancellationToken) =>
        await db.FiscalIssues
            .Include(x => x.Company)
            .SingleAsync(x => x.Id == id && x.OrganizationId == currentUser.OrganizationId, cancellationToken);

    private static FiscalIssueDetailsDto ToDetailsDto(FiscalIssue issue) =>
        new(
            issue.Id,
            issue.CompanyId,
            issue.Company!.LegalName,
            issue.Type,
            issue.Title,
            issue.Description,
            issue.Severity,
            issue.Status,
            issue.Origin,
            issue.ExternalIdentifier,
            issue.DetectedAt,
            issue.UpdatedAt,
            issue.ResolvedAt,
            issue.Notes,
            issue.Recommendation,
            issue.EvidenceType,
            issue.DeduplicationKey);
}
