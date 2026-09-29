using FiscalGuard.Application;
using Microsoft.EntityFrameworkCore;

namespace FiscalGuard.Infrastructure.Services;

public sealed class OrganizationService(
    FiscalGuardDbContext db,
    ICurrentUserContext currentUser) : IOrganizationService
{
    public async Task<OrganizationDto> GetAsync(CancellationToken cancellationToken)
    {
        var organization = await db.Organizations.SingleAsync(
            x => x.Id == currentUser.OrganizationId,
            cancellationToken);

        return new OrganizationDto(
            organization.Id,
            organization.Name,
            organization.Document,
            organization.Phone,
            organization.IsActive);
    }

    public async Task<OrganizationDto> UpdateAsync(UpdateOrganizationRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new InvalidOperationException("Nome da organizacao e obrigatorio.");
        }

        var organization = await db.Organizations.SingleAsync(
            x => x.Id == currentUser.OrganizationId,
            cancellationToken);

        organization.Name = request.Name.Trim();
        organization.Document = string.IsNullOrWhiteSpace(request.Document) ? null : request.Document.Trim();
        organization.Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim();

        await db.SaveChangesAsync(cancellationToken);

        return new OrganizationDto(
            organization.Id,
            organization.Name,
            organization.Document,
            organization.Phone,
            organization.IsActive);
    }
}
