using FiscalGuard.Application;

namespace FiscalGuard.Infrastructure.Services;

public sealed class FiscalMonitoringService(
    ICurrentUserContext currentUser,
    IFiscalMonitoringProcessor processor) : IFiscalMonitoringService
{
    public Task<FiscalConsultationResult> ConsultCompanyAsync(Guid companyId, CancellationToken cancellationToken) =>
        processor.ProcessAsync(currentUser.OrganizationId, companyId, currentUser.UserId, cancellationToken);
}
