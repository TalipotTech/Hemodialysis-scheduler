import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { CurrentShiftService, CurrentShiftStaff } from '../../services/current-shift.service';

@Component({
  selector: 'app-on-duty-widget',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatIconModule,
    MatChipsModule,
    MatProgressSpinnerModule
  ],
  template: `
    <mat-card class="on-duty-card">
      <mat-card-header>
        <mat-icon class="header-icon">people</mat-icon>
        <mat-card-title>On Duty Now</mat-card-title>
        <mat-card-subtitle>{{ currentShift?.slotName }} ({{ currentShift?.slotTime }})</mat-card-subtitle>
      </mat-card-header>
      
      <mat-card-content>
        <div *ngIf="loading" class="loading">
          <mat-spinner diameter="40"></mat-spinner>
        </div>

        <div *ngIf="!loading && currentShift" class="staff-grid">
          <!-- Doctors -->
          <div class="staff-section">
            <div class="section-header">
              <mat-icon class="role-icon doctor">medical_services</mat-icon>
              <h3>Doctors ({{ currentShift.doctors.length }})</h3>
            </div>
            <div class="staff-list" *ngIf="currentShift.doctors.length > 0">
              <mat-chip *ngFor="let doctor of currentShift.doctors" class="staff-chip doctor-chip">
                {{ doctor.name }}
              </mat-chip>
            </div>
            <p *ngIf="currentShift.doctors.length === 0" class="no-staff">No doctors assigned</p>
          </div>

          <!-- Nurses -->
          <div class="staff-section">
            <div class="section-header">
              <mat-icon class="role-icon nurse">local_hospital</mat-icon>
              <h3>Nurses ({{ currentShift.nurses.length }})</h3>
            </div>
            <div class="staff-list" *ngIf="currentShift.nurses.length > 0">
              <mat-chip *ngFor="let nurse of currentShift.nurses" class="staff-chip nurse-chip">
                {{ nurse.name }}
              </mat-chip>
            </div>
            <p *ngIf="currentShift.nurses.length === 0" class="no-staff">No nurses assigned</p>
          </div>

          <!-- Technicians -->
          <div class="staff-section">
            <div class="section-header">
              <mat-icon class="role-icon tech">engineering</mat-icon>
              <h3>Technicians ({{ currentShift.technicians.length }})</h3>
            </div>
            <div class="staff-list" *ngIf="currentShift.technicians.length > 0">
              <mat-chip *ngFor="let tech of currentShift.technicians" class="staff-chip tech-chip">
                {{ tech.name }}
              </mat-chip>
            </div>
            <p *ngIf="currentShift.technicians.length === 0" class="no-staff">No technicians assigned</p>
          </div>

          <!-- Total Staff -->
          <div class="total-staff">
            <mat-icon>group</mat-icon>
            <span>Total Staff: <strong>{{ currentShift.totalStaff }}</strong></span>
          </div>
        </div>

        <div *ngIf="!loading && !currentShift" class="error">
          <mat-icon>error_outline</mat-icon>
          <p>Unable to load shift information</p>
        </div>
      </mat-card-content>
    </mat-card>
  `,
  styles: [`
    .on-duty-card {
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
      font-size: 32px;
      width: 32px;
      height: 32px;
    }

    mat-card-title {
      color: white;
      font-size: 20px;
      margin: 0;
    }

    mat-card-subtitle {
      color: rgba(255, 255, 255, 0.9);
      font-size: 14px;
    }

    .loading {
      display: flex;
      justify-content: center;
      padding: 32px;
    }

    .staff-grid {
      display: flex;
      flex-direction: column;
      gap: 20px;
    }

    .staff-section {
      background: rgba(255, 255, 255, 0.1);
      padding: 16px;
      border-radius: 8px;
      backdrop-filter: blur(10px);
    }

    .section-header {
      display: flex;
      align-items: center;
      gap: 8px;
      margin-bottom: 12px;
    }

    .section-header h3 {
      margin: 0;
      font-size: 16px;
      font-weight: 600;
    }

    .role-icon {
      font-size: 24px;
      width: 24px;
      height: 24px;
    }

    .role-icon.doctor { color: #4fc3f7; }
    .role-icon.nurse { color: #81c784; }
    .role-icon.tech { color: #ba68c8; }

    .staff-list {
      display: flex;
      flex-wrap: wrap;
      gap: 8px;
    }

    .staff-chip {
      background: rgba(255, 255, 255, 0.2);
      color: white;
      font-size: 13px;
      height: 28px;
      padding: 0 12px;
    }

    .doctor-chip { border-left: 3px solid #4fc3f7; }
    .nurse-chip { border-left: 3px solid #81c784; }
    .tech-chip { border-left: 3px solid #ba68c8; }

    .no-staff {
      color: rgba(255, 255, 255, 0.7);
      font-style: italic;
      margin: 0;
      font-size: 13px;
    }

    .total-staff {
      display: flex;
      align-items: center;
      gap: 8px;
      justify-content: center;
      padding: 12px;
      background: rgba(255, 255, 255, 0.15);
      border-radius: 8px;
      font-size: 16px;
    }

    .total-staff mat-icon {
      font-size: 24px;
      width: 24px;
      height: 24px;
    }

    .error {
      text-align: center;
      padding: 32px;
      color: rgba(255, 255, 255, 0.8);
    }

    .error mat-icon {
      font-size: 48px;
      width: 48px;
      height: 48px;
      margin-bottom: 8px;
    }
  `]
})
export class OnDutyWidgetComponent implements OnInit {
  currentShift: CurrentShiftStaff | null = null;
  loading = true;

  constructor(private shiftService: CurrentShiftService) {}

  ngOnInit(): void {
    this.loadCurrentShift();
    // Refresh every 5 minutes
    setInterval(() => this.loadCurrentShift(), 5 * 60 * 1000);
  }

  loadCurrentShift(): void {
    this.loading = true;
    this.shiftService.getCurrentShiftStaff().subscribe({
      next: (shift) => {
        this.currentShift = shift;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading current shift:', error);
        this.loading = false;
      }
    });
  }
}
