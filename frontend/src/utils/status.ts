import type { FiscalStatus, IssueSeverity } from '../types/api';

export function fiscalStatusMeta(status: FiscalStatus) {
  const map: Record<FiscalStatus, { label: string; color: string; description: string }> = {
    NotConsulted: { label: 'Nao consultado', color: 'bg-slate-400', description: 'CNPJ ainda sem consulta fiscal registrada.' },
    Processing: { label: 'Em processamento', color: 'bg-blue-400', description: 'Consulta em andamento.' },
    Regular: { label: 'Regular', color: 'bg-emerald-500', description: 'Nenhuma pendencia simulada encontrada.' },
    Attention: { label: 'Atencao', color: 'bg-amber-500', description: 'Ha sinais que merecem revisao do contador.' },
    Irregular: { label: 'Irregular', color: 'bg-rose-600', description: 'Pendencia fiscal simulada detectada.' },
    QueryError: { label: 'Erro na consulta', color: 'bg-orange-600', description: 'A fonte fiscal nao respondeu corretamente.' },
    DataUnavailable: { label: 'Dados indisponiveis', color: 'bg-slate-500', description: 'Nao foi possivel confirmar a situacao.' }
  };
  return map[status];
}

export function severityClass(severity: IssueSeverity) {
  return {
    Informational: 'text-slate-600 bg-slate-100',
    Low: 'text-blue-700 bg-blue-50',
    Medium: 'text-amber-700 bg-amber-50',
    High: 'text-orange-700 bg-orange-50',
    Critical: 'text-rose-700 bg-rose-50'
  }[severity];
}
