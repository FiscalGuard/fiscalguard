import { Plus, Search } from 'lucide-react';
import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { EmptyState } from '../../components/EmptyState';
import { api } from '../../services/api';
import type { CompanySummary } from '../../types/api';
import { fiscalStatusMeta } from '../../utils/status';

export function CompaniesPage() {
  const [companies, setCompanies] = useState<CompanySummary[]>([]);
  const [search, setSearch] = useState('');

  useEffect(() => {
    api<CompanySummary[]>(`/api/companies${search ? `?search=${encodeURIComponent(search)}` : ''}`).then(setCompanies).catch(() => setCompanies([]));
  }, [search]);

  return (
    <div className="space-y-4">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-bold text-fiscal-navy">Empresas</h1>
          <p className="text-sm text-slate-500">Pesquisa por CNPJ, razao social ou nome fantasia.</p>
        </div>
        <Link className="btn-primary" to="/app/empresas/nova"><Plus size={18} /> Cadastrar CNPJ</Link>
      </div>
      <label className="relative block max-w-md">
        <Search className="absolute left-3 top-2.5 text-slate-400" size={18} />
        <input className="field pl-10" value={search} onChange={(event) => setSearch(event.target.value)} placeholder="Buscar empresas" />
      </label>
      {companies.length === 0 ? <EmptyState title="Nenhuma empresa encontrada" description="Cadastre o primeiro CNPJ para iniciar o monitoramento." /> : (
        <div className="overflow-hidden rounded-md border border-slate-200 bg-white">
          <table className="w-full text-left text-sm">
            <thead className="bg-slate-50 text-slate-500">
              <tr><th className="p-3">Empresa</th><th>CNPJ</th><th>Situacao</th><th>Pendencias</th></tr>
            </thead>
            <tbody>
              {companies.map((company) => {
                const meta = fiscalStatusMeta(company.fiscalStatus);
                return (
                  <tr key={company.id} className="border-t border-slate-100">
                    <td className="p-3"><Link className="font-semibold text-fiscal-navy hover:text-fiscal-blue" to={`/app/empresas/${company.id}`}>{company.legalName}</Link><div className="text-xs text-slate-500">{company.tradeName}</div></td>
                    <td>{company.cnpj}</td>
                    <td><span className={`status-dot ${meta.color}`} /> <span className="ml-2">{meta.label}</span></td>
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
