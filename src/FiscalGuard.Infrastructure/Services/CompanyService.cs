using FiscalGuard.Application;
using FiscalGuard.Domain;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

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
                db.FiscalIssues.Count(i => i.CompanyId == x.Id && i.Status == IssueStatus.Open),
                x.RiskScore,
                x.RiskLevel))
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
        return ToSummary(company, 0);
    }

    public async Task<CompanyImportResult> ImportCsvAsync(Stream stream, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(stream);
        var content = await reader.ReadToEndAsync(cancellationToken);
        var rows = content.Replace("\r\n", "\n").Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var results = new List<CompanyImportRowResult>();
        var imported = 0;
        var duplicated = 0;
        var invalid = 0;

        if (rows.Length == 0)
        {
            return new CompanyImportResult(0, 0, 0, 0, []);
        }

        var startIndex = LooksLikeHeader(rows[0]) ? 1 : 0;
        for (var index = startIndex; index < rows.Length; index++)
        {
            var rowNumber = index + 1;
            var columns = SplitCsvRow(rows[index]);
            var cnpj = Cnpj.Normalize(GetColumn(columns, 0));
            if (!Cnpj.IsValid(cnpj))
            {
                invalid++;
                results.Add(new CompanyImportRowResult(rowNumber, cnpj, "invalido", "CNPJ invalido ou ausente.", null));
                continue;
            }

            var exists = await db.Companies.AnyAsync(
                x => x.OrganizationId == currentUser.OrganizationId && x.Cnpj == cnpj,
                cancellationToken);
            if (exists)
            {
                duplicated++;
                results.Add(new CompanyImportRowResult(rowNumber, cnpj, "duplicado", "CNPJ ja estava cadastrado nesta organizacao.", null));
                continue;
            }

            try
            {
                await planLimits.EnsureCanAddCompanyAsync(currentUser.OrganizationId, cancellationToken);
            }
            catch (InvalidOperationException ex)
            {
                invalid++;
                results.Add(new CompanyImportRowResult(rowNumber, cnpj, "bloqueado", ex.Message, null));
                continue;
            }

            var legalName = FirstNonEmpty(GetColumn(columns, 1), $"Empresa {cnpj}");
            var tradeName = EmptyToNull(GetColumn(columns, 2));
            var email = EmptyToNull(GetColumn(columns, 3));
            var responsible = EmptyToNull(GetColumn(columns, 4));
            var tags = EmptyToNull(GetColumn(columns, 5));

            var customer = new Customer
            {
                OrganizationId = currentUser.OrganizationId,
                LegalName = legalName,
                TradeName = tradeName,
                Email = email,
                ResponsibleName = responsible
            };
            var company = new Company
            {
                OrganizationId = currentUser.OrganizationId,
                Customer = customer,
                LegalName = legalName,
                TradeName = tradeName,
                Cnpj = cnpj,
                TaxRegime = TaxRegime.SimplesNacional,
                Email = email,
                ResponsibleName = responsible,
                Tags = tags,
                MonitoringFrequency = MonitoringFrequency.Weekly,
                NextConsultationAt = DateTimeOffset.UtcNow,
                RiskScore = 15,
                RiskLevel = "Nao avaliado",
                RiskSummary = "Empresa importada e aguardando primeira consulta fiscal."
            };

            db.Customers.Add(customer);
            db.Companies.Add(company);
            db.AuditLogs.Add(new AuditLog
            {
                OrganizationId = currentUser.OrganizationId,
                UserId = currentUser.UserId,
                Action = "company.imported",
                EntityName = nameof(Company),
                EntityId = company.Id.ToString()
            });

            await db.SaveChangesAsync(cancellationToken);
            imported++;
            results.Add(new CompanyImportRowResult(rowNumber, cnpj, "importado", "Empresa importada com sucesso.", company.Id));
        }

        return new CompanyImportResult(rows.Length - startIndex, imported, duplicated, invalid, results);
    }

    public async Task<CompanyDetails> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var company = await GetCompanyAsync(id, cancellationToken);
        return await ToDetailsAsync(company, cancellationToken);
    }

    public async Task<CompanyDetails> UpdateAsync(Guid id, CompanyRequest request, CancellationToken cancellationToken)
    {
        var company = await GetCompanyAsync(id, cancellationToken);
        var cnpj = Cnpj.Normalize(request.Cnpj);
        if (!Cnpj.IsValid(cnpj))
        {
            throw new InvalidOperationException("CNPJ invalido.");
        }

        var duplicated = await db.Companies.AnyAsync(
            x => x.OrganizationId == currentUser.OrganizationId && x.Id != id && x.Cnpj == cnpj,
            cancellationToken);
        if (duplicated)
        {
            throw new InvalidOperationException("CNPJ ja cadastrado nesta organizacao.");
        }

        company.LegalName = request.LegalName;
        company.TradeName = request.TradeName;
        company.Cnpj = cnpj;
        company.StateRegistration = request.StateRegistration;
        company.TaxRegime = request.TaxRegime;
        company.Email = request.Email;
        company.Phone = request.Phone;
        company.ResponsibleName = request.ResponsibleName;
        company.Notes = request.Notes;
        company.Tags = request.Tags;
        company.MonitoringFrequency = request.MonitoringFrequency;

        if (company.Customer is not null)
        {
            company.Customer.LegalName = request.LegalName;
            company.Customer.TradeName = request.TradeName;
            company.Customer.Email = request.Email;
            company.Customer.Phone = request.Phone;
            company.Customer.ResponsibleName = request.ResponsibleName;
            company.Customer.Notes = request.Notes;
        }

        db.AuditLogs.Add(new AuditLog
        {
            OrganizationId = currentUser.OrganizationId,
            UserId = currentUser.UserId,
            Action = "company.updated",
            EntityName = nameof(Company),
            EntityId = company.Id.ToString()
        });

        await db.SaveChangesAsync(cancellationToken);
        return await ToDetailsAsync(company, cancellationToken);
    }

    public async Task<CompanyDetails> UpdateStatusAsync(Guid id, UpdateCompanyStatusRequest request, CancellationToken cancellationToken)
    {
        var company = await GetCompanyAsync(id, cancellationToken);
        company.Status = request.Status;

        db.AuditLogs.Add(new AuditLog
        {
            OrganizationId = currentUser.OrganizationId,
            UserId = currentUser.UserId,
            Action = "company.status.updated",
            EntityName = nameof(Company),
            EntityId = company.Id.ToString(),
            AfterValues = request.Status.ToString()
        });

        await db.SaveChangesAsync(cancellationToken);
        return await ToDetailsAsync(company, cancellationToken);
    }

    public async Task<IReadOnlyList<FiscalConsultationDto>> ListConsultationsAsync(Guid id, CancellationToken cancellationToken)
    {
        _ = await GetCompanyAsync(id, cancellationToken);

        return await db.FiscalConsultations
            .Where(x => x.OrganizationId == currentUser.OrganizationId && x.CompanyId == id)
            .OrderByDescending(x => x.CreatedAt)
            .Take(100)
            .Select(x => new FiscalConsultationDto(
                x.Id,
                x.CompanyId,
                x.Provider,
                x.Success,
                x.NormalizedStatus,
                x.Error,
                x.DurationMs,
                x.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<FiscalIssueDto>> ListIssuesAsync(Guid id, CancellationToken cancellationToken)
    {
        _ = await GetCompanyAsync(id, cancellationToken);

        return await db.FiscalIssues
            .Where(x => x.OrganizationId == currentUser.OrganizationId && x.CompanyId == id)
            .OrderByDescending(x => x.DetectedAt)
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

    private async Task<Company> GetCompanyAsync(Guid id, CancellationToken cancellationToken) =>
        await db.Companies
            .Include(x => x.Customer)
            .SingleAsync(x => x.Id == id && x.OrganizationId == currentUser.OrganizationId, cancellationToken);

    private async Task<CompanyDetails> ToDetailsAsync(Company company, CancellationToken cancellationToken)
    {
        var openIssues = await db.FiscalIssues.CountAsync(
            x => x.OrganizationId == currentUser.OrganizationId
                && x.CompanyId == company.Id
                && x.Status == IssueStatus.Open,
            cancellationToken);

        return new CompanyDetails(
            company.Id,
            company.CustomerId,
            company.LegalName,
            company.TradeName,
            company.Cnpj,
            company.StateRegistration,
            company.TaxRegime,
            company.Email,
            company.Phone,
            company.ResponsibleName,
            company.Notes,
            company.Tags,
            company.MonitoringFrequency,
            company.FiscalStatus,
            company.Status,
            company.RegistrationStatus,
            company.RegistrationStatusDate,
            company.MainCnaeCode,
            company.MainCnaeDescription,
            company.PublicAddress,
            company.PublicDataSource,
            company.PublicDataUpdatedAt,
            company.IsSimplesOption,
            company.IsMeiOption,
            company.RiskScore,
            company.RiskLevel,
            company.RiskSummary,
            company.LastConsultedAt,
            company.NextConsultationAt,
            openIssues);
    }

    private static CompanySummary ToSummary(Company company, int openIssues) =>
        new(
            company.Id,
            company.LegalName,
            company.TradeName,
            company.Cnpj,
            company.FiscalStatus,
            company.Status,
            company.LastConsultedAt,
            openIssues,
            company.RiskScore,
            company.RiskLevel);

    private static bool LooksLikeHeader(string row)
    {
        var first = GetColumn(SplitCsvRow(row), 0);
        return first.Contains("cnpj", StringComparison.OrdinalIgnoreCase);
    }

    private static IReadOnlyList<string> SplitCsvRow(string row)
    {
        var separator = row.Count(x => x == ';') > row.Count(x => x == ',') ? ';' : ',';
        return row.Split(separator).Select(x => x.Trim().Trim('"')).ToList();
    }

    private static string GetColumn(IReadOnlyList<string> columns, int index) =>
        columns.Count > index ? columns[index].Trim() : string.Empty;

    private static string FirstNonEmpty(params string?[] values) =>
        values.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x))!.Trim();

    private static string? EmptyToNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
