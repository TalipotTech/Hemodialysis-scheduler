import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, NavigationEnd, ActivatedRoute } from '@angular/router';
import { filter, distinctUntilChanged } from 'rxjs/operators';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Location } from '@angular/common';
import { AuthService } from '../../../core/services/auth.service';

export interface Breadcrumb {
  label: string;
  url: string;
}

@Component({
  selector: 'app-breadcrumb',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatButtonModule, MatTooltipModule],
  templateUrl: './breadcrumb.component.html',
  styleUrl: './breadcrumb.component.scss'
})
export class BreadcrumbComponent implements OnInit {
  breadcrumbs: Breadcrumb[] = [];

  constructor(
    private router: Router,
    private activatedRoute: ActivatedRoute,
    private location: Location,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.router.events
      .pipe(
        filter((event) => event instanceof NavigationEnd),
        distinctUntilChanged()
      )
      .subscribe(() => {
        this.breadcrumbs = this.buildBreadcrumbs(this.activatedRoute.root);
      });

    // Initial breadcrumbs
    this.breadcrumbs = this.buildBreadcrumbs(this.activatedRoute.root);
  }

  private buildBreadcrumbs(
    route: ActivatedRoute,
    url: string = '',
    breadcrumbs: Breadcrumb[] = []
  ): Breadcrumb[] {
    const children: ActivatedRoute[] = route.children;

    if (children.length === 0) {
      return breadcrumbs;
    }

    for (const child of children) {
      const routeURL: string = child.snapshot.url
        .map((segment) => segment.path)
        .join('/');

      if (routeURL !== '') {
        url += `/${routeURL}`;
      }

      const label = child.snapshot.data['breadcrumb'] || this.getDefaultLabel(routeURL);
      
      if (label && !breadcrumbs.some(b => b.url === url)) {
        breadcrumbs.push({ label, url });
      }

      return this.buildBreadcrumbs(child, url, breadcrumbs);
    }

    return breadcrumbs;
  }

  private getDefaultLabel(path: string): string {
    if (!path) return '';

    // Custom labels for common routes
    const labelMap: { [key: string]: string } = {
      'admin': 'Admin Dashboard',
      'patients': 'Patient Management',
      'patient-list': 'Patient List',
      'patient-form': 'Patient Form',
      'schedule': 'HD Schedule',
      'hd-session-form': 'HD Session Form',
      'vital-monitoring': 'Vital Monitoring',
      'shift-schedule': 'Shift Schedule',
      'user-management': 'User Management',
      'staff-management': 'Staff Management',
      'system-settings': 'System Settings',
      'ai-settings': 'AI Integration',
      'reports': 'Reports',
      'audit-logs': 'Audit Logs',
      'patient-history': 'Patient History',
      'doctor': 'Doctor Dashboard',
      'hod': 'HOD Dashboard',
      'nurse': 'Nurse Dashboard',
      'technician': 'Technician Dashboard',
      'new': 'New',
      'edit': 'Edit'
    };

    // Check for direct match
    if (labelMap[path]) {
      return labelMap[path];
    }

    // Check for ID patterns (numbers)
    if (/^\d+$/.test(path)) {
      return `Details`;
    }

    // Format path as title case
    return path
      .split('-')
      .map(word => word.charAt(0).toUpperCase() + word.slice(1))
      .join(' ');
  }

  navigateTo(url: string): void {
    this.router.navigate([url]);
  }

  goBack(): void {
    this.location.back();
  }

  isDashboard(): boolean {
    const currentUrl = this.router.url;
    const dashboardRoutes = ['/admin', '/doctor', '/hod', '/nurse', '/technician'];
    return dashboardRoutes.some(route => currentUrl === route || currentUrl === route + '/');
  }

  goHome(): void {
    // Navigate to appropriate dashboard based on user role
    const userRole = this.authService.getUserRole();
    
    switch(userRole?.toLowerCase()) {
      case 'admin':
        this.router.navigate(['/admin']);
        break;
      case 'doctor':
        this.router.navigate(['/doctor']);
        break;
      case 'hod':
        this.router.navigate(['/hod']);
        break;
      case 'nurse':
        this.router.navigate(['/nurse']);
        break;
      case 'technician':
        this.router.navigate(['/technician']);
        break;
      default:
        this.router.navigate(['/login']);
    }
  }

  toggleMenu(): void {
    // Emit event to toggle sidebar
    window.dispatchEvent(new CustomEvent('toggleSidebar'));
  }
}
