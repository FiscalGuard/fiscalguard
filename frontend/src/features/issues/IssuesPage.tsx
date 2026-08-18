import { useEffect, useState } from 'react';
import { api } from '../../services/api';
import type { FiscalIssue } from '../../types/api';
import { severityClass } from '../../utils/status';

export function IssuesPage() {
  const [issues, setIssues] = useState<FiscalIssue[]>([]);
  useEffect(() => { api<FiscalIssue[]>('/api/issues').then(setIssues).catch(() => setIssues([])); }, []);
  return (
    <div className="space-y-4">
      <h1 className="text-2xl font-bold text-fiscal-navy">Pendencias</h1>
      <div className="space-y-3">
        {issues.map((issue) => (
          <article key={issue.id} className="rounded-md border border-slate-200 bg-white p-4">
            <div className="flex flex-wrap items-center justify-between gap-2">
              <h2 className="font-semibold text-fiscal-ink">{issue.title}</h2>
              <span className={`rounded-md px-2 py-1 text-xs font-semibold ${severityClass(issue.severity)}`}>{issue.severity}</span>
            </div>
            <p className="mt-1 text-sm text-slate-500">{issue.companyName} - detectada em {new Date(issue.detectedAt).toLocaleString()}</p>
            <p className="mt-3 text-sm text-slate-600">Proxima acao recomendada: revisar a origem da informacao e validar nos canais oficiais.</p>
          </article>
        ))}
      </div>
    </div>
  );
}
