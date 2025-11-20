import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAuthenticated()) {
    // Check for required roles
    const requiredRoles = route.data['roles'] as string[];
    
    if (requiredRoles && requiredRoles.length > 0) {
      const userRole = authService.getUserRole();
      
      if (userRole && requiredRoles.includes(userRole)) {
        return true;
      } else {
        // User doesn't have required role, redirect to unauthorized page
        router.navigate(['/unauthorized']);
        return false;
      }
    }
    
    // No specific roles required, just authentication
    return true;
  }

  // Not authenticated, redirect to login
  router.navigate(['/login'], { queryParams: { returnUrl: state.url } });
  return false;
};
