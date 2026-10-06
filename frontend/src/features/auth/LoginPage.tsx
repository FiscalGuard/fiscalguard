import { FormEvent, type InputHTMLAttributes, useState } from 'react';
import { ArrowRight, CheckCircle2, LockKeyhole, Mail, ShieldCheck, type LucideIcon } from 'lucide-react';
import { Link, useNavigate } from 'react-router-dom';
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

  return <AuthForm title="Bem-vindo de volta" subtitle="Acesse o painel para monitorar sua carteira de empresas." cta="Entrar" error={error} onSubmit={submit} />;
}

export function AuthForm({ title, subtitle, cta, error, onSubmit, register }: { title: string; subtitle: string; cta: string; error?: string; onSubmit: (event: FormEvent<HTMLFormElement>) => void; register?: boolean }) {
  return (
    <main className="grid min-h-screen bg-white lg:grid-cols-[1.05fr_0.95fr]">
      <section className="relative hidden overflow-hidden bg-fiscal-navy px-12 py-10 text-white lg:flex lg:flex-col lg:justify-between">
        <div className="absolute inset-0 opacity-25" style={{ backgroundImage: 'radial-gradient(circle at 1px 1px, rgba(255,255,255,.9) 1px, transparent 0)', backgroundSize: '36px 36px' }} />
        <div className="relative z-10 flex items-center gap-2">
          <img src="/fiscalguard-logo.jpeg" className="h-10 w-10 rounded-md object-cover" alt="FiscalGuard Tech" />
          <div className="leading-tight">
            <div className="font-bold">FiscalGuard</div>
            <div className="text-[10px] font-semibold uppercase text-blue-200">Tech</div>
          </div>
        </div>

        <div className="relative z-10 mx-auto max-w-xl">
          <h1 className="text-5xl font-black leading-tight">Monitoramento fiscal preventivo e inteligente.</h1>
          <p className="mt-6 text-lg leading-8 text-blue-100">
            Integração com dados públicos, análise automática de riscos e alertas para que nenhuma empresa da carteira seja pega de surpresa.
          </p>
          <ul className="mt-7 space-y-3 text-blue-50">
            <AuthBullet>Detecção de irregularidades cadastrais</AuthBullet>
            <AuthBullet>Priorização por score e severidade</AuthBullet>
            <AuthBullet>Radar oficial da Receita, Simples e Câmara</AuthBullet>
          </ul>
        </div>

        <div className="relative z-10 text-sm text-blue-100">© 2026 FiscalGuard Tech</div>
      </section>

      <section className="flex min-h-screen items-center justify-center px-6 py-10">
        <div className="w-full max-w-xl">
          <div className="mb-10 flex items-center justify-between lg:hidden">
            <Link to="/" className="flex items-center gap-2">
              <img src="/fiscalguard-logo.jpeg" className="h-10 w-10 rounded-md object-cover" alt="FiscalGuard Tech" />
              <span className="font-bold text-fiscal-navy">FiscalGuard Tech</span>
            </Link>
            <Link to="/" className="text-sm font-semibold text-slate-500">Início</Link>
          </div>

          <div>
            <div className="inline-flex items-center gap-2 rounded-md bg-emerald-50 px-3 py-1 text-xs font-semibold text-emerald-700">
              <ShieldCheck size={14} /> Ambiente seguro
            </div>
            <h1 className="mt-8 text-4xl font-black text-fiscal-navy">{title}</h1>
            <p className="mt-3 text-base leading-7 text-slate-600">{subtitle}</p>
          </div>

          {error && <div className="mt-6 rounded-md border border-rose-100 bg-rose-50 p-3 text-sm text-rose-700">{error}</div>}

          <form onSubmit={onSubmit} className="mt-8 space-y-5">
            {register && <LabeledField name="responsibleName" label="Responsável" placeholder="Nome do responsável" required />}
            {register && <LabeledField name="organizationName" label="Escritório" placeholder="Nome do escritório contábil" required />}
            <LabeledField name="email" label="E-mail corporativo" type="email" placeholder="fiscalguard@escritorio.com.br" icon={Mail} required />
            <LabeledField name="password" label="Senha" type="password" placeholder={register ? 'Mínimo de 8 caracteres' : 'Digite sua senha'} icon={LockKeyhole} required minLength={8} />
            {register && <LabeledField name="phone" label="Telefone" placeholder="Opcional" />}
            {register && <LabeledField name="officeDocument" label="CPF ou CNPJ" placeholder="Documento do escritório" />}
            {register && (
              <label className="flex items-start gap-3 rounded-md bg-slate-50 p-3 text-sm leading-6 text-slate-600">
                <input name="acceptedTerms" type="checkbox" required className="mt-1" />
                Aceito os termos de uso e confirmo que as consultas serão usadas para fins informativos e preventivos.
              </label>
            )}
            {!register && <div className="text-right"><button type="button" className="text-xs font-semibold text-fiscal-blue">Esqueci minha senha</button></div>}
            <button className="btn-primary w-full py-3 text-base">
              {cta} <ArrowRight size={18} />
            </button>
          </form>

          <div className="mt-8 flex flex-wrap items-center justify-between gap-3 text-sm text-slate-500">
            {register ? (
              <>
                <span>Já tem conta?</span>
                <Link to="/login" className="font-semibold text-fiscal-blue hover:text-fiscal-navy">Entrar</Link>
              </>
            ) : (
              <>
                <span>Ainda não tem acesso?</span>
                <Link to="/cadastro" className="font-semibold text-fiscal-blue hover:text-fiscal-navy">Iniciar demonstração</Link>
              </>
            )}
          </div>
        </div>
      </section>
    </main>
  );
}

function AuthBullet({ children }: { children: string }) {
  return <li className="flex items-center gap-3"><CheckCircle2 size={18} className="text-blue-200" />{children}</li>;
}

function LabeledField({ label, icon: Icon, ...props }: InputHTMLAttributes<HTMLInputElement> & { label: string; icon?: LucideIcon }) {
  return (
    <label className="block">
      <span className="text-sm font-semibold text-fiscal-ink">{label}</span>
      <span className="relative mt-2 block">
        <input className={`field py-3 shadow-[0_8px_20px_rgba(15,23,42,0.08)] ${Icon ? 'pr-10' : ''}`} {...props} />
        {Icon && <Icon size={18} className="pointer-events-none absolute right-3 top-1/2 -translate-y-1/2 text-slate-500" />}
      </span>
    </label>
  );
}
