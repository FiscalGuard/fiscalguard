import { FormEvent, useEffect, useState } from 'react';
import { api } from '../../services/api';
import type { Organization } from '../../types/api';

export function SettingsPage() {
  const [organization, setOrganization] = useState<Organization | null>(null);
  const [error, setError] = useState('');
  const [saved, setSaved] = useState(false);

  useEffect(() => {
    api<Organization>('/api/organization').then(setOrganization).catch((err) => setError(err.message));
  }, []);

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const form = new FormData(event.currentTarget);
    try {
      const updated = await api<Organization>('/api/organization', {
        method: 'PUT',
        body: JSON.stringify({
          name: form.get('name'),
          document: form.get('document'),
          phone: form.get('phone')
        })
      });
      setOrganization(updated);
      setSaved(true);
      setError('');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erro ao salvar organizacao');
    }
  }

  return (
    <form onSubmit={submit} className="max-w-2xl rounded-md border border-slate-200 bg-white p-5 shadow-sm">
      <h1 className="text-2xl font-bold text-fiscal-navy">Configuracoes da organizacao</h1>
      <p className="mt-2 text-sm text-slate-600">Dados do escritorio usados no contexto multi-tenant.</p>
      {error && <div className="mt-4 rounded-md bg-rose-50 p-3 text-sm text-rose-700">{error}</div>}
      {saved && <div className="mt-4 rounded-md bg-emerald-50 p-3 text-sm text-emerald-700">Organizacao atualizada.</div>}
      <div className="mt-5 space-y-3">
        <input className="field" name="name" placeholder="Nome da organizacao" defaultValue={organization?.name ?? ''} required />
        <input className="field" name="document" placeholder="CPF/CNPJ" defaultValue={organization?.document ?? ''} />
        <input className="field" name="phone" placeholder="Telefone" defaultValue={organization?.phone ?? ''} />
      </div>
      <button className="btn-primary mt-5">Salvar configuracoes</button>
    </form>
  );
}
