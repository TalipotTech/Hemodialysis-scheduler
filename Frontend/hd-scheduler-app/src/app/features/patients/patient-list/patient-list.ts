import { Component, OnInit } from '@angular/core';
import { CommonModule, Location } from '@angular/common';
import { Router } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTabsModule } from '@angular/material/tabs';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { FormsModule } from '@angular/forms';
import { PatientService } from '../../../core/services/patient.service';
import { ScheduleService } from '../../../core/services/schedule.service';
import { AuthService } from '../../../core/services/auth.service';
import { Patient } from '../../../core/models/patient.model';

@Component({
  selector: 'app-patient-list',
  imports: [
    CommonModule,
    FormsModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatProgressSpinnerModule,
    MatFormFieldModule,
    MatInputModule,
    MatChipsModule,
    MatTooltipModule,
    MatSnackBarModule,
    MatTabsModule,
    MatExpansionModule,
    MatProgressBarModule
  ],
  templateUrl: './patient-list.html',
  styleUrl: './patient-list.scss',
})
export class PatientList implements OnInit {
  patients: Patient[] = [];
  filteredPatients: Patient[] = [];
  loading = false;
  errorMessage = '';
  searchTerm = '';
  
  // Discharged patients tab
  selectedTabIndex = 0;
  dischargedPatients: Patient[] = [];
  filteredDischargedPatients: Patient[] = [];
  loadingDischarged = false;
  dischargedErrorMessage = '';
  dischargedSearchTerm = '';
  
  // Role-based access control
  canEdit = false;
  userRole = '';
  isReadOnly = false;
  
  displayedColumns: string[] = [
    'id',
    'mrn',
    'name',
    'age',
    'gender',
    'contact',
    'emergencyContact',
    'address',
    'guardian',
    'status',
    'schedule',
    'actions'
  ];

  constructor(
    private patientService: PatientService,
    private scheduleService: ScheduleService,
    private authService: AuthService,
    private router: Router,
    private location: Location,
    private snackBar: MatSnackBar
  ) {}

  goHome(): void {
    this.router.navigate(['/admin']);
  }

  goBack(): void {
    this.location.back();
  }

  ngOnInit(): void {
    // Check user role and set permissions
    this.userRole = this.authService.getUserRole() || '';
    this.canEdit = this.authService.hasRole(['Admin', 'Doctor', 'Nurse']);
    this.isReadOnly = this.userRole === 'Technician';
    
    this.loadPatients();
    // Auto-refresh when page becomes visible
    document.addEventListener('visibilitychange', this.handleVisibilityChange.bind(this));
  }

  ngOnDestroy(): void {
    document.removeEventListener('visibilitychange', this.handleVisibilityChange.bind(this));
  }

  handleVisibilityChange(): void {
    if (!document.hidden) {
      // Page is visible again, refresh the data
      this.loadPatients();
    }
  }

  onRefresh(): void {
    this.loadPatients();
  }

  loadPatients(): void {
    this.loading = true;
    this.errorMessage = '';
    
    // Load only active patients with non-discharged sessions
    this.patientService.getActivePatients().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          // Filter out patients with discharged sessions
          this.patients = response.data.filter(p => !p.isDischarged);
          this.filteredPatients = this.patients;
        } else {
          this.errorMessage = response.message || 'Failed to load patients';
        }
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading patients:', error);
        this.errorMessage = 'Failed to load patients. Please try again.';
        this.loading = false;
      }
    });
  }

  onSearch(): void {
    const term = this.searchTerm.toLowerCase().trim();
    if (!term) {
      this.filteredPatients = this.patients;
      return;
    }
    
    this.filteredPatients = this.patients.filter(patient =>
      patient.name.toLowerCase().includes(term)
    );
  }

  onAddPatient(): void {
    this.router.navigate(['/patients/new']);
  }

  onEditPatient(patient: Patient): void {
    this.router.navigate(['/patients', patient.patientID]);
  }

  onViewHistory(patient: Patient): void {
    this.router.navigate(['/patients', patient.patientID, 'history']);
  }

  onScheduleHD(patient: Patient): void {
    // If patient has an active schedule, navigate to edit page
    // Otherwise, create a new schedule
    if (patient.scheduleID && !patient.isDischarged) {
      this.router.navigate(['/schedule/hd-session/edit', patient.scheduleID]);
    } else {
      this.router.navigate(['/schedule/hd-session/new', patient.patientID]);
    }
  }

  onPostSchedule(patient: Patient): void {
    // Navigate to post-dialysis data entry page
    if (patient.scheduleID && !patient.isDischarged) {
      this.router.navigate(['/patients', patient.patientID, 'post-schedule', patient.scheduleID]);
    } else {
      this.snackBar.open('No active session found for this patient', 'Close', {
        duration: 3000
      });
    }
  }

  onRecordVitals(patient: Patient): void {
    // Navigate to intra-dialytic vital monitoring page
    if (patient.scheduleID && !patient.isDischarged) {
      this.router.navigate(['/patients', patient.patientID, 'monitoring', patient.scheduleID]);
    } else {
      this.snackBar.open('No active session found for this patient', 'Close', {
        duration: 3000
      });
    }
  }

  onDischargePatient(patient: Patient): void {
    // Allow discharge even without active session
    const message = patient.scheduleID 
      ? `Mark ${patient.name}'s current dialysis session as complete and discharge?`
      : `Discharge ${patient.name} without an active session?`;
    
    if (confirm(message)) {
      this.loading = true;
      
      if (patient.scheduleID) {
        // If there's an active session, complete it first
        this.scheduleService.forceDischargeSession(patient.scheduleID).subscribe({
          next: (response) => {
            if (response.success) {
              this.snackBar.open('Patient session completed and discharged successfully', 'Close', {
                duration: 3000
              });
              // Small delay to ensure database transaction completes
              setTimeout(() => {
                // Reload both active and discharged patient lists
                this.loadPatients();
                // Always load discharged patients to ensure the tab is updated
                this.loadDischargedPatients();
              }, 500);
            } else {
              this.snackBar.open(response.message || 'Failed to complete session', 'Close', {
                duration: 3000
              });
              this.loading = false;
            }
          },
          error: (error: any) => {
            console.error('Error completing session:', error);
            this.snackBar.open('Failed to complete session. Please try again.', 'Close', {
              duration: 3000
            });
            this.loading = false;
          }
        });
      } else {
        // No active session, just mark patient as discharged
        this.patientService.dischargePatient(patient.patientID).subscribe({
          next: (response: any) => {
            if (response.success) {
              this.snackBar.open('Patient discharged successfully', 'Close', {
                duration: 3000
              });
              // Small delay to ensure database transaction completes
              setTimeout(() => {
                // Reload both active and discharged patient lists
                this.loadPatients();
                // Always load discharged patients to ensure the tab is updated
                this.loadDischargedPatients();
              }, 500);
            } else {
              this.snackBar.open(response.message || 'Failed to discharge patient', 'Close', {
                duration: 3000
              });
              this.loading = false;
            }
          },
          error: (error: any) => {
            console.error('Error discharging patient:', error);
            this.snackBar.open('Failed to discharge patient. Please try again.', 'Close', {
              duration: 3000
            });
            this.loading = false;
          }
        });
      }
    }
  }

  getSlotName(slotId: number | null): string {
    if (!slotId) return 'Not Assigned';
    const slots: { [key: number]: string } = {
      1: 'Morning',
      2: 'Afternoon',
      3: 'Evening',
      4: 'Night'
    };
    return slots[slotId] || 'Unknown';
  }

  getBedNumber(bedNumber: number | null): string {
    return bedNumber ? `Bed ${bedNumber}` : 'Not Assigned';
  }

  getStatusChipClass(isDischarged: boolean): string {
    return isDischarged ? 'status-discharged' : 'status-active';
  }

  getPatientStatus(patient: Patient): string {
    if (patient.isDischarged) {
      return 'Discharged';
    }

    // Check if patient has an active session today
    if (patient.sessionStartTime) {
      const sessionStart = new Date(patient.sessionStartTime);
      const now = new Date();
      const diffHours = (now.getTime() - sessionStart.getTime()) / (1000 * 60 * 60);

      // If dialysis session started and it's been 4+ hours, mark as Completed
      if (diffHours >= 4) {
        return 'Completed';
      } else if (diffHours >= 0) {
        return 'In Progress';
      }
    }

    return 'Active';
  }

  getPatientStatusClass(patient: Patient): string {
    const status = this.getPatientStatus(patient);
    switch (status) {
      case 'Discharged':
        return 'status-discharged';
      case 'In Progress':
        return 'status-in-progress';
      case 'Completed':
        return 'status-completed';
      default:
        return 'status-active';
    }
  }

  onTabChange(index: number): void {
    this.selectedTabIndex = index;
    if (index === 1) {
      // Load discharged patients when switching to History tab
      this.loadDischargedPatients();
    }
  }

  loadDischargedPatients(): void {
    this.loadingDischarged = true;
    this.dischargedErrorMessage = '';
    
    console.log('Loading discharged patients...');
    
    // Load all patients including inactive and filter for discharged ones
    this.patientService.getAllIncludingInactive().subscribe({
      next: (response) => {
        console.log('Discharged patients response:', response);
        if (response.success && response.data) {
          console.log('Total patients received:', response.data.length);
          console.log('Patient IsActive values:', response.data.map(p => ({ id: p.patientID, name: p.name, isActive: p.isActive })));
          
          // Filter patients who are inactive (IsActive = false or 0)
          // Handle both boolean false and numeric 0 by converting to boolean
          this.dischargedPatients = response.data.filter(
            (p: Patient) => !p.isActive
          );
          console.log('Filtered discharged patients:', this.dischargedPatients.length);
          console.log('Discharged patients:', this.dischargedPatients);
          this.filteredDischargedPatients = this.dischargedPatients;
        } else {
          this.dischargedErrorMessage = 'Failed to load discharged patients';
        }
        this.loadingDischarged = false;
      },
      error: (error) => {
        console.error('Error loading discharged patients:', error);
        this.dischargedErrorMessage = 'Failed to load discharged patients. Please try again.';
        this.loadingDischarged = false;
      }
    });
  }

  onDischargedSearch(): void {
    const term = this.dischargedSearchTerm.toLowerCase().trim();
    if (!term) {
      this.filteredDischargedPatients = this.dischargedPatients;
      return;
    }
    
    this.filteredDischargedPatients = this.dischargedPatients.filter(patient =>
      patient.name.toLowerCase().includes(term) ||
      (patient.mrn && patient.mrn.toLowerCase().includes(term)) ||
      patient.contactNumber.includes(term)
    );
  }

  viewFullHistory(patientId: number): void {
    this.router.navigate(['/patients', patientId, 'history']);
  }
}
