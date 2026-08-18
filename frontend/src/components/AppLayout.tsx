import { Bell, Building2, CreditCard, Gauge, LogOut, Settings, ShieldCheck, Users, ClipboardList } from 'lucide-react';
import { NavLink, Outlet, useNavigate } from 'react-router-dom';
import { clearSession, getSession } from '../services/session';

const items = [
  { to: '/app/dashboard', label: 'Dashboard', icon: Gauge },
  { to: '/app/empresas', label: 'Empresas', icon: Building2 },
  { to: '/app/pendencias', label: 'Pendencias', icon: ClipboardList },
  { to: '/app/alertas', label: 'Alertas', icon: Bell },
  { to: '/app/usuarios', label: 'Usuarios', icon: Users },
  { to: '/app/assinatura', label: 'Plano', icon: CreditCard },
  { to: '/app/configuracoes', label: 'Configuracoes', icon: Settings }
];

export function AppLayout() {
  const navigate = useNavigate();
  const session = getSession();

  return (
    <div className="min-h-screen bg-slate-50">
      <aside className="fixed inset-y-0 left-0 hidden w-64 border-r border-slate-200 bg-white px-4 py-5 md:block">
        <div className="mb-8 flex items-center gap-3">
          <img src="/fiscalguard-logo.jpeg" className="h-10 w-10 rounded-md object-cover" alt="FiscalGuard Tech" />
          <div>
            <strong className="block text-fiscal-navy">FiscalGuard</strong>
            <span className="text-sm text-fiscal-blue">Tech</span>
          </div>
        </div>
        <nav className="space-y-1">
          {items.map((item) => (
            <NavLink key={item.to} to={item.to} className={({ isActive }) => `flex items-center gap-3 rounded-md px-3 py-2 text-sm font-medium ${isActive ? 'bg-fiscal-mist text-fiscal-navy' : 'text-slate-600 hover:bg-slate-100'}`}>
              <item.icon size={18} />
              {item.label}
            </NavLink>
          ))}
        </nav>
      </aside>
      <main className="md:pl-64">
        <header className="sticky top-0 z-10 flex h-16 items-center justify-between border-b border-slate-200 bg-white/95 px-5 backdrop-blur">
          <div className="flex items-center gap-2 text-sm text-slate-600">
            <ShieldCheck size={18} className="text-fiscal-blue" />
            Informacoes de carater informativo; consulte sempre os canais oficiais.
          </div>
          <div className="flex items-center gap-3">
            <span className="text-sm font-medium text-fiscal-ink">{session?.name ?? 'Usuario'}</span>
            <button className="btn-secondary px-3" onClick={() => { clearSession(); navigate('/login'); }} title="Sair">
              <LogOut size={16} />
            </button>
          </div>
        </header>
        <section className="p-5">
          <Outlet />
        </section>
      </main>
    </div>
  );
}
