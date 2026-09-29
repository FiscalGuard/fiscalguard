using System.Net.Http.Json;
using System.Net;
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
            return await EnrichWithPortfolioAsync(CachedSummary with { FromCache = true, CachedUntil = CachedUntil }, cancellationToken);
        }

        await CacheLock.WaitAsync(cancellationToken);
        try
        {
            if (!forceRefresh && CachedSummary is not null && CachedUntil > DateTimeOffset.UtcNow)
            {
                return await EnrichWithPortfolioAsync(CachedSummary with { FromCache = true, CachedUntil = CachedUntil }, cancellationToken);
            }

            using var client = new HttpClient
            {
                BaseAddress = new Uri(baseUrl),
                Timeout = TimeSpan.FromSeconds(15)
            };
            client.DefaultRequestHeaders.UserAgent.ParseAdd("FiscalGuardTech-RegulatoryRadar/1.0");

            var officialAlerts = (await FetchOfficialFeedsAsync(themes, cancellationToken))
                .OrderByDescending(x => x.PresentedAt ?? DateTimeOffset.MinValue)
                .ToList();

            var legislativeAlerts = new List<RegulatoryAlertDto>();
            foreach (var theme in themes)
            {
                try
                {
                    legislativeAlerts.AddRange(await FetchThemeAsync(client, theme, days, cancellationToken));
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to fetch regulatory theme {Theme}", theme);
                }
            }

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
                deduplicated);

            return await EnrichWithPortfolioAsync(CachedSummary, cancellationToken);
        }
        finally
        {
            CacheLock.Release();
        }
    }

    private async Task<IReadOnlyList<RegulatoryAlertDto>> FetchOfficialFeedsAsync(
        IReadOnlyList<string> themes,
        CancellationToken cancellationToken)
    {
        var feeds = GetOfficialFeeds();
        var alerts = new List<RegulatoryAlertDto>();

        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
        client.DefaultRequestHeaders.UserAgent.ParseAdd("FiscalGuardTech-RegulatoryRadar/1.0");

        foreach (var feed in feeds)
        {
            try
            {
                alerts.AddRange(await FetchOfficialFeedAsync(client, feed, themes, cancellationToken));
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to fetch official regulatory feed {Feed}", feed.Name);
            }
        }

        return alerts;
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

    private static async Task<IReadOnlyList<RegulatoryAlertDto>> FetchOfficialFeedAsync(
        HttpClient client,
        OfficialFeedConfig feed,
        IReadOnlyList<string> themes,
        CancellationToken cancellationToken)
    {
        using var response = await client.GetAsync(feed.Url, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var document = await XDocument.LoadAsync(stream, LoadOptions.None, cancellationToken);
        XNamespace rss = "http://purl.org/rss/1.0/";
        XNamespace dc = "http://purl.org/dc/elements/1.1/";

        return document
            .Descendants(rss + "item")
            .Select(item => new
            {
                Title = CleanText(item.Element(rss + "title")?.Value),
                Link = CleanText(item.Element(rss + "link")?.Value),
                Description = CleanText(item.Element(rss + "description")?.Value),
                Type = CleanText(item.Element(dc + "type")?.Value),
                Date = TryParseDate(item.Element(dc + "date")?.Value)
            })
            .Where(item => IsContentItem(item.Type, item.Link))
            .Select(item => ToOfficialAlert(item.Title, item.Description, item.Link, item.Date, feed, themes))
            .Where(alert => alert is not null)
            .Select(alert => alert!)
            .Take(6)
            .ToList();
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

    private async Task<RegulatoryRadarSummary> EnrichWithPortfolioAsync(RegulatoryRadarSummary summary, CancellationToken cancellationToken)
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
                x.MainCnaeDescription,
                x.Tags
            })
            .ToListAsync(cancellationToken);

        var alerts = summary.Alerts
            .Select(alert =>
            {
                var affected = companies
                    .Select(company => MatchCompany(alert, company.Id, company.LegalName, company.Cnpj, company.IsSimplesOption, company.IsMeiOption, company.MainCnaeDescription, company.Tags))
                    .Where(x => x is not null)
                    .Select(x => x!)
                    .ToList();

                return alert with
                {
                    AffectedCompaniesCount = affected.Count,
                    AffectedCompanies = affected.Take(5).ToList()
                };
            })
            .ToList();

        return summary with { Alerts = alerts };
    }

    private static RegulatoryAffectedCompanyDto? MatchCompany(
        RegulatoryAlertDto alert,
        Guid id,
        string legalName,
        string cnpj,
        bool? isSimplesOption,
        bool? isMeiOption,
        string? cnaeDescription,
        string? tags)
    {
        var text = $"{alert.Theme} {alert.Summary}".ToLowerInvariant();
        if ((text.Contains("simples") || text.Contains("microempresa") || text.Contains("pequeno porte")) && isSimplesOption == true)
        {
            return new(id, legalName, cnpj, "Cliente optante pelo Simples Nacional ou pequeno porte.");
        }

        if ((text.Contains("mei") || text.Contains("microempreendedor")) && isMeiOption == true)
        {
            return new(id, legalName, cnpj, "Cliente marcado como MEI.");
        }

        if ((text.Contains("nfse") || text.Contains("nfs-e") || text.Contains("nota fiscal de servico") || text.Contains("iss")) &&
            TextContainsAny(cnaeDescription, "servico servicos contabilidade consultoria tecnologia desenvolvimento suporte manutencao"))
        {
            return new(id, legalName, cnpj, "CNAE sugere prestação de serviços; tema pode afetar emissão de NFS-e/ISS.");
        }

        if (TextContainsAny(cnaeDescription, text) || TextContainsAny(tags, text))
        {
            return new(id, legalName, cnpj, "Tema tem termos relacionados ao CNAE ou tags da empresa.");
        }

        if (text.Contains("tribut") || text.Contains("imposto") || text.Contains("nota fiscal") || text.Contains("icms") || text.Contains("iss"))
        {
            return new(id, legalName, cnpj, "Tema fiscal amplo que pode afetar rotinas de orientação e compliance.");
        }

        return null;
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

    private static bool ContainsAny(string source, params string[] terms) =>
        terms.Any(term => source.Contains(Normalize(term)));

    private static DateTimeOffset? TryParseDate(string? value) =>
        DateTimeOffset.TryParse(value, out var date) ? date : null;

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

        return source
            .ToLowerInvariant()
            .Split([' ', ';', ',', '.', '-', '/'], StringSplitOptions.RemoveEmptyEntries)
            .Where(x => x.Length >= 4)
            .Any(target.Contains);
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
