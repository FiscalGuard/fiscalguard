import type { FiscalStatus, FiscalStatusValue, IssueSeverity, IssueSeverityValue } from '../types/api';

const fiscalStatusByNumber: Record<number, FiscalStatus> = {
  0: 'NotConsulted',
  1: 'Processing',
  2: 'Regular',
  3: 'Attention',
  4: 'Irregular',
  5: 'QueryError',
  6: 'DataUnavailable'
};

export function fiscalStatusMeta(status: FiscalStatusValue) {
  const map: Record<FiscalStatus, { label: string; color: string; description: string }> = {
    NotConsulted: { label: 'Nao consultado', color: 'bg-slate-400', description: 'CNPJ ainda sem consulta fiscal registrada.' },
    Processing: { label: 'Em processamento', color: 'bg-blue-400', description: 'Consulta em andamento.' },
    Regular: { label: 'Regular', color: 'bg-emerald-500', description: 'Nenhuma pendencia encontrada.' },
    Attention: { label: 'Atenção', color: 'bg-amber-500', description: 'Ha sinais que merecem revisao do contador.' },
    Irregular: { label: 'Irregular', color: 'bg-rose-600', description: 'Pendencia fiscal detectada.' },
    QueryError: { label: 'Erro na consulta', color: 'bg-orange-600', description: 'A fonte fiscal nao respondeu corretamente.' },
    DataUnavailable: { label: 'Dados indisponiveis', color: 'bg-slate-500', description: 'Nao foi possivel confirmar a situacao.' }
  };
  const normalized = normalizeFiscalStatus(status);
  return map[normalized] ?? map.NotConsulted;
}

export function severityClass(severity: IssueSeverityValue) {
  const normalized = normalizeSeverity(severity);
  const map: Record<IssueSeverity, string> = {
    Informational: 'text-slate-600 bg-slate-100',
    Low: 'text-blue-700 bg-blue-50',
    Medium: 'text-amber-700 bg-amber-50',
    High: 'text-orange-700 bg-orange-50',
    Critical: 'text-rose-700 bg-rose-50'
  };
  return map[normalized] ?? map.Informational;
}

export function riskClass(level?: string) {
  const normalized = (level ?? '').toLowerCase();
  if (normalized.includes('critico')) return 'text-rose-700 bg-rose-50';
  if (normalized.includes('alto')) return 'text-orange-700 bg-orange-50';
  if (normalized.includes('medio') || normalized.includes('médio')) return 'text-amber-700 bg-amber-50';
  if (normalized.includes('baixo')) return 'text-emerald-700 bg-emerald-50';
  return 'text-slate-600 bg-slate-100';
}

function normalizeFiscalStatus(status: FiscalStatusValue): FiscalStatus {
  if (typeof status === 'number') {
    return fiscalStatusByNumber[status] ?? 'NotConsulted';
  }

  if (typeof status === 'string') {
    if (/^\d+$/.test(status)) {
      return fiscalStatusByNumber[Number(status)] ?? 'NotConsulted';
    }
    return status as FiscalStatus;
  }

  return 'NotConsulted';
}

function normalizeSeverity(severity: IssueSeverityValue): IssueSeverity {
  const byNumber: Record<number, IssueSeverity> = {
    0: 'Informational',
    1: 'Low',
    2: 'Medium',
    3: 'High',
    4: 'Critical'
  };

  if (typeof severity === 'number') {
    return byNumber[severity] ?? 'Informational';
  }

  if (typeof severity === 'string' && /^\d+$/.test(severity)) {
    return byNumber[Number(severity)] ?? 'Informational';
  }

  return (severity as IssueSeverity) ?? 'Informational';
}
