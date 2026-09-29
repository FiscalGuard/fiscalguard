import { AlertTriangle, BriefcaseBusiness, Clock, FileCheck2, Lightbulb, RefreshCw, ShieldCheck, Siren } from 'lucide-react';
import { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { api } from '../../services/api';
import type { CompanyDetails, FiscalConsultation, FiscalIssue } from '../../types/api';
import { fiscalStatusMeta, riskClass, severityClass } from '../../utils/status';

export function CompanyDetailsPage({ mode }: { mode?: 'history' }) {
  const { id } = useParams();
  const [company, setCompany] = useState<CompanyDetails | null>(null);
  const [consultations, setConsultations] = useState<FiscalConsultation[]>([]);
  const [issues, setIssues] = useState<FiscalIssue[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  async function load() {
    if (!id) return;
    try {
      const [companyData, consultationData, issueData] = await Promise.all([
        api<CompanyDetails>(`/api/companies/${id}`),
        api<FiscalConsultation[]>(`/api/companies/${id}/consultations`),
        api<FiscalIssue[]>(`/api/companies/${id}/issues`)
      ]);
      setCompany(companyData);
      setConsultations(consultationData);
      setIssues(issueData);
      setError('');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erro ao carregar empresa');
    }
  }

  useEffect(() => {
    load();
  }, [id]);

  async function consult() {
    if (!id) return;
    setLoading(true);
    try {
      await api(`/api/companies/${id}/consultations`, { method: 'POST' });
      await load();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erro ao consultar CNPJ');
    } finally {
      setLoading(false);
    }
  }

  const meta = fiscalStatusMeta(company?.fiscalStatus);
  const sortedIssues = [...issues].sort((a, b) => severityRank(b.severity) - severityRank(a.severity));
  const mainInsight = company ? buildExpertInsight(company, sortedIssues) : null;

  return (
    <div className="space-y-4">
      <h1 className="text-2xl font-bold text-fiscal-navy">{mode === 'history' ? 'Historico de consultas' : 'Detalhes da empresa'}</h1>
      {error && <div className="rounded-md bg-rose-50 p-3 text-sm text-rose-700">{error}</div>}
      {company && (
        <section className="rounded-md border border-slate-200 bg-white p-5 shadow-sm">
          <div className="flex flex-wrap items-start justify-between gap-4">
            <div>
              <h2 className="text-xl font-semibold text-fiscal-navy">{company.legalName}</h2>
              <p className="text-sm text-slate-500">{company.tradeName || 'Sem nome fantasia'} - {company.cnpj}</p>
            </div>
            <span className="inline-flex items-center gap-2 rounded-md bg-slate-50 px-3 py-2 text-sm">
              <span className={`status-dot ${meta.color}`} /> {meta.label}
            </span>
          </div>
          <div className="mt-5 grid gap-3 md:grid-cols-4">
            <Info label="Responsavel" value={company.responsibleName || '-'} />
            <Info label="E-mail" value={company.email || '-'} />
            <Info label="Telefone" value={company.phone || '-'} />
            <Info label="Pendencias abertas" value={String(company.openIssues)} />
          </div>
          <div className="mt-4 grid gap-3 md:grid-cols-4">
            <Info label="Situacao cadastral" value={company.registrationStatus || 'Nao consultada'} />
            <Info label="Simples Nacional" value={company.isSimplesOption == null ? 'Nao informado' : company.isSimplesOption ? 'Optante' : 'Nao optante'} />
            <Info label="MEI" value={company.isMeiOption == null ? 'Nao informado' : company.isMeiOption ? 'Optante' : 'Nao optante'} />
            <div className="rounded-md bg-slate-50 p-3">
              <div className="text-xs uppercase text-slate-500">Score de risco</div>
              <div className={`mt-1 inline-flex rounded-md px-2 py-1 text-sm font-semibold ${riskClass(company.riskLevel)}`}>{company.riskLevel} {company.riskScore}</div>
            </div>
          </div>
          <div className="mt-4 grid gap-3 md:grid-cols-2">
            <Info label="CNAE principal" value={company.mainCnaeCode ? `${company.mainCnaeCode} - ${company.mainCnaeDescription || ''}` : '-'} />
            <Info label="Endereco publico" value={company.publicAddress || '-'} />
          </div>
          {company.riskSummary && <p className="mt-4 rounded-md bg-blue-50 p-3 text-sm text-fiscal-navy">{company.riskSummary}</p>}
          <div className="mt-4 grid gap-3 md:grid-cols-4">
            <Coverage icon={FileCheck2} title="Dados publicos" status={company.publicDataSource ? 'Atualizados' : 'Aguardando consulta'} description={company.publicDataSource ? `${company.publicDataSource} - ${company.publicDataUpdatedAt ? new Date(company.publicDataUpdatedAt).toLocaleString('pt-BR') : ''}` : 'Execute a consulta para consolidar a fonte.'} />
            <Coverage icon={ShieldCheck} title="Enquadramento" status={company.isSimplesOption ? 'Simples confirmado' : company.isSimplesOption === false ? 'Revisar enquadramento' : 'Nao informado'} description="Indicador publico usado para priorizar revisao da carteira." />
            <Coverage icon={AlertTriangle} title="Priorizacao" status={`${company.riskLevel} ${company.riskScore}`} description="Score calculado a partir dos sinais encontrados." />
            <Coverage icon={Lightbulb} title="Orientacao" status={issues.length > 0 ? `${issues.length} acoes` : 'Sem acao pendente'} description="Recomendações ficam registradas para a equipe contabil." />
          </div>
          <button className="btn-primary mt-5" onClick={consult} disabled={loading}>
            <RefreshCw size={18} /> {loading ? 'Consultando...' : 'Executar consulta manual'}
          </button>
        </section>
      )}

      {company && mainInsight && (
        <section className="rounded-md border border-slate-200 bg-white p-5 shadow-sm">
          <div className="flex flex-wrap items-center justify-between gap-3">
            <div>
              <h2 className="flex items-center gap-2 font-semibold text-fiscal-navy"><BriefcaseBusiness size={18} /> Leitura para o contador</h2>
              <p className="mt-1 text-sm text-slate-500">O FiscalGuard transforma sinais cadastrais em uma fila objetiva de atendimento.</p>
            </div>
            <span className={`rounded-md px-3 py-1 text-sm font-semibold ${riskClass(company.riskLevel)}`}>{company.riskLevel} {company.riskScore}</span>
          </div>
          <div className="mt-4 grid gap-3 md:grid-cols-3">
            <DecisionCard title="O que foi detectado" value={mainInsight.detected} />
            <DecisionCard title="Por que isso importa" value={mainInsight.impact} />
            <DecisionCard title="Proxima melhor acao" value={mainInsight.action} />
          </div>
        </section>
      )}

      <section className="rounded-md border border-slate-200 bg-white p-5 shadow-sm">
        <h2 className="flex items-center gap-2 font-semibold text-fiscal-navy"><Siren size={18} /> Alertas que o FiscalGuard monitora</h2>
        <div className="mt-3 grid gap-3 md:grid-cols-5">
          <AlertType title="Situacao cadastral critica" description="Baixada, inapta, suspensa ou situacao especial." />
          <AlertType title="Enquadramento no Simples" description="Nao optante, MEI ou sinal que tira o cliente do perfil esperado." />
          <AlertType title="CNAE e dados cadastrais" description="CNAE ausente, dados incompletos e divergencias cadastrais." />
          <AlertType title="Falha de fonte externa" description="Erro ou indisponibilidade da consulta, preservado no historico." />
          <AlertType title="Reincidencia" description="Pendencia resolvida que reaparece em nova verificacao." />
        </div>
      </section>

      <section className="rounded-md border border-slate-200 bg-white p-5">
        <h2 className="mb-3 flex items-center gap-2 font-semibold text-fiscal-navy"><Clock size={18} /> Consultas fiscais</h2>
        <div className="space-y-2">
          {consultations.length === 0 && <p className="text-sm text-slate-500">Nenhuma consulta registrada.</p>}
          {consultations.map((consultation) => {
            const status = fiscalStatusMeta(consultation.normalizedStatus);
            return (
              <div key={consultation.id} className="flex flex-wrap items-center justify-between gap-2 rounded-md bg-slate-50 p-3 text-sm">
                <span><span className={`status-dot ${status.color}`} /> <span className="ml-2">{status.label}</span></span>
                <span>{consultation.provider} - {consultation.durationMs}ms</span>
                <span className="text-slate-500">{new Date(consultation.createdAt).toLocaleString()}</span>
              </div>
            );
          })}
        </div>
      </section>

      <section className="rounded-md border border-slate-200 bg-white p-5">
        <h2 className="mb-3 flex items-center gap-2 font-semibold text-fiscal-navy"><AlertTriangle size={18} /> Pendencias da empresa</h2>
        <div className="space-y-2">
          {issues.length === 0 && <p className="text-sm text-slate-500">Nenhuma pendencia encontrada.</p>}
          {issues.map((issue) => (
            <div key={issue.id} className="rounded-md bg-slate-50 p-3 text-sm">
              <div className="flex flex-wrap items-center justify-between gap-2">
                <strong className="text-fiscal-ink">{issue.title}</strong>
                <span className={`rounded-md px-2 py-1 text-xs font-semibold ${severityClass(issue.severity)}`}>{issue.severity}</span>
              </div>
              <p className="mt-2 text-slate-600">{issue.description}</p>
              <div className="mt-3 grid gap-2 md:grid-cols-3">
                <IssueSignal label="Tipo de alerta" value={issueTypeLabel(issue.type)} />
                <IssueSignal label="Impacto pratico" value={issueImpact(issue)} />
                <IssueSignal label="Diferencial" value="Priorizacao automatica com historico e acao sugerida." />
              </div>
              {issue.recommendation && <p className="mt-2 rounded-md bg-white p-3 text-slate-600"><strong>Acao sugerida:</strong> {issue.recommendation}</p>}
              <p className="mt-1 text-slate-500">Status {issue.status} - detectada em {new Date(issue.detectedAt).toLocaleString()}</p>
              <p className="mt-1 text-xs text-slate-400">{issue.evidenceType}</p>
            </div>
          ))}
        </div>
      </section>
    </div>
  );
}

function Info({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-md bg-slate-50 p-3">
      <div className="text-xs uppercase text-slate-500">{label}</div>
      <div className="mt-1 font-medium text-fiscal-ink">{value}</div>
    </div>
  );
}

function Coverage({ icon: Icon, title, status, description }: { icon: typeof FileCheck2; title: string; status: string; description: string }) {
  return (
    <div className="rounded-md border border-slate-200 bg-white p-3">
      <Icon className="text-fiscal-blue" size={18} />
      <div className="mt-2 text-xs uppercase text-slate-500">{title}</div>
      <div className="mt-1 font-semibold text-fiscal-ink">{status}</div>
      <p className="mt-1 text-xs text-slate-500">{description}</p>
    </div>
  );
}

function DecisionCard({ title, value }: { title: string; value: string }) {
  return (
    <div className="rounded-md bg-slate-50 p-4">
      <div className="text-xs uppercase text-slate-500">{title}</div>
      <p className="mt-2 text-sm font-medium leading-6 text-fiscal-ink">{value}</p>
    </div>
  );
}

function AlertType({ title, description }: { title: string; description: string }) {
  return (
    <div className="rounded-md bg-slate-50 p-3">
      <div className="text-sm font-semibold text-fiscal-ink">{title}</div>
      <p className="mt-1 text-xs leading-5 text-slate-500">{description}</p>
    </div>
  );
}

function IssueSignal({ label, value }: { label: string; value: string }) {
  return (
    <div className="rounded-md bg-white p-3">
      <div className="text-xs uppercase text-slate-500">{label}</div>
      <div className="mt-1 text-sm font-medium text-fiscal-ink">{value}</div>
    </div>
  );
}

function buildExpertInsight(company: CompanyDetails, issues: FiscalIssue[]) {
  const primary = issues[0];
  if (!primary) {
    return {
      detected: 'Nenhuma pendencia aberta apos a ultima consulta.',
      impact: 'A equipe pode manter o cliente na rotina normal e usar o historico como evidencia de acompanhamento.',
      action: 'Manter monitoramento automatico e revisar novamente conforme a frequencia configurada.'
    };
  }

  return {
    detected: primary.title,
    impact: issueImpact(primary),
    action: primary.recommendation || company.riskSummary || 'Priorizar revisao pelo responsavel contabil.'
  };
}

function issueImpact(issue: FiscalIssue) {
  const type = String(issue.type);
  const text = `${issue.title} ${issue.description}`.toLowerCase();
  if (type.includes('Registration') || text.includes('baixada') || text.includes('inapta') || text.includes('suspensa')) {
    return 'Evita manter na rotina ativa um CNPJ com situacao cadastral que pode impedir operacoes, emissao e entregas.';
  }
  if (type.includes('Simples') || text.includes('simples')) {
    return 'Ajuda a separar clientes que exigem revisao de regime antes da apuracao e comunicacao ao cliente.';
  }
  if (type.includes('Integration')) {
    return 'Mostra que a fonte externa falhou e impede que a equipe confunda ausencia de dado com regularidade.';
  }
  if (type.includes('MissingFiling')) {
    return 'Transforma obrigacao pendente em tarefa visivel, com responsavel e historico para acompanhamento.';
  }
  return 'Converte um sinal disperso em prioridade operacional para a equipe contabil.';
}

function issueTypeLabel(type: string) {
  const value = String(type);
  if (value.includes('Registration')) return 'Situacao cadastral';
  if (value.includes('Simples')) return 'Enquadramento no Simples';
  if (value.includes('Integration')) return 'Fonte externa';
  if (value.includes('MissingFiling')) return 'Obrigacao acessoria';
  if (value.includes('TaxDebt')) return 'Pendencia fiscal';
  return 'Sinal de risco';
}

function severityRank(severity: unknown) {
  const value = String(severity);
  if (value === '4' || value === 'Critical') return 4;
  if (value === '3' || value === 'High') return 3;
  if (value === '2' || value === 'Medium') return 2;
  if (value === '1' || value === 'Low') return 1;
  return 0;
}
