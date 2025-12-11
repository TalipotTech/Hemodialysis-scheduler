# Hemodialysis Scheduler - Tiered Pricing Architecture

## Overview
Single codebase with feature flags to enable/disable features based on subscription tier.

## Subscription Tiers

### 🥉 BASIC TIER
**Target**: Small clinics, single-shift operations
**Price**: $299/month

**Included Features:**
- ✅ Basic scheduler (Day/Week/Month views only)
- ✅ Patient management (list, add, edit)
- ✅ Simple appointment booking
- ✅ Basic reporting
- ✅ User management (up to 10 users)
- ✅ Single shift scheduling
- ✅ Standard data grid
- ❌ No Timeline views
- ❌ No multi-resource management
- ❌ No AI features
- ❌ No advanced analytics
- ❌ No recurring appointments
- ❌ No equipment tracking
- ❌ Limited to 50 patients

### 🥇 ADVANCED TIER
**Target**: Large hospitals, multi-shift operations
**Price**: $899/month

**Included Features:**
- ✅ All Basic features PLUS:
- ✅ Advanced scheduler (Timeline views)
- ✅ Multi-resource management (beds, equipment)
- ✅ Multi-shift scheduling (Morning/Afternoon/Night)
- ✅ Recurring appointments
- ✅ Equipment tracking & management
- ✅ AI-powered recommendations
- ✅ Advanced analytics dashboard
- ✅ Risk assessment
- ✅ Predictive scheduling
- ✅ Advanced reporting (Excel/PDF export)
- ✅ Audit logs
- ✅ API access
- ✅ Unlimited patients
- ✅ Unlimited users
- ✅ Priority support

## Technical Implementation

### 1. Subscription Model
```typescript
export enum SubscriptionTier {
  BASIC = 'basic',
  ADVANCED = 'advanced',
  ENTERPRISE = 'enterprise'  // Future expansion
}

export interface SubscriptionFeatures {
  // Scheduler
  timelineViews: boolean;
  multiResourceScheduling: boolean;
  recurringAppointments: boolean;
  dragDropScheduling: boolean;
  
  // Patient Management
  maxPatients: number | 'unlimited';
  advancedPatientSearch: boolean;
  patientHistory: boolean;
  
  // Equipment
  equipmentTracking: boolean;
  equipmentUsageAnalytics: boolean;
  
  // AI Features
  aiRecommendations: boolean;
  riskAssessment: boolean;
  predictiveScheduling: boolean;
  aiChat: boolean;
  
  // Reporting
  basicReports: boolean;
  advancedReports: boolean;
  exportToExcel: boolean;
  exportToPdf: boolean;
  customReports: boolean;
  
  // Analytics
  basicAnalytics: boolean;
  advancedAnalytics: boolean;
  realTimeMonitoring: boolean;
  
  // System
  maxUsers: number | 'unlimited';
  auditLogs: boolean;
  apiAccess: boolean;
  prioritySupport: boolean;
}
```

### 2. Feature Configuration
```typescript
export const SUBSCRIPTION_FEATURES: Record<SubscriptionTier, SubscriptionFeatures> = {
  [SubscriptionTier.BASIC]: {
    // Scheduler
    timelineViews: false,
    multiResourceScheduling: false,
    recurringAppointments: false,
    dragDropScheduling: true,
    
    // Patient Management
    maxPatients: 50,
    advancedPatientSearch: false,
    patientHistory: true,
    
    // Equipment
    equipmentTracking: false,
    equipmentUsageAnalytics: false,
    
    // AI Features
    aiRecommendations: false,
    riskAssessment: false,
    predictiveScheduling: false,
    aiChat: false,
    
    // Reporting
    basicReports: true,
    advancedReports: false,
    exportToExcel: false,
    exportToPdf: false,
    customReports: false,
    
    // Analytics
    basicAnalytics: true,
    advancedAnalytics: false,
    realTimeMonitoring: false,
    
    // System
    maxUsers: 10,
    auditLogs: false,
    apiAccess: false,
    prioritySupport: false
  },
  
  [SubscriptionTier.ADVANCED]: {
    // Scheduler
    timelineViews: true,
    multiResourceScheduling: true,
    recurringAppointments: true,
    dragDropScheduling: true,
    
    // Patient Management
    maxPatients: 'unlimited',
    advancedPatientSearch: true,
    patientHistory: true,
    
    // Equipment
    equipmentTracking: true,
    equipmentUsageAnalytics: true,
    
    // AI Features
    aiRecommendations: true,
    riskAssessment: true,
    predictiveScheduling: true,
    aiChat: true,
    
    // Reporting
    basicReports: true,
    advancedReports: true,
    exportToExcel: true,
    exportToPdf: true,
    customReports: true,
    
    // Analytics
    basicAnalytics: true,
    advancedAnalytics: true,
    realTimeMonitoring: true,
    
    // System
    maxUsers: 'unlimited',
    auditLogs: true,
    apiAccess: true,
    prioritySupport: true
  }
};
```

### 3. Usage in Components
```typescript
// Example: Scheduler Component
if (subscriptionService.hasFeature('timelineViews')) {
  this.views = ['Day', 'Week', 'Month', 'TimelineDay', 'TimelineWeek'];
} else {
  this.views = ['Day', 'Week', 'Month'];  // Basic tier
}

// Example: Patient Management
if (patientCount >= subscriptionService.getFeature('maxPatients')) {
  showUpgradeModal('You have reached the patient limit for your subscription tier.');
}

// Example: Equipment Tracking
<div *ngIf="hasFeature('equipmentTracking')">
  <app-equipment-management></app-equipment-management>
</div>
```

## Architecture Benefits

### ✅ Advantages
1. **Single Codebase**: Easier to maintain
2. **Consistent UX**: Same design language
3. **Smaller Bundle**: Only Syncfusion, no dual libraries
4. **Easy Upgrades**: Just flip feature flags
5. **Better Testing**: Test once, deploy to both tiers
6. **Flexible Pricing**: Easy to add new tiers
7. **Progressive Enhancement**: Users can upgrade anytime

### ✅ Technical Benefits
1. **Feature Flags**: Control access dynamically
2. **Runtime Configuration**: No need to rebuild
3. **Database-Driven**: Store tier in user/organization table
4. **Guard-Protected Routes**: Automatic route blocking
5. **Component-Level Control**: Show/hide features
6. **API-Level Validation**: Backend enforces limits

## Database Schema

```sql
-- Organizations Table
ALTER TABLE Organizations ADD COLUMN subscription_tier VARCHAR(20) DEFAULT 'basic';
ALTER TABLE Organizations ADD COLUMN subscription_expires_at DATETIME;
ALTER TABLE Organizations ADD COLUMN max_patients INT DEFAULT 50;
ALTER TABLE Organizations ADD COLUMN max_users INT DEFAULT 10;

-- Feature Usage Tracking
CREATE TABLE FeatureUsage (
    id INT PRIMARY KEY IDENTITY,
    organization_id INT FOREIGN KEY REFERENCES Organizations(id),
    feature_name VARCHAR(100),
    usage_count INT,
    last_used DATETIME,
    CONSTRAINT FK_FeatureUsage_Org FOREIGN KEY (organization_id) REFERENCES Organizations(id)
);

-- Subscription History
CREATE TABLE SubscriptionHistory (
    id INT PRIMARY KEY IDENTITY,
    organization_id INT,
    tier VARCHAR(20),
    started_at DATETIME,
    ended_at DATETIME,
    amount_paid DECIMAL(10,2),
    CONSTRAINT FK_SubHistory_Org FOREIGN KEY (organization_id) REFERENCES Organizations(id)
);
```

## Implementation Plan

### Phase 1: Infrastructure (Week 1)
- [ ] Create subscription models and enums
- [ ] Create SubscriptionService
- [ ] Create FeatureGuard
- [ ] Create subscription configuration
- [ ] Update database schema

### Phase 2: Backend (Week 1-2)
- [ ] Add subscription tier to User/Organization
- [ ] Create subscription endpoints
- [ ] Add feature validation middleware
- [ ] Implement usage tracking
- [ ] Add upgrade/downgrade endpoints

### Phase 3: Frontend (Week 2-3)
- [ ] Implement SubscriptionService
- [ ] Add feature guards to routes
- [ ] Add *ngIf directives for features
- [ ] Create upgrade prompts
- [ ] Update scheduler based on tier
- [ ] Hide/show grid features

### Phase 4: UI/UX (Week 3)
- [ ] Create subscription management page
- [ ] Add upgrade modal components
- [ ] Add feature lock indicators
- [ ] Create pricing page
- [ ] Add tier badges

### Phase 5: Testing (Week 4)
- [ ] Test basic tier limitations
- [ ] Test advanced tier features
- [ ] Test upgrade flow
- [ ] Test downgrade scenarios
- [ ] Load testing

## Migration Strategy

### For Existing Installations
1. Default all existing users to ADVANCED tier (grandfathered)
2. New signups choose tier during registration
3. Allow tier changes with billing cycle alignment

## Pricing Comparison

| Feature | Basic ($299/mo) | Advanced ($899/mo) |
|---------|----------------|-------------------|
| **Scheduler Views** | Day, Week, Month | + Timeline, Resources |
| **Max Patients** | 50 | Unlimited |
| **Max Users** | 10 | Unlimited |
| **Shifts** | Single | Multiple |
| **Equipment Tracking** | ❌ | ✅ |
| **AI Features** | ❌ | ✅ |
| **Advanced Reports** | ❌ | ✅ |
| **Audit Logs** | ❌ | ✅ |
| **API Access** | ❌ | ✅ |
| **Support** | Email | Priority Phone |

## Complexity Assessment

| Aspect | Complexity | Effort |
|--------|-----------|---------|
| Backend Changes | Medium | 2 weeks |
| Frontend Changes | Medium | 2 weeks |
| Database Schema | Low | 3 days |
| Testing | Medium | 1 week |
| Documentation | Low | 3 days |
| **Total** | **Medium** | **~5-6 weeks** |

## Recommendation

✅ **Use Syncfusion for BOTH tiers with feature flags**

This approach is:
- **More maintainable** (single codebase)
- **More cost-effective** (one license)
- **Better UX** (consistent interface)
- **Easier to test** (one set of components)
- **More flexible** (easy to add tiers)
- **Lower technical debt**

## Next Steps

1. **Approve Architecture**: Review this proposal
2. **Create Feature Branch**: `feature/subscription-tiers`
3. **Implement Backend**: Add subscription logic
4. **Implement Frontend**: Add feature guards
5. **Test Thoroughly**: Both tiers
6. **Document**: User guides for each tier
7. **Deploy**: Gradual rollout

---

**Recommendation**: Proceed with feature-tier approach using Syncfusion only. This gives you maximum flexibility with minimum complexity.
