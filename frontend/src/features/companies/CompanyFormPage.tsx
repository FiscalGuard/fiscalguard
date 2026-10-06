import { FormEvent, type InputHTMLAttributes, type ReactNode, type SelectHTMLAttributes, type TextareaHTMLAttributes, useEffect, useState } from 'react';
import { ArrowLeft, Building2, CheckCircle2, FileText, Radar, Save, ShieldCheck } from 'lucide-react';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { api } from '../../services/api';
import type { CompanyDetails } from '../../types/api';

export function CompanyFormPage() {
  const navigate = useNavigate();
  const { id } = useParams();
  const isEditing = Boolean(id);
  const [company, setCompany] = useState<CompanyDetails | null>(null);
  const [loading, setLoading] = useState(isEditing);
  const [error, setError] = useState('');
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    if (!id) return;

    setLoading(true);
    api<CompanyDetails>(`/api/companies/${id}`)
      .then((data) => {
        setCompany(data);
        setError('');
      })
      .catch((err) => setError(err instanceof Error ? err.message : 'Erro ao carregar empresa'))
      .finally(() => setLoading(false));
  }, [id]);

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const form = new FormData(event.currentTarget);
    setSaving(true);
    try {
      await api(isEditing ? `/api/companies/${id}` : '/api/companies', {
        method: isEditing ? 'PUT' : 'POST',
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
      navigate(isEditing && id ? `/app/empresas/${id}` : '/app/empresas');
    } catch (err) {
      setError(err instanceof Error ? err.message : isEditing ? 'Erro ao atualizar empresa' : 'Erro ao cadastrar empresa');
    } finally {
      setSaving(false);
    }
  }

  if (loading) {
    return <div className="rounded-md border border-slate-200 bg-white p-5 text-sm text-slate-600">Carregando dados da empresa...</div>;
  }

  return (
    <div className="mx-auto max-w-6xl space-y-5">
      <div className="flex flex-wrap items-start justify-between gap-4">
        <div>
          <Link to={isEditing && id ? `/app/empresas/${id}` : '/app/empresas'} className="inline-flex items-center gap-2 text-sm font-semibold text-slate-500 hover:text-fiscal-blue">
            <ArrowLeft size={16} /> {isEditing ? 'Voltar para detalhes' : 'Voltar para empresas'}
          </Link>
          <div className="mt-4 text-xs font-semibold uppercase tracking-wide text-fiscal-blue">{isEditing ? 'Atualização cadastral' : 'Novo monitoramento'}</div>
          <h1 className="mt-1 text-3xl font-bold text-fiscal-navy">{isEditing ? 'Editar empresa' : 'Cadastrar empresa'}</h1>
          <p className="mt-1 text-sm text-slate-500">{isEditing ? 'Atualize os dados internos usados pela equipe e pelo monitoramento da carteira.' : 'Informe os dados básicos para incluir o CNPJ na carteira e habilitar consultas preventivas.'}</p>
        </div>
        <div className="rounded-md border border-emerald-100 bg-emerald-50 px-4 py-3 text-sm font-semibold text-emerald-700">
          {isEditing ? 'Alterações ficam registradas na auditoria' : 'Consulta pública disponível após o cadastro'}
        </div>
      </div>

      <div className="grid gap-5 lg:grid-cols-[1fr_340px]">
        <form onSubmit={submit} className="rounded-md border border-slate-200 bg-white p-5 shadow-sm">
          {error && <div className="mb-5 rounded-md border border-rose-100 bg-rose-50 p-3 text-sm text-rose-700">{error}</div>}

          <FormSection icon={Building2} title="Identificação da empresa" description="Dados usados para consulta pública, busca e organização da carteira.">
            <div className="grid gap-4 md:grid-cols-2">
              <Field name="legalName" label="Razão social" placeholder="Ex.: Comércio Azul Ltda" defaultValue={company?.legalName ?? ''} required />
              <Field name="tradeName" label="Nome fantasia" placeholder="Ex.: Comércio Azul" defaultValue={company?.tradeName ?? ''} />
              <Field name="cnpj" label="CNPJ" placeholder="00.000.000/0001-00" defaultValue={company?.cnpj ?? ''} required />
              <Field name="stateRegistration" label="Inscrição estadual" placeholder="Opcional" defaultValue={company?.stateRegistration ?? ''} />
              <SelectField name="taxRegime" label="Regime tributário" defaultValue={toTaxRegimeValue(company?.taxRegime)}>
                <option value="1">Simples Nacional</option>
                <option value="4">MEI</option>
                <option value="2">Lucro Presumido</option>
                <option value="3">Lucro Real</option>
              </SelectField>
              <SelectField name="monitoringFrequency" label="Frequência de monitoramento" defaultValue={toMonitoringFrequencyValue(company?.monitoringFrequency)}>
                <option value="0">Manual</option>
                <option value="1">Diária</option>
                <option value="7">Semanal</option>
                <option value="30">Mensal</option>
              </SelectField>
            </div>
          </FormSection>

          <FormSection icon={ShieldCheck} title="Contato e responsabilidade" description="Ajuda o escritório a saber quem acionar quando um alerta for identificado.">
            <div className="grid gap-4 md:grid-cols-2">
              <Field name="email" label="E-mail" type="email" placeholder="cliente@empresa.com.br" defaultValue={company?.email ?? ''} />
              <Field name="phone" label="Telefone" placeholder="(00) 00000-0000" defaultValue={company?.phone ?? ''} />
              <Field name="responsibleName" label="Responsável" placeholder="Nome do responsável" defaultValue={company?.responsibleName ?? ''} />
              <Field name="tags" label="Tags" placeholder="Ex.: prioridade, serviços, varejo" defaultValue={company?.tags ?? ''} />
            </div>
          </FormSection>

          <FormSection icon={FileText} title="Observações internas" description="Registre detalhes relevantes para atendimento, histórico e apresentação ao cliente.">
            <TextAreaField name="notes" label="Observações" placeholder="Ex.: cliente sensível a prazo de DAS; revisar enquadramento antes do fechamento mensal." defaultValue={company?.notes ?? ''} />
          </FormSection>

          <div className="mt-6 flex flex-wrap items-center justify-between gap-3 border-t border-slate-100 pt-5">
            <p className="text-sm text-slate-500">{isEditing ? 'Salvar não altera o histórico fiscal já registrado para este CNPJ.' : 'Depois de salvar, rode a consulta para enriquecer os dados do CNPJ e calcular o risco.'}</p>
            <button className="btn-primary px-5 py-3" disabled={saving}>
              <Save size={17} /> {saving ? 'Salvando...' : isEditing ? 'Salvar alterações' : 'Salvar empresa'}
            </button>
          </div>
        </form>

        <aside className="space-y-4">
          <div className="rounded-md border border-slate-200 bg-white p-5 shadow-sm">
            <div className="flex items-center gap-2 font-semibold text-fiscal-navy"><Radar size={18} className="text-fiscal-blue" /> O que acontece depois?</div>
            <div className="mt-4 space-y-3">
              <GuideStep title="1. Consulta pública" text="O FiscalGuard busca situação cadastral, CNAE, endereço e indicadores de Simples/MEI quando disponíveis." />
              <GuideStep title="2. Score de risco" text="O sistema combina situação fiscal, pendências e evidências para priorizar a carteira." />
              <GuideStep title="3. Recomendações" text="Cada alerta recebe explicação simples e próxima ação para o contador." />
            </div>
          </div>

          {/* <div className="rounded-md border border-blue-100 bg-blue-50 p-5 text-sm leading-6 text-fiscal-navy">
            Para uma demonstração convincente, use um CNPJ real, rode a consulta e mostre o antes/depois no detalhe da empresa.
          </div> */}
        </aside>
      </div>
    </div>
  );
}

function FormSection({ icon: Icon, title, description, children }: { icon: typeof Building2; title: string; description: string; children: ReactNode }) {
  return (
    <section className="border-b border-slate-100 py-5 first:pt-0 last:border-b-0 last:pb-0">
      <div className="mb-4 flex items-start gap-3">
        <div className="inline-flex h-9 w-9 items-center justify-center rounded-md bg-blue-50 text-fiscal-blue"><Icon size={18} /></div>
        <div>
          <h2 className="font-semibold text-fiscal-navy">{title}</h2>
          <p className="text-sm text-slate-500">{description}</p>
        </div>
      </div>
      {children}
    </section>
  );
}

function Field(props: InputHTMLAttributes<HTMLInputElement> & { label: string }) {
  const { label, ...inputProps } = props;
  return (
    <label className="block">
      <span className="text-sm font-semibold text-fiscal-ink">{label}</span>
      <input className="field mt-2 py-3" {...inputProps} />
    </label>
  );
}

function SelectField(props: SelectHTMLAttributes<HTMLSelectElement> & { label: string; children: ReactNode }) {
  const { label, children, ...selectProps } = props;
  return (
    <label className="block">
      <span className="text-sm font-semibold text-fiscal-ink">{label}</span>
      <select className="field mt-2 py-3" {...selectProps}>{children}</select>
    </label>
  );
}

function TextAreaField(props: TextareaHTMLAttributes<HTMLTextAreaElement> & { label: string }) {
  const { label, ...textareaProps } = props;
  return (
    <label className="block">
      <span className="text-sm font-semibold text-fiscal-ink">{label}</span>
      <textarea className="field mt-2 min-h-32 py-3" {...textareaProps} />
    </label>
  );
}

function GuideStep({ title, text }: { title: string; text: string }) {
  return (
    <div className="rounded-md bg-slate-50 p-3">
      <div className="flex items-center gap-2 font-semibold text-fiscal-navy"><CheckCircle2 size={15} className="text-emerald-600" /> {title}</div>
      <p className="mt-1 text-sm leading-6 text-slate-600">{text}</p>
    </div>
  );
}

function toTaxRegimeValue(value: CompanyDetails['taxRegime'] | undefined) {
  if (value == null) return '1';
  if (typeof value === 'number') return String(value);
  if (/^\d+$/.test(value)) return value;
  const map: Record<string, string> = {
    Unknown: '0',
    SimplesNacional: '1',
    LucroPresumido: '2',
    LucroReal: '3',
    Mei: '4'
  };
  return map[value] ?? '1';
}

function toMonitoringFrequencyValue(value: CompanyDetails['monitoringFrequency'] | undefined) {
  if (value == null) return '7';
  if (typeof value === 'number') return String(value);
  if (/^\d+$/.test(value)) return value;
  const map: Record<string, string> = {
    Manual: '0',
    Daily: '1',
    Weekly: '7',
    Monthly: '30'
  };
  return map[value] ?? '7';
}
