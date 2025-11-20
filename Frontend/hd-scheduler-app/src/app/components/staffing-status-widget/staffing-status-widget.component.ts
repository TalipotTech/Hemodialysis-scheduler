import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { StaffingStatusService, ShiftStaffingStatus } from '../../services/staffing-status.service';

@Component({
  selector: 'app-staffing-status-widget',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatTooltipModule
  ],
  template: `
    <mat-card class="staffing-widget">
      <mat-card-header>
        <mat-icon class="header-icon">analytics</mat-icon>
        <mat-card-title>Staffing Status</mat-card-title>
        <mat-card-subtitle>All Shifts Overview</mat-card-subtitle>
      </mat-card-header>
      
      <mat-card-content>
        <div *ngIf="loading" class="loading">
          <mat-spinner diameter="40"></mat-spinner>
        </div>

        <div *ngIf="!loading && statuses" class="status-grid">
          <div *ngFor="let status of statuses" class="shift-status" [class]="'status-' + status.statusColor">
            <div class="shift-header">
              <span class="shift-name">{{ status.slotName }}</span>
              <mat-icon class="status-icon" [matTooltip]="getStatusTooltip(status)">
                {{ getStatusIcon(status.statusColor) }}
              </mat-icon>
            </div>
            
            <div class="staffing-bar">
              <div class="bar-fill" [style.width.%]="Math.min(status.staffingPercentage, 100)" 
                   [class]="'fill-' + status.statusColor"></div>
            </div>
            
            <div class="staff-counts">
              <div class="count-item">
                <mat-icon class="role-icon doctor">medical_services</mat-icon>
                <span>{{ status.doctorCount }}/{{ status.recommendedDoctors }}</span>
              </div>
              <div class="count-item">
                <mat-icon class="role-icon nurse">local_hospital</mat-icon>
                <span>{{ status.nurseCount }}/{{ status.recommendedNurses }}</span>
              </div>
              <div class="count-item">
                <mat-icon class="role-icon tech">engineering</mat-icon>
                <span>{{ status.technicianCount }}/{{ status.recommendedTechnicians }}</span>
              </div>
            </div>
            
            <div class="status-badge" [class]="'badge-' + status.statusColor">
              {{ status.status }} ({{ status.staffingPercentage }}%)
            </div>
          </div>
        </div>

        <div *ngIf="!loading && !statuses" class="error">
          <mat-icon>error_outline</mat-icon>
          <p>Unable to load staffing status</p>
        </div>
      </mat-card-content>
    </mat-card>
  `,
  styles: [`
    .staffing-widget {
      height: 100%;
      background: white;
    }

    mat-card-header {
      display: flex;
      align-items: center;
      gap: 12px;
      margin-bottom: 16px;
    }

    .header-icon {
      font-size: 28px;
      width: 28px;
      height: 28px;
      color: #667eea;
    }

    mat-card-title {
      color: #424242;
      font-size: 18px;
      margin: 0;
    }

    mat-card-subtitle {
      color: #757575;
      font-size: 13px;
    }

    .loading {
      display: flex;
      justify-content: center;
      padding: 24px;
    }

    .status-grid {
      display: flex;
      flex-direction: column;
      gap: 16px;
    }

    .shift-status {
      padding: 16px;
      border-radius: 8px;
      border-left: 4px solid;
      background: #fafafa;
      transition: all 0.3s;

      &:hover {
        transform: translateX(4px);
        box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
      }

      &.status-green { border-left-color: #4caf50; }
      &.status-yellow { border-left-color: #ff9800; }
      &.status-red { border-left-color: #f44336; }
    }

    .shift-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 12px;

      .shift-name {
        font-weight: 600;
        font-size: 15px;
        color: #424242;
      }

      .status-icon {
        font-size: 20px;
        width: 20px;
        height: 20px;

        &[ng-reflect-tooltip] {
          cursor: help;
        }
      }
    }

    .staffing-bar {
      height: 8px;
      background: #e0e0e0;
      border-radius: 4px;
      overflow: hidden;
      margin-bottom: 12px;

      .bar-fill {
        height: 100%;
        transition: width 0.5s ease;

        &.fill-green { background: #4caf50; }
        &.fill-yellow { background: #ff9800; }
        &.fill-red { background: #f44336; }
      }
    }

    .staff-counts {
      display: flex;
      justify-content: space-around;
      margin-bottom: 12px;

      .count-item {
        display: flex;
        align-items: center;
        gap: 6px;
        font-size: 13px;
        font-weight: 500;

        .role-icon {
          font-size: 18px;
          width: 18px;
          height: 18px;

          &.doctor { color: #42a5f5; }
          &.nurse { color: #66bb6a; }
          &.tech { color: #ab47bc; }
        }
      }
    }

    .status-badge {
      text-align: center;
      padding: 6px 12px;
      border-radius: 12px;
      font-size: 12px;
      font-weight: 600;

      &.badge-green {
        background: rgba(76, 175, 80, 0.15);
        color: #2e7d32;
      }

      &.badge-yellow {
        background: rgba(255, 152, 0, 0.15);
        color: #e65100;
      }

      &.badge-red {
        background: rgba(244, 67, 54, 0.15);
        color: #c62828;
      }
    }

    .error {
      text-align: center;
      padding: 24px;
      color: #757575;

      mat-icon {
        font-size: 32px;
        width: 32px;
        height: 32px;
        margin-bottom: 8px;
      }
    }
  `]
})
export class StaffingStatusWidgetComponent implements OnInit {
  statuses: ShiftStaffingStatus[] | null = null;
  loading = true;
  Math = Math; // Make Math available in template

  constructor(private staffingService: StaffingStatusService) {}

  ngOnInit(): void {
    this.loadStatuses();
    // Refresh every 5 minutes
    setInterval(() => this.loadStatuses(), 5 * 60 * 1000);
  }

  loadStatuses(): void {
    this.loading = true;
    this.staffingService.getAllShiftStatuses().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.statuses = response.data;
        }
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading staffing status:', error);
        this.loading = false;
      }
    });
  }

  getStatusIcon(color: string): string {
    switch (color) {
      case 'green': return 'check_circle';
      case 'yellow': return 'warning';
      case 'red': return 'error';
      default: return 'help';
    }
  }

  getStatusTooltip(status: ShiftStaffingStatus): string {
    return `${status.totalStaff} of ${status.recommendedTotal} recommended staff assigned`;
  }
}
