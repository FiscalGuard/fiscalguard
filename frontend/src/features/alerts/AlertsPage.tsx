import { useEffect, useState } from 'react';
import { api } from '../../services/api';
import type { AlertItem } from '../../types/api';

export function AlertsPage() {
  const [alerts, setAlerts] = useState<AlertItem[]>([]);
  useEffect(() => { api<AlertItem[]>('/api/alerts').then(setAlerts).catch(() => setAlerts([])); }, []);
  return (
    <div className="space-y-4">
      <h1 className="text-2xl font-bold text-fiscal-navy">Central de alertas</h1>
      {alerts.map((alert) => (
        <div key={alert.id} className="rounded-md border border-slate-200 bg-white p-4">
          <h2 className="font-semibold text-fiscal-ink">{alert.title}</h2>
          <p className="text-sm text-slate-600">{alert.message}</p>
        </div>
      ))}
    </div>
  );
}
