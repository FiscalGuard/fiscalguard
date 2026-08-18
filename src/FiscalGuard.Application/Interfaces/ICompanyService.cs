using FiscalGuard.Domain;

namespace FiscalGuard.Application;

public interface ICompanyService
{
    Task<IReadOnlyList<CompanySummary>> ListAsync(string? search, FiscalStatus? status, CancellationToken cancellationToken);
    Task<CompanySummary> CreateAsync(CompanyRequest request, CancellationToken cancellationToken);
}
