namespace FiscalGuard.Domain;

public enum UserRole { Owner = 1, Administrator = 2, Accountant = 3, Assistant = 4, ReadOnly = 5 }
public enum TaxRegime { Unknown = 0, SimplesNacional = 1, LucroPresumido = 2, LucroReal = 3, Mei = 4 }
public enum MonitoringFrequency { Manual = 0, Daily = 1, Weekly = 7, Monthly = 30 }
public enum CompanyStatus { Active = 1, Inactive = 2 }
public enum FiscalStatus { NotConsulted = 0, Processing = 1, Regular = 2, Attention = 3, Irregular = 4, QueryError = 5, DataUnavailable = 6 }
public enum IssueType { Registration = 1, TaxDebt = 2, SimplesNational = 3, MissingFiling = 4, IntegrationFailure = 5 }
public enum IssueSeverity { Informational = 0, Low = 1, Medium = 2, High = 3, Critical = 4 }
public enum IssueStatus { Open = 1, InReview = 2, Resolved = 3, Ignored = 4, Reopened = 5 }
public enum IssueOrigin { FiscalConsultation = 1, Manual = 2, System = 3 }
public enum AlertType { NewIssue = 1, SeverityIncrease = 2, IssueReopened = 3, RepeatedQueryFailure = 4, TrialEnding = 5, PlanLimitNear = 6 }
public enum NotificationChannel { Internal = 1, Email = 2, SmsFuture = 3 }
public enum NotificationStatus { Pending = 1, Sent = 2, Failed = 3, SkippedDuplicate = 4 }
public enum SubscriptionStatus { Trial = 1, Free = 2, Active = 3, PastDue = 4, Blocked = 5, Canceled = 6 }
