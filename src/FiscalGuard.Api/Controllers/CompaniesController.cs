using FiscalGuard.Application;
using FiscalGuard.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FiscalGuard.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/companies")]
public sealed class CompaniesController(
    ICompanyService companyService,
    IFiscalMonitoringService fiscalMonitoringService) : ControllerBase
{
    [HttpGet]
    public Task<IReadOnlyList<CompanySummary>> List(
        [FromQuery] string? search,
        [FromQuery] FiscalStatus? status,
        CancellationToken cancellationToken) =>
        companyService.ListAsync(search, status, cancellationToken);

    [HttpGet("{id:guid}")]
    public Task<CompanyDetails> Get(Guid id, CancellationToken cancellationToken) =>
        companyService.GetAsync(id, cancellationToken);

    [HttpPost]
    [Authorize(Roles = $"{nameof(UserRole.Owner)},{nameof(UserRole.Administrator)},{nameof(UserRole.Accountant)},{nameof(UserRole.Assistant)}")]
    public Task<CompanySummary> Create(CompanyRequest request, CancellationToken cancellationToken) =>
        companyService.CreateAsync(request, cancellationToken);

    [HttpPost("import")]
    [Consumes("multipart/form-data")]
    [Authorize(Roles = $"{nameof(UserRole.Owner)},{nameof(UserRole.Administrator)},{nameof(UserRole.Accountant)},{nameof(UserRole.Assistant)}")]
    public async Task<CompanyImportResult> Import(IFormFile file, CancellationToken cancellationToken)
    {
        if (file.Length == 0)
        {
            throw new InvalidOperationException("Arquivo CSV vazio.");
        }

        await using var stream = file.OpenReadStream();
        return await companyService.ImportCsvAsync(stream, cancellationToken);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = $"{nameof(UserRole.Owner)},{nameof(UserRole.Administrator)},{nameof(UserRole.Accountant)},{nameof(UserRole.Assistant)}")]
    public Task<CompanyDetails> Update(Guid id, CompanyRequest request, CancellationToken cancellationToken) =>
        companyService.UpdateAsync(id, request, cancellationToken);

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = $"{nameof(UserRole.Owner)},{nameof(UserRole.Administrator)},{nameof(UserRole.Accountant)}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await companyService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = $"{nameof(UserRole.Owner)},{nameof(UserRole.Administrator)},{nameof(UserRole.Accountant)}")]
    public Task<CompanyDetails> UpdateStatus(Guid id, UpdateCompanyStatusRequest request, CancellationToken cancellationToken) =>
        companyService.UpdateStatusAsync(id, request, cancellationToken);

    [HttpPost("{id:guid}/consultations")]
    [Authorize(Roles = $"{nameof(UserRole.Owner)},{nameof(UserRole.Administrator)},{nameof(UserRole.Accountant)}")]
    public Task<FiscalConsultationResult> Consult(Guid id, CancellationToken cancellationToken) =>
        fiscalMonitoringService.ConsultCompanyAsync(id, cancellationToken);

    [HttpGet("{id:guid}/consultations")]
    public Task<IReadOnlyList<FiscalConsultationDto>> ListConsultations(Guid id, CancellationToken cancellationToken) =>
        companyService.ListConsultationsAsync(id, cancellationToken);

    [HttpGet("{id:guid}/issues")]
    public Task<IReadOnlyList<FiscalIssueDto>> ListIssues(Guid id, CancellationToken cancellationToken) =>
        companyService.ListIssuesAsync(id, cancellationToken);
}
