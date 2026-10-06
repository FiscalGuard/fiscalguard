import React from 'react';
import ReactDOM from 'react-dom/client';
import { Navigate, RouterProvider, createBrowserRouter } from 'react-router-dom';
import { AppLayout } from './components/AppLayout';
import { LandingPage } from './features/auth/LandingPage';
import { LoginPage } from './features/auth/LoginPage';
import { RegisterPage } from './features/auth/RegisterPage';
import { DashboardPage } from './features/dashboard/DashboardPage';
import { CompaniesPage } from './features/companies/CompaniesPage';
import { CompanyFormPage } from './features/companies/CompanyFormPage';
import { CompanyDetailsPage } from './features/companies/CompanyDetailsPage';
import { IssuesPage } from './features/issues/IssuesPage';
import { AlertsPage } from './features/alerts/AlertsPage';
import { RegulatoryRadarPage } from './features/radar/RegulatoryRadarPage';
import { UsersPage } from './features/users/UsersPage';
import { SubscriptionPage } from './features/subscription/SubscriptionPage';
import { SettingsPage } from './features/settings/SettingsPage';
import './styles.css';

const router = createBrowserRouter([
  { path: '/', element: <LandingPage /> },
  { path: '/login', element: <LoginPage /> },
  { path: '/cadastro', element: <RegisterPage /> },
  {
    path: '/app',
    element: <AppLayout />,
    children: [
      { index: true, element: <Navigate to="/app/dashboard" replace /> },
      { path: 'dashboard', element: <DashboardPage /> },
      { path: 'empresas', element: <CompaniesPage /> },
      { path: 'empresas/nova', element: <CompanyFormPage /> },
      { path: 'empresas/:id/editar', element: <CompanyFormPage /> },
      { path: 'empresas/:id', element: <CompanyDetailsPage /> },
      { path: 'consultas', element: <CompanyDetailsPage mode="history" /> },
      { path: 'pendencias', element: <IssuesPage /> },
      { path: 'alertas', element: <AlertsPage /> },
      { path: 'radar', element: <RegulatoryRadarPage /> },
      { path: 'usuarios', element: <UsersPage /> },
      { path: 'assinatura', element: <SubscriptionPage /> },
      { path: 'configuracoes', element: <SettingsPage /> },
      { path: 'acesso-negado', element: <StatePage title="Acesso negado" /> },
      { path: '*', element: <StatePage title="Pagina nao encontrada" /> }
    ]
  },
  { path: '*', element: <StatePage title="Pagina nao encontrada" /> }
]);

function StatePage({ title }: { title: string }) {
  return <div className="p-8 text-fiscal-ink">{title}</div>;
}

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <RouterProvider router={router} />
  </React.StrictMode>
);
