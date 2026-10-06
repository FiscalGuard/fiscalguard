import { ArrowRight, BellRing, CheckCircle2, CircleDollarSign, Clock3, FileWarning, Radar, ShieldCheck, Sparkles, X } from 'lucide-react';
import { Link } from 'react-router-dom';

export function LandingPage() {
  return (
    <main className="min-h-screen bg-white text-fiscal-ink">
      <nav className="mx-auto flex max-w-7xl items-center justify-between px-6 py-5">
        <Link to="/" className="flex items-center gap-2">
          <img src="/fiscalguard-logo.jpeg" className="h-9 w-9 rounded-md object-cover" alt="FiscalGuard Tech" />
          <div className="leading-tight">
            <div className="text-sm font-bold text-fiscal-navy">FiscalGuard</div>
            <div className="text-[10px] font-semibold uppercase text-fiscal-blue">Tech</div>
          </div>
        </Link>
        <div className="hidden items-center gap-8 text-sm font-medium text-slate-600 md:flex">
          <a href="#problema" className="hover:text-fiscal-blue">Problema</a>
          <a href="#solucao" className="hover:text-fiscal-blue">Solução</a>
          <a href="#funcionalidades" className="hover:text-fiscal-blue">Funcionalidades</a>
          <a href="#demo" className="hover:text-fiscal-blue">Demo</a>
        </div>
        <div className="flex items-center gap-3">
          <Link to="/login" className="hidden text-sm font-semibold text-fiscal-navy hover:text-fiscal-blue sm:inline-flex">Entrar</Link>
          <Link to="/cadastro" className="btn-primary">Testar grátis <ArrowRight size={16} /></Link>
        </div>
      </nav>

      <section className="mx-auto grid max-w-7xl items-center gap-12 px-6 pb-20 pt-10 lg:grid-cols-[0.95fr_1.05fr]">
        <div>
          <div className="inline-flex items-center gap-2 rounded-md border border-emerald-100 bg-emerald-50 px-3 py-1 text-xs font-semibold text-emerald-700">
            <Sparkles size={14} /> Plataforma de inteligência fiscal para escritórios contábeis
          </div>
          <h1 className="mt-6 max-w-2xl text-5xl font-black leading-[0.95] text-fiscal-navy lg:text-6xl">
            Evite multas antes que elas cheguem até você.
          </h1>
          <p className="mt-6 max-w-xl text-lg leading-8 text-slate-600">
            O FiscalGuard monitora CNPJs, cruza dados públicos, identifica sinais de risco e transforma pendências em ações claras para o contador agir no momento certo.
          </p>
          <div className="mt-8 flex flex-wrap gap-3">
            <Link to="/cadastro" className="btn-primary px-5 py-3">Testar grátis por 14 dias <ArrowRight size={17} /></Link>
            <a href="#solucao" className="btn-secondary px-5 py-3">Ver como funciona</a>
          </div>
        </div>

        <div className="relative" id="demo">
          <div className="rounded-md bg-[#101a42] p-4 shadow-panel">
            <div className="mb-4 flex items-center gap-2 text-xs text-slate-300">
              <span className="h-2 w-2 rounded-full bg-rose-400" />
              <span className="h-2 w-2 rounded-full bg-amber-300" />
              <span className="h-2 w-2 rounded-full bg-emerald-400" />
              <span className="ml-3">app.fiscalguard.tech / painel</span>
            </div>
            <div className="rounded-md bg-white p-5">
              <div className="grid gap-4 md:grid-cols-[0.8fr_1.2fr]">
                <aside className="rounded-md bg-fiscal-mist p-4 text-sm text-fiscal-navy">
                  <div className="font-bold">Painel</div>
                  <div className="mt-4 space-y-3 text-slate-500">
                    <div>CNPJs</div>
                    <div>Alertas</div>
                    <div>Radar</div>
                    <div>Relatórios</div>
                  </div>
                </aside>
                <section>
                  <div className="grid grid-cols-3 gap-3">
                    <Metric label="CNPJs" value="128" />
                    <Metric label="Alertas" value="42" />
                    <Metric label="Risco alto" value="17" danger />
                  </div>
                  <div className="mt-4 rounded-md border border-slate-100 p-4">
                    <div className="mb-3 flex items-center justify-between">
                      <div className="font-bold text-fiscal-navy">Conformidade da carteira</div>
                      <span className="rounded-md bg-emerald-50 px-2 py-1 text-xs font-semibold text-emerald-700">Atualizado</span>
                    </div>
                    <div className="h-2 rounded-full bg-slate-100">
                      <div className="h-2 w-[72%] rounded-full bg-fiscal-blue" />
                    </div>
                    <div className="mt-4 space-y-2">
                      <RiskLine title="DAS em aberto vence em 3 dias" tone="amber" />
                      <RiskLine title="Situação cadastral baixada" tone="rose" />
                      <RiskLine title="Radar Receita Federal publicado" tone="blue" />
                    </div>
                  </div>
                </section>
              </div>
            </div>
          </div>
          <div className="absolute -right-4 top-16 hidden rounded-md bg-white p-4 shadow-panel lg:block">
            <div className="text-xs text-slate-400">DAS - 20/11</div>
            <div className="mt-1 text-xl font-bold text-fiscal-navy">Baixo</div>
          </div>
        </div>
      </section>

      <section id="problema" className="bg-[#f3f6fa] px-6 py-20">
        <div className="mx-auto max-w-6xl text-center">
          <Pill>O problema</Pill>
          <h2 className="mx-auto mt-5 max-w-3xl text-3xl font-black leading-tight text-fiscal-navy md:text-4xl">
            Pequenas empresas descobrem o problema fiscal tarde demais.
          </h2>
          <p className="mx-auto mt-4 max-w-2xl text-slate-600">
            A rotina fiscal chega fragmentada: Receita, Simples, prefeitura, e-mail, planilhas e prazos. Quando aparece, muitas vezes já virou multa, bloqueio ou exclusão.
          </p>
          <div className="mt-10 grid gap-4 md:grid-cols-4">
            <ProblemCard icon={CircleDollarSign} title="Multas inesperadas" text="Pequenas falhas viram autos de infração e custo direto para o cliente." />
            <ProblemCard icon={FileWarning} title="Pendências invisíveis" text="O escritório só descobre quando o cliente já precisa resolver rápido." />
            <ProblemCard icon={X} title="Rotina reativa" text="O contador apaga incêndios em vez de orientar preventivamente." />
            <ProblemCard icon={Clock3} title="Prazos perdidos" text="Obrigações e mudanças oficiais se espalham por fontes diferentes." />
          </div>
        </div>
      </section>

      <section id="solucao" className="px-6 py-20">
        <div className="mx-auto max-w-6xl text-center">
          <Pill>A solução</Pill>
          <h2 className="mx-auto mt-5 max-w-3xl text-3xl font-black leading-tight text-fiscal-navy md:text-4xl">
            Um copiloto fiscal que age antes que o problema aconteça.
          </h2>
          <p className="mx-auto mt-4 max-w-2xl text-slate-600">
            O FiscalGuard transforma monitoramento fiscal em uma operação clara: cadastrar, consultar, classificar risco, orientar e alertar.
          </p>
          <div className="mt-10 grid gap-4 md:grid-cols-4">
            <SolutionCard step="1" icon={ShieldCheck} title="Conecte sua carteira" text="Importe CNPJs e organize clientes por escritório." />
            <SolutionCard step="2" icon={Radar} title="Monitore 24/7" text="Dados públicos, Receita, Simples e radar regulatório." />
            <SolutionCard step="3" icon={BellRing} title="Receba alertas" text="Priorização por severidade, impacto e ação sugerida." />
            <SolutionCard step="4" icon={CheckCircle2} title="Resolva com clareza" text="Histórico, evidências e relatório para o cliente." />
          </div>
        </div>
      </section>

      <section id="funcionalidades" className="bg-[#f3f6fa] px-6 py-20">
        <div className="mx-auto grid max-w-6xl gap-6 lg:grid-cols-[1.1fr_0.9fr]">
          <div className="rounded-md bg-[#101a42] p-8 text-white">
            <h2 className="text-2xl font-black">Monitoramento contínuo de CNPJ</h2>
            <p className="mt-3 max-w-xl text-slate-300">Acompanhe situação cadastral, Simples/MEI, CNAE, consultas, pendências e mudanças oficiais em um painel único.</p>
            <div className="mt-8 grid gap-4 sm:grid-cols-2">
              {['Status do CNPJ e regime tributário', 'Alertas por severidade', 'Histórico de consultas', 'Radar Receita e Câmara'].map((item) => (
                <div key={item} className="rounded-md border border-white/10 bg-white/5 p-4 text-sm text-slate-200">{item}</div>
              ))}
            </div>
          </div>
          <div className="grid gap-4">
            <Feature title="Alertas inteligentes" text="Notificações por risco fiscal, fonte pública, radar oficial e prioridade de atendimento." />
            <Feature title="Riscos previstos" text="Score fiscal que ajuda o escritório a saber onde agir primeiro." />
            <Feature title="Relatórios executivos" text="Uma leitura simples para explicar ao cliente o que aconteceu e qual ação tomar." />
          </div>
        </div>
      </section>
    </main>
  );
}

function Metric({ label, value, danger }: { label: string; value: string; danger?: boolean }) {
  return (
    <div className="rounded-md bg-slate-50 p-3">
      <div className={`text-lg font-black ${danger ? 'text-rose-600' : 'text-fiscal-navy'}`}>{value}</div>
      <div className="text-xs text-slate-500">{label}</div>
    </div>
  );
}

function RiskLine({ title, tone }: { title: string; tone: 'amber' | 'rose' | 'blue' }) {
  const color = tone === 'rose' ? 'bg-rose-500' : tone === 'amber' ? 'bg-amber-400' : 'bg-fiscal-blue';
  return <div className="flex items-center gap-2 rounded-md bg-slate-50 p-2 text-xs text-slate-600"><span className={`h-2 w-2 rounded-full ${color}`} />{title}</div>;
}

function Pill({ children }: { children: string }) {
  return <span className="inline-flex rounded-md border border-emerald-100 bg-emerald-50 px-3 py-1 text-xs font-bold uppercase text-emerald-700">{children}</span>;
}

function ProblemCard({ icon: Icon, title, text }: { icon: typeof CircleDollarSign; title: string; text: string }) {
  return (
    <div className="rounded-md border border-slate-200 bg-white p-5 text-left shadow-sm">
      <div className="inline-flex h-9 w-9 items-center justify-center rounded-md bg-amber-50 text-amber-600"><Icon size={18} /></div>
      <h3 className="mt-4 font-bold text-fiscal-navy">{title}</h3>
      <p className="mt-2 text-sm leading-6 text-slate-600">{text}</p>
    </div>
  );
}

function SolutionCard({ step, icon: Icon, title, text }: { step: string; icon: typeof ShieldCheck; title: string; text: string }) {
  return (
    <div className="rounded-md border border-slate-200 bg-white p-5 text-left shadow-sm">
      <div className="inline-flex h-9 w-9 items-center justify-center rounded-md bg-fiscal-navy text-white"><Icon size={17} /></div>
      <h3 className="mt-4 font-bold text-fiscal-navy">{step}. {title}</h3>
      <p className="mt-2 text-sm leading-6 text-slate-600">{text}</p>
    </div>
  );
}

function Feature({ title, text }: { title: string; text: string }) {
  return (
    <div className="rounded-md border border-slate-200 bg-white p-6 shadow-sm">
      <h3 className="font-bold text-fiscal-navy">{title}</h3>
      <p className="mt-2 text-sm leading-6 text-slate-600">{text}</p>
    </div>
  );
}
