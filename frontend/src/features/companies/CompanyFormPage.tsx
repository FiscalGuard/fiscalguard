import { FormEvent, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { api } from '../../services/api';

export function CompanyFormPage() {
  const navigate = useNavigate();
  const [error, setError] = useState('');

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const form = new FormData(event.currentTarget);
    try {
      await api('/api/companies', {
        method: 'POST',
        body: JSON.stringify({
          legalName: form.get('legalName'),
          tradeName: form.get('tradeName'),
          cnpj: form.get('cnpj'),
          stateRegistration: form.get('stateRegistration'),
          taxRegime: Number(form.get('taxRegime')),
          email: form.get('email'),
          phone: form.get('phone'),
          responsibleName: form.get('responsibleName'),
          notes: form.get('notes'),
          tags: form.get('tags'),
          monitoringFrequency: Number(form.get('monitoringFrequency'))
        })
      });
      navigate('/app/empresas');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erro ao cadastrar empresa');
    }
  }

  return (
    <form onSubmit={submit} className="max-w-3xl space-y-4 rounded-md border border-slate-200 bg-white p-5 shadow-sm">
      <h1 className="text-2xl font-bold text-fiscal-navy">Cadastrar empresa</h1>
      {error && <div className="rounded-md bg-rose-50 p-3 text-sm text-rose-700">{error}</div>}
      <div className="grid gap-3 md:grid-cols-2">
        <input className="field" name="legalName" placeholder="Razao social" required />
        <input className="field" name="tradeName" placeholder="Nome fantasia" />
        <input className="field" name="cnpj" placeholder="CNPJ" required />
        <input className="field" name="stateRegistration" placeholder="Inscricao estadual" />
        <select className="field" name="taxRegime" defaultValue="1"><option value="1">Simples Nacional</option><option value="2">Lucro Presumido</option><option value="3">Lucro Real</option><option value="4">MEI</option></select>
        <select className="field" name="monitoringFrequency" defaultValue="7"><option value="0">Manual</option><option value="1">Diario</option><option value="7">Semanal</option><option value="30">Mensal</option></select>
        <input className="field" name="email" type="email" placeholder="E-mail" />
        <input className="field" name="phone" placeholder="Telefone" />
        <input className="field" name="responsibleName" placeholder="Responsavel" />
        <input className="field" name="tags" placeholder="Tags" />
      </div>
      <textarea className="field min-h-24" name="notes" placeholder="Observacoes" />
      <button className="btn-primary">Salvar empresa</button>
    </form>
  );
}
