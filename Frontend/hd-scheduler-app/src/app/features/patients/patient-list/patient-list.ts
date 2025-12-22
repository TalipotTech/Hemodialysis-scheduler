import { Component, OnInit, Inject } from '@angular/core';
import { CommonModule, Location } from '@angular/common';
import { Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDialog, MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { PatientInfoDialogComponent } from '../patient-info-dialog/patient-info-dialog.component';
// Syncfusion imports
import { GridModule, PageService, SortService, FilterService, ToolbarService, ExcelExportService, PdfExportService } from '@syncfusion/ej2-angular-grids';
import { ButtonModule, ChipListModule } from '@syncfusion/ej2-angular-buttons';
import { ToastModule, ToastUtility } from '@syncfusion/ej2-angular-notifications';
import { TabModule } from '@syncfusion/ej2-angular-navigations';
import { TooltipModule } from '@syncfusion/ej2-angular-popups';
import { TextBoxModule } from '@syncfusion/ej2-angular-inputs';
import { PatientService } from '../../../core/services/patient.service';
import { ScheduleService } from '../../../core/services/schedule.service';
import { ReservationService } from '../../../core/services/reservation.service';
import { AuthService } from '../../../core/services/auth.service';
import { Patient } from '../../../core/models/patient.model';

@Component({
  selector: 'app-patient-list',
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatIconModule,
    MatTooltipModule,
    MatDialogModule,
    MatButtonModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatFormFieldModule,
    MatInputModule,
    GridModule,
    ButtonModule,
    ChipListModule,
    ToastModule,
    TabModule,
    TooltipModule,
    TextBoxModule
  ],
  providers: [PageService, SortService, FilterService, ToolbarService, ExcelExportService, PdfExportService],
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
  
  // Global search (top-level search for all patients)
  globalSearchTerm = '';
  globalSearchResults: Patient[] = [];
  showGlobalSearchResults = false;
  
  // Reserved patients tab
  reservedPatients: any[] = [];
  filteredReservedPatients: any[] = [];
  loadingReserved = false;
  reservedSearchTerm = '';
  selectedPreScheduleDateFilter: string = 'all';
  preScheduleCustomStartDate: Date | null = null;
  preScheduleCustomEndDate: Date | null = null;
  
  // Completed sessions tab
  completedSessions: Patient[] = [];
  filteredCompletedSessions: Patient[] = [];
  loadingCompleted = false;
  completedSearchTerm = '';
  selectedDateFilter: string = 'today';
  customStartDate: Date | null = null;
  customEndDate: Date | null = null;
  
  // Role-based access control
  canEdit = false;
  userRole = '';
  isReadOnly = false;
  
  // Expanded patient cards tracking
  private expandedPatients: Set<number> = new Set();
  
  // Flag to prevent double-loading when switching tabs after activation
  private skipTabReload = false;

  constructor(
    private patientService: PatientService,
    private scheduleService: ScheduleService,
    private reservationService: ReservationService,
    private authService: AuthService,
    private router: Router,
    private location: Location,
    private dialog: MatDialog
  ) {}

  private showToast(message: string, title: string = 'Notification'): void {
    ToastUtility.show({
      title: title,
      content: message,
      position: { X: 'Right', Y: 'Top' },
      showCloseButton: true,
      timeOut: 3000
    });
  }

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
    
    // Backend now returns only patients with TODAY'S MANUALLY ACTIVATED sessions
    // Excludes auto-generated "Pre-Scheduled" sessions until explicitly activated
    // To activate a pre-scheduled patient:
    // 1. Go to Pre-Schedule tab
    // 2. Click "ACTIVATE" button on the patient
    // 3. This will move them to Active Patients tab for today's treatment
    this.patientService.getActivePatients().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          // Backend filters for today's manually activated sessions only
          this.patients = response.data;
          this.filteredPatients = this.patients;
          console.log('Active patients with today\'s sessions:', this.patients.length);
          console.log('📋 DETAILED ACTIVE PATIENTS:', this.patients.map(p => ({
            id: p.patientID,
            name: p.name,
            scheduleID: p.scheduleID,
            sessionStatus: p.sessionStatus,
            sessionDate: p.sessionDate
          })));
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
    console.log('Add New Patient clicked - navigating to /patients/new');
    this.router.navigate(['/patients/new']);
  }

  onEditPatient(patient: any): void {
    // Handle both Patient and reserved patient data structures
    const patientId = patient.patientID || patient.patientId;
    console.log('Edit patient clicked:', patientId);
    this.router.navigate(['/patients', patientId]);
  }

  onViewHistory(patient: any): void {
    // Handle both patientID and PatientID (case sensitivity from API)
    const patientId = patient.patientID || patient.PatientID;
    console.log('Navigating to history for patient:', patientId, patient);
    
    if (!patientId) {
      this.showToast('Patient ID not found', 'Error');
      return;
    }
    
    this.router.navigate(['/patients', patientId, 'history']);
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

  onQuickSchedule(patient: Patient): void {
    // Navigate to quick schedule form with AI recommendations
    this.router.navigate(['/patients', patient.patientID, 'hd-session']);
  }

  onPostSchedule(patient: Patient): void {
    // Navigate to post-dialysis data entry page
    if (patient.scheduleID && !patient.isDischarged) {
      this.router.navigate(['/patients', patient.patientID, 'post-schedule', patient.scheduleID]);
    } else {
      this.showToast('No active session found for this patient', 'Warning');
    }
  }

  onRecordVitals(patient: Patient): void {
    // Navigate to intra-dialytic vital monitoring page
    if (patient.scheduleID && !patient.isDischarged) {
      this.router.navigate(['/patients', patient.patientID, 'monitoring', patient.scheduleID]);
    } else {
      this.showToast('No active session found for this patient', 'Warning');
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
              this.showToast('Patient session completed and discharged successfully', 'Success');
              // Small delay to ensure database transaction completes
              setTimeout(() => {
                // Reload both active and discharged patient lists
                this.loadPatients();
                // Switch to Discharged History tab (tab index 3) and load data
                this.selectedTabIndex = 3;
                this.loadDischargedPatients();
              }, 500);
            } else {
              this.showToast(response.message || 'Failed to complete session', 'Error');
              this.loading = false;
            }
          },
          error: (error: any) => {
            console.error('Error completing session:', error);
            this.showToast('Failed to complete session. Please try again.', 'Error');
            this.loading = false;
          }
        });
      } else {
        // No active session, just mark patient as discharged
        this.patientService.dischargePatient(patient.patientID).subscribe({
          next: (response: any) => {
            if (response.success) {
              this.showToast('Patient discharged successfully', 'Success');
              // Small delay to ensure database transaction completes
              setTimeout(() => {
                // Reload both active and discharged patient lists
                this.loadPatients();
                // Switch to Discharged History tab (tab index 3) and load data
                this.selectedTabIndex = 3;
                this.loadDischargedPatients();
              }, 500);
            } else {
              this.showToast(response.message || 'Failed to discharge patient', 'Error');
              this.loading = false;
            }
          },
          error: (error: any) => {
            console.error('Error discharging patient:', error);
            this.showToast('Failed to discharge patient. Please try again.', 'Error');
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
    // Hide global search results when switching tabs
    this.showGlobalSearchResults = false;
    
    // Skip reload if we just activated a patient (data already refreshed)
    if (this.skipTabReload) {
      console.log('⏭️ Skipping tab reload - data already refreshed after activation');
      this.skipTabReload = false;
      return;
    }
    
    if (index === 0) {
      // Pre Schedule tab
      this.loadReservedPatients();
    } else if (index === 1) {
      // Active Patients tab (TODAY's sessions only)
      this.loadPatients();
    } else if (index === 2) {
      // Completed sessions tab
      this.loadCompletedSessions();
    } else if (index === 3) {
      // Discharged patients tab
      this.loadDischargedPatients();
    }
  }

  loadReservedPatients(dateFilter: string = 'all'): void {
    this.loadingReserved = true;
    this.selectedPreScheduleDateFilter = dateFilter;
    
    this.reservationService.getPatientsWithReservationStatus().subscribe({
      next: (response) => {
        console.log('Reserved patients response:', response);
        if (response.success && response.data) {
          // Filter for patients with "Reserved" status (no active session today but have future sessions)
          // This includes both manually scheduled and pre-scheduled (auto-generated) sessions
          let allReserved = response.data.patients.filter((p: any) => p.status === 'Reserved');
          
          // Debug: Log the dates to see what we're getting
          console.log('All reserved patients with dates:', allReserved.map((p: any) => ({
            name: p.name,
            nextScheduledDate: p.nextScheduledDate,
            nextScheduledDay: p.nextScheduledDay
          })));
          
          // Apply date filter
          this.reservedPatients = this.filterReservedByDate(allReserved, dateFilter);
          this.filteredReservedPatients = this.reservedPatients;
          console.log(`Pre-scheduled patients (${dateFilter}):`, this.reservedPatients.length);
        }
        this.loadingReserved = false;
      },
      error: (error) => {
        console.error('Error loading reserved patients:', error);
        this.loadingReserved = false;
      }
    });
  }

  filterReservedByDate(patients: any[], filterType: string): any[] {
    const now = new Date();
    const today = new Date(now.getFullYear(), now.getMonth(), now.getDate());
    
    console.log(`Filtering for: ${filterType}, Today's date: ${today.toISOString().split('T')[0]}`);
    
    return patients.filter((patient: any) => {
      // Check for both possible field names: nextScheduledDate and nextScheduledDay
      const nextSessionField = patient.nextScheduledDate || patient.nextScheduledDay;
      if (!nextSessionField) return false;
      
      const nextDate = new Date(nextSessionField);
      const nextDateOnly = new Date(nextDate.getFullYear(), nextDate.getMonth(), nextDate.getDate());
      
      switch (filterType) {
        case 'all':
          // Show all future sessions including today
          return nextDateOnly >= today;
        
        case 'today':
          return nextDateOnly.getTime() === today.getTime();
        
        case 'tomorrow':
          const tomorrow = new Date(today);
          tomorrow.setDate(tomorrow.getDate() + 1);
          return nextDateOnly.getTime() === tomorrow.getTime();
        
        case 'thisweek':
          const weekEnd = new Date(today);
          weekEnd.setDate(weekEnd.getDate() + 7);
          return nextDateOnly >= today && nextDateOnly <= weekEnd;
        
        case 'thismonth':
          return nextDateOnly.getMonth() === today.getMonth() && 
                 nextDateOnly.getFullYear() === today.getFullYear() &&
                 nextDateOnly >= today;
        
        case 'custom':
          if (!this.preScheduleCustomStartDate || !this.preScheduleCustomEndDate) return false;
          const startDate = new Date(this.preScheduleCustomStartDate);
          const endDate = new Date(this.preScheduleCustomEndDate);
          const start = new Date(startDate.getFullYear(), startDate.getMonth(), startDate.getDate());
          const end = new Date(endDate.getFullYear(), endDate.getMonth(), endDate.getDate());
          return nextDateOnly >= start && nextDateOnly <= end;
        
        default:
          return nextDateOnly >= today;
      }
    });
  }

  onPreScheduleDateFilterChange(filterType: string): void {
    if (filterType === 'custom') {
      this.openPreScheduleCustomDatePicker();
      return;
    }
    this.loadReservedPatients(filterType);
  }

  openPreScheduleCustomDatePicker(): void {
    const dialogRef = this.dialog.open(CustomDatePickerDialog, {
      width: '400px',
      data: {
        startDate: this.preScheduleCustomStartDate,
        endDate: this.preScheduleCustomEndDate,
        title: 'Pre-Schedule Date Range'
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.preScheduleCustomStartDate = result.startDate;
        this.preScheduleCustomEndDate = result.endDate;
        this.loadReservedPatients('custom');
      }
    });
  }

  onReservedSearch(): void {
    const term = this.reservedSearchTerm.toLowerCase().trim();
    if (!term) {
      // Re-apply date filter when search is cleared
      this.filteredReservedPatients = this.reservedPatients;
      return;
    }
    
    // Search within already date-filtered patients
    this.filteredReservedPatients = this.reservedPatients.filter((patient: any) => {
      // Handle both uppercase and lowercase property names
      const name = (patient.name || patient.Name || '').toString().toLowerCase();
      const mrn = (patient.mrn || patient.MRN || patient.Mrn || '').toString().toLowerCase();
      const contact = (patient.contactNumber || patient.ContactNumber || '').toString();
      
      return name.includes(term) || 
             mrn.includes(term) || 
             contact.includes(term);
    });
    
    console.log(`Pre Schedule search for "${term}": ${this.filteredReservedPatients.length} results`);
  }

  loadCompletedSessions(filterType: string = 'today'): void {
    this.loadingCompleted = true;
    this.selectedDateFilter = filterType;
    
    const { startDate, endDate } = this.getDateRange(filterType);
    
    console.log(`Loading completed sessions - Filter: ${filterType}, Range: ${startDate} to ${endDate}`);
    
    this.patientService.getCompletedSessionsByDateRange(startDate, endDate).subscribe({
      next: (response) => {
        console.log('Completed sessions response:', response);
        if (response.success && response.data) {
          this.completedSessions = response.data;
          this.filteredCompletedSessions = this.completedSessions;
          console.log(`Completed sessions loaded: ${this.completedSessions.length} patients`);
        } else {
          this.completedSessions = [];
          this.filteredCompletedSessions = [];
        }
        this.loadingCompleted = false;
      },
      error: (error) => {
        console.error('Error loading completed sessions:', error);
        this.completedSessions = [];
        this.filteredCompletedSessions = [];
        this.loadingCompleted = false;
      }
    });
  }

  getDateRange(filterType: string): { startDate: string, endDate: string } {
    const now = new Date();
    const today = new Date(now.getFullYear(), now.getMonth(), now.getDate());
    let startDate: Date;
    let endDate: Date;

    switch (filterType) {
      case 'today':
        startDate = today;
        endDate = today;
        break;
      case 'last24h':
        startDate = new Date(now.getTime() - 24 * 60 * 60 * 1000);
        endDate = now;
        break;
      case 'yesterday':
        const yesterday = new Date(today);
        yesterday.setDate(yesterday.getDate() - 1);
        startDate = yesterday;
        endDate = yesterday;
        break;
      case 'last7days':
        startDate = new Date(today);
        startDate.setDate(startDate.getDate() - 7);
        endDate = today;
        break;
      case 'thismonth':
        startDate = new Date(now.getFullYear(), now.getMonth(), 1);
        endDate = today;
        break;
      case 'custom':
        startDate = this.customStartDate || today;
        endDate = this.customEndDate || today;
        break;
      default:
        startDate = today;
        endDate = today;
    }

    return {
      startDate: startDate.toISOString().split('T')[0],
      endDate: endDate.toISOString().split('T')[0]
    };
  }

  onDateFilterChange(filterType: string): void {
    if (filterType === 'custom') {
      this.openCustomDatePicker();
      return;
    }
    this.loadCompletedSessions(filterType);
  }

  openCustomDatePicker(): void {
    const dialogRef = this.dialog.open(CustomDatePickerDialog, {
      width: '400px',
      data: {
        startDate: this.customStartDate,
        endDate: this.customEndDate
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.customStartDate = result.startDate;
        this.customEndDate = result.endDate;
        this.loadCompletedSessions('custom');
      }
    });
  }

  onCompletedSearch(): void {
    const term = this.completedSearchTerm.toLowerCase().trim();
    if (!term) {
      this.filteredCompletedSessions = this.completedSessions;
      return;
    }
    
    this.filteredCompletedSessions = this.completedSessions.filter(patient =>
      patient.name.toLowerCase().includes(term) ||
      (patient.mrn && patient.mrn.toLowerCase().includes(term)) ||
      patient.contactNumber.includes(term)
    );
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

  getPatientTooltip(patient: any): string {
    const hdCycle = patient.hdCycle || 'Not Set';
    const preferredSlot = this.getPreferredSlotName(patient.preferredSlotID);
    const bedAssignment = patient.bedNumber ? `Bed ${patient.bedNumber}` : 'Not Assigned';
    
    return `HD Cycle: ${hdCycle}\nPreferred Time: ${preferredSlot}\nBed Assignment: ${bedAssignment}`;
  }

  getPreferredSlotName(slotId: number | null): string {
    if (!slotId) return 'Not Set';
    
    const slots: { [key: number]: string } = {
      1: 'Morning (06:00 - 10:00)',
      2: 'Afternoon (11:00 - 15:00)',
      3: 'Evening (16:00 - 20:00)',
      4: 'Night (21:00 - 01:00)'
    };
    
    return slots[slotId] || 'Not Set';
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

  // Global search - searches ALL registered patients
  onGlobalSearch(): void {
    const term = this.globalSearchTerm.toLowerCase().trim();
    
    if (!term) {
      this.showGlobalSearchResults = false;
      this.globalSearchResults = [];
      return;
    }
    
    // Search all registered patients
    this.patientService.getAllPatients().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.globalSearchResults = response.data.filter(p => 
            p.isActive && (
              p.name.toLowerCase().includes(term) ||
              (p.mrn && p.mrn.toLowerCase().includes(term)) ||
              p.contactNumber.includes(term)
            )
          );
          this.showGlobalSearchResults = true;
          console.log(`Global search for "${term}": ${this.globalSearchResults.length} results`);
        }
      },
      error: (error) => {
        console.error('Error in global search:', error);
        this.showGlobalSearchResults = false;
      }
    });
  }

  // Activate a reserved patient - change from Pre-Scheduled to Active
  onActivateReservedPatient(patient: any): void {
    console.log('🔵 ACTIVATE BUTTON CLICKED - Patient data received:', patient);
    console.log('🔵 Patient object keys:', Object.keys(patient));
    
    // Handle both patientId and patientID (case variations from API)
    const patientId = patient.patientId || patient.PatientID || patient.patientID;
    
    console.log('🔵 Extracted patient ID:', patientId);
    
    if (!patient || !patientId) {
      console.error('❌ Invalid patient data - missing ID:', patient);
      this.showToast('Invalid patient data - Patient ID not found', 'Error');
      return;
    }

    console.log('🔵 Calling activation API for patient:', { id: patientId, name: patient.name });
    this.loading = true;
    
    this.reservationService.activateReservedPatient(patientId).subscribe({
      next: (response) => {
        console.log('✅ Patient activated successfully:', response);
        this.showToast(`${patient.name} activated successfully!`, 'Success');
        
        // Add small delay to ensure database transaction completes
        setTimeout(() => {
          // Refresh the active patients list
          this.patientService.getActivePatients().subscribe({
            next: (activeResponse) => {
              if (activeResponse.success && activeResponse.data) {
                this.patients = activeResponse.data;
                this.filteredPatients = this.patients;
                console.log('✅ Active patients refreshed:', this.patients.length);
                console.log('📋 Active patients details:', this.patients.map(p => ({ 
                  id: p.patientID, 
                  name: p.name, 
                  sessionStatus: p.sessionStatus,
                  scheduleID: p.scheduleID,
                  sessionDate: p.sessionDate
                })));
              }
              
              // Set flag to skip the tab change reload (data already fresh)
              this.skipTabReload = true;
              
              // Then switch to Active Patients tab (index 1)
              this.selectedTabIndex = 1;
              this.loading = false;
              
              // Refresh pre-schedule list in background to remove activated patient
              this.loadReservedPatients();
            },
            error: (error) => {
              console.error('❌ Error refreshing active patients:', error);
              this.loading = false;
              // Still switch to active tab even if refresh fails
              this.skipTabReload = true;
              this.selectedTabIndex = 1;
            }
          });
        }, 500); // Wait 500ms for database transaction to complete
      },
      error: (error) => {
        console.error('❌ Error activating patient:', error);
        const errorMsg = error.error?.message || error.message || 'Failed to activate patient';
        this.showToast(errorMsg, 'Error');
        this.loading = false;
      }
    });
  }

  // View history for a reserved patient
  onViewReservedPatientHistory(patient: any): void {
    this.router.navigate(['/patients', patient.patientId, 'history']);
  }

  openPatientInfoDialog(patient: Patient): void {
    this.dialog.open(PatientInfoDialogComponent, {
      width: '450px',
      data: {
        name: patient.name,
        hdStartDate: patient.hdStartDate,
        hdCycle: patient.hdCycle,
        bedNumber: patient.bedNumber,
        totalDialysisCompleted: patient.totalDialysisCompleted || 0
      }
    });
  }

  togglePatientCard(patientId: number): void {
    if (this.expandedPatients.has(patientId)) {
      this.expandedPatients.delete(patientId);
    } else {
      this.expandedPatients.add(patientId);
    }
  }

  isPatientExpanded(patientId: number): boolean {
    return this.expandedPatients.has(patientId);
  }
}

// Custom Date Picker Dialog Component
@Component({
  selector: 'custom-date-picker-dialog',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatDialogModule,
    MatButtonModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule
  ],
  template: `
    <h2 mat-dialog-title>
      <mat-icon>calendar_month</mat-icon>
      Select Custom Date Range
    </h2>
    <mat-dialog-content>
      <div class="date-picker-container">
        <mat-form-field appearance="outline" class="date-field">
          <mat-label>Start Date</mat-label>
          <input matInput [matDatepicker]="startPicker" [(ngModel)]="startDate" [max]="endDate || maxDate">
          <mat-datepicker-toggle matIconSuffix [for]="startPicker"></mat-datepicker-toggle>
          <mat-datepicker #startPicker></mat-datepicker>
        </mat-form-field>

        <mat-form-field appearance="outline" class="date-field">
          <mat-label>End Date</mat-label>
          <input matInput [matDatepicker]="endPicker" [(ngModel)]="endDate" [min]="startDate" [max]="maxDate">
          <mat-datepicker-toggle matIconSuffix [for]="endPicker"></mat-datepicker-toggle>
          <mat-datepicker #endPicker></mat-datepicker>
        </mat-form-field>
      </div>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button (click)="onCancel()">
        <mat-icon>close</mat-icon>
        Cancel
      </button>
      <button mat-raised-button color="primary" (click)="onApply()" [disabled]="!startDate || !endDate">
        <mat-icon>check</mat-icon>
        Apply Filter
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    .date-picker-container {
      display: flex;
      flex-direction: column;
      gap: 16px;
      padding: 20px 0;
      min-width: 300px;
    }

    .date-field {
      width: 100%;
    }

    h2 {
      display: flex;
      align-items: center;
      gap: 8px;
      margin: 0;
      padding: 16px 24px;
      background-color: #667eea;
      color: white;
      
      mat-icon {
        font-size: 24px;
        width: 24px;
        height: 24px;
      }
    }

    mat-dialog-content {
      padding: 0 24px;
    }

    mat-dialog-actions {
      padding: 16px 24px;
      
      button {
        display: flex;
        align-items: center;
        gap: 6px;
        
        mat-icon {
          font-size: 18px;
          width: 18px;
          height: 18px;
        }
      }
    }
  `]
})
export class CustomDatePickerDialog {
  startDate: Date | null = null;
  endDate: Date | null = null;
  maxDate = new Date();

  constructor(
    public dialogRef: MatDialogRef<CustomDatePickerDialog>,
    @Inject(MAT_DIALOG_DATA) public data: any
  ) {
    if (data?.startDate) {
      this.startDate = new Date(data.startDate);
    }
    if (data?.endDate) {
      this.endDate = new Date(data.endDate);
    }
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  onApply(): void {
    if (this.startDate && this.endDate) {
      this.dialogRef.close({
        startDate: this.startDate,
        endDate: this.endDate
      });
    }
  }
}
