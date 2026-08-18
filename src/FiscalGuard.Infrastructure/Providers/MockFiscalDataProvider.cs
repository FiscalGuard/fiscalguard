using FiscalGuard.Application;
using FiscalGuard.Domain;

namespace FiscalGuard.Infrastructure.Providers;

public sealed class MockFiscalDataProvider : IFiscalDataProvider
{
    public async Task<FiscalConsultationResult> ConsultAsync(string cnpj, CancellationToken cancellationToken)
    {
        await Task.Delay(250, cancellationToken);
        var scenario = int.Parse(cnpj[^2..]) % 6;

        return scenario switch
        {
            0 => new("mock", FiscalStatus.Regular, true, """{"status":"regular"}""", null, []),
            1 => new("mock", FiscalStatus.Attention, true, """{"status":"attention"}""", null, [
                new("simples-warning", IssueType.SimplesNational, "Possivel divergencia no Simples Nacional", "Foi identificado um indicador que exige revisao do contador.", IssueSeverity.Medium)
            ]),
            2 => new("mock", FiscalStatus.Irregular, true, """{"status":"irregular"}""", null, [
                new("missing-filing", IssueType.MissingFiling, "Declaracao pendente", "A demonstracao simulada aponta uma obrigacao acessoria pendente.", IssueSeverity.High)
            ]),
            3 => new("mock", FiscalStatus.QueryError, false, """{"error":"provider_failure"}""", "Falha simulada de integracao.", [
                new("provider-failure", IssueType.IntegrationFailure, "Falha repetida na consulta", "A fonte fiscal simulada nao respondeu corretamente.", IssueSeverity.Low)
            ]),
            4 => new("mock", FiscalStatus.DataUnavailable, false, """{"status":"unavailable"}""", "Dados indisponiveis no provedor.", []),
            _ => new("mock", FiscalStatus.Irregular, true, """{"status":"irregular","items":2}""", null, [
                new("debt", IssueType.TaxDebt, "Pendencia fiscal critica", "Existe uma pendencia simulada com potencial impacto operacional.", IssueSeverity.Critical),
                new("registration", IssueType.Registration, "Cadastro requer atencao", "Dados cadastrais simulados precisam ser revisados.", IssueSeverity.Medium)
            ])
        };
    }
}
