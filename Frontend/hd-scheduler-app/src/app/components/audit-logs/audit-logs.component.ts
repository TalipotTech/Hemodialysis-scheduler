import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTabsModule } from '@angular/material/tabs';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { GridModule, PageService, SortService, FilterService, ToolbarService } from '@syncfusion/ej2-angular-grids';
import { Location } from '@angular/common';
import { AuditLogsService, AuditLog, AuditStatistics, UserActivity } from '../../services/audit-logs.service';

@Component({
  selector: 'app-audit-logs',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatTabsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatProgressSpinnerModule,
    MatChipsModule,
    MatTooltipModule,
    GridModule
  ],
  providers: [PageService, SortService, FilterService, ToolbarService],
  templateUrl: './audit-logs.component.html',
  styleUrls: ['./audit-logs.component.scss']
})
export class AuditLogsComponent implements OnInit {
  // All Logs
  allLogsData: AuditLog[] = [];
  allLogsLoading = true;
  
  // Login History
  loginHistoryData: AuditLog[] = [];
  loginHistoryLoading = true;
  loginDays = 30;

  // Grid settings
  public pageSettings = { pageSize: 10, pageSizes: [10, 20, 50, 100] };
  public filterSettings = { type: 'Excel' };
  public toolbar = ['Search'];

  // Statistics
  statistics: AuditStatistics | null = null;
  statisticsLoading = true;
  statisticsDays = 30;

  // User Activity
  userActivity: UserActivity[] = [];
  activityLoading = true;

  // Filters
  selectedAction = '';
  actions = ['LOGIN', 'LOGOUT', 'CREATE', 'UPDATE', 'DELETE', 'VIEW', 'EXPORT'];
  startDate: Date | null = null;
  endDate: Date | null = null;

  constructor(
    private auditLogsService: AuditLogsService,
    private location: Location
  ) {}

  ngOnInit(): void {
    this.loadAllLogs();
    this.loadLoginHistory();
    this.loadStatistics();
    this.loadUserActivity();
  }

  loadAllLogs(): void {
    this.allLogsLoading = true;
    this.auditLogsService.getAllLogs(1, 1000).subscribe({
      next: (response) => {
        this.allLogsData = response.data;
        this.allLogsLoading = false;
      },
      error: (error) => {
        console.error('Error loading audit logs:', error);
        this.allLogsLoading = false;
      }
    });
  }

  loadLoginHistory(): void {
    this.loginHistoryLoading = true;
    this.auditLogsService.getLoginHistory(undefined, this.loginDays).subscribe({
      next: (response) => {
        this.loginHistoryData = response.data;
        this.loginHistoryLoading = false;
      },
      error: (error) => {
        console.error('Error loading login history:', error);
        this.loginHistoryLoading = false;
      }
    });
  }

  loadStatistics(): void {
    this.statisticsLoading = true;
    this.auditLogsService.getStatistics(this.statisticsDays).subscribe({
      next: (response) => {
        this.statistics = response.data;
        this.statisticsLoading = false;
      },
      error: (error) => {
        console.error('Error loading statistics:', error);
        this.statisticsLoading = false;
      }
    });
  }

  loadUserActivity(): void {
    this.activityLoading = true;
    this.auditLogsService.getActivitySummary().subscribe({
      next: (response) => {
        this.userActivity = response.data;
        this.activityLoading = false;
      },
      error: (error) => {
        console.error('Error loading user activity:', error);
        this.activityLoading = false;
      }
    });
  }

  filterByAction(): void {
    if (this.selectedAction) {
      this.allLogsLoading = true;
      this.auditLogsService.getLogsByAction(this.selectedAction).subscribe({
        next: (response) => {
          this.allLogsData = response.data;
          this.allLogsLoading = false;
        },
        error: (error) => {
          console.error('Error filtering by action:', error);
          this.allLogsLoading = false;
        }
      });
    } else {
      this.loadAllLogs();
    }
  }

  filterByDateRange(): void {
    if (this.startDate && this.endDate) {
      this.allLogsLoading = true;
      this.auditLogsService.getLogsByDateRange(this.startDate, this.endDate).subscribe({
        next: (response) => {
          this.allLogsData = response.data;
          this.allLogsLoading = false;
        },
        error: (error) => {
          console.error('Error filtering by date range:', error);
          this.allLogsLoading = false;
        }
      });
    }
  }

  clearFilters(): void {
    this.selectedAction = '';
    this.startDate = null;
    this.endDate = null;
    this.loadAllLogs();
  }

  updateLoginHistory(): void {
    this.loadLoginHistory();
  }

  updateStatistics(): void {
    this.loadStatistics();
  }

  getActionColor(action: string): string {
    const colors: { [key: string]: string } = {
      'LOGIN': 'primary',
      'LOGOUT': 'accent',
      'CREATE': 'success',
      'UPDATE': 'warning',
      'DELETE': 'danger',
      'VIEW': 'info',
      'EXPORT': 'secondary'
    };
    return colors[action] || 'default';
  }

  formatDate(date: Date): string {
    return new Date(date).toLocaleString();
  }

  goBack(): void {
    this.location.back();
  }
}
