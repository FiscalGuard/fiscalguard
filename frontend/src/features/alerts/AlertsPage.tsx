import { AlertTriangle, FileWarning, RefreshCw, ShieldAlert, ShieldCheck } from 'lucide-react';
import { useEffect, useState } from 'react';
import { api } from '../../services/api';
import type { AlertItem } from '../../types/api';
import { severityClass } from '../../utils/status';

export function AlertsPage() {
  const [alerts, setAlerts] = useState<AlertItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const loadAlerts = () => {
    setLoading(true);
    api<AlertItem[]>('/api/alerts')
      .then(setAlerts)
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false));
  };

  useEffect(loadAlerts, []);

  const markRead = async (alertId: string) => {
    await api<void>(`/api/alerts/${alertId}/read`, { method: 'PATCH' });
    setAlerts((current) => current.map((alert) => alert.id === alertId ? { ...alert, isRead: true } : alert));
  };

  return (
    <div className="space-y-4">
      <div>
        <h1 className="text-2xl font-bold text-fiscal-navy">Central de alertas</h1>
        <p className="text-sm text-slate-500">Fila operacional para o escritorio agir antes que um sinal cadastral ou de enquadramento vire retrabalho.</p>
      </div>
      <section className="grid gap-3 md:grid-cols-4">
        <AlertCategory icon={ShieldAlert} title="CNPJ critico" description="Baixada, inapta, suspensa ou situacao especial." />
        <AlertCategory icon={ShieldCheck} title="Simples Nacional" description="Nao optante, MEI ou enquadramento fora do esperado." />
        <AlertCategory icon={FileWarning} title="Cadastro/CNAE" description="Dados publicos ausentes, incompletos ou que pedem revisao." />
        <AlertCategory icon={RefreshCw} title="Fonte externa" description="Falha de consulta registrada para nova tentativa e auditoria." />
      </section>
      {loading && <div className="rounded-md border border-slate-200 bg-white p-4 text-sm text-slate-600">Carregando alertas...</div>}
      {error && <div className="rounded-md border border-rose-200 bg-rose-50 p-4 text-sm text-rose-700">{error}</div>}
      {!loading && !error && alerts.length === 0 && (
        <div className="rounded-md border border-slate-200 bg-white p-4 text-sm text-slate-600">Nenhum alerta fiscal registrado.</div>
      )}
      {alerts.map((alert) => (
        <div key={alert.id} className={`rounded-md border bg-white p-4 ${alert.isRead ? 'border-slate-200' : 'border-fiscal-blue shadow-sm'}`}>
          <div className="flex flex-wrap items-start justify-between gap-3">
            <div>
              <div className="flex items-center gap-2">
                <span className={`rounded-full px-2 py-0.5 text-xs font-medium ${severityClass(alert.severity)}`}>{alert.severity}</span>
                <h2 className="font-semibold text-fiscal-ink">{alert.title}</h2>
              </div>
              <p className="mt-1 text-sm text-slate-600">{alert.message}</p>
              <p className="mt-2 text-xs text-slate-400">{new Date(alert.createdAt).toLocaleString('pt-BR')}</p>
            </div>
            {!alert.isRead && (
              <button className="btn-secondary text-sm" onClick={() => markRead(alert.id)}>
                Marcar como lido
              </button>
            )}
          </div>
        </div>
      ))}
    </div>
  );
}

function AlertCategory({ icon: Icon, title, description }: { icon: typeof AlertTriangle; title: string; description: string }) {
  return (
    <div className="rounded-md border border-slate-200 bg-white p-4 shadow-sm">
      <Icon className="text-fiscal-blue" size={18} />
      <div className="mt-2 text-sm font-semibold text-fiscal-ink">{title}</div>
      <p className="mt-1 text-xs leading-5 text-slate-500">{description}</p>
    </div>
  );
}
