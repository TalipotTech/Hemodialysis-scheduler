import { Component, OnInit, OnDestroy, Inject } from '@angular/core';
import { CommonModule, Location } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, AbstractControl, ValidationErrors } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDialog, MatDialogModule, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { Subject, debounceTime, takeUntil } from 'rxjs';
import { PatientService } from '../../../core/services/patient.service';
import { StaffManagementService } from '../../../services/staff-management.service';
import { AuthService } from '../../../core/services/auth.service';
import { CreatePatientRequest, UpdatePatientRequest } from '../../../core/models/patient.model';
import { EquipmentUsageAlertComponent } from '../../../shared/components/equipment-usage-alert/equipment-usage-alert.component';

@Component({
  selector: 'app-patient-form',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatSelectModule,
    MatProgressSpinnerModule,
    MatIconModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatCheckboxModule,
    MatTooltipModule,
    MatSnackBarModule,
    MatDialogModule,
    EquipmentUsageAlertComponent
  ],
  templateUrl: './patient-form.html',
  styleUrl: './patient-form.scss',
})
export class PatientForm implements OnInit, OnDestroy {
  patientForm: FormGroup;
  loading = false;
  errorMessage = '';
  isEditMode = false;
  patientId: number | null = null;
  
  // Role-based access control
  canEdit = false;
  userRole = '';
  isReadOnly = false;

  // Auto-save functionality
  private destroy$ = new Subject<void>();
  private formChanges$ = new Subject<void>();
  saveStatus: 'idle' | 'saving' | 'saved' = 'idle';
  lastSavedTime: Date | null = null;

  // HD Cycle options
  hdCycles = [
    { value: 'Every day', label: 'Every day (Daily)', frequency: 7 },
    { value: 'Every 2 days', label: 'Every 2 days (3-4x/week)', frequency: 3 },
    { value: 'Every 3 days', label: 'Every 3 days (2-3x/week)', frequency: 2 },
    { value: 'Every 4 days', label: 'Every 4 days (2x/week)', frequency: 2 },
    { value: 'Every 5 days', label: 'Every 5 days (1-2x/week)', frequency: 1 },
    { value: 'Every week', label: 'Every week (1x/week)', frequency: 1 },
    { value: 'Monday, Wednesday, Friday (3x/week)', label: 'Monday, Wednesday, Friday (3x/week)', frequency: 3 },
    { value: 'Tuesday, Thursday, Saturday (3x/week)', label: 'Tuesday, Thursday, Saturday (3x/week)', frequency: 3 }
  ];
  
  dialyserTypes = ['HI', 'LO'];
  dialysatePrescriptions = ['Normal', 'K+ Free', 'Ca++', 'Dextrose'];

  constructor(
    private fb: FormBuilder,
    private patientService: PatientService,
    private staffService: StaffManagementService,
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute,
    private location: Location,
    private snackBar: MatSnackBar,
    private dialog: MatDialog
  ) {
    // Basic Patient Information - as per specification
    this.patientForm = this.fb.group({
      mrn: [''], // Medical Record Number
      name: ['', [Validators.required, Validators.minLength(2)]], // Patient full name
      age: ['', [Validators.required, Validators.min(1), Validators.max(120)]], // Patient age in years
      gender: ['', Validators.required], // Patient gender
      contactNumber: ['', [Validators.required, Validators.pattern(/^[0-9]{10}$/)]], // Patient contact information - exactly 10 digits
      emergencyContact: ['', [Validators.pattern(/^[0-9]{10}$/)]], // Emergency contact details - exactly 10 digits
      address: [''], // Patient address
      guardianName: [''], // Guardian name
      
      // Basic HD Information
      dryWeight: [''], // Dry weight in kg
      hdStartDate: [''], // Date when HD treatment started
      hdCycle: [''], // HD frequency pattern
      hdFrequency: [''], // Sessions per week
      preferredSlotID: [null], // Preferred time slot (null = not set yet)
      prescribedDuration: [''], // Duration in hours
      
      // Dialyser & Dialysate
      dialyserType: [''], // HI or LO flux
      dialyserModel: [''], // Dialyser model
      prescribedBFR: [''], // Blood flow rate
      dialysatePrescription: [''], // Dialysate type
      
      // Equipment Usage & Session History (Start at 0 for new patients)
      dialyserCount: [0, [Validators.min(0), Validators.max(7)]], // Current dialyser usage count (Max: 7) - NOT required, 0 is valid
      bloodTubingCount: [0, [Validators.min(0), Validators.max(12)]], // Current blood tubing usage count (Max: 12) - NOT required, 0 is valid
      totalDialysisCompleted: [0, [Validators.min(0)]] // Total dialysis sessions completed (auto-calculated)
    }, { validators: this.contactNumberValidator() }); // Validate that contact numbers are different
  }

  // Custom validator to ensure Contact Number and Emergency Contact are different
  private contactNumberValidator() {
    return (control: AbstractControl): ValidationErrors | null => {
      const contactNumber = control.get('contactNumber')?.value;
      const emergencyContact = control.get('emergencyContact')?.value;
      
      // Skip validation if emergency contact is empty or null
      if (!emergencyContact || emergencyContact.trim() === '') {
        return null;
      }
      
      // Only validate if BOTH fields have values AND they are the same
      if (contactNumber && emergencyContact && contactNumber === emergencyContact) {
        console.warn('⚠️ Contact numbers are the same:', contactNumber, emergencyContact);
        return { contactsSame: true };
      }
      
      return null;
    };
  }

  ngOnInit(): void {
    // Check user role and set permissions
    this.userRole = this.authService.getUserRole() || '';
    this.canEdit = this.authService.hasRole(['Admin', 'Doctor', 'Nurse']);
    this.isReadOnly = this.userRole === 'Technician';
    
    // If technician tries to access form, redirect to view-only patient list
    if (this.isReadOnly && !this.isEditMode) {
      alert('Technicians have read-only access. You cannot create or edit patients.');
      this.router.navigate(['/patients']);
      return;
    }
    
    // Disable form for read-only users
    if (this.isReadOnly) {
      this.patientForm.disable();
    }
    
    // Check if we're in edit mode
    this.route.params.subscribe(params => {
      if (params['id']) {
        this.isEditMode = true;
        this.patientId = +params['id'];
        this.loadPatient(this.patientId);
      } else {
        // Setup auto-save for new patients only
        this.setupAutoSave();
        this.loadDraft();
      }
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  setupAutoSave(): void {
    // Listen to form changes with debounce
    this.patientForm.valueChanges
      .pipe(
        debounceTime(600000), // Wait 10 minutes after user stops typing
        takeUntil(this.destroy$)
      )
      .subscribe(() => {
        this.autoSaveDraft();
      });
  }

  autoSaveDraft(): void {
    if (this.patientForm.invalid || this.isEditMode) {
      return;
    }

    this.saveStatus = 'saving';
    
    // Save to localStorage
    const draftData = {
      formData: this.patientForm.value,
      timestamp: new Date().toISOString()
    };
    
    localStorage.setItem('patient_form_draft', JSON.stringify(draftData));
    
    setTimeout(() => {
      this.saveStatus = 'saved';
      this.lastSavedTime = new Date();
      
      // Reset to idle after 3 seconds
      setTimeout(() => {
        this.saveStatus = 'idle';
      }, 3000);
    }, 500);
  }

  loadDraft(): void {
    const draftStr = localStorage.getItem('patient_form_draft');
    if (!draftStr) return;

    try {
      const draft = JSON.parse(draftStr);
      const draftAge = Date.now() - new Date(draft.timestamp).getTime();
      
      // Only restore if draft is less than 24 hours old
      if (draftAge < 24 * 60 * 60 * 1000) {
        const snackBarRef = this.snackBar.open(
          `Found unsaved draft from ${new Date(draft.timestamp).toLocaleString()}. Restore it?`,
          'Restore',
          { duration: 10000 }
        );

        snackBarRef.onAction().subscribe(() => {
          this.patientForm.patchValue(draft.formData);
          this.snackBar.open('Draft restored successfully', 'OK', { duration: 3000 });
        });
      } else {
        localStorage.removeItem('patient_form_draft');
      }
    } catch (error) {
      console.error('Error loading draft:', error);
      localStorage.removeItem('patient_form_draft');
    }
  }

  clearDraft(): void {
    localStorage.removeItem('patient_form_draft');
  }

  validateDialyserCount(event: any): void {
    const value = parseInt(event.target.value);
    if (value > 7) {
      this.patientForm.patchValue({ dialyserCount: 7 });
      this.snackBar.open('⚠️ Maximum dialyser count is 7. Patient must bring new dialyser!', 'OK', { duration: 4000 });
    } else if (value < 0) {
      this.patientForm.patchValue({ dialyserCount: 0 });
    }
  }

  validateBloodTubingCount(event: any): void {
    const value = parseInt(event.target.value);
    if (value > 12) {
      this.patientForm.patchValue({ bloodTubingCount: 12 });
      this.snackBar.open('⚠️ Maximum blood tubing count is 12. Patient must bring new tubing!', 'OK', { duration: 4000 });
    } else if (value < 0) {
      this.patientForm.patchValue({ bloodTubingCount: 0 });
    }
  }

  loadPatient(id: number): void {
    this.loading = true;
    this.patientService.getPatient(id).subscribe({
      next: (response: any) => {
        if (response.success && response.data) {
          const patient = response.data;
          console.log('Loading patient data:', patient);
          this.patientForm.patchValue({
            mrn: patient.mrn || '',
            name: patient.name || '',
            age: patient.age || '',
            gender: patient.gender || '',
            contactNumber: patient.contactNumber || '',
            emergencyContact: patient.emergencyContact || '',
            address: patient.address || '',
            guardianName: patient.guardianName || '',
            
            // HD Information
            dryWeight: patient.dryWeight || '',
            hdStartDate: patient.hdStartDate || '',
            hdCycle: patient.hdCycle || '',
            hdFrequency: patient.hdFrequency || '',
            preferredSlotID: patient.preferredSlotID != null ? patient.preferredSlotID : null,
            prescribedDuration: patient.prescribedDuration || '',
            dialyserType: patient.dialyserType || '',
            dialyserModel: patient.dialyserModel || '',
            prescribedBFR: patient.prescribedBFR || '',
            dialysatePrescription: patient.dialysatePrescription || '',
            
            // Equipment Usage
            dialyserCount: patient.dialyserCount || 0,
            bloodTubingCount: patient.bloodTubingCount || 0,
            totalDialysisCompleted: patient.totalDialysisCompleted || 0
          });
          
          console.log('✅ Patient data loaded successfully');
          console.log('📝 Form valid after load:', this.patientForm.valid);
          console.log('📝 Form value:', this.patientForm.value);
          console.log('📝 Form errors:', this.patientForm.errors);
          
          // Debug: Check which fields are invalid
          if (this.patientForm.invalid) {
            console.warn('⚠️ Form is INVALID after loading. Invalid fields:');
            Object.keys(this.patientForm.controls).forEach(key => {
              const control = this.patientForm.get(key);
              if (control && control.invalid) {
                console.warn(`  - ${key}:`, control.errors);
              }
            });
          }
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

  onSubmit(): void {
    console.log('📝 Form submitted');
    console.log('📝 Form valid:', this.patientForm.valid);
    console.log('📝 Form invalid:', this.patientForm.invalid);
    console.log('📝 Form errors:', this.patientForm.errors);
    
    if (this.patientForm.invalid) {
      // Log which fields are invalid
      const invalidFields: string[] = [];
      Object.keys(this.patientForm.controls).forEach(key => {
        const control = this.patientForm.get(key);
        if (control && control.invalid) {
          invalidFields.push(`${key}: ${JSON.stringify(control.errors)}`);
        }
      });
      console.error('❌ Invalid fields:', invalidFields);
      
      this.patientForm.markAllAsTouched();
      this.snackBar.open(`Please fix validation errors: ${invalidFields.join(', ')}`, 'OK', { duration: 5000 });
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    // Create patient with basic information and HD treatment details
    const formValue = this.patientForm.value;
    
    // Helper function to format date as YYYY-MM-DD to avoid timezone issues
    const formatDateOnly = (date: any): string | null => {
      if (!date) return null;
      const d = new Date(date);
      if (isNaN(d.getTime())) return null;
      const year = d.getFullYear();
      const month = String(d.getMonth() + 1).padStart(2, '0');
      const day = String(d.getDate()).padStart(2, '0');
      return `${year}-${month}-${day}`;
    };
    
    const patientData: CreatePatientRequest = {
      mrn: formValue.mrn || null,
      name: formValue.name,
      age: Number(formValue.age),
      gender: formValue.gender,
      contactNumber: formValue.contactNumber,
      emergencyContact: formValue.emergencyContact || null,
      address: formValue.address || null,
      guardianName: formValue.guardianName || null,
      
      // HD Information
      dryWeight: formValue.dryWeight ? Number(formValue.dryWeight) : null,
      hdStartDate: formatDateOnly(formValue.hdStartDate) as any, // Format as YYYY-MM-DD to avoid timezone shift
      hdCycle: formValue.hdCycle || null,
      hdFrequency: formValue.hdFrequency ? Number(formValue.hdFrequency) : null,
      preferredSlotID: formValue.preferredSlotID != null ? Number(formValue.preferredSlotID) : null,
      prescribedDuration: formValue.prescribedDuration ? Number(formValue.prescribedDuration) : null,
      dialyserType: formValue.dialyserType || null,
      dialyserModel: formValue.dialyserModel || null,
      prescribedBFR: formValue.prescribedBFR ? Number(formValue.prescribedBFR) : null,
      dialysatePrescription: formValue.dialysatePrescription || null,
      
      // Equipment Usage
      dialyserCount: formValue.dialyserCount ? Number(formValue.dialyserCount) : 0,
      bloodTubingCount: formValue.bloodTubingCount ? Number(formValue.bloodTubingCount) : 0,
      totalDialysisCompleted: formValue.totalDialysisCompleted ? Number(formValue.totalDialysisCompleted) : 0
    };
    
    // Debug logging
    console.log('Form Values:', formValue);
    console.log('Patient Data being sent:', patientData);
    console.log('PreferredSlotID - Raw:', formValue.preferredSlotID, 'Type:', typeof formValue.preferredSlotID);
    console.log('PreferredSlotID - Converted:', patientData.preferredSlotID);

    if (this.isEditMode && this.patientId) {
      // Update patient information
      const updateData: UpdatePatientRequest = {
        patientID: this.patientId,
        ...patientData
      };
      this.patientService.updatePatient(this.patientId, updateData).subscribe({
        next: (response: any) => {
          if (response.success) {
            this.snackBar.open('Patient updated successfully', 'OK', { duration: 3000 });
            this.router.navigate(['/patients']);
          } else {
            this.errorMessage = response.message || 'Failed to update patient';
            this.loading = false;
          }
        },
        error: (error: any) => {
          console.error('Error updating patient:', error);
          this.errorMessage = 'Failed to update patient. Please try again.';
          this.snackBar.open(this.errorMessage, 'OK', { duration: 5000 });
          this.loading = false;
        }
      });
    } else {
      // Create new patient and return to patient list
      this.patientService.createPatient(patientData).subscribe({
        next: (response) => {
          console.log('Create patient response:', response);
          if (response.success && response.data) {
            // Clear draft after successful save
            this.clearDraft();
            this.snackBar.open('Patient created successfully', 'OK', { duration: 3000 });
            // Navigate back to patient list
            this.router.navigate(['/patients']);
          } else {
            console.error('Patient creation failed:', response);
            this.errorMessage = response.message || 'Failed to create patient';
            this.snackBar.open(this.errorMessage, 'OK', { duration: 5000 });
            this.loading = false;
          }
        },
        error: (error) => {
          console.error('Error creating patient:', error);
          this.errorMessage = error.error?.message || 'Failed to create patient. Please try again.';
          this.snackBar.open(this.errorMessage, 'OK', { duration: 5000 });
          this.loading = false;
        }
      });
    }
  }

  onCancel(): void {
    this.router.navigate(['/patients']);
  }

  goBack(): void {
    this.location.back();
  }

  goToAdminDashboard(): void {
    this.router.navigate(['/admin']);
  }

  reloadEquipmentCounts(): void {
    if (!this.isEditMode || !this.patientId) {
      this.snackBar.open('Cannot reload counts for new patients', 'OK', { duration: 3000 });
      return;
    }

    this.loading = true;
    this.patientService.getPatient(this.patientId).subscribe({
      next: (response: any) => {
        if (response.success && response.data) {
          const patient = response.data;
          this.patientForm.patchValue({
            dialyserCount: patient.dialyserCount || 0,
            bloodTubingCount: patient.bloodTubingCount || 0,
            totalDialysisCompleted: patient.totalDialysisCompleted || 0
          });
          this.snackBar.open('Equipment counts reloaded successfully', 'OK', { duration: 3000 });
        }
        this.loading = false;
      },
      error: (error: any) => {
        console.error('Error reloading equipment counts:', error);
        this.snackBar.open('Failed to reload equipment counts', 'OK', { duration: 3000 });
        this.loading = false;
      }
    });
  }

  onHDCycleChange(): void {
    const selectedCycle = this.patientForm.get('hdCycle')?.value;
    const cycleOption = this.hdCycles.find(c => c.value === selectedCycle);
    
    if (cycleOption && cycleOption.frequency) {
      // Auto-populate HD Frequency based on selected cycle
      this.patientForm.patchValue({ hdFrequency: cycleOption.frequency });
      console.log(`Auto-filled hdFrequency to ${cycleOption.frequency} for cycle ${selectedCycle}`);
    }
  }

  // Check if MRN already exists when user leaves the MRN field
  onMrnBlur(): void {
    const mrnValue = this.patientForm.get('mrn')?.value;
    
    // Skip check if MRN is empty or if we're in edit mode
    if (!mrnValue || this.isEditMode) {
      return;
    }
    
    this.patientService.checkMrnExists(mrnValue).subscribe({
      next: (response) => {
        if (response.success && response.data === true) {
          // MRN already exists, show popup dialog
          this.dialog.open(MrnExistsDialogComponent, {
            width: '400px',
            data: { mrn: mrnValue }
          });
          
          // Clear the MRN field
          this.patientForm.patchValue({ mrn: '' });
        }
      },
      error: (error) => {
        console.error('Error checking MRN:', error);
        this.snackBar.open('Error checking MRN. Please try again.', 'OK', { duration: 3000 });
      }
    });
  }

  // Helper method to check if contact numbers are the same
  get hasContactNumberError(): boolean {
    return (this.patientForm.hasError('contactsSame') ?? false) && 
           ((this.patientForm.get('contactNumber')?.touched ?? false) || (this.patientForm.get('emergencyContact')?.touched ?? false));
  }

  // Helper method to get invalid field names for debugging
  getInvalidFields(): string[] {
    const invalidFields: string[] = [];
    Object.keys(this.patientForm.controls).forEach(key => {
      const control = this.patientForm.get(key);
      if (control && control.invalid) {
        invalidFields.push(key);
      }
    });
    return invalidFields;
  }
}

// Dialog component for MRN exists popup
@Component({
  selector: 'app-mrn-exists-dialog',
  template: `
    <h2 mat-dialog-title>MRN Already Exists</h2>
    <mat-dialog-content>
      <p>The MRN "{{ data.mrn }}" is already registered in the system.</p>
      <p>Please enter a different MRN number or check existing patient records.</p>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-raised-button color="primary" mat-dialog-close>OK</button>
    </mat-dialog-actions>
  `,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule
  ]
})
export class MrnExistsDialogComponent {
  constructor(
    @Inject(MAT_DIALOG_DATA) public data: { mrn: string }
  ) {}
}
