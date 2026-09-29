namespace FiscalGuard.Application;

public interface IFiscalMonitoringProcessor
{
    Task<FiscalConsultationResult> ProcessAsync(
        Guid organizationId,
        Guid companyId,
        Guid? requestedByUserId,
        CancellationToken cancellationToken);
}
