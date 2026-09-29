import { useEffect, useState } from 'react';
import { api } from '../../services/api';
import type { FiscalIssue, IssueStatus } from '../../types/api';
import { severityClass } from '../../utils/status';

const statuses: IssueStatus[] = ['Open', 'InReview', 'Resolved', 'Ignored'];

export function IssuesPage() {
  const [issues, setIssues] = useState<FiscalIssue[]>([]);
  const [error, setError] = useState('');

  async function load() {
    try {
      setIssues(await api<FiscalIssue[]>('/api/issues'));
      setError('');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erro ao carregar pendencias');
    }
  }

  useEffect(() => { load(); }, []);

  async function updateStatus(id: string, status: IssueStatus) {
    try {
      await api(`/api/issues/${id}/status`, {
        method: 'PATCH',
        body: JSON.stringify({ status, notes: `Status alterado para ${status}` })
      });
      await load();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erro ao atualizar pendencia');
    }
  }

  return (
    <div className="space-y-4">
      <h1 className="text-2xl font-bold text-fiscal-navy">Pendencias</h1>
      {error && <div className="rounded-md bg-rose-50 p-3 text-sm text-rose-700">{error}</div>}
      <div className="space-y-3">
        {issues.map((issue) => (
          <article key={issue.id} className="rounded-md border border-slate-200 bg-white p-4">
            <div className="flex flex-wrap items-center justify-between gap-2">
              <h2 className="font-semibold text-fiscal-ink">{issue.title}</h2>
              <span className={`rounded-md px-2 py-1 text-xs font-semibold ${severityClass(issue.severity)}`}>{issue.severity}</span>
            </div>
            <p className="mt-1 text-sm text-slate-500">{issue.companyName} - detectada em {new Date(issue.detectedAt).toLocaleString()}</p>
            <p className="mt-3 text-sm text-slate-600">Proxima acao recomendada: revisar a origem da informacao e validar nos canais oficiais.</p>
            <div className="mt-3 flex flex-wrap gap-2">
              {statuses.map((status) => (
                <button key={status} className="btn-secondary px-3 py-1 text-xs" onClick={() => updateStatus(issue.id, status)} disabled={issue.status === status}>
                  {status}
                </button>
              ))}
            </div>
          </article>
        ))}
      </div>
    </div>
  );
}
