import { Plus, Search, Upload } from 'lucide-react';
import { ChangeEvent, useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { EmptyState } from '../../components/EmptyState';
import { api } from '../../services/api';
import type { CompanyImportResult, CompanySummary } from '../../types/api';
import { fiscalStatusMeta, riskClass } from '../../utils/status';

export function CompaniesPage() {
  const [companies, setCompanies] = useState<CompanySummary[]>([]);
  const [search, setSearch] = useState('');
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

  return (
    <div className="space-y-4">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-bold text-fiscal-navy">Empresas</h1>
          <p className="text-sm text-slate-500">Pesquisa por CNPJ, razao social ou nome fantasia.</p>
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
          Importacao concluida: {importResult.imported} importados, {importResult.duplicated} duplicados e {importResult.invalid} invalidos em {importResult.totalRows} linhas.
        </div>
      )}
      <label className="relative block max-w-md">
        <Search className="absolute left-3 top-2.5 text-slate-400" size={18} />
        <input className="field pl-10" value={search} onChange={(event) => setSearch(event.target.value)} placeholder="Buscar empresas" />
      </label>
      {companies.length === 0 ? <EmptyState title="Nenhuma empresa encontrada" description="Cadastre o primeiro CNPJ para iniciar o monitoramento." /> : (
        <div className="overflow-hidden rounded-md border border-slate-200 bg-white">
          <table className="w-full text-left text-sm">
            <thead className="bg-slate-50 text-slate-500">
              <tr><th className="p-3">Empresa</th><th>CNPJ</th><th>Situacao</th><th>Risco</th><th>Pendencias</th></tr>
            </thead>
            <tbody>
              {companies.map((company) => {
                const meta = fiscalStatusMeta(company.fiscalStatus);
                return (
                  <tr key={company.id} className="border-t border-slate-100">
                    <td className="p-3"><Link className="font-semibold text-fiscal-navy hover:text-fiscal-blue" to={`/app/empresas/${company.id}`}>{company.legalName}</Link><div className="text-xs text-slate-500">{company.tradeName}</div></td>
                    <td>{company.cnpj}</td>
                    <td><span className={`status-dot ${meta.color}`} /> <span className="ml-2">{meta.label}</span></td>
                    <td><span className={`rounded-md px-2 py-1 text-xs font-semibold ${riskClass(company.riskLevel)}`}>{company.riskLevel} {company.riskScore}</span></td>
                    <td>{company.openIssues}</td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
