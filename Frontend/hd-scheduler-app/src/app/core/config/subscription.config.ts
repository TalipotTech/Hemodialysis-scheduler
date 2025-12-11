import { SubscriptionTier, SubscriptionFeatures, SubscriptionPlan } from '../models/subscription.model';

// Feature configuration for each tier
export const SUBSCRIPTION_FEATURES: Record<SubscriptionTier, SubscriptionFeatures> = {
  [SubscriptionTier.BASIC]: {
    // Scheduler Features
    timelineViews: false,
    multiResourceScheduling: false,
    recurringAppointments: false,
    dragDropScheduling: true,
    shiftManagement: false,
    
    // Patient Management
    maxPatients: 50,
    advancedPatientSearch: false,
    patientHistory: true,
    bulkPatientImport: false,
    
    // Equipment Management
    equipmentTracking: false,
    equipmentUsageAnalytics: false,
    equipmentMaintenanceScheduling: false,
    
    // AI Features
    aiRecommendations: false,
    riskAssessment: false,
    predictiveScheduling: false,
    aiChat: false,
    aiFeatureSuggestions: false,
    
    // Reporting & Analytics
    basicReports: true,
    advancedReports: false,
    exportToExcel: false,
    exportToPdf: false,
    customReports: false,
    realTimeAnalytics: false,
    
    // Data Management
    basicAnalytics: true,
    advancedAnalytics: false,
    realTimeMonitoring: false,
    dataRetentionDays: 365, // 1 year
    
    // System Features
    maxUsers: 10,
    auditLogs: false,
    apiAccess: false,
    webhooks: false,
    customBranding: false,
    prioritySupport: false,
    slaGuarantee: false,
    
    // Workflow Features
    workflowAutomation: false,
    approvalWorkflows: false,
    notificationCustomization: false
  },
  
  [SubscriptionTier.ADVANCED]: {
    // Scheduler Features
    timelineViews: true,
    multiResourceScheduling: true,
    recurringAppointments: true,
    dragDropScheduling: true,
    shiftManagement: true,
    
    // Patient Management
    maxPatients: 'unlimited',
    advancedPatientSearch: true,
    patientHistory: true,
    bulkPatientImport: true,
    
    // Equipment Management
    equipmentTracking: true,
    equipmentUsageAnalytics: true,
    equipmentMaintenanceScheduling: true,
    
    // AI Features
    aiRecommendations: true,
    riskAssessment: true,
    predictiveScheduling: true,
    aiChat: true,
    aiFeatureSuggestions: true,
    
    // Reporting & Analytics
    basicReports: true,
    advancedReports: true,
    exportToExcel: true,
    exportToPdf: true,
    customReports: true,
    realTimeAnalytics: true,
    
    // Data Management
    basicAnalytics: true,
    advancedAnalytics: true,
    realTimeMonitoring: true,
    dataRetentionDays: 1825, // 5 years
    
    // System Features
    maxUsers: 'unlimited',
    auditLogs: true,
    apiAccess: true,
    webhooks: true,
    customBranding: true,
    prioritySupport: true,
    slaGuarantee: true,
    
    // Workflow Features
    workflowAutomation: true,
    approvalWorkflows: true,
    notificationCustomization: true
  },
  
  [SubscriptionTier.ENTERPRISE]: {
    // Enterprise tier - all features enabled
    // This is a placeholder for future expansion
    timelineViews: true,
    multiResourceScheduling: true,
    recurringAppointments: true,
    dragDropScheduling: true,
    shiftManagement: true,
    maxPatients: 'unlimited',
    advancedPatientSearch: true,
    patientHistory: true,
    bulkPatientImport: true,
    equipmentTracking: true,
    equipmentUsageAnalytics: true,
    equipmentMaintenanceScheduling: true,
    aiRecommendations: true,
    riskAssessment: true,
    predictiveScheduling: true,
    aiChat: true,
    aiFeatureSuggestions: true,
    basicReports: true,
    advancedReports: true,
    exportToExcel: true,
    exportToPdf: true,
    customReports: true,
    realTimeAnalytics: true,
    basicAnalytics: true,
    advancedAnalytics: true,
    realTimeMonitoring: true,
    dataRetentionDays: 3650, // 10 years
    maxUsers: 'unlimited',
    auditLogs: true,
    apiAccess: true,
    webhooks: true,
    customBranding: true,
    prioritySupport: true,
    slaGuarantee: true,
    workflowAutomation: true,
    approvalWorkflows: true,
    notificationCustomization: true
  }
};

// Subscription plans with pricing
export const SUBSCRIPTION_PLANS: SubscriptionPlan[] = [
  {
    tier: SubscriptionTier.BASIC,
    name: 'Basic',
    description: 'Perfect for small clinics with single-shift operations',
    monthlyPrice: 299,
    annualPrice: 2990, // ~17% discount
    features: SUBSCRIPTION_FEATURES[SubscriptionTier.BASIC],
    popular: false
  },
  {
    tier: SubscriptionTier.ADVANCED,
    name: 'Advanced',
    description: 'Ideal for large hospitals with multi-shift operations',
    monthlyPrice: 899,
    annualPrice: 8990, // ~17% discount
    features: SUBSCRIPTION_FEATURES[SubscriptionTier.ADVANCED],
    popular: true
  },
  {
    tier: SubscriptionTier.ENTERPRISE,
    name: 'Enterprise',
    description: 'Custom solution for healthcare networks',
    monthlyPrice: 0, // Custom pricing
    annualPrice: 0, // Custom pricing
    features: SUBSCRIPTION_FEATURES[SubscriptionTier.ENTERPRISE],
    popular: false
  }
];

// Helper function to get plan by tier
export function getPlanByTier(tier: SubscriptionTier): SubscriptionPlan | undefined {
  return SUBSCRIPTION_PLANS.find(plan => plan.tier === tier);
}

// Helper function to check if feature is available
export function isFeatureAvailable(tier: SubscriptionTier, featureName: keyof SubscriptionFeatures): boolean {
  return SUBSCRIPTION_FEATURES[tier][featureName] as boolean;
}
