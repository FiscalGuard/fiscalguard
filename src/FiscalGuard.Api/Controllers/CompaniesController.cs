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

    [HttpPost]
    [Authorize(Roles = $"{nameof(UserRole.Owner)},{nameof(UserRole.Administrator)},{nameof(UserRole.Accountant)},{nameof(UserRole.Assistant)}")]
    public Task<CompanySummary> Create(CompanyRequest request, CancellationToken cancellationToken) =>
        companyService.CreateAsync(request, cancellationToken);

    [HttpPost("{id:guid}/consultations")]
    [Authorize(Roles = $"{nameof(UserRole.Owner)},{nameof(UserRole.Administrator)},{nameof(UserRole.Accountant)}")]
    public Task<FiscalConsultationResult> Consult(Guid id, CancellationToken cancellationToken) =>
        fiscalMonitoringService.ConsultCompanyAsync(id, cancellationToken);
}
