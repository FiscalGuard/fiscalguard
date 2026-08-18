import { AlertTriangle, Building2, CheckCircle2, ClipboardList } from 'lucide-react';
import { useEffect, useState } from 'react';
import { api } from '../../services/api';
import type { DashboardSummary } from '../../types/api';

const fallback: DashboardSummary = { totalCompanies: 0, regularCompanies: 0, attentionCompanies: 0, irregularCompanies: 0, openIssues: 0, recentAlerts: 0, consultations: 0, planName: 'Trial', cnpjLimit: 5, daysRemaining: 14 };

export function DashboardPage() {
  const [data, setData] = useState<DashboardSummary>(fallback);
  const [error, setError] = useState('');

  useEffect(() => {
    api<DashboardSummary>('/api/dashboard').then(setData).catch((err) => setError(err.message));
  }, []);

  return (
    <div className="space-y-5">
      <div>
        <h1 className="text-2xl font-bold text-fiscal-navy">Dashboard do escritorio</h1>
        <p className="text-sm text-slate-500">Visao consolidada dos CNPJs monitorados, pendencias e uso do plano.</p>
      </div>
      {error && <div className="rounded-md bg-amber-50 p-3 text-sm text-amber-700">API indisponivel: exibindo estrutura da tela.</div>}
      <div className="grid gap-4 md:grid-cols-4">
        <Metric icon={Building2} label="CNPJs monitorados" value={`${data.totalCompanies}/${data.cnpjLimit}`} />
        <Metric icon={CheckCircle2} label="Regulares" value={data.regularCompanies} />
        <Metric icon={AlertTriangle} label="Em atencao" value={data.attentionCompanies + data.irregularCompanies} />
        <Metric icon={ClipboardList} label="Pendencias abertas" value={data.openIssues} />
      </div>
      <section className="rounded-md border border-slate-200 bg-white p-5 shadow-sm">
        <h2 className="font-semibold text-fiscal-ink">Uso do plano</h2>
        <div className="mt-3 h-3 rounded-full bg-slate-100">
          <div className="h-3 rounded-full bg-fiscal-blue" style={{ width: `${Math.min(100, (data.totalCompanies / Math.max(data.cnpjLimit, 1)) * 100)}%` }} />
        </div>
        <p className="mt-2 text-sm text-slate-600">{data.planName}: {data.daysRemaining} dias restantes no trial.</p>
      </section>
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
