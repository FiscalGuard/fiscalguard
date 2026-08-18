import { RefreshCw } from 'lucide-react';
import { useParams } from 'react-router-dom';
import { api } from '../../services/api';

export function CompanyDetailsPage({ mode }: { mode?: 'history' }) {
  const { id } = useParams();

  async function consult() {
    if (!id) return;
    await api(`/api/companies/${id}/consultations`, { method: 'POST' });
    window.location.reload();
  }

  return (
    <div className="space-y-4">
      <h1 className="text-2xl font-bold text-fiscal-navy">{mode === 'history' ? 'Historico de consultas' : 'Detalhes da empresa'}</h1>
      <div className="rounded-md border border-slate-200 bg-white p-5">
        <p className="text-sm text-slate-600">Tela preparada para consolidar dados cadastrais, ultima consulta, pendencias, origem da informacao e historico.</p>
        {id && <button className="btn-primary mt-4" onClick={consult}><RefreshCw size={18} /> Executar consulta manual</button>}
      </div>
    </div>
  );
}
