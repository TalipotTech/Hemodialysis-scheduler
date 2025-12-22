import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';

export interface PatientInfoData {
  name: string;
  hdStartDate: string | null;
  hdCycle: string | null;
  bedNumber: number | null;
  totalDialysisCompleted: number;
}

@Component({
  selector: 'app-patient-info-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatIconModule,
    MatButtonModule
  ],
  template: `
    <div class="patient-info-dialog">
      <h2 mat-dialog-title>
        <mat-icon>person</mat-icon>
        {{ data.name }}
      </h2>
      
      <mat-dialog-content>
        <div class="info-grid">
          <div class="info-item">
            <div class="info-label">
              <mat-icon>event</mat-icon>
              HD Start Date
            </div>
            <div class="info-value">
              {{ data.hdStartDate ? (data.hdStartDate | date: 'dd MMM yyyy') : 'Not Set' }}
            </div>
          </div>

          <div class="info-item">
            <div class="info-label">
              <mat-icon>calendar_today</mat-icon>
              HD Cycle
            </div>
            <div class="info-value">
              {{ data.hdCycle || 'Not Set' }}
            </div>
          </div>

          <div class="info-item">
            <div class="info-label">
              <mat-icon>hotel</mat-icon>
              Bed Number
            </div>
            <div class="info-value">
              {{ data.bedNumber ? 'Bed ' + data.bedNumber : 'Not Assigned' }}
            </div>
          </div>

          <div class="info-item">
            <div class="info-label">
              <mat-icon>assignment_turned_in</mat-icon>
              Completed Sessions
            </div>
            <div class="info-value completed-count">
              {{ data.totalDialysisCompleted }}
            </div>
          </div>
        </div>
      </mat-dialog-content>

      <mat-dialog-actions align="end">
        <button mat-button (click)="onClose()">Close</button>
      </mat-dialog-actions>
    </div>
  `,
  styles: [`
    .patient-info-dialog {
      min-width: 400px;
    }

    h2[mat-dialog-title] {
      display: flex;
      align-items: center;
      gap: 12px;
      color: #1976d2;
      margin-bottom: 20px;
      
      mat-icon {
        font-size: 28px;
        width: 28px;
        height: 28px;
      }
    }

    mat-dialog-content {
      padding: 20px 0;
    }

    .info-grid {
      display: grid;
      gap: 20px;
    }

    .info-item {
      border-left: 3px solid #1976d2;
      padding-left: 16px;
      transition: all 0.2s ease;
    }

    .info-item:hover {
      border-left-color: #2196f3;
      background-color: #f5f5f5;
      padding-left: 20px;
    }

    .info-label {
      display: flex;
      align-items: center;
      gap: 8px;
      font-size: 13px;
      color: #666;
      margin-bottom: 6px;
      font-weight: 500;
      
      mat-icon {
        font-size: 18px;
        width: 18px;
        height: 18px;
        color: #1976d2;
      }
    }

    .info-value {
      font-size: 16px;
      color: #333;
      font-weight: 600;
      padding-left: 26px;
    }

    .completed-count {
      color: #4caf50;
      font-size: 24px;
      font-weight: 700;
    }

    mat-dialog-actions {
      padding: 16px 0 8px;
      margin-top: 16px;
      border-top: 1px solid #e0e0e0;
    }
  `]
})
export class PatientInfoDialogComponent {
  constructor(
    public dialogRef: MatDialogRef<PatientInfoDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: PatientInfoData
  ) {}

  onClose(): void {
    this.dialogRef.close();
  }
}
