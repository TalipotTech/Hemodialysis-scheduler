import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment.development';
import { Staff } from '../../services/staff-management.service';

interface Slot {
  slotID: number;
  slotName: string;
  startTime: string;
  endTime: string;
}

@Component({
  selector: 'app-slot-assignment-dialog',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatSelectModule,
    MatFormFieldModule
  ],
  template: `
    <h2 mat-dialog-title>Assign {{ data.staff.name }} to Slot</h2>
    <mat-dialog-content>
      <form [formGroup]="slotForm">
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Select Time Slot</mat-label>
          <mat-select formControlName="slotId" required>
            <mat-option *ngFor="let slot of slots" [value]="slot.slotID">
              {{ slot.slotName }} ({{ slot.startTime }} - {{ slot.endTime }})
            </mat-option>
          </mat-select>
          <mat-error *ngIf="slotForm.get('slotId')?.hasError('required')">
            Slot is required
          </mat-error>
        </mat-form-field>
        
        <p class="current-assignment" *ngIf="data.staff.assignedSlotName">
          <strong>Current Assignment:</strong> {{ data.staff.assignedSlotName }}
        </p>
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="onCancel()">Cancel</button>
      <button mat-raised-button color="primary" (click)="onSave()" [disabled]="!slotForm.valid">
        Assign
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    .full-width {
      width: 100%;
      margin-bottom: 16px;
    }
    mat-dialog-content {
      min-height: 150px;
      padding: 20px;
    }
    .current-assignment {
      color: rgba(0, 0, 0, 0.6);
      font-size: 14px;
    }
  `]
})
export class SlotAssignmentDialogComponent implements OnInit {
  slotForm: FormGroup;
  slots: Slot[] = [
    { slotID: 1, slotName: 'Morning', startTime: '08:00 AM', endTime: '02:00 PM' },
    { slotID: 2, slotName: 'Afternoon', startTime: '02:00 PM', endTime: '08:00 PM' },
    { slotID: 3, slotName: 'Evening', startTime: '08:00 PM', endTime: '02:00 AM' }
  ];

  constructor(
    private fb: FormBuilder,
    private http: HttpClient,
    public dialogRef: MatDialogRef<SlotAssignmentDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { staff: Staff }
  ) {
    this.slotForm = this.fb.group({
      slotId: [data.staff.assignedSlot || '', Validators.required]
    });
  }

  ngOnInit(): void {
    // Load actual slots from backend if needed
    this.loadSlots();
  }

  loadSlots(): void {
    const token = localStorage.getItem('token');
    this.http.get<any>(`${environment.apiUrl}/systemsettings/slots`, {
      headers: { 'Authorization': `Bearer ${token}` }
    }).subscribe({
      next: (response) => {
        if (response.success && response.data.length > 0) {
          this.slots = response.data;
        }
      },
      error: () => {
        // Use default slots if API fails
        console.log('Using default slots');
      }
    });
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  onSave(): void {
    if (this.slotForm.valid) {
      this.dialogRef.close(this.slotForm.value);
    }
  }
}
