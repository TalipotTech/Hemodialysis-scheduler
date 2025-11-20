import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { Staff } from '../../services/staff-management.service';

@Component({
  selector: 'app-staff-dialog',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatInputModule,
    MatSelectModule,
    MatFormFieldModule
  ],
  template: `
    <h2 mat-dialog-title>{{ data.mode === 'create' ? 'Create Staff Member' : 'Edit Staff Member' }}</h2>
    <mat-dialog-content>
      <form [formGroup]="staffForm">
        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Name</mat-label>
          <input matInput formControlName="name" placeholder="Enter full name" required>
          <mat-error *ngIf="staffForm.get('name')?.hasError('required')">
            Name is required
          </mat-error>
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Role</mat-label>
          <mat-select formControlName="role" required>
            <mat-option value="Doctor">Doctor</mat-option>
            <mat-option value="Nurse">Nurse</mat-option>
            <mat-option value="Technician">Technician</mat-option>
          </mat-select>
          <mat-error *ngIf="staffForm.get('role')?.hasError('required')">
            Role is required
          </mat-error>
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Contact Number</mat-label>
          <input matInput formControlName="contactNumber" placeholder="+91-XXXXXXXXXX">
        </mat-form-field>

        <mat-form-field appearance="outline" class="full-width">
          <mat-label>Specialization</mat-label>
          <input matInput formControlName="specialization" placeholder="e.g., Nephrology, Critical Care">
        </mat-form-field>
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="onCancel()">Cancel</button>
      <button mat-raised-button color="primary" (click)="onSave()" [disabled]="!staffForm.valid">
        {{ data.mode === 'create' ? 'Create' : 'Update' }}
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    .full-width {
      width: 100%;
      margin-bottom: 16px;
    }
    mat-dialog-content {
      min-height: 300px;
      padding: 20px;
    }
  `]
})
export class StaffDialogComponent {
  staffForm: FormGroup;

  constructor(
    private fb: FormBuilder,
    public dialogRef: MatDialogRef<StaffDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { mode: 'create' | 'edit', staff?: Staff }
  ) {
    this.staffForm = this.fb.group({
      name: [data.staff?.name || '', Validators.required],
      role: [data.staff?.role || '', Validators.required],
      contactNumber: [data.staff?.contactNumber || ''],
      specialization: [data.staff?.staffSpecialization || '']
    });
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  onSave(): void {
    if (this.staffForm.valid) {
      this.dialogRef.close(this.staffForm.value);
    }
  }
}
