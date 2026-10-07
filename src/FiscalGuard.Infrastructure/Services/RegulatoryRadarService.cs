using System.Net.Http.Json;
using System.Net;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using FiscalGuard.Application;
using FiscalGuard.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FiscalGuard.Infrastructure.Services;

public sealed class RegulatoryRadarService(
    FiscalGuardDbContext db,
    ICurrentUserContext currentUser,
    IConfiguration configuration,
    ILogger<RegulatoryRadarService> logger) : IRegulatoryRadarService
{
    private static readonly SemaphoreSlim CacheLock = new(1, 1);
    private static RegulatoryRadarSummary? CachedSummary;
    private static DateTimeOffset CachedUntil;

    private static readonly string[] DefaultThemes =
    [
        "simples nacional",
        "mei",
        "microempresa",
        "empresa de pequeno porte",
        "tributario",
        "nota fiscal",
        "nfse",
        "sped",
        "dctfweb",
        "efd-reinf",
        "reforma tributaria",
        "parcelamento",
        "icms",
        "iss"
    ];

    public async Task<RegulatoryRadarSummary> GetLatestAsync(bool forceRefresh, CancellationToken cancellationToken)
    {
        var configuredThemes = configuration
            .GetSection("RegulatoryRadar:Themes")
            .GetChildren()
            .Select(x => x.Value)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!)
            .ToArray();
        var themes = configuredThemes.Length > 0 ? configuredThemes : DefaultThemes;
        var baseUrl = configuration["RegulatoryRadar:CamaraBaseUrl"] ?? "https://dadosabertos.camara.leg.br/api/v2/";
        var days = int.TryParse(configuration["RegulatoryRadar:LookbackDays"], out var configuredDays) ? configuredDays : 1460;
        var maxItems = int.TryParse(configuration["RegulatoryRadar:MaxItems"], out var configuredMax) ? configuredMax : 8;
        var cacheMinutes = int.TryParse(configuration["RegulatoryRadar:CacheMinutes"], out var configuredCache) ? configuredCache : 30;

        if (!forceRefresh && CachedSummary is not null && CachedUntil > DateTimeOffset.UtcNow)
        {
            return await EnrichWithPortfolioAsync(CachedSummary with { FromCache = true, CachedUntil = CachedUntil }, false, cancellationToken);
        }

        await CacheLock.WaitAsync(cancellationToken);
        try
        {
            if (!forceRefresh && CachedSummary is not null && CachedUntil > DateTimeOffset.UtcNow)
            {
                return await EnrichWithPortfolioAsync(CachedSummary with { FromCache = true, CachedUntil = CachedUntil }, false, cancellationToken);
            }

            using var client = new HttpClient
            {
                BaseAddress = new Uri(baseUrl),
                Timeout = TimeSpan.FromSeconds(15)
            };
            client.DefaultRequestHeaders.UserAgent.ParseAdd("FiscalGuardTech-RegulatoryRadar/1.0");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var officialResult = await FetchOfficialFeedsAsync(themes, cancellationToken);
            var diagnostics = officialResult.Diagnostics.ToList();
            var officialAlerts = officialResult.Alerts
                .OrderByDescending(x => x.PresentedAt ?? DateTimeOffset.MinValue)
                .ToList();

            var legislativeAlerts = new List<RegulatoryAlertDto>();
            var legislativeRun = StartRun("camara", "Câmara dos Deputados - Dados Abertos", $"{baseUrl}proposicoes");
            var legislativeWatch = Stopwatch.StartNew();
            foreach (var theme in themes)
            {
                try
                {
                    var alerts = await FetchThemeAsync(client, theme, days, cancellationToken);
                    legislativeAlerts.AddRange(alerts);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to fetch regulatory theme {Theme}", theme);
                    legislativeRun = legislativeRun with
                    {
                        Success = false,
                        Error = MergeError(legislativeRun.Error, $"{theme}: {ex.Message}")
                    };
                }
            }
            legislativeWatch.Stop();
            diagnostics.Add(legislativeRun with
            {
                FinishedAt = DateTimeOffset.UtcNow,
                DurationMs = legislativeWatch.ElapsedMilliseconds,
                DocumentsFound = legislativeAlerts.Count,
                DocumentsAccepted = legislativeAlerts.GroupBy(x => x.Id).Count(),
                StatusMessage = legislativeAlerts.Count == 0
                    ? "A fonte respondeu, mas nenhum projeto passou pelos temas configurados."
                    : "Proposições encontradas e normalizadas para o radar."
            });

            var officialSlots = Math.Min(4, maxItems);
            var selectedOfficialAlerts = officialAlerts
                .GroupBy(x => x.Id)
                .Select(x => x.First())
                .Take(officialSlots)
                .ToList();

            var selectedLegislativeAlerts = legislativeAlerts
                .GroupBy(x => x.Id)
                .Select(x => x.First())
                .OrderByDescending(x => x.PresentedAt ?? DateTimeOffset.MinValue)
                .Take(Math.Max(0, maxItems - selectedOfficialAlerts.Count))
                .ToList();

            var deduplicated = selectedOfficialAlerts
                .Concat(selectedLegislativeAlerts)
                .OrderByDescending(x => x.SourceType == "Notícia ou ato oficial")
                .ThenByDescending(x => x.PresentedAt ?? DateTimeOffset.MinValue)
                .Take(maxItems)
                .ToList();

            CachedUntil = DateTimeOffset.UtcNow.AddMinutes(cacheMinutes);
            CachedSummary = new RegulatoryRadarSummary(
                DateTimeOffset.UtcNow,
                "Receita Federal, Simples Nacional e Câmara dos Deputados",
                false,
                CachedUntil,
                themes,
                diagnostics,
                0,
                0,
                "Regras auditáveis por tema, regime tributário, CNAE, UF/município e tags da empresa. IA pode ser adicionada depois como apoio, sem substituir a evidência oficial.",
                deduplicated);

            return await EnrichWithPortfolioAsync(CachedSummary, true, cancellationToken);
        }
        finally
        {
            CacheLock.Release();
        }
    }

    private async Task<(IReadOnlyList<RegulatoryAlertDto> Alerts, IReadOnlyList<RegulatorySourceDiagnosticDto> Diagnostics)> FetchOfficialFeedsAsync(
        IReadOnlyList<string> themes,
        CancellationToken cancellationToken)
    {
        var feeds = GetOfficialFeeds();
        var alerts = new List<RegulatoryAlertDto>();
        var diagnostics = new List<RegulatorySourceDiagnosticDto>();

        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
        client.DefaultRequestHeaders.UserAgent.ParseAdd("FiscalGuardTech-RegulatoryRadar/1.0");
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/rss+xml"));
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/xml"));

        foreach (var feed in feeds)
        {
            var run = StartRun(BuildSourceKey(feed.Name), feed.Name, feed.Url);
            var watch = Stopwatch.StartNew();
            try
            {
                var result = await FetchOfficialFeedAsync(client, feed, themes, cancellationToken);
                alerts.AddRange(result.Alerts);
                watch.Stop();
                diagnostics.Add(run with
                {
                    FinishedAt = DateTimeOffset.UtcNow,
                    DurationMs = watch.ElapsedMilliseconds,
                    DocumentsFound = result.DocumentsFound,
                    DocumentsAccepted = result.Alerts.Count,
                    StatusMessage = result.Alerts.Count == 0
                        ? "Feed lido, mas nenhuma publicação combinou com os temas fiscais monitorados."
                        : "Feed oficial lido e publicações fiscais normalizadas."
                });
            }
            catch (Exception ex)
            {
                watch.Stop();
                logger.LogWarning(ex, "Failed to fetch official regulatory feed {Feed}", feed.Name);
                diagnostics.Add(run with
                {
                    Success = false,
                    FinishedAt = DateTimeOffset.UtcNow,
                    DurationMs = watch.ElapsedMilliseconds,
                    Error = ex.Message,
                    StatusMessage = "Não foi possível ler esta fonte nesta atualização."
                });
            }
        }

        return (alerts, diagnostics);
    }

    private IReadOnlyList<OfficialFeedConfig> GetOfficialFeeds()
    {
        var configured = configuration
            .GetSection("RegulatoryRadar:OfficialFeeds")
            .GetChildren()
            .Select(x => new OfficialFeedConfig(
                x["Name"] ?? "Fonte oficial",
                x["Url"] ?? string.Empty,
                x["Theme"] ?? "fiscal"))
            .Where(x => Uri.TryCreate(x.Url, UriKind.Absolute, out _))
            .ToArray();

        return configured.Length > 0
            ? configured
            : [
                new(
                    "Receita Federal - Notícias oficiais",
                    "https://www.gov.br/receitafederal/pt-br/assuntos/noticias/RSS",
                    "receita federal"),
                new(
                    "Receita Federal / Simples Nacional",
                    "https://www.gov.br/receitafederal/simples/pt-br/assuntos/noticias/RSS",
                    "simples nacional")
            ];
    }

    private static async Task<(IReadOnlyList<RegulatoryAlertDto> Alerts, int DocumentsFound)> FetchOfficialFeedAsync(
        HttpClient client,
        OfficialFeedConfig feed,
        IReadOnlyList<string> themes,
        CancellationToken cancellationToken)
    {
        using var response = await client.GetAsync(feed.Url, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var document = await XDocument.LoadAsync(stream, LoadOptions.None, cancellationToken);

        var items = document
            .Descendants()
            .Where(x => x.Name.LocalName.Equals("item", StringComparison.OrdinalIgnoreCase))
            .Select(item => new
            {
                Title = CleanText(GetElementValue(item, "title")),
                Link = CleanText(GetElementValue(item, "link")),
                Description = CleanText(GetElementValue(item, "description")),
                Type = CleanText(GetElementValue(item, "type")),
                Date = TryParseDate(GetElementValue(item, "date")) ?? TryParseDate(GetElementValue(item, "pubDate"))
            })
            .ToList();

        var alerts = items
            .Where(item => IsContentItem(item.Type, item.Link))
            .Select(item => ToOfficialAlert(item.Title, item.Description, item.Link, item.Date, feed, themes))
            .Where(alert => alert is not null)
            .Select(alert => alert!)
            .Take(6)
            .ToList();

        return (alerts, items.Count);
    }

    private static async Task<IReadOnlyList<RegulatoryAlertDto>> FetchThemeAsync(
        HttpClient client,
        string theme,
        int lookbackDays,
        CancellationToken cancellationToken)
    {
        var alerts = new List<RegulatoryAlertDto>();
        foreach (var query in BuildQueries(theme))
        {
            var startDate = DateTime.UtcNow.AddDays(-lookbackDays).ToString("yyyy-MM-dd");
            var path = $"proposicoes?keywords={Uri.EscapeDataString(query)}&dataApresentacaoInicio={startDate}&ordem=DESC&ordenarPor=id&itens=5";
            var response = await client.GetFromJsonAsync<CamaraProposicoesResponse>(path, cancellationToken);
            var items = response?.Dados?
                .Select(item => ToAlert(item, theme))
                .ToList() ?? [];

            alerts.AddRange(items);
            if (alerts.Count > 0)
            {
                break;
            }
        }

        if (alerts.Count > 0)
        {
            return alerts;
        }

        foreach (var query in BuildQueries(theme))
        {
            var path = $"proposicoes?keywords={Uri.EscapeDataString(query)}&ordem=DESC&ordenarPor=id&itens=5";
            var response = await client.GetFromJsonAsync<CamaraProposicoesResponse>(path, cancellationToken);
            alerts.AddRange(response?.Dados?
            .Select(item => ToAlert(item, theme))
            .ToList() ?? []);

            if (alerts.Count > 0)
            {
                break;
            }
        }

        return alerts;
    }

    private static IReadOnlyList<string> BuildQueries(string theme)
    {
        var normalized = theme.ToLowerInvariant();
        return normalized switch
        {
            "tributario" => ["tributario", "tributaria", "tributacao", "imposto"],
            "nota fiscal" => ["nota fiscal", "documento fiscal", "nfs"],
            "nfse" => ["nfse", "nfs-e", "nota fiscal de servico"],
            "sped" => ["sped", "efd", "escrituracao fiscal"],
            "dctfweb" => ["dctfweb", "dctf web"],
            "efd-reinf" => ["efd-reinf", "reinf"],
            "reforma tributaria" => ["reforma tributaria", "cbs", "ibs"],
            "parcelamento" => ["parcelamento", "regularizacao"],
            "empresa de pequeno porte" => ["empresa de pequeno porte", "pequeno porte", "epp"],
            "mei" => ["microempreendedor individual", "mei"],
            "icms" => ["icms"],
            "iss" => ["iss", "imposto sobre servicos"],
            _ => [theme]
        };
    }

    private static RegulatoryAlertDto ToAlert(CamaraProposicao item, string theme)
    {
        var impact = ClassifyImpact(item.Ementa, theme);
        var title = $"{item.SiglaTipo} {item.Numero}/{item.Ano}";
        return new RegulatoryAlertDto(
            item.Id.ToString(),
            title,
            item.Ementa ?? "Proposição legislativa sem ementa resumida.",
            "Câmara dos Deputados",
            $"https://www.camara.leg.br/proposicoesWeb/fichadetramitacao?idProposicao={item.Id}",
            "Proposição legislativa",
            theme,
            impact.Level,
            impact.Reason,
            impact.BusinessImpact,
            impact.SuggestedAction,
            item.DataApresentacao,
            0,
            []);
    }

    private static RegulatoryAlertDto? ToOfficialAlert(
        string title,
        string description,
        string link,
        DateTimeOffset? date,
        OfficialFeedConfig feed,
        IReadOnlyList<string> monitoredThemes)
    {
        var text = $"{title} {description} {feed.Theme}";
        if (!IsRelevantOfficialAlert(text))
        {
            return null;
        }

        var theme = DetectTheme(text, feed.Theme, monitoredThemes);
        var impact = ClassifyOfficialImpact(text, theme);
        var id = $"official:{link}".ToLowerInvariant();

        return new RegulatoryAlertDto(
            id,
            title,
            string.IsNullOrWhiteSpace(description) ? "Publicação oficial sem resumo curto no feed." : description,
            feed.Name,
            link,
            "Notícia ou ato oficial",
            theme,
            impact.Level,
            impact.Reason,
            impact.BusinessImpact,
            impact.SuggestedAction,
            date,
            0,
            []);
    }

    private static (string Level, string Reason, string BusinessImpact, string SuggestedAction) ClassifyImpact(string? summary, string theme)
    {
        var text = $"{summary} {theme}".ToLowerInvariant();
        if (text.Contains("simples") || text.Contains("mei") || text.Contains("microempresa") || text.Contains("empresa de pequeno porte"))
        {
            return (
                "Alto",
                "Tema ligado diretamente a Simples Nacional, MEI ou empresas de pequeno porte.",
                "Pode alterar enquadramento, obrigações, limites ou benefícios usados por clientes de pequeno porte.",
                "Acompanhar a tramitação e separar clientes do Simples/MEI que podem precisar de comunicação preventiva.");
        }

        if (text.Contains("tribut") || text.Contains("imposto") || text.Contains("icms") || text.Contains("iss"))
        {
            return (
                "Médio",
                "Tema tributário com potencial de afetar apuração, documentos fiscais ou compliance.",
                "Pode impactar apuração, compliance fiscal, precificação ou orientação tributária ao cliente.",
                "Avaliar se o tema afeta CNAEs monitorados e registrar pauta para revisão técnica do escritório.");
        }

        if (text.Contains("nota fiscal") || text.Contains("nfs") || text.Contains("documento fiscal"))
        {
            return (
                "Médio",
                "Tema operacional ligado à emissão de documento fiscal.",
                "Pode impactar emissão de documentos fiscais e processos operacionais do cliente.",
                "Monitorar prazos e preparar comunicação objetiva para empresas afetadas.");
        }

        return (
            "Baixo",
            "Tema ainda em acompanhamento, sem regra operacional imediata identificada.",
            "Tema legislativo relacionado ao ambiente de negócios e monitorado para contexto preventivo.",
            "Manter no radar e revisar se a tramitação avançar.");
    }

    private static (string Level, string Reason, string BusinessImpact, string SuggestedAction) ClassifyOfficialImpact(string text, string theme)
    {
        var normalized = Normalize(text);
        if (ContainsAny(normalized, "obrigatoriedade", "obrigatorio", "prazo", "vencimento", "prorroga", "exclusao", "indeferimento", "parada programada", "indisponivel"))
        {
            return (
                "Alto",
                "Publicação oficial com prazo, obrigação, indisponibilidade ou mudança operacional.",
                "Pode exigir ação do escritório ou comunicação preventiva aos clientes afetados.",
                "Separar empresas cruzadas, validar obrigação aplicável e abrir pauta de atendimento antes do prazo indicado.");
        }

        if (ContainsAny(normalized, "resolucao", "instrucao normativa", "ato declaratorio", "decreto", "comunicado", "reforma tributaria", "cbs", "ibs"))
        {
            return (
                "Alto",
                "Publicação oficial com norma ou orientação já divulgada por órgão competente.",
                "Pode alterar procedimento, sistema fiscal, orientação ao cliente ou preparação para nova regra.",
                "Ler a publicação oficial, registrar interpretação técnica e sinalizar clientes do regime ou atividade afetada.");
        }

        if (ContainsAny(normalized, "sped", "dctf", "reinf", "nfse", "nfs-e", "nota fiscal", "documento fiscal", "cnpj"))
        {
            return (
                "Médio",
                "Tema operacional acompanhado na rotina fiscal e cadastral do escritório.",
                "Pode afetar emissão de documentos, cadastro, entrega de declaração ou uso de sistemas oficiais.",
                "Verificar se há clientes com CNAE/regime compatível e preparar checklist operacional.");
        }

        return ClassifyImpact(text, theme);
    }

    private async Task<RegulatoryRadarSummary> EnrichWithPortfolioAsync(
        RegulatoryRadarSummary summary,
        bool persist,
        CancellationToken cancellationToken)
    {
        var companies = await db.Companies
            .AsNoTracking()
            .Where(x => x.OrganizationId == currentUser.OrganizationId && x.Status == CompanyStatus.Active)
            .Select(x => new
            {
                x.Id,
                x.LegalName,
                x.Cnpj,
                x.IsSimplesOption,
                x.IsMeiOption,
                x.MainCnaeCode,
                x.MainCnaeDescription,
                x.PublicAddress,
                x.Tags
            })
            .ToListAsync(cancellationToken);

        var alertMatches = new Dictionary<string, IReadOnlyList<RegulatoryAffectedCompanyDto>>();
        var alerts = summary.Alerts
            .Select(alert =>
            {
                var affected = companies
                    .Select(company => MatchCompany(alert, company.Id, company.LegalName, company.Cnpj, company.IsSimplesOption, company.IsMeiOption, company.MainCnaeCode, company.MainCnaeDescription, company.PublicAddress, company.Tags))
                    .Where(x => x is not null)
                    .Select(x => x!)
                    .OrderByDescending(x => x.Score)
                    .ToList();
                alertMatches[alert.Id] = affected;

                return alert with
                {
                    AffectedCompaniesCount = affected.Count,
                    AffectedCompanies = affected.Take(5).ToList()
                };
            })
            .ToList();

        if (persist)
        {
            await PersistRadarResultAsync(summary.Diagnostics, alerts, alertMatches, cancellationToken);
        }

        var documentsStored = await db.RegulatoryDocuments.CountAsync(x => x.OrganizationId == currentUser.OrganizationId, cancellationToken);
        var matchesStored = await db.RegulatoryMatches.CountAsync(x => x.OrganizationId == currentUser.OrganizationId, cancellationToken);

        return summary with
        {
            Alerts = alerts,
            DocumentsStored = documentsStored,
            MatchesStored = matchesStored
        };
    }

    private static RegulatoryAffectedCompanyDto? MatchCompany(
        RegulatoryAlertDto alert,
        Guid id,
        string legalName,
        string cnpj,
        bool? isSimplesOption,
        bool? isMeiOption,
        string? cnaeCode,
        string? cnaeDescription,
        string? publicAddress,
        string? tags)
    {
        var text = Normalize($"{alert.Theme} {alert.Title} {alert.Summary}");
        var score = 0;
        var terms = new List<string>();
        var reasons = new List<string>();

        if ((text.Contains("simples") || text.Contains("microempresa") || text.Contains("pequeno porte") || text.Contains("cgsn")) && isSimplesOption == true)
        {
            score += 45;
            terms.Add("Simples Nacional");
            reasons.Add("cliente optante pelo Simples Nacional ou pequeno porte");
        }

        if ((text.Contains("mei") || text.Contains("microempreendedor")) && isMeiOption == true)
        {
            score += 45;
            terms.Add("MEI");
            reasons.Add("cliente marcado como MEI");
        }

        if ((text.Contains("nfse") || text.Contains("nfs-e") || text.Contains("nota fiscal de servico") || text.Contains("iss")) &&
            TextContainsAny(cnaeDescription, "servico servicos contabilidade consultoria tecnologia desenvolvimento suporte manutencao"))
        {
            score += 35;
            terms.Add("serviços/NFS-e/ISS");
            reasons.Add("CNAE sugere prestação de serviços");
        }

        if (TextContainsAny(cnaeDescription, text))
        {
            score += 30;
            terms.Add("CNAE");
            reasons.Add("tema relacionado ao CNAE principal");
        }

        if (TextContainsAny(tags, text))
        {
            score += 30;
            terms.Add("tags");
            reasons.Add("tema relacionado às tags internas da empresa");
        }

        if (text.Contains("tribut") || text.Contains("imposto") || text.Contains("nota fiscal") || text.Contains("icms") || text.Contains("iss"))
        {
            score += 15;
            terms.Add("fiscal amplo");
            reasons.Add("tema fiscal amplo que pode afetar rotinas de orientação e compliance");
        }

        if (score <= 0)
        {
            return null;
        }

        return new(
            id,
            legalName,
            cnpj,
            string.Join("; ", reasons.Distinct()),
            Math.Min(score, 100),
            string.Join(", ", terms.Distinct()));
    }

    private async Task PersistRadarResultAsync(
        IReadOnlyList<RegulatorySourceDiagnosticDto> diagnostics,
        IReadOnlyList<RegulatoryAlertDto> alerts,
        IReadOnlyDictionary<string, IReadOnlyList<RegulatoryAffectedCompanyDto>> matchesByAlertId,
        CancellationToken cancellationToken)
    {
        var organizationId = currentUser.OrganizationId;

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);

        foreach (var diagnostic in diagnostics)
        {
            db.RegulatorySourceRuns.Add(new RegulatorySourceRun
            {
                OrganizationId = organizationId,
                SourceKey = diagnostic.SourceKey,
                SourceName = diagnostic.SourceName,
                StartedAt = ToUtc(diagnostic.StartedAt),
                FinishedAt = ToUtc(diagnostic.FinishedAt),
                Success = diagnostic.Success,
                DocumentsFound = diagnostic.DocumentsFound,
                DocumentsAccepted = diagnostic.DocumentsAccepted,
                DurationMs = diagnostic.DurationMs,
                Error = diagnostic.Error,
                StatusMessage = diagnostic.StatusMessage
            });
        }

        var externalIds = alerts.Select(x => BuildExternalId(x.Id)).Distinct().ToList();
        var existingDocuments = await db.RegulatoryDocuments
            .Where(x => x.OrganizationId == organizationId && externalIds.Contains(x.ExternalId))
            .ToDictionaryAsync(x => x.ExternalId, cancellationToken);

        foreach (var alert in alerts)
        {
            var externalId = BuildExternalId(alert.Id);
            if (!existingDocuments.TryGetValue(externalId, out var document))
            {
                document = new RegulatoryDocument
                {
                    OrganizationId = organizationId,
                    ExternalId = externalId,
                    SourceKey = BuildSourceKey(alert.Source),
                    SourceName = alert.Source,
                    SourceType = alert.SourceType,
                    Title = alert.Title,
                    Summary = alert.Summary,
                    SourceUrl = alert.SourceUrl,
                    Theme = alert.Theme,
                    ImpactLevel = alert.ImpactLevel,
                    ImpactReason = alert.ImpactReason,
                    BusinessImpact = alert.BusinessImpact,
                    SuggestedAction = alert.SuggestedAction,
                    RawText = $"{alert.Title}\n{alert.Summary}",
                    SearchText = Normalize($"{alert.Title} {alert.Summary} {alert.Theme} {alert.SourceType}"),
                    PresentedAt = ToUtc(alert.PresentedAt),
                    CapturedAt = DateTimeOffset.UtcNow,
                    RelevanceScore = ToRelevanceScore(alert.ImpactLevel)
                };
                db.RegulatoryDocuments.Add(document);
                existingDocuments[externalId] = document;
            }
            else
            {
                document.SourceName = alert.Source;
                document.SourceType = alert.SourceType;
                document.Title = alert.Title;
                document.Summary = alert.Summary;
                document.SourceUrl = alert.SourceUrl;
                document.Theme = alert.Theme;
                document.ImpactLevel = alert.ImpactLevel;
                document.ImpactReason = alert.ImpactReason;
                document.BusinessImpact = alert.BusinessImpact;
                document.SuggestedAction = alert.SuggestedAction;
                document.SearchText = Normalize($"{alert.Title} {alert.Summary} {alert.Theme} {alert.SourceType}");
                document.PresentedAt = ToUtc(alert.PresentedAt);
                document.RelevanceScore = ToRelevanceScore(alert.ImpactLevel);
            }
        }

        await db.SaveChangesAsync(cancellationToken);

        var documentIds = existingDocuments.Values.Select(x => x.Id).ToList();
        var existingMatches = await db.RegulatoryMatches
            .Where(x => x.OrganizationId == organizationId && documentIds.Contains(x.RegulatoryDocumentId))
            .ToDictionaryAsync(x => $"{x.RegulatoryDocumentId}:{x.CompanyId}", cancellationToken);

        foreach (var alert in alerts)
        {
            var document = existingDocuments[BuildExternalId(alert.Id)];
            if (!matchesByAlertId.TryGetValue(alert.Id, out var matches))
            {
                continue;
            }

            foreach (var match in matches)
            {
                var key = $"{document.Id}:{match.Id}";
                if (!existingMatches.TryGetValue(key, out var entity))
                {
                    db.RegulatoryMatches.Add(new RegulatoryMatch
                    {
                        OrganizationId = organizationId,
                        RegulatoryDocumentId = document.Id,
                        CompanyId = match.Id,
                        Reason = match.Reason,
                        MatchedTerms = match.MatchedTerms,
                        Score = match.Score
                    });
                }
                else
                {
                    entity.Reason = match.Reason;
                    entity.MatchedTerms = match.MatchedTerms;
                    entity.Score = match.Score;
                }
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private static bool IsContentItem(string type, string link)
    {
        var normalizedType = Normalize(type);
        if (normalizedType.Contains("image") || normalizedType.Contains("folder") || normalizedType.Contains("collection") || normalizedType.Contains("link"))
        {
            return false;
        }

        return Uri.TryCreate(link, UriKind.Absolute, out var uri) &&
            uri.Host.EndsWith("gov.br", StringComparison.OrdinalIgnoreCase) &&
            !link.EndsWith(".png/view", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsRelevantOfficialAlert(string text)
    {
        var normalized = Normalize(text);
        return ContainsAny(
            normalized,
            "simples nacional",
            "mei",
            "microempreendedor",
            "microempresa",
            "empresa de pequeno porte",
            "nfse",
            "nfs-e",
            "nota fiscal",
            "documento fiscal",
            "sped",
            "dctf",
            "reinf",
            "das",
            "darf",
            "cnpj",
            "cbs",
            "ibs",
            "reforma tributaria",
            "obrigacao acessoria",
            "declaracao",
            "parcelamento",
            "exclusao",
            "regularizacao",
            "prazo",
            "resolucao cgsn");
    }

    private static string DetectTheme(string text, string fallback, IReadOnlyList<string> monitoredThemes)
    {
        var normalized = Normalize(text);
        foreach (var theme in monitoredThemes)
        {
            if (normalized.Contains(Normalize(theme)))
            {
                return theme;
            }
        }

        if (ContainsAny(normalized, "nfse", "nfs-e", "nota fiscal", "documento fiscal"))
        {
            return "nota fiscal";
        }

        if (ContainsAny(normalized, "reforma tributaria", "cbs", "ibs", "imposto", "tribut"))
        {
            return "tributário";
        }

        if (ContainsAny(normalized, "mei", "microempreendedor"))
        {
            return "mei";
        }

        if (ContainsAny(normalized, "simples", "cgsn", "microempresa", "pequeno porte"))
        {
            return "simples nacional";
        }

        return fallback;
    }

    private static RegulatorySourceDiagnosticDto StartRun(string sourceKey, string sourceName, string? requestUrl) =>
        new(
            sourceKey,
            sourceName,
            true,
            0,
            0,
            0,
            null,
            requestUrl is null ? "Consulta iniciada." : $"Consulta iniciada em {requestUrl}.",
            DateTimeOffset.UtcNow,
            null);

    private static string? MergeError(string? current, string next) =>
        string.IsNullOrWhiteSpace(current) ? next : $"{current} | {next}";

    private static string? GetElementValue(XElement element, string localName) =>
        element
            .Elements()
            .FirstOrDefault(x => x.Name.LocalName.Equals(localName, StringComparison.OrdinalIgnoreCase))
            ?.Value;

    private static string BuildSourceKey(string source)
    {
        var normalized = Normalize(source);
        var key = Regex.Replace(normalized, "[^a-z0-9]+", "-").Trim('-');
        return string.IsNullOrWhiteSpace(key) ? "fonte-oficial" : key[..Math.Min(80, key.Length)];
    }

    private static string BuildExternalId(string value)
    {
        if (value.Length <= 240)
        {
            return value;
        }

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return $"sha256:{Convert.ToHexString(bytes).ToLowerInvariant()}";
    }

    private static int ToRelevanceScore(string impactLevel) =>
        Normalize(impactLevel) switch
        {
            "critico" => 100,
            "alto" => 80,
            "medio" => 55,
            "baixo" => 25,
            _ => 10
        };

    private static bool ContainsAny(string source, params string[] terms) =>
        terms.Any(term => source.Contains(Normalize(term)));

    private static DateTimeOffset? TryParseDate(string? value) =>
        DateTimeOffset.TryParse(value, out var date) ? ToUtc(date) : null;

    private static DateTimeOffset ToUtc(DateTimeOffset value) =>
        value.Offset == TimeSpan.Zero ? value : value.ToUniversalTime();

    private static DateTimeOffset? ToUtc(DateTimeOffset? value) =>
        value.HasValue ? ToUtc(value.Value) : null;

    private static string CleanText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var withoutTags = Regex.Replace(value, "<.*?>", " ");
        return WebUtility.HtmlDecode(withoutTags)
            .Replace('\r', ' ')
            .Replace('\n', ' ')
            .Trim();
    }

    private static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return value
            .ToLowerInvariant()
            .Replace("á", "a")
            .Replace("à", "a")
            .Replace("ã", "a")
            .Replace("â", "a")
            .Replace("é", "e")
            .Replace("ê", "e")
            .Replace("í", "i")
            .Replace("ó", "o")
            .Replace("ô", "o")
            .Replace("õ", "o")
            .Replace("ú", "u")
            .Replace("ç", "c");
    }

    private static bool TextContainsAny(string? source, string target)
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            return false;
        }

        var normalizedTarget = Normalize(target);
        return Normalize(source)
            .Split([' ', ';', ',', '.', '-', '/'], StringSplitOptions.RemoveEmptyEntries)
            .Where(x => x.Length >= 4)
            .Any(normalizedTarget.Contains);
    }

    private sealed record CamaraProposicoesResponse(
        [property: JsonPropertyName("dados")] IReadOnlyList<CamaraProposicao>? Dados);

    private sealed record CamaraProposicao(
        [property: JsonPropertyName("id")] long Id,
        [property: JsonPropertyName("uri")] string? Uri,
        [property: JsonPropertyName("siglaTipo")] string? SiglaTipo,
        [property: JsonPropertyName("numero")] int? Numero,
        [property: JsonPropertyName("ano")] int? Ano,
        [property: JsonPropertyName("ementa")] string? Ementa,
        [property: JsonPropertyName("dataApresentacao")] DateTimeOffset? DataApresentacao);

    private sealed record OfficialFeedConfig(string Name, string Url, string Theme);
}
