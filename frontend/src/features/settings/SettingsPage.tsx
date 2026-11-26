
import { FormEvent, useEffect, useState } from 'react';
import {
  AlertCircle,
  Building2,
  CheckCircle2,
  ChevronRight,
  LockKeyhole,
  RefreshCw,
  Save,
  ShieldCheck,
  UserRound,
  RotateCcw,
  Info
} from 'lucide-react';

import { api } from '../../services/api';
import { getSession } from '../../services/session';
import type { Organization } from '../../types/api';

type Tab = 'organization' | 'profile' | 'security';

type FormValues = {
  name: string;
  document: string;
  phone: string;
};

const emptyForm: FormValues = {
  name: '',
  document: '',
  phone: ''
};

export function SettingsPage() {
  const [tab, setTab] = useState<Tab>('organization');
  const [organization, setOrganization] =
    useState<Organization | null>(null);
  const [values, setValues] = useState<FormValues>(emptyForm);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  const [saved, setSaved] = useState(false);

  const session = getSession();

  async function loadOrganization() {
    setLoading(true);
    setError('');

    try {
      const result = await api<Organization>('/api/organization');

      setOrganization(result);
      setValues({
        name: result.name ?? '',
        document: result.document ?? '',
        phone: result.phone ?? ''
      });
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : 'Não foi possível carregar a organização.'
      );
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void loadOrganization();
  }, []);

  const changed = organization !== null && (
    values.name.trim() !== (organization.name ?? '') ||
    values.document.trim() !== (organization.document ?? '') ||
    values.phone.trim() !== (organization.phone ?? '')
  );

  function resetChanges() {
    if (!organization) return;

    setValues({
      name: organization.name ?? '',
      document: organization.document ?? '',
      phone: organization.phone ?? ''
    });

    setSaved(false);
    setError('');
  }

  function update(field: keyof FormValues, value: string) {
    setValues(current => ({
      ...current,
      [field]: value
    }));

    setSaved(false);
    setError('');
  }

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();

    if (
      saving ||
      loading ||
      !organization ||
      !changed ||
      !values.name.trim()
    ) return;

    setSaving(true);
    setSaved(false);
    setError('');

    try {
      const updated = await api<Organization>('/api/organization', {
        method: 'PUT',
        body: JSON.stringify({
          name: values.name.trim(),
          document: values.document.trim(),
          phone: values.phone.trim()
        })
      });

      setOrganization(updated);

      setValues({
        name: updated.name ?? '',
        document: updated.document ?? '',
        phone: updated.phone ?? ''
      });

      setSaved(true);
    } catch (err) {
      setError(
        err instanceof Error
          ? err.message
          : 'Não foi possível salvar as configurações.'
      );
    } finally {
      setSaving(false);
    }
  }

  const tabs = [
    {
      id: 'organization' as const,
      label: 'Organização',
      icon: Building2
    },
    {
      id: 'profile' as const,
      label: 'Meu perfil',
      icon: UserRound
    },
    {
      id: 'security' as const,
      label: 'Segurança',
      icon: LockKeyhole
    }
  ];

  return (
    <div className="mx-auto max-w-6xl space-y-6 pb-10">

      {/* CABEÇALHO */}
      <div className="flex flex-wrap items-start justify-between gap-4">
        <div>
          <div className="mb-2 flex items-center gap-2 text-xs font-medium text-slate-500">
            <span>Painel</span>
            <ChevronRight size={13} />
            <span className="text-fiscal-blue">
              Configurações
            </span>
          </div>

          <h1 className="text-2xl font-bold tracking-tight text-fiscal-navy sm:text-3xl">
            Configurações
          </h1>

          <p className="mt-2 text-sm text-slate-500">
            Gerencie as informações e os dados da sua conta FiscalGuard.
          </p>
        </div>

        <div className="flex items-center gap-2 rounded-full border border-sky-100 bg-fiscal-mist px-3 py-2 text-xs font-semibold text-fiscal-navy">
          <ShieldCheck size={15} className="text-fiscal-blue" />
          Ambiente FiscalGuard
        </div>
      </div>

      {/* CONTAINER PRINCIPAL */}
      <div className="overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-sm">

        {/* ABAS */}
        <div className="border-b border-slate-200 px-4 sm:px-7">
          <div
            role="tablist"
            aria-label="Seções de configurações"
            className="flex gap-2 overflow-x-auto"
          >
            {tabs.map(item => (
              <button
                key={item.id}
                type="button"
                role="tab"
                id={`tab-${item.id}`}
                aria-selected={tab === item.id}
                aria-controls={`panel-${item.id}`}
                onClick={() => setTab(item.id)}
                className={`flex shrink-0 items-center gap-2 border-b-2 px-3 py-5 text-sm font-semibold transition-colors ${
                  tab === item.id
                    ? 'border-fiscal-blue text-fiscal-navy'
                    : 'border-transparent text-slate-500 hover:border-slate-200 hover:text-fiscal-navy'
                }`}
              >
                <item.icon size={17} />
                {item.label}
              </button>
            ))}
          </div>
        </div>

        {/* ABA ORGANIZAÇÃO */}
        {tab === 'organization' && (
          <section
            role="tabpanel"
            id="panel-organization"
            aria-labelledby="tab-organization"
            className="p-5 sm:p-8"
          >
            <div className="mb-7 flex items-start gap-4">
              <div className="rounded-xl bg-fiscal-mist p-3 text-fiscal-blue">
                <Building2 size={24} />
              </div>

              <div>
                <h2 className="text-lg font-bold text-fiscal-navy">
                  Dados da organização
                </h2>

                <p className="mt-1 text-sm text-slate-500">
                  Mantenha os dados cadastrais do seu escritório atualizados.
                </p>
              </div>
            </div>

            {error && (
              <div
                role="alert"
                className="mb-5 flex items-start gap-2 rounded-lg border border-rose-200 bg-rose-50 p-3 text-sm text-rose-700"
              >
                <AlertCircle size={18} className="shrink-0" />
                {error}
              </div>
            )}

            {saved && (
              <div
                role="status"
                className="mb-5 flex items-center gap-2 rounded-lg border border-emerald-200 bg-emerald-50 p-3 text-sm text-emerald-700"
              >
                <CheckCircle2 size={18} />
                Organização atualizada com sucesso.
              </div>
            )}

            {loading ? (
              <div
                role="status"
                className="flex items-center gap-3 py-12 text-sm text-slate-500"
              >
                <RefreshCw size={18} className="animate-spin" />
                Carregando informações...
              </div>
            ) : !organization ? (
              <button
                type="button"
                onClick={() => void loadOrganization()}
                className="btn-secondary inline-flex items-center gap-2"
              >
                <RefreshCw size={16} />
                Tentar novamente
              </button>
            ) : (
              <form onSubmit={submit} className="space-y-6">

                {/* INFORMAÇÕES */}
                <div className="rounded-xl border border-sky-100 bg-sky-50/60 p-4">
                  <div className="flex items-start gap-3">
                    <Info
                      size={19}
                      className="mt-0.5 shrink-0 text-fiscal-blue"
                    />

                    <div>
                      <p className="text-sm font-semibold text-fiscal-navy">
                        Informações cadastrais
                      </p>

                      <p className="mt-1 text-xs leading-5 text-slate-600">
                        Esses dados identificam a organização no FiscalGuard.
                        Confira as informações antes de salvar.
                      </p>
                    </div>
                  </div>
                </div>

                {/* CAMPOS */}
                <div className="grid gap-5 md:grid-cols-3">
                  <label className="block md:col-span-2">
                    <span className="mb-2 block text-sm font-semibold text-slate-700">
                      Nome da organização
                      <span className="text-rose-500"> *</span>
                    </span>

                    <input
                      className="field w-full"
                      name="name"
                      value={values.name}
                      onChange={e => update('name', e.target.value)}
                      placeholder="Ex.: Escritório Contábil"
                      maxLength={200}
                      required
                    />
                  </label>

                  <label className="block">
                    <span className="mb-2 block text-sm font-semibold text-slate-700">
                      CPF/CNPJ
                    </span>

                    <input
                      className="field w-full"
                      name="document"
                      value={values.document}
                      onChange={e => update('document', e.target.value)}
                      placeholder="Documento da organização"
                      maxLength={30}
                    />
                  </label>

                  <label className="block">
                    <span className="mb-2 block text-sm font-semibold text-slate-700">
                      Telefone
                    </span>

                    <input
                      className="field w-full"
                      name="phone"
                      type="tel"
                      value={values.phone}
                      onChange={e => update('phone', e.target.value)}
                      placeholder="(00) 00000-0000"
                      maxLength={30}
                    />
                  </label>
                </div>

                {/* AÇÕES */}
                <div className="flex flex-wrap items-center justify-between gap-4 border-t border-slate-100 pt-6">
                  <p className="text-xs text-slate-500">
                    {changed
                      ? 'Você possui alterações não salvas.'
                      : 'Todos os dados estão atualizados.'}
                  </p>

                  <div className="flex flex-wrap items-center gap-3">
                    <button
                      type="button"
                      onClick={resetChanges}
                      disabled={!changed || saving}
                      className="btn-secondary inline-flex items-center gap-2 disabled:cursor-not-allowed disabled:opacity-50"
                    >
                      <RotateCcw size={16} />
                      Descartar
                    </button>

                    <button
                      type="submit"
                      disabled={!changed || saving || !values.name.trim()}
                      className="btn-primary inline-flex items-center gap-2 disabled:cursor-not-allowed disabled:opacity-50"
                    >
                      {saving ? (
                        <RefreshCw size={17} className="animate-spin" />
                      ) : (
                        <Save size={17} />
                      )}

                      {saving ? 'Salvando...' : 'Salvar alterações'}
                    </button>
                  </div>
                </div>
              </form>
            )}
          </section>
        )}

        {/* ABA MEU PERFIL */}
        {tab === 'profile' && (
          <section
            role="tabpanel"
            id="panel-profile"
            aria-labelledby="tab-profile"
            className="p-5 sm:p-8"
          >
            <div className="mb-6 flex items-start gap-4">
              <div className="rounded-xl bg-fiscal-mist p-3 text-fiscal-blue">
                <UserRound size={24} />
              </div>

              <div>
                <h2 className="text-lg font-bold text-fiscal-navy">
                  Meu perfil
                </h2>

                <p className="mt-1 text-sm text-slate-500">
                  Informações da sessão autenticada.
                </p>
              </div>
            </div>

            {/* CARTÃO DO USUÁRIO */}
            <div className="mb-6 flex items-center gap-4 rounded-xl border border-slate-200 bg-slate-50 p-5">
              <div className="flex h-14 w-14 shrink-0 items-center justify-center rounded-full bg-fiscal-mist text-xl font-bold text-fiscal-blue">
                {(session?.name || 'U')
                  .trim()
                  .charAt(0)
                  .toUpperCase()}
              </div>

              <div className="min-w-0">
                <p className="truncate font-semibold text-fiscal-navy">
                  {session?.name || 'Usuário FiscalGuard'}
                </p>

                <p className="mt-1 text-xs text-slate-500">
                  Perfil vinculado à sessão atual
                </p>
              </div>
            </div>

            <div className="w-full space-y-4">
              <div>
                <span className="mb-2 block text-sm font-semibold text-slate-700">
                  Nome
                </span>

                <div className="rounded-lg border border-slate-200 bg-slate-50 px-4 py-3 text-sm text-slate-700">
                  {session?.name || 'Não informado'}
                </div>
              </div>

              <div>
                <span className="mb-2 block text-sm font-semibold text-slate-700">
                  Permissão
                </span>

                <div className="rounded-lg border border-slate-200 bg-slate-50 px-4 py-3 text-sm text-slate-700">
                  {session?.role || 'Não informada'}
                </div>
              </div>

              <p className="rounded-lg border border-sky-100 bg-fiscal-mist p-4 text-sm text-fiscal-navy">
                Os dados acima são somente leitura.
                A edição ficará disponível quando a API de atualização
                de usuário estiver integrada.
              </p>
            </div>
          </section>
        )}

        {/* ABA SEGURANÇA */}
        {tab === 'security' && (
          <section
            role="tabpanel"
            id="panel-security"
            aria-labelledby="tab-security"
            className="p-5 sm:p-8"
          >
            <div className="mb-6 flex items-start gap-4">
              <div className="rounded-xl bg-fiscal-mist p-3 text-fiscal-blue">
                <LockKeyhole size={24} />
              </div>

              <div>
                <h2 className="text-lg font-bold text-fiscal-navy">
                  Segurança da conta
                </h2>

                <p className="mt-1 text-sm text-slate-500">
                  Gerencie a proteção do seu acesso ao FiscalGuard.
                </p>
              </div>
            </div>

            <div className="w-full rounded-xl border border-slate-200 p-5">
              <div className="flex items-start gap-3">
                <ShieldCheck
                  className="shrink-0 text-fiscal-blue"
                  size={22}
                />

                <div>
                  <h3 className="font-semibold text-fiscal-navy">
                    Acesso autenticado
                  </h3>

                  <p className="mt-1 text-sm text-slate-500">
                    Sua sessão utiliza o mecanismo de autenticação
                    já existente no sistema.
                  </p>
                </div>
              </div>

              <div className="mt-5 border-t border-slate-100 pt-5">
                <h3 className="font-semibold text-fiscal-navy">
                  Alteração de senha
                </h3>

                <p className="mt-1 text-sm text-slate-500">
                  A alteração de senha ficará disponível após
                  a integração de um endpoint seguro.
                  Por enquanto, nenhuma senha pode ser modificada
                  por esta tela.
                </p>
              </div>
            </div>
          </section>
        )}
      </div>
    </div>
  );
}
