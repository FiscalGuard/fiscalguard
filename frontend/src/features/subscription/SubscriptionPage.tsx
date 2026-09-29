import { useEffect, useState } from 'react';
import { api } from '../../services/api';
import type { Plan, Subscription } from '../../types/api';

export function SubscriptionPage() {
  const [plans, setPlans] = useState<Plan[]>([]);
  const [subscription, setSubscription] = useState<Subscription | null>(null);
  const [error, setError] = useState('');

  async function load() {
    try {
      const [planData, subscriptionData] = await Promise.all([
        api<Plan[]>('/api/plans'),
        api<Subscription>('/api/subscription')
      ]);
      setPlans(planData);
      setSubscription(subscriptionData);
      setError('');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erro ao carregar assinatura');
    }
  }

  useEffect(() => {
    load();
  }, []);

  async function changePlan(planCode: string) {
    try {
      await api('/api/subscription/plan', {
        method: 'PATCH',
        body: JSON.stringify({ planCode })
      });
      await load();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erro ao alterar plano');
    }
  }

  return (
    <div className="space-y-4">
      <h1 className="text-2xl font-bold text-fiscal-navy">Plano e assinatura</h1>
      {error && <div className="rounded-md bg-rose-50 p-3 text-sm text-rose-700">{error}</div>}
      {subscription && (
        <section className="rounded-md border border-slate-200 bg-white p-5 shadow-sm">
          <h2 className="font-semibold text-fiscal-navy">Assinatura atual: {subscription.planName}</h2>
          <p className="mt-2 text-sm text-slate-600">
            Status {subscription.status}. Uso: {subscription.activeCompanies}/{subscription.cnpjLimit} CNPJs e {subscription.activeUsers}/{subscription.userLimit} usuarios.
            {subscription.daysRemaining > 0 && ` Trial com ${subscription.daysRemaining} dias restantes.`}
          </p>
        </section>
      )}
      <div className="grid gap-4 md:grid-cols-4">
        {plans.map((plan) => (
          <div key={plan.code} className="rounded-md border border-slate-200 bg-white p-4">
            <h2 className="font-semibold text-fiscal-navy">{plan.name}</h2>
            <p className="mt-2 text-sm text-slate-600">{plan.cnpjLimit} CNPJs, {plan.userLimit} usuarios.</p>
            <ul className="mt-3 space-y-1 text-xs text-slate-500">
              <li>Monitoramento automatico: {plan.automatedMonitoring ? 'sim' : 'nao'}</li>
              <li>Alertas por e-mail: {plan.emailAlerts ? 'sim' : 'nao'}</li>
              <li>Historico completo: {plan.fullHistory ? 'sim' : 'nao'}</li>
            </ul>
            <button className="btn-secondary mt-4 w-full" onClick={() => changePlan(plan.code)} disabled={subscription?.planCode === plan.code}>
              {subscription?.planCode === plan.code ? 'Plano atual' : 'Selecionar'}
            </button>
          </div>
        ))}
      </div>
    </div>
  );
}
