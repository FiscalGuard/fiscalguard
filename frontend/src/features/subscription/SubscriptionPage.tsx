export function SubscriptionPage() {
  return (
    <div className="space-y-4">
      <h1 className="text-2xl font-bold text-fiscal-navy">Plano e assinatura</h1>
      <div className="grid gap-4 md:grid-cols-4">
        {['Trial', 'Gratuito', 'Profissional', 'Escritorio'].map((plan) => (
          <div key={plan} className="rounded-md border border-slate-200 bg-white p-4">
            <h2 className="font-semibold text-fiscal-navy">{plan}</h2>
            <p className="mt-2 text-sm text-slate-600">Limites centralizados na API e estrutura pronta para gateway futuro.</p>
          </div>
        ))}
      </div>
    </div>
  );
}
