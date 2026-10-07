import { Activity, Building2, Database, ExternalLink, Newspaper, RefreshCw, Scale, ShieldCheck, Sparkles } from 'lucide-react';
import { Link } from 'react-router-dom';
import { useEffect, useState } from 'react';
import { api } from '../../services/api';
import type { RegulatoryRadarSummary } from '../../types/api';
import { riskClass } from '../../utils/status';

export function RegulatoryRadarPage() {
  const [data, setData] = useState<RegulatoryRadarSummary | null>(null);
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [error, setError] = useState('');

  const loadRadar = (forceRefresh = false) => {
    if (forceRefresh) {
      setRefreshing(true);
    } else {
      setLoading(true);
    }
    setError('');

    api<RegulatoryRadarSummary>(`/api/regulatory-radar${forceRefresh ? '?forceRefresh=true' : ''}`)
      .then(setData)
      .catch((err) => setError(err.message))
      .finally(() => {
        setLoading(false);
        setRefreshing(false);
      });
  };

  useEffect(() => {
    loadRadar();
  }, []);

  if (loading) {
    return <div className="rounded-md border border-slate-200 bg-white p-5 text-sm text-slate-600">Carregando radar regulatório...</div>;
  }

  if (error || !data) {
    return <div className="rounded-md border border-rose-200 bg-rose-50 p-5 text-sm text-rose-700">Não foi possível carregar o radar. {error}</div>;
  }

  const impactedCompanies = data.alerts.reduce((total, alert) => total + alert.affectedCompaniesCount, 0);
  const diagnostics = data.diagnostics ?? [];
  const documentsFound = diagnostics.reduce((total, item) => total + item.documentsFound, 0);
  const documentsAccepted = diagnostics.reduce((total, item) => total + item.documentsAccepted, 0);

  return (
    <div className="space-y-5">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h1 className="text-2xl font-bold text-fiscal-navy">Radar fiscal e regulatório</h1>
          <p className="text-sm text-slate-500">Publicações oficiais, normas e temas legislativos cruzados com a carteira do escritório.</p>
        </div>
        <button type="button" className="btn-primary text-sm" onClick={() => loadRadar(true)} disabled={refreshing}>
          <RefreshCw size={16} className={refreshing ? 'animate-spin' : ''} />
          {refreshing ? 'Atualizando...' : 'Atualizar radar'}
        </button>
      </div>

      <section className="grid gap-4 md:grid-cols-4">
        <RadarMetric icon={Newspaper} label="Fontes monitoradas" value={data.source} />
        <RadarMetric icon={Scale} label="Documentos lidos" value={`${documentsFound} lidos / ${documentsAccepted} aceitos`} />
        <RadarMetric icon={ShieldCheck} label="Evidências salvas" value={String(data.documentsStored ?? 0)} />
        <RadarMetric icon={Sparkles} label="Cruzamentos com a carteira" value={`${impactedCompanies} agora / ${data.matchesStored ?? 0} histórico`} />
      </section>

      <section className="rounded-md border border-blue-100 bg-blue-50 p-4 text-sm text-fiscal-navy">
        <div className="flex flex-wrap items-center justify-between gap-2">
          <span>
            Radar atualizado em {new Date(data.generatedAt).toLocaleString('pt-BR')}
            {data.fromCache && data.cachedUntil ? `, usando cache válido até ${new Date(data.cachedUntil).toLocaleTimeString('pt-BR')}` : ', consultando as fontes oficiais nesta atualização'}.
          </span>
          <span className="inline-flex items-center gap-2 rounded-md bg-white px-3 py-1 text-xs font-semibold text-fiscal-blue">
            <Activity size={14} /> Motor auditável
          </span>
        </div>
        <p className="mt-2 leading-6 text-slate-600">{data.matchingModel}</p>
      </section>

      <section className="rounded-md border border-slate-200 bg-white p-5 shadow-sm">
        <h2 className="font-semibold text-fiscal-ink">Temas acompanhados</h2>
        <div className="mt-3 flex flex-wrap gap-2">
          {data.monitoredThemes.map((theme) => (
            <span key={theme} className="rounded-md bg-slate-100 px-3 py-1 text-xs font-medium text-slate-600">{theme}</span>
          ))}
        </div>
      </section>

      <section className="rounded-md border border-slate-200 bg-white p-5 shadow-sm">
        <div className="flex items-center gap-2 font-semibold text-fiscal-ink">
          <Database size={18} className="text-fiscal-blue" /> Diagnóstico das fontes
        </div>
        <div className="mt-4 grid gap-3 lg:grid-cols-3">
          {diagnostics.map((item) => (
            <div key={`${item.sourceKey}-${item.startedAt}`} className="rounded-md border border-slate-100 bg-slate-50 p-4">
              <div className="flex items-start justify-between gap-3">
                <div>
                  <div className="font-semibold text-fiscal-navy">{item.sourceName}</div>
                  <div className="mt-1 text-xs text-slate-500">{item.durationMs} ms</div>
                </div>
                <span className={`rounded-md px-2 py-1 text-xs font-semibold ${item.success ? 'bg-emerald-50 text-emerald-700' : 'bg-rose-50 text-rose-700'}`}>
                  {item.success ? 'Online' : 'Falha'}
                </span>
              </div>
              <div className="mt-3 grid grid-cols-2 gap-2 text-sm">
                <MiniStat label="Lidos" value={item.documentsFound} />
                <MiniStat label="Aceitos" value={item.documentsAccepted} />
              </div>
              <p className="mt-3 text-sm leading-6 text-slate-600">{item.error || item.statusMessage || 'Consulta registrada.'}</p>
            </div>
          ))}
        </div>
      </section>

      <section className="space-y-3">
        {data.alerts.length === 0 && (
          <div className="rounded-md border border-slate-200 bg-white p-5 text-sm text-slate-600">
            Nenhuma publicação passou pelos filtros atuais. O diagnóstico acima mostra se as fontes responderam, quantos documentos foram lidos e quantos foram aceitos pelo motor. Para ampliar a cobertura, revise os temas monitorados, aumente a janela de busca ou cadastre tags mais específicas nas empresas da carteira.
          </div>
        )}
        {data.alerts.map((alert) => (
          <article key={alert.id} className="rounded-md border border-slate-200 bg-white p-5 shadow-sm">
            <div className="flex flex-wrap items-start justify-between gap-3">
              <div>
                <div className="flex flex-wrap items-center gap-2">
                  <span className={`rounded-md px-2 py-1 text-xs font-semibold ${riskClass(alert.impactLevel)}`}>{alert.impactLevel}</span>
                  <span className="rounded-md bg-blue-50 px-2 py-1 text-xs font-medium text-fiscal-blue">{alert.theme}</span>
                  <span className="rounded-md bg-emerald-50 px-2 py-1 text-xs font-medium text-emerald-700">{alert.sourceType}</span>
                </div>
                <h2 className="mt-3 text-lg font-semibold text-fiscal-navy">{alert.title}</h2>
                <p className="mt-2 text-sm leading-6 text-slate-600">{alert.summary}</p>
              </div>
              <a className="btn-secondary text-sm" href={alert.sourceUrl} target="_blank" rel="noreferrer">
                <ExternalLink size={16} /> Página oficial
              </a>
            </div>
            <div className="mt-4 grid gap-3 md:grid-cols-3">
              <Insight label="Por que entrou no radar" value={alert.impactReason} />
              <Insight label="Impacto para o cliente" value={alert.businessImpact} />
              <Insight label="Ação para o escritório" value={alert.suggestedAction} />
            </div>
            <div className="mt-4 rounded-md bg-white p-3 ring-1 ring-slate-100">
              <div className="flex items-center gap-2 text-sm font-semibold text-fiscal-ink">
                <Building2 size={16} className="text-fiscal-blue" /> Empresas da carteira potencialmente impactadas
              </div>
              {alert.affectedCompanies.length === 0 ? (
                <p className="mt-2 text-sm text-slate-500">Nenhuma empresa da carteira cruzou diretamente com este tema pelos critérios atuais de regime, CNAE ou tags.</p>
              ) : (
                <div className="mt-3 space-y-2">
                  {alert.affectedCompanies.map((company) => (
                    <Link key={`${alert.id}-${company.id}`} to={`/app/empresas/${company.id}`} className="block rounded-md bg-slate-50 p-3 hover:bg-slate-100">
                      <div className="font-semibold text-fiscal-navy">{company.legalName}</div>
                      <div className="text-xs text-slate-500">{company.cnpj} - aderência {company.score}/100 - {company.reason}</div>
                      <div className="mt-1 text-xs text-fiscal-blue">Termos: {company.matchedTerms}</div>
                    </Link>
                  ))}
                  {alert.affectedCompaniesCount > alert.affectedCompanies.length && (
                    <p className="text-xs text-slate-500">Mais empresas podem ser afetadas; exibindo as primeiras {alert.affectedCompanies.length}.</p>
                  )}
                </div>
              )}
            </div>
            <p className="mt-3 text-xs text-slate-400">
              {alert.source} {alert.presentedAt ? `- publicado em ${new Date(alert.presentedAt).toLocaleDateString('pt-BR')}` : ''}
            </p>
          </article>
        ))}
      </section>
    </div>
  );
}

function MiniStat({ label, value }: { label: string; value: number }) {
  return (
    <div className="rounded-md bg-white p-2 ring-1 ring-slate-100">
      <div className="text-[11px] uppercase text-slate-400">{label}</div>
      <div className="font-semibold text-fiscal-navy">{value}</div>
    </div>
  );
}

function RadarMetric({ icon: Icon, label, value }: { icon: typeof Newspaper; label: string; value: string }) {
  return (
    <div className="rounded-md border border-slate-200 bg-white p-4 shadow-sm">
      <Icon className="text-fiscal-blue" />
      <div className="mt-3 text-sm text-slate-500">{label}</div>
      <div className="mt-1 font-semibold text-fiscal-navy">{value}</div>
    </div>
  );
}

function Insight({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-md bg-slate-50 p-3">
      <div className="text-xs uppercase text-slate-500">{label}</div>
      <p className="mt-1 text-sm leading-6 text-fiscal-ink">{value}</p>
    </div>
  );
}
