import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Location } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { CurrentShiftService, CurrentShiftStaff } from '../../services/current-shift.service';

interface ShiftScheduleData {
  slotName: string;
  slotTime: string;
  doctors: any[];
  nurses: any[];
  technicians: any[];
  totalStaff: number;
}

@Component({
  selector: 'app-shift-schedule',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatIconModule,
    MatButtonModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatTooltipModule
  ],
  template: `
    <div class="schedule-container">
      <div class="header">
        <button mat-icon-button (click)="goBack()" class="back-button">
          <mat-icon>arrow_back</mat-icon>
        </button>
        <div class="header-content">
          <h1>
            <mat-icon>calendar_view_week</mat-icon>
            Shift Schedule Overview
          </h1>
          <p>View all staff assignments across shifts</p>
        </div>
        <button mat-raised-button color="primary" (click)="refreshSchedule()">
          <mat-icon>refresh</mat-icon>
          Refresh
        </button>
      </div>

      <div *ngIf="loading" class="loading">
        <mat-spinner diameter="60"></mat-spinner>
        <p>Loading shift schedule...</p>
      </div>

      <div *ngIf="!loading && shifts" class="shifts-grid">
        <mat-card *ngFor="let shift of shiftsArray" [class.current-shift]="isCurrentShift(shift.slotName)" class="shift-card">
          <mat-card-header>
            <div class="shift-header">
              <div class="shift-title">
                <mat-icon [class]="'shift-icon ' + getShiftClass(shift.slotName)">
                  {{ getShiftIcon(shift.slotName) }}
                </mat-icon>
                <div>
                  <h2>{{ shift.slotName }} Shift</h2>
                  <p class="shift-time">{{ shift.slotTime }}</p>
                </div>
              </div>
              <mat-chip *ngIf="isCurrentShift(shift.slotName)" class="current-chip">
                <mat-icon>circle</mat-icon>
                Current
              </mat-chip>
            </div>
          </mat-card-header>

          <mat-card-content>
            <!-- Doctors Section -->
            <div class="role-section">
              <div class="role-header">
                <mat-icon class="role-icon doctor">medical_services</mat-icon>
                <h3>Doctors</h3>
                <span class="count-badge doctor-badge">{{ shift.doctors.length }}</span>
              </div>
              <div class="staff-list" *ngIf="shift.doctors.length > 0">
                <div *ngFor="let doctor of shift.doctors" class="staff-item doctor-item">
                  <mat-icon>person</mat-icon>
                  <div class="staff-info">
                    <span class="staff-name">{{ doctor.name }}</span>
                    <span class="staff-specialization" *ngIf="doctor.staffSpecialization">
                      {{ doctor.staffSpecialization }}
                    </span>
                  </div>
                </div>
              </div>
              <div *ngIf="shift.doctors.length === 0" class="empty-slot">
                <mat-icon>warning</mat-icon>
                <span>No doctors assigned</span>
              </div>
            </div>

            <!-- Nurses Section -->
            <div class="role-section">
              <div class="role-header">
                <mat-icon class="role-icon nurse">local_hospital</mat-icon>
                <h3>Nurses</h3>
                <span class="count-badge nurse-badge">{{ shift.nurses.length }}</span>
              </div>
              <div class="staff-list" *ngIf="shift.nurses.length > 0">
                <div *ngFor="let nurse of shift.nurses" class="staff-item nurse-item">
                  <mat-icon>person</mat-icon>
                  <div class="staff-info">
                    <span class="staff-name">{{ nurse.name }}</span>
                    <span class="staff-specialization" *ngIf="nurse.staffSpecialization">
                      {{ nurse.staffSpecialization }}
                    </span>
                  </div>
                </div>
              </div>
              <div *ngIf="shift.nurses.length === 0" class="empty-slot">
                <mat-icon>warning</mat-icon>
                <span>No nurses assigned</span>
              </div>
            </div>

            <!-- Technicians Section -->
            <div class="role-section">
              <div class="role-header">
                <mat-icon class="role-icon tech">engineering</mat-icon>
                <h3>Technicians</h3>
                <span class="count-badge tech-badge">{{ shift.technicians.length }}</span>
              </div>
              <div class="staff-list" *ngIf="shift.technicians.length > 0">
                <div *ngFor="let tech of shift.technicians" class="staff-item tech-item">
                  <mat-icon>person</mat-icon>
                  <div class="staff-info">
                    <span class="staff-name">{{ tech.name }}</span>
                    <span class="staff-specialization" *ngIf="tech.staffSpecialization">
                      {{ tech.staffSpecialization }}
                    </span>
                  </div>
                </div>
              </div>
              <div *ngIf="shift.technicians.length === 0" class="empty-slot">
                <mat-icon>warning</mat-icon>
                <span>No technicians assigned</span>
              </div>
            </div>

            <!-- Total Staff -->
            <div class="total-section">
              <mat-icon>group</mat-icon>
              <span>Total Staff: <strong>{{ shift.totalStaff }}</strong></span>
            </div>
          </mat-card-content>
        </mat-card>
      </div>

      <div *ngIf="!loading && !shifts" class="error">
        <mat-icon>error_outline</mat-icon>
        <p>Unable to load shift schedule</p>
        <button mat-raised-button color="primary" (click)="refreshSchedule()">
          Try Again
        </button>
      </div>
    </div>
  `,
  styles: [`
    .schedule-container {
      padding: 20px;
      max-width: 1600px;
      margin: 0 auto;
    }

    .header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 32px;
      gap: 20px;

      .back-button {
        flex-shrink: 0;
      }

      .header-content {
        flex: 1;

        h1 {
          margin: 0;
          font-size: 28px;
          font-weight: 500;
          color: #424242;
          display: flex;
          align-items: center;
          gap: 12px;

          mat-icon {
            font-size: 32px;
            width: 32px;
            height: 32px;
            color: #667eea;
          }
        }

        p {
          margin: 4px 0 0 44px;
          color: #757575;
        }
      }

      button[color="primary"] {
        flex-shrink: 0;

        mat-icon {
          margin-right: 8px;
        }
      }
    }

    .loading {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: 80px 20px;
      gap: 20px;

      p {
        color: #757575;
        font-size: 16px;
      }
    }

    .shifts-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(380px, 1fr));
      gap: 24px;
    }

    .shift-card {
      position: relative;
      transition: all 0.3s;

      &:hover {
        transform: translateY(-4px);
        box-shadow: 0 8px 24px rgba(0, 0, 0, 0.15);
      }

      &.current-shift {
        border: 2px solid #667eea;
        box-shadow: 0 4px 16px rgba(102, 126, 234, 0.3);
      }
    }

    .shift-header {
      width: 100%;
      display: flex;
      justify-content: space-between;
      align-items: center;

      .shift-title {
        display: flex;
        align-items: center;
        gap: 12px;

        h2 {
          margin: 0;
          font-size: 20px;
          font-weight: 600;
          color: #424242;
        }

        .shift-time {
          margin: 4px 0 0 0;
          font-size: 14px;
          color: #757575;
        }
      }

      .shift-icon {
        font-size: 36px;
        width: 36px;
        height: 36px;

        &.morning { color: #ffa726; }
        &.afternoon { color: #42a5f5; }
        &.evening { color: #ab47bc; }
        &.night { color: #5c6bc0; }
      }

      .current-chip {
        background: #667eea;
        color: white;
        font-weight: 600;

        mat-icon {
          font-size: 14px;
          width: 14px;
          height: 14px;
          margin-right: 4px;
          animation: pulse 2s infinite;
        }
      }
    }

    @keyframes pulse {
      0%, 100% { opacity: 1; }
      50% { opacity: 0.5; }
    }

    mat-card-content {
      display: flex;
      flex-direction: column;
      gap: 20px;
      padding: 16px;
    }

    .role-section {
      background: #f5f5f5;
      padding: 16px;
      border-radius: 8px;
    }

    .role-header {
      display: flex;
      align-items: center;
      gap: 8px;
      margin-bottom: 12px;

      .role-icon {
        font-size: 24px;
        width: 24px;
        height: 24px;

        &.doctor { color: #42a5f5; }
        &.nurse { color: #66bb6a; }
        &.tech { color: #ab47bc; }
      }

      h3 {
        margin: 0;
        font-size: 16px;
        font-weight: 600;
        color: #424242;
        flex: 1;
      }

      .count-badge {
        min-width: 28px;
        height: 28px;
        padding: 0 8px;
        border-radius: 14px;
        display: flex;
        align-items: center;
        justify-content: center;
        font-weight: 600;
        font-size: 13px;

        &.doctor-badge {
          background: rgba(66, 165, 245, 0.2);
          color: #1976d2;
        }

        &.nurse-badge {
          background: rgba(102, 187, 106, 0.2);
          color: #388e3c;
        }

        &.tech-badge {
          background: rgba(171, 71, 188, 0.2);
          color: #7b1fa2;
        }
      }
    }

    .staff-list {
      display: flex;
      flex-direction: column;
      gap: 8px;
    }

    .staff-item {
      display: flex;
      align-items: center;
      gap: 12px;
      padding: 10px;
      background: white;
      border-radius: 6px;
      border-left: 3px solid;

      &.doctor-item { border-left-color: #42a5f5; }
      &.nurse-item { border-left-color: #66bb6a; }
      &.tech-item { border-left-color: #ab47bc; }

      mat-icon {
        font-size: 20px;
        width: 20px;
        height: 20px;
        color: #757575;
      }

      .staff-info {
        display: flex;
        flex-direction: column;
        gap: 2px;

        .staff-name {
          font-weight: 500;
          color: #424242;
          font-size: 14px;
        }

        .staff-specialization {
          font-size: 12px;
          color: #757575;
        }
      }
    }

    .empty-slot {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 12px;
      background: rgba(255, 152, 0, 0.1);
      border-radius: 6px;
      color: #f57c00;
      font-size: 13px;

      mat-icon {
        font-size: 18px;
        width: 18px;
        height: 18px;
      }
    }

    .total-section {
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 8px;
      padding: 12px;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      color: white;
      border-radius: 8px;
      font-size: 15px;

      mat-icon {
        font-size: 24px;
        width: 24px;
        height: 24px;
      }
    }

    .error {
      text-align: center;
      padding: 80px 20px;

      mat-icon {
        font-size: 64px;
        width: 64px;
        height: 64px;
        color: #f44336;
        margin-bottom: 16px;
      }

      p {
        margin: 0 0 20px 0;
        color: #757575;
        font-size: 16px;
      }
    }

    @media (max-width: 768px) {
      .shifts-grid {
        grid-template-columns: 1fr;
      }

      .header {
        flex-direction: column;
        align-items: flex-start;
      }
    }
  `]
})
export class ShiftScheduleComponent implements OnInit {
  shifts: { [key: string]: CurrentShiftStaff } | null = null;
  shiftsArray: ShiftScheduleData[] = [];
  loading = true;
  currentShiftName: string = '';

  constructor(
    private shiftService: CurrentShiftService,
    private location: Location
  ) {}

  ngOnInit(): void {
    this.loadSchedule();
  }

  loadSchedule(): void {
    this.loading = true;
    this.shiftService.getAllShiftsStaff().subscribe({
      next: (data) => {
        this.shifts = data;
        this.shiftsArray = this.convertToArray(data);
        this.determineCurrentShift();
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading shift schedule:', error);
        this.loading = false;
      }
    });
  }

  convertToArray(shifts: { [key: string]: CurrentShiftStaff }): ShiftScheduleData[] {
    const order = ['Morning', 'Afternoon', 'Evening', 'Night'];
    return order
      .filter(name => shifts[name])
      .map(name => shifts[name]);
  }

  determineCurrentShift(): void {
    const hour = new Date().getHours();
    if (hour >= 6 && hour < 11) {
      this.currentShiftName = 'Morning';
    } else if (hour >= 11 && hour < 16) {
      this.currentShiftName = 'Afternoon';
    } else if (hour >= 16 && hour < 21) {
      this.currentShiftName = 'Evening';
    } else {
      this.currentShiftName = 'Night';
    }
  }

  isCurrentShift(shiftName: string): boolean {
    return shiftName === this.currentShiftName;
  }

  getShiftIcon(shiftName: string): string {
    const icons: { [key: string]: string } = {
      'Morning': 'wb_sunny',
      'Afternoon': 'wb_twilight',
      'Evening': 'nights_stay',
      'Night': 'bedtime'
    };
    return icons[shiftName] || 'schedule';
  }

  getShiftClass(shiftName: string): string {
    return shiftName.toLowerCase();
  }

  refreshSchedule(): void {
    this.loadSchedule();
  }

  goBack(): void {
    this.location.back();
  }
}
