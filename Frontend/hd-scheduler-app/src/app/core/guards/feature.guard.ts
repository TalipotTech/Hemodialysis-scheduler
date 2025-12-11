import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { SubscriptionService } from '../services/subscription.service';
import { SubscriptionFeatures } from '../models/subscription.model';

/**
 * Guard to protect routes based on subscription features
 * Usage: canActivate: [featureGuard('timelineViews')]
 */
export function featureGuard(featureName: keyof SubscriptionFeatures): CanActivateFn {
  return () => {
    const subscriptionService = inject(SubscriptionService);
    const router = inject(Router);
    
    if (subscriptionService.hasFeature(featureName)) {
      return true;
    }
    
    // Show upgrade modal or redirect
    subscriptionService.showUpgradeModal(`This feature requires a subscription upgrade.`);
    router.navigate(['/upgrade']);
    return false;
  };
}

/**
 * Guard to check if subscription is active
 */
export const subscriptionActiveGuard: CanActivateFn = () => {
  const subscriptionService = inject(SubscriptionService);
  const router = inject(Router);
  
  if (subscriptionService.isActive()) {
    return true;
  }
  
  router.navigate(['/subscription-expired']);
  return false;
};
