using FiscalGuard.Domain;

namespace FiscalGuard.Application;

public interface ICompanyService
{
    Task<IReadOnlyList<CompanySummary>> ListAsync(string? search, FiscalStatus? status, CancellationToken cancellationToken);
    Task<CompanyDetails> GetAsync(Guid id, CancellationToken cancellationToken);
    Task<CompanySummary> CreateAsync(CompanyRequest request, CancellationToken cancellationToken);
    Task<CompanyImportResult> ImportCsvAsync(Stream stream, CancellationToken cancellationToken);
    Task<CompanyDetails> UpdateAsync(Guid id, CompanyRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task<CompanyDetails> UpdateStatusAsync(Guid id, UpdateCompanyStatusRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<FiscalConsultationDto>> ListConsultationsAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<FiscalIssueDto>> ListIssuesAsync(Guid id, CancellationToken cancellationToken);
}
