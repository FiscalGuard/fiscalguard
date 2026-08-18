import { FormEvent, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { api } from '../../services/api';
import { setSession } from '../../services/session';
import type { AuthResponse } from '../../types/api';

export function LoginPage() {
  const navigate = useNavigate();
  const [error, setError] = useState('');

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const form = new FormData(event.currentTarget);
    try {
      const session = await api<AuthResponse>('/api/auth/login', {
        method: 'POST',
        body: JSON.stringify({ email: form.get('email'), password: form.get('password') })
      });
      setSession(session);
      navigate('/app/dashboard');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erro ao entrar');
    }
  }

  return <AuthForm title="Entrar" cta="Entrar" error={error} onSubmit={submit} />;
}

export function AuthForm({ title, cta, error, onSubmit, register }: { title: string; cta: string; error?: string; onSubmit: (event: FormEvent<HTMLFormElement>) => void; register?: boolean }) {
  return (
    <main className="grid min-h-screen place-items-center bg-fiscal-mist px-4">
      <form onSubmit={onSubmit} className="w-full max-w-md rounded-md bg-white p-6 shadow-panel">
        <img src="/fiscalguard-logo.jpeg" className="mb-6 h-14 w-auto" alt="FiscalGuard Tech" />
        <h1 className="text-2xl font-bold text-fiscal-navy">{title}</h1>
        {error && <div className="mt-4 rounded-md bg-rose-50 p-3 text-sm text-rose-700">{error}</div>}
        <div className="mt-5 space-y-3">
          {register && <input className="field" name="responsibleName" placeholder="Nome do responsavel" required />}
          {register && <input className="field" name="organizationName" placeholder="Nome do escritorio" required />}
          <input className="field" name="email" type="email" placeholder="E-mail" required />
          <input className="field" name="password" type="password" placeholder="Senha" required minLength={8} />
          {register && <input className="field" name="phone" placeholder="Telefone opcional" />}
          {register && <input className="field" name="officeDocument" placeholder="CPF ou CNPJ do escritorio" />}
          {register && <label className="flex gap-2 text-sm text-slate-600"><input name="acceptedTerms" type="checkbox" required /> Aceito os termos de uso</label>}
        </div>
        <button className="btn-primary mt-5 w-full">{cta}</button>
      </form>
    </main>
  );
}
