using FiscalGuard.Domain;

namespace FiscalGuard.Application;

public interface IIssueService
{
    Task<IReadOnlyList<FiscalIssueDto>> ListAsync(IssueStatus? status, CancellationToken cancellationToken);
    Task<FiscalIssueDetailsDto> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<FiscalIssueDetailsDto> UpdateStatusAsync(Guid id, UpdateIssueStatusRequest request, CancellationToken cancellationToken);
}
