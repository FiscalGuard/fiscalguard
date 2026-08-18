namespace FiscalGuard.Application;

public interface IFiscalDataProvider
{
    Task<FiscalConsultationResult> ConsultAsync(string cnpj, CancellationToken cancellationToken);
}
