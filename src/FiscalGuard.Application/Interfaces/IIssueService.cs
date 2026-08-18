using FiscalGuard.Domain;

namespace FiscalGuard.Application;

public interface IIssueService
{
    Task<IReadOnlyList<FiscalIssueDto>> ListAsync(IssueStatus? status, CancellationToken cancellationToken);
}
