using FiscalGuard.Application;
using FiscalGuard.Domain;
using Microsoft.EntityFrameworkCore;

namespace FiscalGuard.Infrastructure.Services;

public sealed class CompanyService(
    FiscalGuardDbContext db,
    ICurrentUserContext currentUser,
    IPlanLimitService planLimits) : ICompanyService
{
    public async Task<IReadOnlyList<CompanySummary>> ListAsync(
        string? search,
        FiscalStatus? status,
        CancellationToken cancellationToken)
    {
        var query = db.Companies.Where(x => x.OrganizationId == currentUser.OrganizationId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalized = search.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.Cnpj.Contains(normalized)
                || x.LegalName.ToLower().Contains(normalized)
                || (x.TradeName != null && x.TradeName.ToLower().Contains(normalized)));
        }

        if (status.HasValue)
        {
            query = query.Where(x => x.FiscalStatus == status.Value);
        }

        return await query
            .OrderBy(x => x.LegalName)
            .Select(x => new CompanySummary(
                x.Id,
                x.LegalName,
                x.TradeName,
                x.Cnpj,
                x.FiscalStatus,
                x.Status,
                x.LastConsultedAt,
                db.FiscalIssues.Count(i => i.CompanyId == x.Id && i.Status == IssueStatus.Open)))
            .Take(100)
            .ToListAsync(cancellationToken);
    }

    public async Task<CompanySummary> CreateAsync(CompanyRequest request, CancellationToken cancellationToken)
    {
        var cnpj = Cnpj.Normalize(request.Cnpj);
        if (!Cnpj.IsValid(cnpj))
        {
            throw new InvalidOperationException("CNPJ invalido.");
        }

        await planLimits.EnsureCanAddCompanyAsync(currentUser.OrganizationId, cancellationToken);
        var duplicated = await db.Companies.AnyAsync(
            x => x.OrganizationId == currentUser.OrganizationId && x.Cnpj == cnpj,
            cancellationToken);

        if (duplicated)
        {
            throw new InvalidOperationException("CNPJ ja cadastrado nesta organizacao.");
        }

        var customer = new Customer
        {
            OrganizationId = currentUser.OrganizationId,
            LegalName = request.LegalName,
            TradeName = request.TradeName,
            Email = request.Email,
            Phone = request.Phone,
            ResponsibleName = request.ResponsibleName,
            Notes = request.Notes
        };
        var company = new Company
        {
            OrganizationId = currentUser.OrganizationId,
            Customer = customer,
            LegalName = request.LegalName,
            TradeName = request.TradeName,
            Cnpj = cnpj,
            StateRegistration = request.StateRegistration,
            TaxRegime = request.TaxRegime,
            Email = request.Email,
            Phone = request.Phone,
            ResponsibleName = request.ResponsibleName,
            Notes = request.Notes,
            Tags = request.Tags,
            MonitoringFrequency = request.MonitoringFrequency,
            NextConsultationAt = DateTimeOffset.UtcNow
        };

        db.Customers.Add(customer);
        db.Companies.Add(company);
        db.AuditLogs.Add(new AuditLog
        {
            OrganizationId = currentUser.OrganizationId,
            UserId = currentUser.UserId,
            Action = "company.created",
            EntityName = nameof(Company),
            EntityId = company.Id.ToString()
        });

        await db.SaveChangesAsync(cancellationToken);
        return new CompanySummary(company.Id, company.LegalName, company.TradeName, company.Cnpj, company.FiscalStatus, company.Status, company.LastConsultedAt, 0);
    }
}
