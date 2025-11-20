import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Location } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { AuthService } from '../../../core/services/auth.service';
import { OnDutyWidgetComponent } from '../../../components/on-duty-widget/on-duty-widget.component';
import { StaffingStatusWidgetComponent } from '../../../components/staffing-status-widget/staffing-status-widget.component';

@Component({
  selector: 'app-hod-dashboard',
  imports: [CommonModule, MatCardModule, MatButtonModule, MatIconModule, OnDutyWidgetComponent, StaffingStatusWidgetComponent],
  templateUrl: './hod-dashboard.html',
  styleUrl: './hod-dashboard.scss',
})
export class HodDashboard {
  constructor(
    private authService: AuthService,
    private router: Router,
    private location: Location
  ) {}

  goBack(): void {
    this.location.back();
  }

  navigateToSchedule(): void {
    this.router.navigate(['/schedule']);
  }

  navigateToShiftSchedule(): void {
    this.router.navigate(['/shift-schedule']);
  }

  navigateToStaffManagement(): void {
    this.router.navigate(['/admin/staff-management']);
  }

  navigateToReports(): void {
    this.router.navigate(['/admin/reports']);
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
