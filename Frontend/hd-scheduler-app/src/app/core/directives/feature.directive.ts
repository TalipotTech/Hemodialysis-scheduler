import { Directive, Input, TemplateRef, ViewContainerRef, OnInit, OnDestroy, inject } from '@angular/core';
import { Subject, takeUntil } from 'rxjs';
import { SubscriptionService } from '../services/subscription.service';
import { SubscriptionFeatures } from '../models/subscription.model';

/**
 * Structural directive to conditionally show/hide elements based on subscription features
 * 
 * Usage:
 * <div *appFeature="'timelineViews'">
 *   This content only shows for subscriptions with timeline views
 * </div>
 * 
 * <div *appFeature="'aiRecommendations'; else upgradeTemplate">
 *   AI features content
 * </div>
 * <ng-template #upgradeTemplate>
 *   <button>Upgrade to access AI features</button>
 * </ng-template>
 */
@Directive({
  selector: '[appFeature]',
  standalone: true
})
export class FeatureDirective implements OnInit, OnDestroy {
  private subscriptionService = inject(SubscriptionService);
  private templateRef = inject(TemplateRef<any>);
  private viewContainer = inject(ViewContainerRef);
  private destroy$ = new Subject<void>();
  
  private featureName!: keyof SubscriptionFeatures;
  private elseTemplateRef: TemplateRef<any> | null = null;

  @Input() set appFeature(featureName: keyof SubscriptionFeatures) {
    this.featureName = featureName;
    this.updateView();
  }

  @Input() set appFeatureElse(templateRef: TemplateRef<any>) {
    this.elseTemplateRef = templateRef;
    this.updateView();
  }

  ngOnInit() {
    // Subscribe to feature changes
    this.subscriptionService.features$
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.updateView();
      });
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private updateView() {
    this.viewContainer.clear();
    
    if (this.subscriptionService.hasFeature(this.featureName)) {
      // Feature is available, show main template
      this.viewContainer.createEmbeddedView(this.templateRef);
    } else if (this.elseTemplateRef) {
      // Feature not available, show else template
      this.viewContainer.createEmbeddedView(this.elseTemplateRef);
    }
    // If no else template and feature not available, show nothing
  }
}

/**
 * Directive to show content when feature is NOT available (opposite of appFeature)
 * 
 * Usage:
 * <button *appFeatureLocked="'aiRecommendations'" (click)="showUpgrade()">
 *   Unlock AI Features - Upgrade Now
 * </button>
 */
@Directive({
  selector: '[appFeatureLocked]',
  standalone: true
})
export class FeatureLockedDirective implements OnInit, OnDestroy {
  private subscriptionService = inject(SubscriptionService);
  private templateRef = inject(TemplateRef<any>);
  private viewContainer = inject(ViewContainerRef);
  private destroy$ = new Subject<void>();
  
  private featureName!: keyof SubscriptionFeatures;

  @Input() set appFeatureLocked(featureName: keyof SubscriptionFeatures) {
    this.featureName = featureName;
    this.updateView();
  }

  ngOnInit() {
    this.subscriptionService.features$
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.updateView();
      });
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private updateView() {
    this.viewContainer.clear();
    
    // Show only if feature is NOT available
    if (!this.subscriptionService.hasFeature(this.featureName)) {
      this.viewContainer.createEmbeddedView(this.templateRef);
    }
  }
}
