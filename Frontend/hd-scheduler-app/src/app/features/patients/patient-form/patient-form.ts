import { Component, OnInit } from '@angular/core';
import { CommonModule, Location } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
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
import { PatientService } from '../../../core/services/patient.service';
import { StaffManagementService } from '../../../services/staff-management.service';
import { AuthService } from '../../../core/services/auth.service';
import { CreatePatientRequest, UpdatePatientRequest } from '../../../core/models/patient.model';

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
    MatTooltipModule
  ],
  templateUrl: './patient-form.html',
  styleUrl: './patient-form.scss',
})
export class PatientForm implements OnInit {
  patientForm: FormGroup;
  loading = false;
  errorMessage = '';
  isEditMode = false;
  patientId: number | null = null;
  
  // Role-based access control
  canEdit = false;
  userRole = '';
  isReadOnly = false;

  dialyserTypes = ['HI', 'LO'];
  accessTypes = ['AVF', 'AVG', 'CVC'];
  dialysatePrescriptions = ['Normal', 'K+ Free', 'Ca++', 'Dextrose'];
  anticoagulationTypes = ['Heparin', 'Without Heparin'];
  syringeTypes = ['10 ml', '20 ml'];
  slots = [
    { id: 1, name: 'Morning' },
    { id: 2, name: 'Afternoon' },
    { id: 3, name: 'Evening' },
    { id: 4, name: 'Night' }
  ];
  beds = Array.from({ length: 10 }, (_, i) => i + 1);
  allDoctors: any[] = [];
  allNurses: any[] = [];
  filteredDoctors: any[] = [];
  filteredNurses: any[] = [];

  constructor(
    private fb: FormBuilder,
    private patientService: PatientService,
    private staffService: StaffManagementService,
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute,
    private location: Location
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
      guardianName: [''] // Guardian name
    });
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
    
    // Load doctors and nurses first
    this.loadStaff();
    
    // Check if we're in edit mode
    this.route.params.subscribe(params => {
      if (params['id']) {
        this.isEditMode = true;
        this.patientId = +params['id'];
        this.loadPatient(this.patientId);
      }
    });
  }

  loadStaff(): void {
    // Load all active doctors and nurses
    this.staffService.getStaffByRole('Doctor').subscribe({
      next: (response: any) => {
        console.log('Raw doctor response:', response);
        if (response.success && response.data) {
          // API returns active staff already, but double check
          this.allDoctors = response.data;
          console.log('All active doctors loaded:', this.allDoctors);
          console.log('Sample doctor object:', this.allDoctors[0]);
          this.filterStaffBySlot();
        }
      },
      error: (err) => console.error('Error loading doctors:', err)
    });

    this.staffService.getStaffByRole('Nurse').subscribe({
      next: (response: any) => {
        console.log('Raw nurse response:', response);
        if (response.success && response.data) {
          // API returns active staff already, but double check
          this.allNurses = response.data;
          console.log('All active nurses loaded:', this.allNurses);
          console.log('Sample nurse object:', this.allNurses[0]);
          this.filterStaffBySlot();
        }
      },
      error: (err) => console.error('Error loading nurses:', err)
    });
  }

  filterStaffBySlot(): void {
    const selectedSlotId = this.patientForm.get('slotID')?.value;
    console.log('Filtering staff for slot:', selectedSlotId);
    
    if (selectedSlotId) {
      // Filter staff assigned to the selected slot
      this.filteredDoctors = this.allDoctors.filter(
        (d: any) => d.assignedSlot === selectedSlotId
      );
      this.filteredNurses = this.allNurses.filter(
        (n: any) => n.assignedSlot === selectedSlotId
      );

      console.log('Filtered doctors for slot', selectedSlotId, ':', this.filteredDoctors);
      console.log('Filtered nurses for slot', selectedSlotId, ':', this.filteredNurses);

      // Auto-assign if only one option available
      if (this.filteredDoctors.length === 1) {
        this.patientForm.patchValue({ assignedDoctor: this.filteredDoctors[0].staffID });
        console.log('Auto-assigned doctor:', this.filteredDoctors[0].name);
      }
      if (this.filteredNurses.length === 1) {
        this.patientForm.patchValue({ assignedNurse: this.filteredNurses[0].staffID });
        console.log('Auto-assigned nurse:', this.filteredNurses[0].name);
      }
    } else {
      // No slot selected, show all staff
      this.filteredDoctors = this.allDoctors;
      this.filteredNurses = this.allNurses;
      console.log('No slot selected - showing all staff');
    }
  }

  onSlotChange(): void {
    // Clear current assignments when slot changes
    this.patientForm.patchValue({
      assignedDoctor: null,
      assignedNurse: null
    });
    
    // Filter staff for the new slot
    this.filterStaffBySlot();
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
            guardianName: patient.guardianName || ''
          });
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
    if (this.patientForm.invalid) {
      this.patientForm.markAllAsTouched();
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    // Create patient with basic information
    const formValue = this.patientForm.value;
    const patientData: CreatePatientRequest = {
      mrn: formValue.mrn || null,
      name: formValue.name,
      age: Number(formValue.age),
      gender: formValue.gender,
      contactNumber: formValue.contactNumber,
      emergencyContact: formValue.emergencyContact || null,
      address: formValue.address || null,
      guardianName: formValue.guardianName || null
    };

    if (this.isEditMode && this.patientId) {
      // Update patient information
      const updateData: UpdatePatientRequest = {
        patientID: this.patientId,
        ...patientData
      };
      this.patientService.updatePatient(this.patientId, updateData).subscribe({
        next: (response: any) => {
          if (response.success) {
            this.router.navigate(['/patients']);
          } else {
            this.errorMessage = response.message || 'Failed to update patient';
            this.loading = false;
          }
        },
        error: (error: any) => {
          console.error('Error updating patient:', error);
          this.errorMessage = 'Failed to update patient. Please try again.';
          this.loading = false;
        }
      });
    } else {
      // Create new patient and return to patient list
      this.patientService.createPatient(patientData).subscribe({
        next: (response) => {
          console.log('Create patient response:', response);
          if (response.success && response.data) {
            // Navigate back to patient list
            this.router.navigate(['/patients']);
          } else {
            console.error('Patient creation failed:', response);
            this.errorMessage = response.message || 'Failed to create patient';
            this.loading = false;
          }
        },
        error: (error) => {
          console.error('Error creating patient:', error);
          this.errorMessage = error.error?.message || 'Failed to create patient. Please try again.';
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
}
