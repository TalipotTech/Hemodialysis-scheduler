import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Location } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { AuthService } from '../../../core/services/auth.service';
import { OnDutyWidgetComponent } from '../../../components/on-duty-widget/on-duty-widget.component';
import { StaffingStatusWidgetComponent } from '../../../components/staffing-status-widget/staffing-status-widget.component';
import { SystemOverviewWidgetComponent } from '../../../components/system-overview-widget/system-overview-widget.component';
import { SidebarNavComponent } from '../../../shared/components/sidebar-nav/sidebar-nav.component';

@Component({
  selector: 'app-admin-dashboard',
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatTooltipModule,
    OnDutyWidgetComponent,
    StaffingStatusWidgetComponent,
    SystemOverviewWidgetComponent,
    SidebarNavComponent
  ],
  templateUrl: './admin-dashboard.html',
  styleUrl: './admin-dashboard.scss',
})
export class AdminDashboard {
  userRole: string = '';

  constructor(
    private authService: AuthService,
    private router: Router,
    private location: Location
  ) {
    this.userRole = this.authService.getUserRole() || '';
  }

  goBack(): void {
    this.location.back();
  }

  goHome(): void {
    this.router.navigate(['/admin']);
  }

  navigateToPatients(): void {
    this.router.navigate(['/patients']);
  }

  navigateToSchedule(): void {
    this.router.navigate(['/schedule']);
  }

  navigateToUserManagement(): void {
    this.router.navigate(['/admin/user-management']);
  }

  navigateToStaffManagement(): void {
    this.router.navigate(['/admin/staff-management']);
  }

  navigateToSystemSettings(): void {
    this.router.navigate(['/admin/system-settings']);
  }

  navigateToReports(): void {
    this.router.navigate(['/admin/reports']);
  }

  navigateToAuditLogs(): void {
    this.router.navigate(['/admin/audit-logs']);
  }

  navigateToShiftSchedule(): void {
    this.router.navigate(['/shift-schedule']);
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
