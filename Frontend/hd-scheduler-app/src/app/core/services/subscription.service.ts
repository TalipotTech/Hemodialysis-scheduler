import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { map, tap, catchError } from 'rxjs/operators';
import { 
  SubscriptionTier, 
  SubscriptionFeatures, 
  OrganizationSubscription,
  SubscriptionPlan 
} from '../models/subscription.model';
import { SUBSCRIPTION_FEATURES, SUBSCRIPTION_PLANS } from '../config/subscription.config';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class SubscriptionService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/subscription`;
  
  // Current subscription state
  private subscriptionSubject = new BehaviorSubject<OrganizationSubscription | null>(null);
  public subscription$ = this.subscriptionSubject.asObservable();
  
  // Current features based on subscription
  private featuresSubject = new BehaviorSubject<SubscriptionFeatures>(
    SUBSCRIPTION_FEATURES[SubscriptionTier.BASIC]
  );
  public features$ = this.featuresSubject.asObservable();

  constructor() {
    this.loadSubscription();
  }

  /**
   * Load current organization's subscription
   */
  private loadSubscription(): void {
    this.http.get<OrganizationSubscription>(`${this.apiUrl}/current`)
      .pipe(
        tap(subscription => {
          this.subscriptionSubject.next(subscription);
          this.featuresSubject.next(SUBSCRIPTION_FEATURES[subscription.tier]);
        }),
        catchError(error => {
          console.error('Failed to load subscription:', error);
          // Default to basic tier if error
          this.featuresSubject.next(SUBSCRIPTION_FEATURES[SubscriptionTier.BASIC]);
          return of(null);
        })
      )
      .subscribe();
  }

  /**
   * Get current subscription tier
   */
  getCurrentTier(): SubscriptionTier {
    const subscription = this.subscriptionSubject.value;
    return subscription?.tier || SubscriptionTier.BASIC;
  }

  /**
   * Get current subscription
   */
  getCurrentSubscription(): OrganizationSubscription | null {
    return this.subscriptionSubject.value;
  }

  /**
   * Check if a specific feature is available
   */
  hasFeature(featureName: keyof SubscriptionFeatures): boolean {
    const features = this.featuresSubject.value;
    const featureValue = features[featureName];
    
    // Handle boolean features
    if (typeof featureValue === 'boolean') {
      return featureValue;
    }
    
    // Handle unlimited features
    if (featureValue === 'unlimited') {
      return true;
    }
    
    // Handle numeric limits (assume available if limit > 0)
    if (typeof featureValue === 'number') {
      return featureValue > 0;
    }
    
    return false;
  }

  /**
   * Get feature value (useful for limits like maxPatients)
   */
  getFeatureValue<T = any>(featureName: keyof SubscriptionFeatures): T {
    const features = this.featuresSubject.value;
    return features[featureName] as T;
  }

  /**
   * Check if current usage exceeds limit
   */
  isLimitReached(featureName: keyof SubscriptionFeatures, currentUsage: number): boolean {
    const limit = this.getFeatureValue<number | 'unlimited'>(featureName);
    
    if (limit === 'unlimited') {
      return false;
    }
    
    if (typeof limit === 'number') {
      return currentUsage >= limit;
    }
    
    return false;
  }

  /**
   * Get all available subscription plans
   */
  getAvailablePlans(): SubscriptionPlan[] {
    return SUBSCRIPTION_PLANS;
  }

  /**
   * Upgrade subscription to a higher tier
   */
  upgradeTier(newTier: SubscriptionTier): Observable<OrganizationSubscription> {
    return this.http.post<OrganizationSubscription>(`${this.apiUrl}/upgrade`, { tier: newTier })
      .pipe(
        tap(subscription => {
          this.subscriptionSubject.next(subscription);
          this.featuresSubject.next(SUBSCRIPTION_FEATURES[subscription.tier]);
        })
      );
  }

  /**
   * Downgrade subscription to a lower tier
   */
  downgradeTier(newTier: SubscriptionTier): Observable<OrganizationSubscription> {
    return this.http.post<OrganizationSubscription>(`${this.apiUrl}/downgrade`, { tier: newTier })
      .pipe(
        tap(subscription => {
          this.subscriptionSubject.next(subscription);
          this.featuresSubject.next(SUBSCRIPTION_FEATURES[subscription.tier]);
        })
      );
  }

  /**
   * Cancel subscription
   */
  cancelSubscription(): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/cancel`, {});
  }

  /**
   * Check if subscription is active
   */
  isActive(): boolean {
    const subscription = this.subscriptionSubject.value;
    if (!subscription) return false;
    
    const now = new Date();
    const expiryDate = new Date(subscription.expiryDate);
    
    return subscription.status === 'active' && expiryDate > now;
  }

  /**
   * Check if subscription is in trial
   */
  isTrial(): boolean {
    const subscription = this.subscriptionSubject.value;
    return subscription?.isTrial || false;
  }

  /**
   * Get days until subscription expires
   */
  getDaysUntilExpiry(): number {
    const subscription = this.subscriptionSubject.value;
    if (!subscription) return 0;
    
    const now = new Date();
    const expiryDate = new Date(subscription.expiryDate);
    const diffTime = expiryDate.getTime() - now.getTime();
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
    
    return Math.max(0, diffDays);
  }

  /**
   * Show upgrade modal (to be implemented in UI)
   */
  showUpgradeModal(reason?: string): void {
    // This will be implemented with a modal component
    console.log('Upgrade required:', reason);
    // TODO: Implement modal logic
  }

  /**
   * Track feature usage
   */
  trackFeatureUsage(featureName: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/track-usage`, { featureName });
  }

  /**
   * Get feature usage statistics
   */
  getFeatureUsage(): Observable<any> {
    return this.http.get(`${this.apiUrl}/usage-stats`);
  }

  /**
   * Refresh subscription data
   */
  refreshSubscription(): void {
    this.loadSubscription();
  }
}
