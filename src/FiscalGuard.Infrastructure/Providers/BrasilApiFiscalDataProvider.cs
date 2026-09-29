using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FiscalGuard.Application;
using FiscalGuard.Domain;
using Microsoft.Extensions.Logging;

namespace FiscalGuard.Infrastructure.Providers;

public sealed class BrasilApiFiscalDataProvider(
    HttpClient httpClient,
    ILogger<BrasilApiFiscalDataProvider> logger) : IFiscalDataProvider
{
    private const string ProviderName = "brasilapi-cnpj";

    public async Task<FiscalConsultationResult> ConsultAsync(string cnpj, CancellationToken cancellationToken)
    {
        var normalized = Cnpj.Normalize(cnpj);
        using var response = await httpClient.GetAsync($"cnpj/v1/{normalized}", cancellationToken);
        var payload = await response.Content.ReadAsStringAsync(cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return new FiscalConsultationResult(
                ProviderName,
                FiscalStatus.DataUnavailable,
                false,
                payload,
                "CNPJ nao encontrado na fonte publica.",
                [
                    new(
                        "cnpj-not-found",
                        IssueType.Registration,
                        "CNPJ nao encontrado na fonte publica",
                        "A consulta publica nao retornou dados cadastrais para este CNPJ.",
                        IssueSeverity.Medium,
                        "Conferir se o CNPJ foi digitado corretamente antes de orientar o cliente.",
                        "Fonte publica")
                ]);
        }

        if (!response.IsSuccessStatusCode)
        {
            logger.LogWarning("BrasilAPI returned {StatusCode} for CNPJ {Cnpj}", response.StatusCode, normalized);
            return new FiscalConsultationResult(
                ProviderName,
                FiscalStatus.QueryError,
                false,
                payload,
                $"Fonte publica retornou HTTP {(int)response.StatusCode}.",
                [
                    new(
                        "brasilapi-unavailable",
                        IssueType.IntegrationFailure,
                        "Fonte publica indisponivel",
                        "A consulta de CNPJ nao respondeu corretamente.",
                        IssueSeverity.Low,
                        "Tentar novamente mais tarde. O historico preserva a falha para auditoria.",
                        "Falha de integracao")
                ]);
        }

        var data = JsonSerializer.Deserialize<BrasilApiCnpjResponse>(payload, SerializerOptions);
        if (data is null)
        {
            return new FiscalConsultationResult(
                ProviderName,
                FiscalStatus.QueryError,
                false,
                payload,
                "Resposta da fonte publica nao pode ser interpretada.",
                [
                    new(
                        "invalid-provider-payload",
                        IssueType.IntegrationFailure,
                        "Resposta externa invalida",
                        "O FiscalGuard recebeu dados em formato inesperado.",
                        IssueSeverity.Low,
                        "Registrar a falha e reexecutar a consulta; se repetir, revisar o conector.",
                        "Falha de integracao")
                ]);
        }

        var findings = BuildFindings(data);
        var status = findings.Any(x => x.Severity >= IssueSeverity.High)
            ? FiscalStatus.Irregular
            : findings.Count > 0 ? FiscalStatus.Attention : FiscalStatus.Regular;

        return new FiscalConsultationResult(
            ProviderName,
            status,
            true,
            payload,
            null,
            findings,
            BuildSnapshot(data));
    }

    private static IReadOnlyList<FiscalFinding> BuildFindings(BrasilApiCnpjResponse data)
    {
        var findings = new List<FiscalFinding>();
        var registration = data.DescricaoSituacaoCadastral?.Trim();
        if (!string.Equals(registration, "ATIVA", StringComparison.OrdinalIgnoreCase))
        {
            findings.Add(new FiscalFinding(
                $"registration-{NormalizeKey(registration ?? "desconhecida")}",
                IssueType.Registration,
                "Situacao cadastral requer atencao",
                BuildRegistrationDescription(registration),
                IsCriticalRegistration(registration) ? IssueSeverity.Critical : IssueSeverity.High,
                BuildRegistrationRecommendation(registration),
                "Fonte publica"));
        }

        if (data.OpcaoPeloSimples is false)
        {
            findings.Add(new FiscalFinding(
                "not-simples-option",
                IssueType.SimplesNational,
                "Empresa nao consta como optante pelo Simples",
                "A fonte publica indica que o CNPJ nao esta marcado como optante pelo Simples Nacional. Para escritorios focados em Simples, este e um sinal de desalinhamento da carteira, mudanca de regime ou cadastro que precisa de revisao.",
                IssueSeverity.High,
                "Conferir o enquadramento no Portal do Simples Nacional, validar se houve desenquadramento ou erro cadastral e decidir se o cliente deve permanecer na rotina automatizada de Simples.",
                "Fonte publica"));
        }

        if (string.IsNullOrWhiteSpace(data.CnaeFiscalDescricao))
        {
            findings.Add(new FiscalFinding(
                "missing-main-cnae",
                IssueType.Registration,
                "CNAE principal nao retornado",
                "A consulta publica nao trouxe a descricao do CNAE principal.",
                IssueSeverity.Medium,
                "Conferir o cadastro na Receita Federal e atualizar a ficha do cliente.",
                "Dado derivado"));
        }

        if (!string.IsNullOrWhiteSpace(data.SituacaoEspecial))
        {
            findings.Add(new FiscalFinding(
                $"special-status-{NormalizeKey(data.SituacaoEspecial)}",
                IssueType.Registration,
                "Situacao especial informada",
                $"A fonte publica retornou situacao especial: {data.SituacaoEspecial}.",
                IssueSeverity.Medium,
                "Avaliar o impacto da situacao especial antes de classificar o cliente como regular.",
                "Fonte publica"));
        }

        return findings;
    }

    private static FiscalCompanySnapshot BuildSnapshot(BrasilApiCnpjResponse data) =>
        new(
            data.RazaoSocial,
            data.NomeFantasia,
            data.DescricaoSituacaoCadastral,
            ParseDate(data.DataSituacaoCadastral),
            data.CnaeFiscal?.ToString(),
            data.CnaeFiscalDescricao,
            FormatAddress(data),
            ProviderName,
            DateTimeOffset.UtcNow,
            data.OpcaoPeloSimples,
            data.OpcaoPeloMei);

    private static bool IsCriticalRegistration(string? status) =>
        status?.Contains("BAIXADA", StringComparison.OrdinalIgnoreCase) == true
        || status?.Contains("INAPTA", StringComparison.OrdinalIgnoreCase) == true
        || status?.Contains("SUSPENSA", StringComparison.OrdinalIgnoreCase) == true;

    private static string BuildRegistrationDescription(string? status)
    {
        var normalized = status ?? "nao informada";
        if (normalized.Contains("BAIXADA", StringComparison.OrdinalIgnoreCase))
        {
            return "A Receita Federal informa situacao cadastral \"BAIXADA\". Para o contador, isso indica que o CNPJ nao deve seguir tratado como cliente operacional ativo sem uma revisao da baixa, das obrigacoes finais e do historico de encerramento.";
        }

        if (normalized.Contains("INAPTA", StringComparison.OrdinalIgnoreCase))
        {
            return "A Receita Federal informa situacao cadastral \"INAPTA\". Este sinal pode impedir operacoes do cliente e costuma exigir acao cadastral antes de rotinas fiscais recorrentes.";
        }

        if (normalized.Contains("SUSPENSA", StringComparison.OrdinalIgnoreCase))
        {
            return "A Receita Federal informa situacao cadastral \"SUSPENSA\". O escritorio deve priorizar a causa da suspensao antes de tratar a empresa como regular.";
        }

        return $"A Receita Federal informa situacao cadastral \"{normalized}\" para este CNPJ.";
    }

    private static string BuildRegistrationRecommendation(string? status)
    {
        var normalized = status ?? string.Empty;
        if (normalized.Contains("BAIXADA", StringComparison.OrdinalIgnoreCase))
        {
            return "Reclassificar o cliente na carteira, conferir obrigacoes de encerramento e evitar que a equipe siga executando rotinas periodicas como se a empresa estivesse ativa.";
        }

        if (normalized.Contains("INAPTA", StringComparison.OrdinalIgnoreCase) || normalized.Contains("SUSPENSA", StringComparison.OrdinalIgnoreCase))
        {
            return "Abrir uma tarefa prioritaria de regularizacao cadastral, validar a situacao no comprovante oficial e alinhar com o cliente antes de novas entregas fiscais.";
        }

        return "Validar a situacao no Comprovante de Inscricao e orientar regularizacao cadastral antes de novas operacoes.";
    }

    private static string NormalizeKey(string value) =>
        new(value.ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray());

    private static DateOnly? ParseDate(string? value) =>
        DateOnly.TryParse(value, out var parsed) ? parsed : null;

    private static string? FormatAddress(BrasilApiCnpjResponse data)
    {
        var street = string.Join(" ", new[] { data.DescricaoTipoLogradouro, data.Logradouro }.Where(x => !string.IsNullOrWhiteSpace(x)));
        var parts = new[] { street, data.Numero, data.Complemento, data.Bairro, data.Municipio, data.Uf, data.Cep }
            .Where(x => !string.IsNullOrWhiteSpace(x));
        var address = string.Join(", ", parts);
        return string.IsNullOrWhiteSpace(address) ? null : address;
    }

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private sealed record BrasilApiCnpjResponse(
        [property: JsonPropertyName("razao_social")] string? RazaoSocial,
        [property: JsonPropertyName("nome_fantasia")] string? NomeFantasia,
        [property: JsonPropertyName("descricao_situacao_cadastral")] string? DescricaoSituacaoCadastral,
        [property: JsonPropertyName("data_situacao_cadastral")] string? DataSituacaoCadastral,
        [property: JsonPropertyName("cnae_fiscal")] long? CnaeFiscal,
        [property: JsonPropertyName("cnae_fiscal_descricao")] string? CnaeFiscalDescricao,
        [property: JsonPropertyName("descricao_tipo_logradouro")] string? DescricaoTipoLogradouro,
        [property: JsonPropertyName("logradouro")] string? Logradouro,
        [property: JsonPropertyName("numero")] string? Numero,
        [property: JsonPropertyName("complemento")] string? Complemento,
        [property: JsonPropertyName("bairro")] string? Bairro,
        [property: JsonPropertyName("municipio")] string? Municipio,
        [property: JsonPropertyName("uf")] string? Uf,
        [property: JsonPropertyName("cep")] string? Cep,
        [property: JsonPropertyName("opcao_pelo_simples")] bool? OpcaoPeloSimples,
        [property: JsonPropertyName("opcao_pelo_mei")] bool? OpcaoPeloMei,
        [property: JsonPropertyName("situacao_especial")] string? SituacaoEspecial);
}
