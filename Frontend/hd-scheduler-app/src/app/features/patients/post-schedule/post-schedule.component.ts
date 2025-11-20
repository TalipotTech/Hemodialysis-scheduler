import { Component, OnInit } from '@angular/core';
import { CommonModule, Location, DatePipe } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { PatientService } from '../../../core/services/patient.service';
import { ScheduleService } from '../../../core/services/schedule.service';

@Component({
  selector: 'app-post-schedule',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    DatePipe
  ],
  templateUrl: './post-schedule.component.html',
  styleUrl: './post-schedule.component.scss'
})
export class PostScheduleComponent implements OnInit {
  postDialysisForm!: FormGroup;
  patientId!: number;
  scheduleId!: number;
  patient: any = null;
  sessionData: any = null;
  loading = false;
  errorMessage = '';
  autoSaveTimer: any = null;

  medicationTypes = ['Anticoagulant', 'EPO', 'Iron', 'Vitamin', 'Antibiotic', 'Antihypertensive', 'Other'];
  medicationRoutes = ['IV', 'Oral', 'Subcutaneous', 'Intramuscular'];
  alertTypes = ['Hypotension', 'Hypertension', 'Cramping', 'Nausea', 'Headache', 'Chest Pain', 'Access Issue', 'Equipment Malfunction', 'Other'];
  severityLevels = ['Low', 'Medium', 'High', 'Critical'];

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private location: Location,
    private patientService: PatientService,
    private scheduleService: ScheduleService,
    private snackBar: MatSnackBar
  ) {
    this.initializeForm();
  }

  ngOnInit(): void {
    // Get patient ID and schedule ID from route
    this.route.params.subscribe(params => {
      this.patientId = +params['patientId'];
      this.scheduleId = +params['scheduleId'];
      
      if (this.scheduleId) {
        this.loadExistingSessionData();
      }
      
      if (this.patientId) {
        this.loadPatientDetails();
      }
      
      // Setup auto-save on form changes (debounced)
      if (this.scheduleId) {
        this.postDialysisForm.valueChanges.subscribe(() => {
          if (this.autoSaveTimer) {
            clearTimeout(this.autoSaveTimer);
          }
          this.autoSaveTimer = setTimeout(() => {
            this.autoSaveData();
          }, 2000); // Auto-save after 2 seconds of no typing
        });
      }
    });
  }

  initializeForm(): void {
    this.postDialysisForm = this.fb.group({
      // Post-Dialysis Vital Signs (with test default values)
      postWeight: [52.5],
      postSBP: [125],
      postDBP: [75],
      postHR: [72],
      accessBleedingTime: ['5'],
      totalFluidRemoved: [2.5],
      postAccessStatus: ['Site clean, no bleeding, dressing intact'],
      
      // Post-Dialysis Medications
      medicationType: ['EPO'],
      medicationName: ['Erythropoietin'],
      dose: ['4000 IU'],
      route: ['IV'],
      administeredAt: ['14:30'],
      
      // Treatment Alerts
      alertType: ['Hypotension'],
      severity: ['Medium'],
      alertMessage: ['Blood pressure dropped to 90/60 during treatment'],
      resolution: ['Reduced UF rate, administered saline bolus, patient stabilized'],
      
      // Additional Notes
      notes: ['Patient tolerated treatment well overall. Vital signs stable at end of session. Will continue current prescription.']
    });
  }

  loadPatientDetails(): void {
    this.loading = true;
    this.patientService.getPatient(this.patientId).subscribe({
      next: (response: any) => {
        if (response.success && response.data) {
          this.patient = response.data;
        }
        this.loading = false;
      },
      error: (error: any) => {
        console.error('Error loading patient:', error);
        this.errorMessage = 'Failed to load patient details';
        this.loading = false;
      }
    });
  }

  loadExistingSessionData(): void {
    // Load existing session data if available
    this.scheduleService.getScheduleById(this.scheduleId).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.sessionData = response.data;
          
          // Populate form with existing data
          this.postDialysisForm.patchValue({
            postWeight: this.sessionData.postWeight,
            postSBP: this.sessionData.postSBP,
            postDBP: this.sessionData.postDBP,
            postHR: this.sessionData.postHR,
            accessBleedingTime: this.sessionData.accessBleedingTime,
            totalFluidRemoved: this.sessionData.totalFluidRemoved,
            postAccessStatus: this.sessionData.postAccessStatus || this.sessionData.accessStatus,
            medicationType: this.sessionData.medicationType,
            medicationName: this.sessionData.medicationName,
            dose: this.sessionData.dose,
            route: this.sessionData.route,
            administeredAt: this.sessionData.administeredAt,
            alertType: this.sessionData.alertType,
            severity: this.sessionData.severity,
            alertMessage: this.sessionData.alertMessage,
            resolution: this.sessionData.resolution,
            notes: this.sessionData.notes
          });
        }
      },
      error: (error) => {
        console.error('Error loading session data:', error);
      }
    });
  }

  onSubmit(): void {
    if (this.postDialysisForm.invalid) {
      this.snackBar.open('Please fill in all required fields', 'Close', {
        duration: 3000,
        panelClass: ['error-snackbar']
      });
      return;
    }

    if (!this.sessionData) {
      this.snackBar.open('Session data not loaded. Please refresh the page.', 'Close', {
        duration: 3000,
        panelClass: ['error-snackbar']
      });
      return;
    }

    this.loading = true;
    const formData = this.postDialysisForm.value;

    // Helper to convert to string
    const toString = (value: any): string | null => {
      if (value === '' || value === null || value === undefined) return null;
      return String(value);
    };

    // Create update request - merge with existing session data
    const requestData = {
      ...this.sessionData,
      // Update with post-dialysis form values
      PostWeight: formData.postWeight || null,
      PostSBP: formData.postSBP || null,
      PostDBP: formData.postDBP || null,
      PostHR: formData.postHR || null,
      AccessBleedingTime: toString(formData.accessBleedingTime),
      TotalFluidRemoved: formData.totalFluidRemoved || null,
      PostAccessStatus: toString(formData.postAccessStatus),
      MedicationType: toString(formData.medicationType),
      MedicationName: toString(formData.medicationName),
      Dose: toString(formData.dose),
      Route: toString(formData.route),
      AdministeredAt: toString(formData.administeredAt),
      AlertType: toString(formData.alertType),
      Severity: toString(formData.severity),
      AlertMessage: toString(formData.alertMessage),
      Resolution: toString(formData.resolution),
      Notes: toString(formData.notes)
    };

    // Update the existing schedule with post-dialysis data
    this.scheduleService.updateSchedule(this.scheduleId, requestData).subscribe({
      next: (response) => {
        if (response.success) {
          this.snackBar.open('✓ Post-dialysis data saved successfully!', 'Close', {
            duration: 3000,
            panelClass: ['success-snackbar']
          });
          
          // Navigate to patient history page
          this.router.navigate(['/patients', this.patientId, 'history']);
        } else {
          this.errorMessage = response.message || 'Failed to save data';
          this.loading = false;
        }
      },
      error: (error) => {
        console.error('Error saving post-dialysis data:', error);
        this.errorMessage = 'Failed to save data. Please try again.';
        this.loading = false;
        
        this.snackBar.open('Error saving data', 'Close', {
          duration: 3000,
          panelClass: ['error-snackbar']
        });
      }
    });
  }

  goHome(): void {
    this.router.navigate(['/admin']);
  }

  goBack(): void {
    this.location.back();
  }

  getSlotName(slotId: number): string {
    const slots: { [key: number]: string } = {
      1: 'Morning (06:00 AM - 10:00 AM)',
      2: 'Afternoon (10:00 AM - 02:00 PM)',
      3: 'Evening (04:00 PM - 08:00 PM)',
      4: 'Night (08:00 PM - 12:00 AM)'
    };
    return slots[slotId] || 'Unknown';
  }

  autoSaveData(): void {
    if (!this.sessionData || this.postDialysisForm.pristine) {
      return; // Don't auto-save if no changes or no session data
    }

    const formData = this.postDialysisForm.value;

    // Helper to convert to string
    const toString = (value: any): string | null => {
      if (value === '' || value === null || value === undefined) return null;
      return String(value);
    };

    // Merge existing session data with post-dialysis form data
    const requestData = {
      ...this.sessionData,
      PostWeight: formData.postWeight || null,
      PostSBP: formData.postSBP || null,
      PostDBP: formData.postDBP || null,
      PostHR: formData.postHR || null,
      AccessBleedingTime: toString(formData.accessBleedingTime),
      TotalFluidRemoved: formData.totalFluidRemoved || null,
      PostAccessStatus: toString(formData.postAccessStatus),
      MedicationType: toString(formData.medicationType),
      MedicationName: toString(formData.medicationName),
      Dose: toString(formData.dose),
      Route: toString(formData.route),
      AdministeredAt: toString(formData.administeredAt),
      AlertType: toString(formData.alertType),
      Severity: toString(formData.severity),
      AlertMessage: toString(formData.alertMessage),
      Resolution: toString(formData.resolution),
      Notes: toString(formData.notes)
    };

    // Auto-save without showing loading spinner or notifications
    this.scheduleService.updateSchedule(this.scheduleId, requestData).subscribe({
      next: (response) => {
        if (response.success) {
          console.log('✓ Post-dialysis data auto-saved at', new Date().toLocaleTimeString());
          this.postDialysisForm.markAsPristine(); // Mark as saved
        }
      },
      error: (error) => {
        console.error('Auto-save failed:', error.error);
      }
    });
  }
}
