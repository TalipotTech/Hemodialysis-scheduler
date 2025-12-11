// Subscription tier enumeration
export enum SubscriptionTier {
  BASIC = 'basic',
  ADVANCED = 'advanced',
  ENTERPRISE = 'enterprise' // Future expansion
}

// Subscription status
export enum SubscriptionStatus {
  ACTIVE = 'active',
  EXPIRED = 'expired',
  TRIAL = 'trial',
  CANCELLED = 'cancelled',
  SUSPENDED = 'suspended'
}

// Feature definitions
export interface SubscriptionFeatures {
  // Scheduler Features
  timelineViews: boolean;
  multiResourceScheduling: boolean;
  recurringAppointments: boolean;
  dragDropScheduling: boolean;
  shiftManagement: boolean;
  
  // Patient Management
  maxPatients: number | 'unlimited';
  advancedPatientSearch: boolean;
  patientHistory: boolean;
  bulkPatientImport: boolean;
  
  // Equipment Management
  equipmentTracking: boolean;
  equipmentUsageAnalytics: boolean;
  equipmentMaintenanceScheduling: boolean;
  
  // AI Features
  aiRecommendations: boolean;
  riskAssessment: boolean;
  predictiveScheduling: boolean;
  aiChat: boolean;
  aiFeatureSuggestions: boolean;
  
  // Reporting & Analytics
  basicReports: boolean;
  advancedReports: boolean;
  exportToExcel: boolean;
  exportToPdf: boolean;
  customReports: boolean;
  realTimeAnalytics: boolean;
  
  // Data Management
  basicAnalytics: boolean;
  advancedAnalytics: boolean;
  realTimeMonitoring: boolean;
  dataRetentionDays: number;
  
  // System Features
  maxUsers: number | 'unlimited';
  auditLogs: boolean;
  apiAccess: boolean;
  webhooks: boolean;
  customBranding: boolean;
  prioritySupport: boolean;
  slaGuarantee: boolean;
  
  // Workflow Features
  workflowAutomation: boolean;
  approvalWorkflows: boolean;
  notificationCustomization: boolean;
}

// Subscription plan details
export interface SubscriptionPlan {
  tier: SubscriptionTier;
  name: string;
  description: string;
  monthlyPrice: number;
  annualPrice: number;
  features: SubscriptionFeatures;
  popular?: boolean;
}

// Organization subscription info
export interface OrganizationSubscription {
  organizationId: number;
  tier: SubscriptionTier;
  status: SubscriptionStatus;
  startDate: Date;
  expiryDate: Date;
  trialEndsAt?: Date;
  isTrial: boolean;
  autoRenew: boolean;
  paymentMethod?: string;
  
  // Usage tracking
  currentPatientCount: number;
  currentUserCount: number;
  
  // Limits
  maxPatients: number | 'unlimited';
  maxUsers: number | 'unlimited';
}

// Feature usage tracking
export interface FeatureUsage {
  featureName: string;
  usageCount: number;
  lastUsed: Date;
  limitReached: boolean;
}
