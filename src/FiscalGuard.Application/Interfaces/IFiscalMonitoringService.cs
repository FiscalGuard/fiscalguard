namespace FiscalGuard.Application;

public interface IFiscalMonitoringService
{
    Task<FiscalConsultationResult> ConsultCompanyAsync(Guid companyId, CancellationToken cancellationToken);
}
