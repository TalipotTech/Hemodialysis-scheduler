# Implementation Examples

## How to Use Subscription Features in Your Application

### 1. Using Feature Directive in Templates

```typescript
// In your component
import { FeatureDirective, FeatureLockedDirective } from './core/directives/feature.directive';

@Component({
  selector: 'app-scheduler',
  imports: [FeatureDirective, FeatureLockedDirective, ...],
  template: `
    <!-- Show timeline views only for Advanced tier -->
    <div *appFeature="'timelineViews'">
      <button (click)="switchToTimeline()">Timeline View</button>
    </div>

    <!-- Show upgrade button when feature is locked -->
    <div *appFeatureLocked="'timelineViews'">
      <button class="upgrade-btn" (click)="showUpgrade()">
        🔒 Upgrade to unlock Timeline Views
      </button>
    </div>

    <!-- With else template -->
    <div *appFeature="'aiRecommendations'; else upgradeTemplate">
      <app-ai-recommendations></app-ai-recommendations>
    </div>
    <ng-template #upgradeTemplate>
      <div class="locked-feature">
        <mat-icon>lock</mat-icon>
        <h3>AI Recommendations</h3>
        <p>Available in Advanced tier</p>
        <button mat-raised-button color="primary" (click)="upgrade()">
          Upgrade Now
        </button>
      </div>
    </ng-template>
  `
})
```

### 2. Using in TypeScript Code

```typescript
import { SubscriptionService } from './core/services/subscription.service';

export class SchedulerComponent {
  private subscriptionService = inject(SubscriptionService);

  loadSchedulerViews() {
    // Conditionally load views based on subscription
    if (this.subscriptionService.hasFeature('timelineViews')) {
      this.views = ['Day', 'Week', 'Month', 'TimelineDay', 'TimelineWeek'];
    } else {
      this.views = ['Day', 'Week', 'Month'];  // Basic tier only
    }
  }

  onAddPatient() {
    const currentCount = this.patients.length;
    const maxPatients = this.subscriptionService.getFeatureValue<number>('maxPatients');
    
    if (this.subscriptionService.isLimitReached('maxPatients', currentCount)) {
      this.subscriptionService.showUpgradeModal(
        `You've reached your patient limit of ${maxPatients}. Upgrade to add more patients.`
      );
      return;
    }
    
    // Proceed with adding patient
    this.addPatient();
  }

  setupEquipmentTracking() {
    if (!this.subscriptionService.hasFeature('equipmentTracking')) {
      // Hide equipment menu or show locked state
      this.equipmentMenuVisible = false;
      return;
    }
    
    // Initialize equipment tracking
    this.initializeEquipmentTracking();
  }
}
```

### 3. Using Feature Guards in Routes

```typescript
// app.routes.ts
import { featureGuard } from './core/guards/feature.guard';

export const routes: Routes = [
  {
    path: 'analytics',
    loadComponent: () => import('./features/analytics/analytics.component'),
    canActivate: [authGuard, featureGuard('advancedAnalytics')],
    data: { roles: ['Admin'], breadcrumb: 'Analytics' }
  },
  {
    path: 'ai-chat',
    loadComponent: () => import('./components/ai-chat/ai-chat.component'),
    canActivate: [authGuard, featureGuard('aiChat')],
    data: { roles: ['Admin', 'Doctor'], breadcrumb: 'AI Assistant' }
  },
  {
    path: 'equipment',
    loadComponent: () => import('./features/equipment/equipment.component'),
    canActivate: [authGuard, featureGuard('equipmentTracking')],
    data: { roles: ['Admin', 'Technician'], breadcrumb: 'Equipment Management' }
  }
];
```

### 4. Dynamic Scheduler Configuration

```typescript
// syncfusion-scheduler.component.ts
import { SubscriptionService } from './core/services/subscription.service';

export class SyncfusionSchedulerComponent {
  private subscriptionService = inject(SubscriptionService);
  
  public views: View[] = [];
  public showResourceGrouping = false;
  public enableRecurrence = false;

  ngOnInit() {
    this.configureSchedulerBasedOnTier();
  }

  private configureSchedulerBasedOnTier() {
    // Base views (available in all tiers)
    this.views = ['Day', 'Week', 'Month'];
    
    // Add timeline views for Advanced tier
    if (this.subscriptionService.hasFeature('timelineViews')) {
      this.views.push('TimelineDay', 'TimelineWeek', 'TimelineMonth');
    }
    
    // Enable resource grouping for Advanced tier
    this.showResourceGrouping = this.subscriptionService.hasFeature('multiResourceScheduling');
    
    // Enable recurring appointments for Advanced tier
    this.enableRecurrence = this.subscriptionService.hasFeature('recurringAppointments');
    
    // Configure event settings
    this.eventSettings = {
      dataSource: this.appointments,
      enableRecurrence: this.enableRecurrence
    };
    
    // Configure resources if available
    if (this.showResourceGrouping) {
      this.resources = this.bedResources;
      this.group = { resources: ['Beds', 'Shifts'] };
    }
  }
}
```

### 5. Grid with Conditional Export

```typescript
export class PatientGridComponent {
  private subscriptionService = inject(SubscriptionService);
  
  public toolbar: string[] = ['Search'];

  ngOnInit() {
    this.configureToolbar();
  }

  private configureToolbar() {
    // Base toolbar
    this.toolbar = ['Search'];
    
    // Add export options for Advanced tier
    if (this.subscriptionService.hasFeature('exportToExcel')) {
      this.toolbar.push('ExcelExport');
    }
    
    if (this.subscriptionService.hasFeature('exportToPdf')) {
      this.toolbar.push('PdfExport');
    }
  }

  onExcelExport() {
    if (!this.subscriptionService.hasFeature('exportToExcel')) {
      this.subscriptionService.showUpgradeModal('Excel export is only available in Advanced tier.');
      return;
    }
    
    this.grid.excelExport();
  }
}
```

### 6. Showing Tier Badges

```html
<!-- In your navigation or dashboard -->
<div class="subscription-badge">
  <span class="tier-name">{{ currentTier$ | async }}</span>
  <button *appFeatureLocked="'timelineViews'" (click)="showPricingPage()">
    Upgrade
  </button>
</div>

<div class="feature-lock-overlay" *appFeatureLocked="'aiRecommendations'">
  <div class="lock-content">
    <mat-icon>lock</mat-icon>
    <h3>AI Features Locked</h3>
    <p>Upgrade to Advanced tier to unlock AI-powered recommendations</p>
    <button mat-raised-button color="primary" routerLink="/pricing">
      View Plans
    </button>
  </div>
</div>
```

### 7. Patient Limit Check

```typescript
export class PatientService {
  private subscriptionService = inject(SubscriptionService);

  async addPatient(patient: Patient): Promise<void> {
    // Check current patient count
    const currentCount = await this.getPatientCount();
    
    if (this.subscriptionService.isLimitReached('maxPatients', currentCount)) {
      const limit = this.subscriptionService.getFeatureValue('maxPatients');
      throw new Error(`Patient limit reached (${limit}). Please upgrade your subscription.`);
    }
    
    // Proceed with adding patient
    await this.savePatient(patient);
  }
}
```

### 8. Subscription Status Display

```typescript
export class SubscriptionStatusComponent {
  private subscriptionService = inject(SubscriptionService);
  
  public subscription$ = this.subscriptionService.subscription$;
  public daysUntilExpiry = 0;
  public showRenewalWarning = false;

  ngOnInit() {
    this.daysUntilExpiry = this.subscriptionService.getDaysUntilExpiry();
    this.showRenewalWarning = this.daysUntilExpiry <= 30;
  }
}
```

```html
<div class="subscription-status" *ngIf="subscription$ | async as sub">
  <div class="status-header">
    <h3>{{ sub.tier | titlecase }} Plan</h3>
    <span class="status-badge" [class.active]="sub.status === 'active'">
      {{ sub.status }}
    </span>
  </div>
  
  <div class="subscription-details">
    <p>Expires: {{ sub.expiryDate | date }}</p>
    <p *ngIf="showRenewalWarning" class="warning">
      ⚠️ Subscription expires in {{ daysUntilExpiry }} days
    </p>
  </div>
  
  <div class="usage-stats">
    <div class="stat">
      <span>Patients:</span>
      <span>{{ sub.currentPatientCount }} / {{ sub.maxPatients }}</span>
    </div>
    <div class="stat">
      <span>Users:</span>
      <span>{{ sub.currentUserCount }} / {{ sub.maxUsers }}</span>
    </div>
  </div>
</div>
```

### 9. Feature Comparison Page

```typescript
export class PricingComponent {
  public plans = SUBSCRIPTION_PLANS;
  public currentTier = this.subscriptionService.getCurrentTier();

  constructor(private subscriptionService: SubscriptionService) {}

  isCurrentPlan(tier: SubscriptionTier): boolean {
    return this.currentTier === tier;
  }

  async upgradeTo(tier: SubscriptionTier) {
    try {
      await this.subscriptionService.upgradeTier(tier).toPromise();
      // Show success message
    } catch (error) {
      // Show error message
    }
  }
}
```

## Summary

This architecture allows you to:
1. ✅ Use a **single codebase** with Syncfusion
2. ✅ Control features with **simple flags**
3. ✅ Show/hide features with **directives**
4. ✅ Protect routes with **guards**
5. ✅ Check limits **programmatically**
6. ✅ Easy to add **new tiers**
7. ✅ Simple to **upgrade/downgrade**

The implementation is clean, maintainable, and scalable!
