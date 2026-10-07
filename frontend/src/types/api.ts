export type FiscalStatus = 'NotConsulted' | 'Processing' | 'Regular' | 'Attention' | 'Irregular' | 'QueryError' | 'DataUnavailable';
export type FiscalStatusValue = FiscalStatus | number | string | null | undefined;
export type IssueSeverity = 'Informational' | 'Low' | 'Medium' | 'High' | 'Critical';
export type IssueSeverityValue = IssueSeverity | number | string | null | undefined;
export type IssueStatus = 'Open' | 'InReview' | 'Resolved' | 'Ignored' | 'Reopened';
export type UserRole = 'Owner' | 'Administrator' | 'Accountant' | 'Assistant' | 'ReadOnly';
export type SubscriptionStatus = 'Trial' | 'Free' | 'Active' | 'PastDue' | 'Blocked' | 'Canceled';

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
  highRiskCompanies: number;
  criticalRiskCompanies: number;
  notConsultedCompanies: number;
  queryErrorCompanies: number;
  companiesWithPublicData: number;
  simplesOptInCompanies: number;
  simplesNotOptInCompanies: number;
  recommendationsGenerated: number;
  topRiskCompanies: DashboardRiskCompany[];
  valueIndicators: DashboardValueIndicator[];
  planName: string;
  cnpjLimit: number;
  daysRemaining: number;
}

export interface DashboardRiskCompany {
  id: string;
  legalName: string;
  cnpj: string;
  fiscalStatus: FiscalStatusValue;
  riskScore: number;
  riskLevel: string;
  openIssues: number;
}

export interface DashboardValueIndicator {
  title: string;
  value: string;
  description: string;
}

export interface CompanySummary {
  id: string;
  legalName: string;
  tradeName?: string;
  cnpj: string;
  fiscalStatus: FiscalStatusValue;
  status: string;
  lastConsultedAt?: string;
  openIssues: number;
  riskScore: number;
  riskLevel: string;
}

export interface CompanyDetails extends CompanySummary {
  customerId: string;
  stateRegistration?: string;
  taxRegime: number | string;
  email?: string;
  phone?: string;
  responsibleName?: string;
  notes?: string;
  tags?: string;
  monitoringFrequency: number | string;
  registrationStatus?: string;
  registrationStatusDate?: string;
  mainCnaeCode?: string;
  mainCnaeDescription?: string;
  publicAddress?: string;
  publicDataSource?: string;
  publicDataUpdatedAt?: string;
  isSimplesOption?: boolean;
  isMeiOption?: boolean;
  riskSummary?: string;
  nextConsultationAt?: string;
}

export interface CompanyImportResult {
  totalRows: number;
  imported: number;
  duplicated: number;
  invalid: number;
  rows: CompanyImportRowResult[];
}

export interface CompanyImportRowResult {
  rowNumber: number;
  cnpj?: string;
  status: string;
  message: string;
  companyId?: string;
}

export interface FiscalConsultation {
  id: string;
  companyId: string;
  provider: string;
  success: boolean;
  normalizedStatus: FiscalStatusValue;
  error?: string;
  durationMs: number;
  createdAt: string;
}

export interface FiscalIssue {
  id: string;
  companyId: string;
  companyName: string;
  type: string;
  title: string;
  description: string;
  severity: IssueSeverityValue;
  status: IssueStatus;
  detectedAt: string;
  recommendation?: string;
  evidenceType: string;
}

export interface FiscalIssueDetails extends FiscalIssue {
  type: string;
  description: string;
  origin: string;
  externalIdentifier?: string;
  updatedAt: string;
  resolvedAt?: string;
  notes?: string;
  recommendation?: string;
  evidenceType: string;
  deduplicationKey: string;
}

export interface AlertItem {
  id: string;
  title: string;
  message: string;
  type: string;
  severity: IssueSeverityValue;
  isRead: boolean;
  createdAt: string;
}

export interface RegulatoryRadarSummary {
  generatedAt: string;
  source: string;
  fromCache: boolean;
  cachedUntil?: string;
  monitoredThemes: string[];
  diagnostics: RegulatorySourceDiagnostic[];
  documentsStored: number;
  matchesStored: number;
  matchingModel: string;
  alerts: RegulatoryAlert[];
}

export interface RegulatorySourceDiagnostic {
  sourceKey: string;
  sourceName: string;
  success: boolean;
  documentsFound: number;
  documentsAccepted: number;
  durationMs: number;
  error?: string;
  statusMessage?: string;
  startedAt: string;
  finishedAt?: string;
}

export interface RegulatoryAlert {
  id: string;
  title: string;
  summary: string;
  source: string;
  sourceUrl: string;
  sourceType: string;
  theme: string;
  impactLevel: string;
  impactReason: string;
  businessImpact: string;
  suggestedAction: string;
  presentedAt?: string;
  affectedCompaniesCount: number;
  affectedCompanies: RegulatoryAffectedCompany[];
}

export interface RegulatoryAffectedCompany {
  id: string;
  legalName: string;
  cnpj: string;
  reason: string;
  score: number;
  matchedTerms: string;
}

export interface Organization {
  id: string;
  name: string;
  document?: string;
  phone?: string;
  isActive: boolean;
}

export interface OrganizationUser {
  id: string;
  userId: string;
  name: string;
  email: string;
  phone?: string;
  role: UserRole;
  isActive: boolean;
  createdAt: string;
}

export interface UserInvitation {
  id: string;
  email: string;
  role: UserRole;
  expiresAt: string;
  acceptedAt?: string;
}

export interface Plan {
  code: string;
  name: string;
  cnpjLimit: number;
  userLimit: number;
  automatedMonitoring: boolean;
  emailAlerts: boolean;
  fullHistory: boolean;
}

export interface Subscription {
  id: string;
  planCode: string;
  planName: string;
  status: SubscriptionStatus;
  startedAt: string;
  trialEndsAt?: string;
  endsAt?: string;
  cnpjLimit: number;
  userLimit: number;
  activeCompanies: number;
  activeUsers: number;
  daysRemaining: number;
}
