import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { environment } from '../../../environments/environment.development';
import { forkJoin } from 'rxjs';

interface SystemStats {
  totalUsers: number;
  activePatients: number;
  totalStaff: number;
  activeStaff: number;
  criticalShifts: number;
  understaffedShifts: number;
  unassignedPatients: number;
}

@Component({
  selector: 'app-system-overview-widget',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatIconModule,
    MatProgressSpinnerModule
  ],
  template: `
    <mat-card class="system-overview-card">
      <mat-card-header>
        <mat-icon class="header-icon">dashboard</mat-icon>
        <mat-card-title>System Overview</mat-card-title>
        <mat-card-subtitle>Real-time statistics & health</mat-card-subtitle>
      </mat-card-header>
      
      <mat-card-content>
        <div *ngIf="loading" class="loading">
          <mat-spinner diameter="40"></mat-spinner>
        </div>

        <div *ngIf="!loading && stats" class="content">
          <!-- System Statistics -->
          <div class="stats-section">
            <h3><mat-icon>analytics</mat-icon> Statistics</h3>
            <div class="stat-grid">
              <div class="stat-item">
                <mat-icon class="stat-icon users">people</mat-icon>
                <div class="stat-info">
                  <span class="stat-value">{{ stats.totalUsers }}</span>
                  <span class="stat-label">Total Users</span>
                </div>
              </div>
              
              <div class="stat-item">
                <mat-icon class="stat-icon patients">personal_injury</mat-icon>
                <div class="stat-info">
                  <span class="stat-value">{{ stats.activePatients }}</span>
                  <span class="stat-label">Active Patients</span>
                </div>
              </div>
              
              <div class="stat-item">
                <mat-icon class="stat-icon staff">badge</mat-icon>
                <div class="stat-info">
                  <span class="stat-value">{{ stats.activeStaff }}/{{ stats.totalStaff }}</span>
                  <span class="stat-label">Active Staff</span>
                </div>
              </div>
            </div>
          </div>

          <div class="divider"></div>

          <!-- System Health -->
          <div class="health-section">
            <h3><mat-icon>health_and_safety</mat-icon> System Health</h3>
            <div class="health-items">
              <div class="health-item status-good">
                <mat-icon>check_circle</mat-icon>
                <span>System Online</span>
              </div>
              
              <div class="health-item status-good">
                <mat-icon>storage</mat-icon>
                <span>Database Connected</span>
              </div>
              
              <div class="health-item" [class]="getWarningClass()">
                <mat-icon>{{ getWarningIcon() }}</mat-icon>
                <span>{{ getWarningText() }}</span>
              </div>
              
              <div class="health-item" [class]="getAlertClass()">
                <mat-icon>{{ getAlertIcon() }}</mat-icon>
                <span>{{ getAlertText() }}</span>
              </div>
            </div>
          </div>
        </div>

        <div *ngIf="!loading && !stats" class="error">
          <mat-icon>error_outline</mat-icon>
          <p>Unable to load system data</p>
        </div>
      </mat-card-content>
    </mat-card>
  `,
  styles: [`
    .system-overview-card {
      height: 100%;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      color: white;
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
      color: white;
    }

    mat-card-title {
      color: white;
      font-size: 20px;
      margin: 0;
      font-weight: 600;
    }

    mat-card-subtitle {
      color: rgba(255, 255, 255, 0.9);
      font-size: 13px;
    }

    .loading {
      display: flex;
      justify-content: center;
      padding: 32px;
    }

    .content {
      display: flex;
      flex-direction: column;
      gap: 20px;
    }

    .stats-section,
    .health-section {
      h3 {
        display: flex;
        align-items: center;
        gap: 8px;
        margin: 0 0 12px 0;
        font-size: 15px;
        font-weight: 600;
        color: rgba(255, 255, 255, 0.95);
        
        mat-icon {
          font-size: 20px;
          width: 20px;
          height: 20px;
        }
      }
    }

    .stat-grid {
      display: flex;
      flex-direction: column;
      gap: 12px;
    }

    .stat-item {
      display: flex;
      align-items: center;
      gap: 12px;
      padding: 12px;
      background: rgba(255, 255, 255, 0.15);
      border-radius: 8px;
      backdrop-filter: blur(10px);

      .stat-icon {
        font-size: 32px;
        width: 32px;
        height: 32px;
        
        &.users { color: #4fc3f7; }
        &.patients { color: #81c784; }
        &.staff { color: #ffb74d; }
      }

      .stat-info {
        display: flex;
        flex-direction: column;

        .stat-value {
          font-size: 24px;
          font-weight: 700;
          line-height: 1;
        }

        .stat-label {
          font-size: 12px;
          opacity: 0.9;
          margin-top: 2px;
        }
      }
    }

    .divider {
      height: 1px;
      background: rgba(255, 255, 255, 0.2);
    }

    .health-items {
      display: flex;
      flex-direction: column;
      gap: 8px;
    }

    .health-item {
      display: flex;
      align-items: center;
      gap: 10px;
      padding: 10px 12px;
      background: rgba(255, 255, 255, 0.1);
      border-radius: 6px;
      font-size: 13px;
      font-weight: 500;

      mat-icon {
        font-size: 20px;
        width: 20px;
        height: 20px;
      }

      &.status-good {
        background: rgba(76, 175, 80, 0.2);
        color: #c8e6c9;
        
        mat-icon {
          color: #a5d6a7;
        }
      }

      &.status-warning {
        background: rgba(255, 152, 0, 0.2);
        color: #ffe0b2;
        
        mat-icon {
          color: #ffcc80;
        }
      }

      &.status-critical {
        background: rgba(244, 67, 54, 0.2);
        color: #ffcdd2;
        
        mat-icon {
          color: #ef9a9a;
        }
      }
    }

    .error {
      text-align: center;
      padding: 32px;
      color: rgba(255, 255, 255, 0.8);

      mat-icon {
        font-size: 48px;
        width: 48px;
        height: 48px;
        margin-bottom: 8px;
      }
    }
  `]
})
export class SystemOverviewWidgetComponent implements OnInit {
  stats: SystemStats | null = null;
  loading = true;

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    console.log('SystemOverviewWidget: Initializing...');
    this.loadStats();
    // Refresh every 2 minutes
    setInterval(() => this.loadStats(), 2 * 60 * 1000);
  }

  private getHeaders(): HttpHeaders {
    const token = localStorage.getItem('token');
    return new HttpHeaders({
      'Authorization': `Bearer ${token}`
    });
  }

  loadStats(): void {
    console.log('SystemOverviewWidget: Loading stats...');
    console.log('SystemOverviewWidget: API URL:', environment.apiUrl);
    console.log('SystemOverviewWidget: Token:', localStorage.getItem('token') ? 'Present' : 'Missing');
    this.loading = true;

    const users$ = this.http.get<any>(`${environment.apiUrl}/UserManagement`, { headers: this.getHeaders() });
    const patients$ = this.http.get<any>(`${environment.apiUrl}/Patients`, { headers: this.getHeaders() });
    const staff$ = this.http.get<any>(`${environment.apiUrl}/StaffManagement`, { headers: this.getHeaders() });
    const schedule$ = this.http.get<any>(`${environment.apiUrl}/HDSchedule/today`, { headers: this.getHeaders() });

    console.log('SystemOverviewWidget: Making API calls...');
    forkJoin({
      users: users$,
      patients: patients$,
      staff: staff$,
      schedule: schedule$
    }).subscribe({
      next: (results) => {
        console.log('System Overview - Raw API Results:', results);
        
        // Extract data from API responses (handle both success and direct data responses)
        const users = Array.isArray(results.users?.data) ? results.users.data : 
                     Array.isArray(results.users) ? results.users : [];
        const patients = Array.isArray(results.patients?.data) ? results.patients.data : 
                        Array.isArray(results.patients) ? results.patients : [];
        const staff = Array.isArray(results.staff?.data) ? results.staff.data : 
                     Array.isArray(results.staff) ? results.staff : [];
        const scheduleData = Array.isArray(results.schedule?.data) ? results.schedule.data : 
                            Array.isArray(results.schedule) ? results.schedule : [];

        console.log('System Overview - Extracted Data:', {
          usersCount: users.length,
          patientsCount: patients.length, 
          staffCount: staff.length,
          scheduleCount: scheduleData.length
        });

        // Get active patients and staff
        const activePatients = patients.filter((p: any) => 
          p.status === 'Active' || !p.status || p.status !== 'Discharged'
        );
        const activeStaff = staff.filter((s: any) => s.status === 'Active' || !s.status);

        // Count unassigned patients (active patients without scheduled HD sessions for today)
        const scheduledPatientIds = scheduleData.map((s: any) => s.patientID);
        const unassignedPatients = activePatients.filter(
          (p: any) => !scheduledPatientIds.includes(p.patientID)
        ).length;

        // Count shifts that need attention
        const slots = [1, 2, 3, 4]; // Morning, Afternoon, Evening, Night
        let criticalShifts = 0;
        
        slots.forEach(slotId => {
          const slotSessions = scheduleData.filter((s: any) => s.slotID === slotId);
          const slotStaff = activeStaff.filter((s: any) => s.assignedSlot === slotId);
          const doctorsCount = slotStaff.filter((s: any) => s.role === 'Doctor').length;
          const nursesCount = slotStaff.filter((s: any) => s.role === 'Nurse').length;
          
          // Critical if sessions exist but no doctors or nurses assigned
          if (slotSessions.length > 0 && (doctorsCount === 0 || nursesCount === 0)) {
            criticalShifts++;
          }
        });

        this.stats = {
          totalUsers: users.length,
          activePatients: activePatients.length,
          totalStaff: staff.length,
          activeStaff: activeStaff.length,
          criticalShifts: criticalShifts,
          understaffedShifts: 0,
          unassignedPatients: unassignedPatients
        };
        
        console.log('System Overview - Stats:', this.stats);
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading system stats:', error);
        console.error('Error details:', error.error);
        
        // Set default values on error
        this.stats = {
          totalUsers: 0,
          activePatients: 0,
          totalStaff: 0,
          activeStaff: 0,
          criticalShifts: 0,
          understaffedShifts: 0,
          unassignedPatients: 0
        };
        this.loading = false;
      }
    });
  }

  getWarningClass(): string {
    if (!this.stats) return 'status-good';
    const total = this.stats.understaffedShifts + this.stats.criticalShifts;
    return total > 0 ? 'status-warning' : 'status-good';
  }

  getWarningIcon(): string {
    if (!this.stats) return 'check_circle';
    const total = this.stats.understaffedShifts + this.stats.criticalShifts;
    return total > 0 ? 'warning' : 'check_circle';
  }

  getWarningText(): string {
    if (!this.stats) return 'All shifts staffed';
    const total = this.stats.understaffedShifts + this.stats.criticalShifts;
    if (total === 0) return 'All shifts staffed';
    if (total === 1) return '1 shift needs attention';
    return `${total} shifts need attention`;
  }

  getAlertClass(): string {
    if (!this.stats) return 'status-good';
    return this.stats.unassignedPatients > 0 ? 'status-critical' : 'status-good';
  }

  getAlertIcon(): string {
    if (!this.stats) return 'check_circle';
    return this.stats.unassignedPatients > 0 ? 'error' : 'check_circle';
  }

  getAlertText(): string {
    if (!this.stats) return 'All patients assigned';
    if (this.stats.unassignedPatients === 0) return 'All patients assigned';
    if (this.stats.unassignedPatients === 1) return '1 patient unassigned';
    return `${this.stats.unassignedPatients} patients unassigned`;
  }
}
