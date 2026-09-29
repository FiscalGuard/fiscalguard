import { AlertTriangle, Building2, CheckCircle2, ClipboardList, FileCheck2, Lightbulb, Newspaper, ShieldAlert } from 'lucide-react';
import { Link } from 'react-router-dom';
import { useEffect, useState } from 'react';
import { api } from '../../services/api';
import type { DashboardSummary, RegulatoryRadarSummary } from '../../types/api';
import { fiscalStatusMeta, riskClass } from '../../utils/status';

export function DashboardPage() {
  const [data, setData] = useState<DashboardSummary | null>(null);
  const [radar, setRadar] = useState<RegulatoryRadarSummary | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    api<DashboardSummary>('/api/dashboard')
      .then(setData)
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false));
    api<RegulatoryRadarSummary>('/api/regulatory-radar')
      .then(setRadar)
      .catch(() => setRadar(null));
  }, []);

  if (loading) {
    return <div className="rounded-md border border-slate-200 bg-white p-5 text-sm text-slate-600">Carregando dashboard...</div>;
  }

  if (error || !data) {
    return (
      <div className="rounded-md border border-rose-200 bg-rose-50 p-5 text-sm text-rose-700">
        Nao foi possivel carregar os dados do dashboard. {error}
      </div>
    );
  }

  return (
    <div className="space-y-5">
      <div>
        <h1 className="text-2xl font-bold text-fiscal-navy">Dashboard do escritorio</h1>
        <p className="text-sm text-slate-500">Carteira monitorada com dados publicos, enquadramento no Simples, risco fiscal e acoes recomendadas.</p>
      </div>
      <div className="grid gap-4 md:grid-cols-4">
        <Metric icon={Building2} label="CNPJs monitorados" value={`${data.totalCompanies}/${data.cnpjLimit}`} />
        <Metric icon={CheckCircle2} label="Regulares" value={data.regularCompanies} />
        <Metric icon={AlertTriangle} label="Em atenção" value={data.attentionCompanies + data.irregularCompanies} />
        <Metric icon={ClipboardList} label="Pendencias abertas" value={data.openIssues} />
      </div>
      <div className="grid gap-4 md:grid-cols-4">
        <Metric icon={ShieldAlert} label="Risco alto ou critico" value={data.highRiskCompanies + data.criticalRiskCompanies} />
        <Metric icon={FileCheck2} label="Dados publicos consolidados" value={`${data.companiesWithPublicData}/${data.totalCompanies}`} />
        <Metric icon={CheckCircle2} label="Optantes pelo Simples" value={data.simplesOptInCompanies} />
        <Metric icon={Lightbulb} label="Recomendações geradas" value={data.recommendationsGenerated} />
      </div>
      <section className="rounded-md border border-slate-200 bg-white p-5 shadow-sm">
        <h2 className="font-semibold text-fiscal-ink">Valor entregue ao escritorio</h2>
        <div className="mt-3 grid gap-3 md:grid-cols-4">
          {data.valueIndicators.map((indicator) => (
            <div key={indicator.title} className="rounded-md bg-slate-50 p-3">
              <div className="text-2xl font-bold text-fiscal-navy">{indicator.value}</div>
              <div className="mt-1 text-sm font-semibold text-fiscal-ink">{indicator.title}</div>
              <p className="mt-1 text-xs text-slate-500">{indicator.description}</p>
            </div>
          ))}
        </div>
      </section>
      <section className="rounded-md border border-slate-200 bg-white p-5 shadow-sm">
        <h2 className="font-semibold text-fiscal-ink">Uso do plano</h2>
        <div className="mt-3 h-3 rounded-full bg-slate-100">
          <div className="h-3 rounded-full bg-fiscal-blue" style={{ width: `${Math.min(100, (data.totalCompanies / Math.max(data.cnpjLimit, 1)) * 100)}%` }} />
        </div>
        <p className="mt-2 text-sm text-slate-600">{data.planName}: {data.daysRemaining} dias restantes no trial.</p>
      </section>
      <section className="rounded-md border border-slate-200 bg-white p-5 shadow-sm">
        <h2 className="font-semibold text-fiscal-ink">Empresas que pedem prioridade</h2>
        <div className="mt-3 space-y-2">
          {data.topRiskCompanies.length === 0 && <p className="text-sm text-slate-500">Nenhuma empresa consultada ainda.</p>}
          {data.topRiskCompanies.map((company) => {
            const status = fiscalStatusMeta(company.fiscalStatus);
            return (
              <Link key={company.id} to={`/app/empresas/${company.id}`} className="flex flex-wrap items-center justify-between gap-3 rounded-md bg-slate-50 p-3 text-sm hover:bg-slate-100">
                <span className="font-semibold text-fiscal-navy">{company.legalName}</span>
                <span className="text-slate-500">{company.cnpj}</span>
                <span><span className={`status-dot ${status.color}`} /> <span className="ml-2">{status.label}</span></span>
                <span className={`rounded-md px-2 py-1 text-xs font-semibold ${riskClass(company.riskLevel)}`}>{company.riskLevel} {company.riskScore}</span>
                <span className="text-slate-500">{company.openIssues} pendencias</span>
              </Link>
            );
          })}
        </div>
      </section>
      <section className="grid gap-4 md:grid-cols-3">
        <Signal title="Monitoramento cadastral" value="Ativo" description="Consulta situacao cadastral, razao social, CNAE, endereco e fonte de atualizacao." />
        <Signal title="Enquadramento publico" value={`${data.simplesOptInCompanies} optantes`} description={`${data.simplesNotOptInCompanies} empresas aparecem como nao optantes e viram prioridade de revisao.`} />
        <Signal title="Automacao preventiva" value={`${data.recentAlerts} alertas`} description="Alertas e recomendações ficam registrados para acompanhamento da equipe." />
      </section>
      {radar && radar.alerts.length > 0 && (
        <section className="rounded-md border border-slate-200 bg-white p-5 shadow-sm">
          <div className="flex flex-wrap items-center justify-between gap-3">
            <div>
              <h2 className="flex items-center gap-2 font-semibold text-fiscal-ink"><Newspaper size={18} /> Radar regulatório</h2>
              <p className="mt-1 text-sm text-slate-500">Sinais legislativos que podem afetar clientes do Simples, MEI e pequenos negocios.</p>
            </div>
            <Link className="btn-secondary text-sm" to="/app/radar">Ver radar</Link>
          </div>
          <div className="mt-3 grid gap-3 md:grid-cols-2">
            {radar.alerts.slice(0, 2).map((alert) => (
              <div key={alert.id} className="rounded-md bg-slate-50 p-3">
                <div className="flex flex-wrap items-center gap-2">
                  <span className={`rounded-md px-2 py-1 text-xs font-semibold ${riskClass(alert.impactLevel)}`}>{alert.impactLevel}</span>
                  <span className="text-xs text-slate-500">{alert.theme}</span>
                </div>
                <div className="mt-2 font-semibold text-fiscal-navy">{alert.title}</div>
                <p className="mt-1 line-clamp-2 text-sm text-slate-600">{alert.businessImpact}</p>
              </div>
            ))}
          </div>
        </section>
      )}
    </div>
  );
}

function Metric({ icon: Icon, label, value }: { icon: typeof Building2; label: string; value: number | string }) {
  return (
    <div className="rounded-md border border-slate-200 bg-white p-4 shadow-sm">
      <Icon className="mb-3 text-fiscal-blue" />
      <div className="text-2xl font-bold text-fiscal-navy">{value}</div>
      <div className="text-sm text-slate-500">{label}</div>
    </div>
  );
}

function Signal({ title, value, description }: { title: string; value: string; description: string }) {
  return (
    <div className="rounded-md border border-slate-200 bg-white p-4 shadow-sm">
      <div className="text-sm font-semibold text-fiscal-ink">{title}</div>
      <div className="mt-2 text-xl font-bold text-fiscal-navy">{value}</div>
      <p className="mt-1 text-sm text-slate-500">{description}</p>
    </div>
  );
}
