import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatTabsModule } from '@angular/material/tabs';
import { MatPaginatorModule, MatPaginator } from '@angular/material/paginator';
import { MatSortModule, MatSort } from '@angular/material/sort';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
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
    MatTableModule,
    MatTabsModule,
    MatPaginatorModule,
    MatSortModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatProgressSpinnerModule,
    MatChipsModule,
    MatTooltipModule
  ],
  templateUrl: './audit-logs.component.html',
  styleUrls: ['./audit-logs.component.scss']
})
export class AuditLogsComponent implements OnInit {
  @ViewChild('allLogsPaginator') allLogsPaginator!: MatPaginator;
  @ViewChild('allLogsSort') allLogsSort!: MatSort;
  @ViewChild('loginPaginator') loginPaginator!: MatPaginator;
  @ViewChild('loginSort') loginSort!: MatSort;

  // All Logs
  allLogsDataSource = new MatTableDataSource<AuditLog>([]);
  allLogsColumns: string[] = ['logID', 'createdAt', 'username', 'action', 'entityType', 'entityID', 'details'];
  allLogsLoading = true;
  
  // Login History
  loginHistoryDataSource = new MatTableDataSource<AuditLog>([]);
  loginHistoryColumns: string[] = ['createdAt', 'username', 'action', 'ipAddress'];
  loginHistoryLoading = true;
  loginDays = 30;

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

  ngAfterViewInit() {
    this.allLogsDataSource.paginator = this.allLogsPaginator;
    this.allLogsDataSource.sort = this.allLogsSort;
    this.loginHistoryDataSource.paginator = this.loginPaginator;
    this.loginHistoryDataSource.sort = this.loginSort;
  }

  loadAllLogs(): void {
    this.allLogsLoading = true;
    this.auditLogsService.getAllLogs(1, 1000).subscribe({
      next: (response) => {
        this.allLogsDataSource.data = response.data;
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
        this.loginHistoryDataSource.data = response.data;
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

  applyFilter(event: Event): void {
    const filterValue = (event.target as HTMLInputElement).value;
    this.allLogsDataSource.filter = filterValue.trim().toLowerCase();
  }

  filterByAction(): void {
    if (this.selectedAction) {
      this.allLogsLoading = true;
      this.auditLogsService.getLogsByAction(this.selectedAction).subscribe({
        next: (response) => {
          this.allLogsDataSource.data = response.data;
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
          this.allLogsDataSource.data = response.data;
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
    this.allLogsDataSource.filter = '';
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
