export type FiscalStatus = 'NotConsulted' | 'Processing' | 'Regular' | 'Attention' | 'Irregular' | 'QueryError' | 'DataUnavailable';
export type IssueSeverity = 'Informational' | 'Low' | 'Medium' | 'High' | 'Critical';
export type IssueStatus = 'Open' | 'InReview' | 'Resolved' | 'Ignored' | 'Reopened';

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  organizationId: string;
  name: string;
  role: string;
}

export interface DashboardSummary {
  totalCompanies: number;
  regularCompanies: number;
  attentionCompanies: number;
  irregularCompanies: number;
  openIssues: number;
  recentAlerts: number;
  consultations: number;
  planName: string;
  cnpjLimit: number;
  daysRemaining: number;
}

export interface CompanySummary {
  id: string;
  legalName: string;
  tradeName?: string;
  cnpj: string;
  fiscalStatus: FiscalStatus;
  status: string;
  lastConsultedAt?: string;
  openIssues: number;
}

export interface FiscalIssue {
  id: string;
  companyId: string;
  companyName: string;
  title: string;
  severity: IssueSeverity;
  status: IssueStatus;
  detectedAt: string;
}

export interface AlertItem {
  id: string;
  title: string;
  message: string;
  type: string;
  severity: IssueSeverity;
  isRead: boolean;
  createdAt: string;
}
