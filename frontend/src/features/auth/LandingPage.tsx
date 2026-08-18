import { ArrowRight, ShieldCheck } from 'lucide-react';
import { Link } from 'react-router-dom';

export function LandingPage() {
  return (
    <main className="min-h-screen bg-white">
      <section className="mx-auto grid min-h-screen max-w-6xl items-center gap-10 px-6 py-10 lg:grid-cols-[1fr_0.9fr]">
        <div>
          <img src="/fiscalguard-logo.jpeg" className="mb-8 h-20 w-auto object-contain" alt="FiscalGuard Tech" />
          <h1 className="max-w-3xl text-5xl font-bold leading-tight text-fiscal-navy">FiscalGuard Tech</h1>
          <p className="mt-5 max-w-2xl text-lg leading-8 text-slate-600">
            Plataforma SaaS para escritórios contábeis monitorarem múltiplos CNPJs, priorizarem pendências e criarem alertas preventivos sem depender de uma única fonte fiscal.
          </p>
          <div className="mt-8 flex flex-wrap gap-3">
            <Link to="/cadastro" className="btn-primary">Iniciar trial <ArrowRight size={18} /></Link>
            <Link to="/login" className="btn-secondary">Entrar</Link>
          </div>
        </div>
        <div className="rounded-md border border-slate-200 bg-fiscal-mist p-6 shadow-panel">
          <div className="mb-4 flex items-center gap-2 font-semibold text-fiscal-navy"><ShieldCheck size={20} /> Monitoramento preventivo</div>
          <div className="space-y-3">
            {['CNPJs regulares, em atenção e irregulares', 'Pendências deduplicadas com severidade', 'Trial de 14 dias com limite configurável', 'Alertas internos e e-mail preparado'].map((item) => (
              <div key={item} className="rounded-md bg-white p-4 text-sm text-slate-700">{item}</div>
            ))}
          </div>
        </div>
      </section>
    </main>
  );
}
