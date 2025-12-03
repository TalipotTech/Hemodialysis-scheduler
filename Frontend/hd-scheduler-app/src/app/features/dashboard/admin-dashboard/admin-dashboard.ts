import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Location } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatInputModule } from '@angular/material/input';
import { MatNativeDateModule } from '@angular/material/core';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { ScheduleService } from '../../../core/services/schedule.service';
import { OnDutyWidgetComponent } from '../../../components/on-duty-widget/on-duty-widget.component';
import { StaffingStatusWidgetComponent } from '../../../components/staffing-status-widget/staffing-status-widget.component';
import { SystemOverviewWidgetComponent } from '../../../components/system-overview-widget/system-overview-widget.component';
import { SidebarNavComponent } from '../../../shared/components/sidebar-nav/sidebar-nav.component';

@Component({
  selector: 'app-admin-dashboard',
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatTooltipModule,
    MatProgressSpinnerModule,
    MatSelectModule,
    MatFormFieldModule,
    MatDatepickerModule,
    MatInputModule,
    MatNativeDateModule,
    OnDutyWidgetComponent,
    StaffingStatusWidgetComponent,
    SystemOverviewWidgetComponent,
    SidebarNavComponent
  ],
  templateUrl: './admin-dashboard.html',
  styleUrl: './admin-dashboard.scss',
})
export class AdminDashboard implements OnInit {
  userRole: string = '';
  statistics: any = null;
  loading = false;
  
  // Filter options
  filterType: 'day' | 'week' | 'month' | 'year' = 'day';
  selectedDate: Date = new Date();

  constructor(
    private authService: AuthService,
    private scheduleService: ScheduleService,
    private router: Router,
    private location: Location
  ) {
    this.userRole = this.authService.getUserRole() || '';
  }

  ngOnInit(): void {
    this.loadStatistics();
  }

  loadStatistics(): void {
    this.loading = true;
    const dateString = this.selectedDate.toISOString().split('T')[0];
    
    this.scheduleService.getPatientStatistics(dateString).subscribe({
      next: (response) => {
        if (response.success) {
          this.statistics = response.data;
        }
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading statistics:', error);
        this.loading = false;
      }
    });
  }

  onDateChange(date: Date): void {
    this.selectedDate = date;
    this.loadStatistics();
  }

  onFilterTypeChange(type: 'day' | 'week' | 'month' | 'year'): void {
    this.filterType = type;
  }

  getDisplayedStats(): any {
    if (!this.statistics) return null;
    return this.statistics[this.filterType];
  }

  refreshStatistics(): void {
    this.loadStatistics();
  }

  goToToday(): void {
    this.selectedDate = new Date();
    this.loadStatistics();
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
