import { AlertTriangle, Building2, CheckCircle2, Plus, Search, ShieldAlert, Upload } from 'lucide-react';
import { ChangeEvent, useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { EmptyState } from '../../components/EmptyState';
import { api } from '../../services/api';
import type { CompanyImportResult, CompanySummary } from '../../types/api';
import { fiscalStatusMeta, riskClass } from '../../utils/status';

export function CompaniesPage() {
  const [companies, setCompanies] = useState<CompanySummary[]>([]);
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState<'all' | 'risk' | 'regular' | 'pending'>('all');
  const [importing, setImporting] = useState(false);
  const [importResult, setImportResult] = useState<CompanyImportResult | null>(null);

  const loadCompanies = () => {
    api<CompanySummary[]>(`/api/companies${search ? `?search=${encodeURIComponent(search)}` : ''}`).then(setCompanies).catch(() => setCompanies([]));
  };

  useEffect(() => {
    loadCompanies();
  }, [search]);

  async function importCsv(event: ChangeEvent<HTMLInputElement>) {
    const file = event.target.files?.[0];
    if (!file) return;

    const form = new FormData();
    form.append('file', file);
    setImporting(true);
    try {
      const result = await api<CompanyImportResult>('/api/companies/import', { method: 'POST', body: form });
      setImportResult(result);
      loadCompanies();
    } finally {
      setImporting(false);
      event.target.value = '';
    }
  }

  const filteredCompanies = companies.filter((company) => {
    if (statusFilter === 'regular') {
      return fiscalStatusMeta(company.fiscalStatus).label === 'Regular';
    }

    if (statusFilter === 'risk') {
      return company.riskLevel.toLowerCase().includes('alto') || company.riskLevel.toLowerCase().includes('critico');
    }

    if (statusFilter === 'pending') {
      return company.openIssues > 0;
    }

    return true;
  });

  const summary = {
    total: companies.length,
    regular: companies.filter((company) => fiscalStatusMeta(company.fiscalStatus).label === 'Regular').length,
    risk: companies.filter((company) => company.riskLevel.toLowerCase().includes('alto') || company.riskLevel.toLowerCase().includes('critico')).length,
    issues: companies.reduce((total, company) => total + company.openIssues, 0)
  };

  return (
    <div className="space-y-5">
      <div className="flex flex-wrap items-start justify-between gap-4">
        <div>
          <div className="text-xs font-semibold uppercase tracking-wide text-fiscal-blue">Carteira monitorada</div>
          <h1 className="mt-1 text-3xl font-bold text-fiscal-navy">Empresas</h1>
          <p className="mt-1 text-sm text-slate-500">Consulte, priorize e acompanhe os CNPJs sob responsabilidade do escritório.</p>
        </div>
        <div className="flex flex-wrap gap-2">
          <label className="btn-secondary cursor-pointer">
            <Upload size={18} /> {importing ? 'Importando...' : 'Importar CSV'}
            <input className="hidden" type="file" accept=".csv,text/csv" onChange={importCsv} disabled={importing} />
          </label>
          <Link className="btn-primary" to="/app/empresas/nova"><Plus size={18} /> Cadastrar CNPJ</Link>
        </div>
      </div>
      {importResult && (
        <div className="rounded-md border border-fiscal-blue/30 bg-blue-50 p-4 text-sm text-fiscal-navy">
          Importação concluída: {importResult.imported} importados, {importResult.duplicated} duplicados e {importResult.invalid} inválidos em {importResult.totalRows} linhas.
        </div>
      )}

      <section className="grid gap-3 md:grid-cols-4">
        <SummaryCard icon={Building2} label="Empresas ativas" value={summary.total} />
        <SummaryCard icon={CheckCircle2} label="Regulares" value={summary.regular} tone="emerald" />
        <SummaryCard icon={ShieldAlert} label="Alto risco" value={summary.risk} tone="rose" />
        <SummaryCard icon={AlertTriangle} label="Pendências abertas" value={summary.issues} tone="amber" />
      </section>

      <section className="rounded-md border border-slate-200 bg-white p-4 shadow-sm">
        <div className="flex flex-wrap items-center gap-3">
          <label className="relative min-w-[280px] flex-1">
            <input
              className="field py-3 pr-11"
              value={search}
              onChange={(event) => setSearch(event.target.value)}
              placeholder="Buscar por CNPJ, razão social ou nome fantasia"
            />
            <Search className="pointer-events-none absolute right-3 top-1/2 -translate-y-1/2 text-slate-400" size={18} />
          </label>
          <FilterButton active={statusFilter === 'all'} onClick={() => setStatusFilter('all')}>Todas</FilterButton>
          <FilterButton active={statusFilter === 'risk'} onClick={() => setStatusFilter('risk')}>Alto risco</FilterButton>
          <FilterButton active={statusFilter === 'pending'} onClick={() => setStatusFilter('pending')}>Com pendências</FilterButton>
          <FilterButton active={statusFilter === 'regular'} onClick={() => setStatusFilter('regular')}>Regulares</FilterButton>
        </div>
      </section>

      {companies.length === 0 ? <EmptyState title="Nenhuma empresa encontrada" description="Cadastre o primeiro CNPJ para iniciar o monitoramento." /> : (
        <div className="overflow-hidden rounded-md border border-slate-200 bg-white shadow-sm">
          <div className="border-b border-slate-100 px-5 py-4">
            <div className="font-semibold text-fiscal-navy">Carteira de empresas</div>
            <div className="text-xs text-slate-500">{filteredCompanies.length} empresa(s) exibida(s)</div>
          </div>
          <table className="w-full text-left text-sm">
            <thead className="bg-slate-50 text-xs uppercase tracking-wide text-slate-500">
              <tr><th className="p-4">Empresa</th><th>CNPJ</th><th>Situação</th><th>Risco</th><th className="text-center">Pendências</th></tr>
            </thead>
            <tbody>
              {filteredCompanies.map((company) => {
                const meta = fiscalStatusMeta(company.fiscalStatus);
                return (
                  <tr key={company.id} className="border-t border-slate-100 transition hover:bg-fiscal-mist/40">
                    <td className="p-4">
                      <Link className="font-semibold text-fiscal-navy hover:text-fiscal-blue" to={`/app/empresas/${company.id}`}>{company.legalName}</Link>
                      <div className="mt-1 max-w-2xl truncate text-xs text-slate-500">{company.tradeName || 'Sem nome fantasia informado'}</div>
                    </td>
                    <td className="font-mono text-slate-600">{formatCnpj(company.cnpj)}</td>
                    <td>
                      <span className="inline-flex items-center gap-2 rounded-md bg-slate-50 px-2.5 py-1 text-xs font-semibold text-slate-700">
                        <span className={`status-dot ${meta.color}`} /> {meta.label}
                      </span>
                    </td>
                    <td><span className={`rounded-md px-2.5 py-1 text-xs font-semibold ${riskClass(company.riskLevel)}`}>{company.riskLevel} {company.riskScore}</span></td>
                    <td className="text-center font-semibold text-fiscal-navy">{company.openIssues}</td>
                  </tr>
                );
              })}
            </tbody>
          </table>
          {filteredCompanies.length === 0 && (
            <div className="p-6 text-sm text-slate-500">Nenhuma empresa corresponde aos filtros selecionados.</div>
          )}
        </div>
      )}
    </div>
  );
}

function SummaryCard({ icon: Icon, label, value, tone = 'blue' }: { icon: typeof Building2; label: string; value: number; tone?: 'blue' | 'emerald' | 'rose' | 'amber' }) {
  const toneClass = {
    blue: 'bg-blue-50 text-fiscal-blue',
    emerald: 'bg-emerald-50 text-emerald-700',
    rose: 'bg-rose-50 text-rose-700',
    amber: 'bg-amber-50 text-amber-700'
  }[tone];

  return (
    <div className="rounded-md border border-slate-200 bg-white p-4 shadow-sm">
      <div className={`inline-flex h-9 w-9 items-center justify-center rounded-md ${toneClass}`}><Icon size={18} /></div>
      <div className="mt-3 text-2xl font-bold text-fiscal-navy">{value}</div>
      <div className="text-xs text-slate-500">{label}</div>
    </div>
  );
}

function FilterButton({ active, onClick, children }: { active: boolean; onClick: () => void; children: string }) {
  return (
    <button
      type="button"
      onClick={onClick}
      className={`rounded-md px-3 py-2 text-sm font-semibold transition ${active ? 'bg-fiscal-navy text-white' : 'bg-slate-100 text-slate-600 hover:bg-slate-200'}`}
    >
      {children}
    </button>
  );
}

function formatCnpj(value: string) {
  const digits = value.replace(/\D/g, '');
  if (digits.length !== 14) return value;
  return `${digits.slice(0, 2)}.${digits.slice(2, 5)}.${digits.slice(5, 8)}/${digits.slice(8, 12)}-${digits.slice(12)}`;
}
