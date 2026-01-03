import { Component, OnInit, Inject } from '@angular/core';
import { CommonModule, Location } from '@angular/common';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
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
    TextBoxModule,
    PatientInfoDialogComponent
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
  
  // Time slot grouping for Today/Tomorrow tabs
  groupedReservedPatients: any = {
    morning: [],
    afternoon: [],
    evening: [],
    night: [],
    unassigned: []
  };
  slotStatistics: any = {
    morning: { total: 10, used: 0, patients: 0 },
    afternoon: { total: 10, used: 0, patients: 0 },
    evening: { total: 10, used: 0, patients: 0 },
    night: { total: 10, used: 0, patients: 0 }
  };
  
  // Missed appointments tracking
  possibleNoShows: any[] = [];
  loadingNoShows = false;
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
  
  // Store patient status colors in memory (persists across grid refreshes)
  private patientStatusColors: Map<number, string> = new Map(); // patientId -> colorClass

  constructor(
    private patientService: PatientService,
    private scheduleService: ScheduleService,
    private reservationService: ReservationService,
    private authService: AuthService,
    private router: Router,
    private location: Location,
    private dialog: MatDialog,
    private http: HttpClient
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
    console.log('✅ PatientList component initialized');
    console.log('✅ onActivateReservedPatient method exists:', typeof this.onActivateReservedPatient === 'function');
    
    // Check user role and set permissions
    this.userRole = this.authService.getUserRole() || '';
    this.canEdit = this.authService.hasRole(['Admin', 'Doctor', 'Nurse']);
    this.isReadOnly = this.userRole === 'Technician';
    
    // Load data for the initially selected tab (default is 0 - Pre Schedule)
    this.onTabChange(this.selectedTabIndex);
    
    // Auto-refresh when page becomes visible
    document.addEventListener('visibilitychange', this.handleVisibilityChange.bind(this));
  }

  ngOnDestroy(): void {
    document.removeEventListener('visibilitychange', this.handleVisibilityChange.bind(this));
  }

  /**
   * Row Data Bound - Apply color coding based on patient status
   * Called for each row rendered in Syncfusion Grid
   */
  onRowDataBound(args: any): void {
    if (!args.data) return;
    
    const patient = args.data;
    const rowElement = args.row as HTMLElement;
    const patientId = patient.patientId || patient.patientID || patient.PatientID;
    
    // Remove all status classes first
    rowElement.classList.remove('row-active', 'row-missed', 'row-late', 'row-rescheduled', 'row-discharged');
    
    // Check if we have a stored color for this patient (from recent button clicks)
    if (patientId && this.patientStatusColors.has(patientId)) {
      const savedColor = this.patientStatusColors.get(patientId);
      if (savedColor) {
        rowElement.classList.add(savedColor);
        console.log(`🎨 Applied saved color ${savedColor} to patient ${patientId}`);
        return;
      }
    }
    
    // Otherwise, apply CSS class based on status from database (priority order)
    if (patient.sessionStatus === 'Active') {
      rowElement.classList.add('row-active');
      console.log('🟢 Row colored GREEN (Active):', patient.name);
    } else if (patient.sessionStatus === 'Missed' || patient.isMissed) {
      rowElement.classList.add('row-missed');
      console.log('🔴 Row colored RED (Missed):', patient.name);
    } else if (patient.isLate) {
      rowElement.classList.add('row-late');
      console.log('🟡 Row colored YELLOW (Late):', patient.name);
    } else if (patient.sessionStatus === 'Rescheduled' || patient.isRescheduled) {
      rowElement.classList.add('row-rescheduled');
      console.log('🔵 Row colored BLUE (Rescheduled):', patient.name);
    } else if (patient.isDischarged) {
      rowElement.classList.add('row-discharged');
      console.log('⚫ Row colored GRAY (Discharged):', patient.name);
    }
  }

  /**
   * Force grid refresh to update row colors after status changes
   */
  refreshGrids(): void {
    // Trigger change detection by reassigning arrays (deep copy to force Angular to detect changes)
    this.groupedReservedPatients = {
      morning: [...this.groupedReservedPatients.morning],
      afternoon: [...this.groupedReservedPatients.afternoon],
      evening: [...this.groupedReservedPatients.evening],
      night: [...this.groupedReservedPatients.night],
      unassigned: [...this.groupedReservedPatients.unassigned]
    };
    this.filteredReservedPatients = [...this.filteredReservedPatients];
    this.filteredPatients = [...this.filteredPatients];
    
    console.log('🔄 Grids refreshed - triggering change detection');
  }

  /**
   * Manually update row color by finding the row in DOM and save to memory
   */
  updateRowColor(patientId: number, colorClass: string): void {
    // Save color to memory so it persists across grid refreshes
    this.patientStatusColors.set(patientId, colorClass);
    console.log(`💾 Saved color ${colorClass} for patient ${patientId} to memory`);
    
    // Find all grid rows and update the one matching this patient
    setTimeout(() => {
      const allRows = document.querySelectorAll('.e-grid .e-row');
      console.log(`🔍 Searching ${allRows.length} rows for patient ID ${patientId}`);
      
      let found = false;
      allRows.forEach((row: any) => {
        // Try to find patient ID in any cell of the row
        const cells = row.querySelectorAll('.e-rowcell');
        cells.forEach((cell: any) => {
          if (cell.textContent && cell.textContent.trim() === String(patientId)) {
            // Remove all color classes
            row.classList.remove('row-active', 'row-late', 'row-missed', 'row-rescheduled', 'row-discharged');
            // Add new color class
            row.classList.add(colorClass);
            found = true;
            console.log(`✅ Manually colored row for patient ${patientId} with class ${colorClass}`);
          }
        });
      });
      
      if (!found) {
        console.warn(`⚠️ Could not find row for patient ID ${patientId}`);
        // Try refreshing grids to trigger rowDataBound which will use saved color
        this.refreshGrids();
      }
    }, 100);
  }

  handleVisibilityChange(): void {
    if (!document.hidden) {
      // Page is visible again, refresh the currently active tab
      this.onTabChange(this.selectedTabIndex);
    }
  }

  onRefresh(): void {
    // Refresh the currently active tab
    this.onTabChange(this.selectedTabIndex);
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
    console.log('🩺 Schedule HD button clicked:', patient);
    
    // If patient has an active schedule, navigate to edit page
    // Otherwise, create a new schedule
    if (patient.scheduleID && !patient.isDischarged) {
      console.log('Navigating to edit HD session:', patient.scheduleID);
      this.router.navigate(['/schedule/hd-session/edit', patient.scheduleID])
        .then(success => {
          if (success) {
            console.log('✅ Navigation to edit HD session successful');
          } else {
            console.error('❌ Navigation to edit HD session failed');
            this.showToast('Failed to navigate to HD session editor', 'Error');
          }
        })
        .catch(error => {
          console.error('❌ Navigation error:', error);
          this.showToast('Navigation error: ' + error.message, 'Error');
        });
    } else {
      console.log('Navigating to new HD session for patient:', patient.patientID);
      this.router.navigate(['/schedule/hd-session/new', patient.patientID])
        .then(success => {
          if (success) {
            console.log('✅ Navigation to new HD session successful');
          } else {
            console.error('❌ Navigation to new HD session failed');
            this.showToast('Failed to navigate to HD session creator', 'Error');
          }
        })
        .catch(error => {
          console.error('❌ Navigation error:', error);
          this.showToast('Navigation error: ' + error.message, 'Error');
        });
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

  onCompleteSession(patient: Patient): void {
    console.log('✅ COMPLETE SESSION BUTTON CLICKED:', patient);
    // Complete today's dialysis session - moves to Completed Sessions tab
    const sessionInfo = this.getSessionInfo(patient);
    const isLast = this.isLastSession(patient);
    
    let message: string;
    if (isLast) {
      message = `✅ Complete Final Session\n\n` +
                `Patient: ${patient.name}\n` +
                `Sessions Completed: ${patient.totalDialysisCompleted || 0}\n` +
                `Remaining: ${sessionInfo.remaining}\n\n` +
                `⚠️ This is the LAST scheduled session.\n` +
                `Mark today's dialysis as complete?`;
    } else if (sessionInfo.remaining > 0) {
      message = `✅ Complete Dialysis Session\n\n` +
                `Patient: ${patient.name}\n` +
                `Sessions Completed: ${patient.totalDialysisCompleted || 0}\n` +
                `Remaining: ${sessionInfo.remaining}\n\n` +
                `Mark today's dialysis as complete?\n` +
                `(Patient will move to Completed Sessions tab)`;
    } else {
      message = `✅ Complete Session\n\n` +
                `Mark ${patient.name}'s dialysis session as complete?`;
    }
    
    if (confirm(message)) {
      this.loading = true;
      
      if (patient.scheduleID) {
        // Complete the session (changes SessionStatus to 'Completed', keeps patient active in system)
        this.scheduleService.forceDischargeSession(patient.scheduleID).subscribe({
          next: (response) => {
            if (response.success) {
              this.showToast(`${patient.name}'s session completed successfully!`, 'Success');
              // Small delay to ensure database transaction completes
              setTimeout(() => {
                // Reload active patients to show completed status
                this.loadPatients();
                // Reload completed sessions tab
                this.loadCompletedSessions();
                // Switch to Completed Sessions tab (tab index 2)
                this.selectedTabIndex = 2;
                this.loading = false;
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
        // No active session - shouldn't happen in normal flow
        this.showToast('No active session found', 'Warning');
        this.loading = false;
      }
    }
  }

  onFullDischargePatient(patient: Patient): void {
    console.log('🗑️ FULL DISCHARGE BUTTON CLICKED:', patient);
    // FULL DISCHARGE: Remove patient from active program permanently
    const sessionInfo = this.getSessionInfo(patient);
    const isLast = this.isLastSession(patient);
    
    let message: string;
    if (isLast) {
      message = `🎯 FULL DISCHARGE - All Sessions Complete\n\n` +
                `Patient: ${patient.name}\n` +
                `Total Sessions: ${patient.totalDialysisCompleted || 0}\n\n` +
                `This patient has completed all scheduled sessions.\n` +
                `Remove from active program and move to Discharged History?`;
    } else {
      message = `⚠️ EARLY DISCHARGE WARNING\n\n` +
                `Patient: ${patient.name}\n` +
                `Sessions Completed: ${patient.totalDialysisCompleted || 0}\n` +
                `Remaining: ${sessionInfo.remaining}\n\n` +
                `This will remove the patient from the active program.\n` +
                `Reasons: Deceased, Transferred, Personal, etc.\n\n` +
                `Continue with full discharge?`;
    }
    
    if (confirm(message)) {
      // Ask for discharge reason
      const reason = prompt(`📝 Enter discharge reason:\n\n` +
                           `Examples:\n` +
                           `• Transferred to City Hospital\n` +
                           `• Deceased\n` +
                           `• Personal reasons\n` +
                           `• Kidney transplant\n` +
                           `• Moving to different city\n\n` +
                           `Reason:`) || 'Discharged';
      
      this.loading = true;
      
      // First record the discharge activity in history
      const activityData = {
        patientID: patient.patientID,
        scheduleID: patient.scheduleID || null,
        reason: reason,
        details: `Patient discharged from dialysis program. Total sessions completed: ${patient.totalDialysisCompleted || 0}`,
        recordedBy: 'System' // TODO: Get current user
      };

      this.http.post('http://localhost:5000/api/PatientActivity/discharged', activityData).subscribe({
        next: (activityResponse: any) => {
          console.log('✅ Discharge recorded in activity log', activityResponse);
          
          // Then discharge the patient
          this.patientService.dischargePatient(patient.patientID).subscribe({
            next: (response: any) => {
              if (response.success) {
              // Update local patient data to trigger row color change
                patient.isDischarged = true;
                patient.sessionStatus = 'Discharged';
                
                // Manually add CSS class to the row immediately
                this.updateRowColor(patient.patientID, 'row-discharged');
                
                // Force grid refresh to show color change
                this.refreshGrids();
                
                this.showToast(`${patient.name} fully discharged. Reason: ${reason}`, 'Success');
                setTimeout(() => {
                  // Reload all tabs
                  this.loadPatients();
                  this.loadCompletedSessions();
                  this.loadDischargedPatients();
                  // Switch to Discharged History tab (tab index 3)
                  this.selectedTabIndex = 3;
                  this.loading = false;
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
        },
        error: (error: any) => {
          console.error('Error recording discharge activity:', error);
          // Still try to discharge even if activity log fails
          this.patientService.dischargePatient(patient.patientID).subscribe({
            next: (response: any) => {
              if (response.success) {
                this.showToast(`${patient.name} discharged (activity not recorded)`, 'Warning');
                setTimeout(() => {
                  this.loadPatients();
                  this.loadCompletedSessions();
                  this.loadDischargedPatients();
                  this.selectedTabIndex = 3;
                  this.loading = false;
                }, 500);
              }
            },
            error: () => {
              this.showToast('Failed to discharge patient', 'Error');
              this.loading = false;
            }
          });
        }
      });
    }
  }

  onMarkLate(patient: any): void {
    console.log('🕐 ========== MARK LATE BUTTON CLICKED ==========');
    console.log('🕐 Patient data:', patient);
    
    if (!patient) {
      console.error('❌ No patient data');
      alert('Error: No patient data provided');
      return;
    }
    
    const patientName = patient.name || patient.Name || 'Patient';
    const patientId = patient.patientId || patient.PatientID || patient.patientID || patient.PatientId;
    
    if (!patientId) {
      console.error('❌ No patient ID found');
      alert(`Error: Patient ID not found!\n\nAvailable fields: ${Object.keys(patient).join(', ')}`);
      return;
    }
    
    const message = `⏰ Mark Late\n\nPatient: ${patientName} (ID: ${patientId})\n\nMark this patient as running late for today's session?`;
    console.log('🕐 Showing confirmation:', message);
    
    if (confirm(message)) {
      console.log('✅ User confirmed - marking as late');
      
      // Update UI IMMEDIATELY - don't wait for backend
      console.log('🎨 Before update:', { isLate: patient.isLate, sessionStatus: patient.sessionStatus });
      patient.isLate = true;
      patient.sessionStatus = 'Late';
      console.log('🎨 After update:', { isLate: patient.isLate, sessionStatus: patient.sessionStatus });
      
      // Manually add CSS class to the row immediately
      this.updateRowColor(patient.patientId || patient.patientID, 'row-late');
      
      // Force grid refresh to show color change
      this.refreshGrids();
      
      // Show success message immediately
      this.showToast(`${patientName} marked as late.`, 'Success');
      
      // Then call backend API in background (don't block on it)
      this.loading = true;
      
      // Record late arrival in patient activity log
      const activityData = {
        patientID: patientId,
        scheduleID: patient.scheduleID || patient.todaySession?.scheduleId || null,
        activityDate: new Date().toISOString(),
        reason: 'Running late',
        details: `Patient ${patientName} marked as running late for today's session`,
        recordedBy: 'System'
      };
      
      console.log('🕐 Sending late notification:', activityData);

      this.http.post('http://localhost:5000/api/PatientActivity/late', activityData).subscribe({
        next: (response: any) => {
          console.log('✅ Late notification response:', response);
          if (!response.success) {
            console.warn('⚠️ Backend API failed but UI already updated');
          }
          this.loading = false;
          
          // Reload data in background after a delay
          setTimeout(() => this.loadReservedPatients(), 3000);
        },
        error: (error: any) => {
          console.error('❌ Error recording late arrival:', error);
          this.showToast(`${patientName} marked as late (not saved to history).`, 'Warning');
          this.loading = false;
        }
      });
    } else {
      console.log('🚫 User cancelled');
    }
  }

  onReschedule(patient: any): void {
    console.log('📅 ========== RESCHEDULE BUTTON CLICKED ==========');
    console.log('📅 Patient data:', patient);
    
    if (!patient) {
      console.error('❌ No patient data');
      alert('Error: No patient data provided');
      return;
    }
    
    const patientName = patient.name || patient.Name || 'Patient';
    const patientId = patient.patientId || patient.PatientID || patient.patientID || patient.PatientId;
    const scheduleId = patient.scheduleID || patient.ScheduleID || null;
    const currentDate = patient.sessionDate || patient.SessionDate || patient.nextScheduledDay;
    
    if (!patientId) {
      console.error('❌ No patient ID found');
      alert(`Error: Patient ID not found!\n\nAvailable fields: ${Object.keys(patient).join(', ')}`);
      return;
    }
    
    if (!scheduleId) {
      this.showToast('No active session found to reschedule', 'Error');
      return;
    }
    
    // Prompt for new date
    const newDateStr = prompt(
      `📅 Reschedule Session\n\n` +
      `Patient: ${patientName}\n` +
      `Current Date: ${currentDate}\n\n` +
      `Enter new date (YYYY-MM-DD):\n` +
      `Example: 2026-01-10`,
      new Date().toISOString().split('T')[0] // Default to today
    );
    
    if (!newDateStr) {
      console.log('🚫 User cancelled reschedule');
      return;
    }
    
    // Validate date format
    const dateRegex = /^\d{4}-\d{2}-\d{2}$/;
    if (!dateRegex.test(newDateStr)) {
      this.showToast('Invalid date format. Use YYYY-MM-DD', 'Error');
      return;
    }
    
    const reason = prompt('📝 Reason for rescheduling:', 'Patient request') || 'Rescheduled';
    
    this.loading = true;
    console.log('📅 Rescheduling session:', { scheduleId, newDate: newDateStr, reason });
    
    // Update session date in backend
    this.scheduleService.rescheduleSession(scheduleId, newDateStr).subscribe({
      next: (response: any) => {
        if (response.success) {
          // Record reschedule in activity log
          const activityData = {
            patientID: patientId,
            scheduleID: scheduleId,
            reason: reason,
            details: `Session rescheduled from ${currentDate} to ${newDateStr}`,
            recordedBy: 'System',
            oldDateTime: currentDate,
            newDateTime: newDateStr
          };
          
          this.http.post('http://localhost:5000/api/PatientActivity/rescheduled', activityData).subscribe({
            next: () => {
              // Update local patient data to trigger row color change
              patient.isRescheduled = true;
              patient.sessionStatus = 'Rescheduled';
              
              // Manually add CSS class to the row immediately
              this.updateRowColor(patientId, 'row-rescheduled');
              
              // Force grid refresh to show color change
              this.refreshGrids();
              
              this.showToast(`${patientName}'s session rescheduled to ${newDateStr}`, 'Success');
              this.loading = false;
              
              // Reload data in background after a delay
              setTimeout(() => {
                this.loadReservedPatients();
                this.loadPatients();
              }, 2000);
            },
            error: (error: any) => {
              console.error('Error recording reschedule activity:', error);
              this.showToast(`Session rescheduled but activity log failed`, 'Warning');
              setTimeout(() => {
                this.loadReservedPatients();
                this.loadPatients();
                this.loading = false;
              }, 500);
            }
          });
        } else {
          this.showToast(response.message || 'Failed to reschedule session', 'Error');
          this.loading = false;
        }
      },
      error: (error: any) => {
        console.error('❌ Error rescheduling session:', error);
        this.showToast('Failed to reschedule session. Please try again.', 'Error');
        this.loading = false;
      }
    });
  }

  onMarkMissed(patient: any): void {
    console.log('❌ ========== MARK MISSED BUTTON CLICKED ==========');
    console.log('❌ Patient data:', patient);
    
    if (!patient) {
      console.error('❌ No patient data');
      alert('Error: No patient data provided');
      return;
    }
    
    const patientName = patient.name || patient.Name || 'Patient';
    const patientId = patient.patientId || patient.PatientID || patient.patientID || patient.PatientId;
    
    if (!patientId) {
      console.error('❌ No patient ID found');
      alert(`Error: Patient ID not found!\n\nAvailable fields: ${Object.keys(patient).join(', ')}`);
      return;
    }
    
    const message = `❌ Mark as MISSED (No-Show)\n\n` +
                    `Patient: ${patientName} (ID: ${patientId})\n\n` +
                    `⚠️ This will:\n` +
                    `• Record a no-show for today's session\n` +
                    `• Update patient attendance record\n` +
                    `• May affect future scheduling\n\n` +
                    `Confirm marking as missed?`;
    
    console.log('❌ Showing confirmation:', message);
    
    if (confirm(message)) {
      console.log('✅ User confirmed - marking as missed');
      this.loading = true;
      
      // Get schedule ID from patient data
      const scheduleId = patient.scheduleID || patient.todaySession?.scheduleId;
      
      if (scheduleId && scheduleId > 0) {
        // Call backend to mark session as missed
        this.scheduleService.markSessionAsMissed(scheduleId, 'No-Show', 'Patient did not arrive for scheduled session').subscribe({
          next: (response) => {
            if (response.success) {
              // Update local patient data to trigger row color change
              patient.isMissed = true;
              patient.sessionStatus = 'Missed';
              
              // Manually add CSS class to the row immediately
              this.updateRowColor(patient.patientId || patient.patientID, 'row-missed');
              
              // Force grid refresh to show color change
              this.refreshGrids();
              
              this.showToast(`${patientName} marked as MISSED. Session recorded as no-show.`, 'Warning');
              this.loading = false;
              
              // Reload data in background after a delay
              setTimeout(() => {
                this.loadReservedPatients();
                this.loadPatients();
              }, 2000);
            } else {
              this.showToast(response.message || 'Failed to mark as missed', 'Error');
              this.loading = false;
            }
          },
          error: (error) => {
            console.error('Error marking as missed:', error);
            this.showToast('Failed to mark as missed. Please try again.', 'Error');
            this.loading = false;
          }
        });
      } else {
        // No schedule ID, show message
        this.showToast(`${patientName} marked as MISSED (no active session to update)`, 'Warning');
        this.loading = false;
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
    const previousTabIndex = this.selectedTabIndex;
    this.selectedTabIndex = index;
    // Hide global search results when switching tabs
    this.showGlobalSearchResults = false;
    
    // Skip reload if we just activated a patient (data already refreshed)
    if (this.skipTabReload) {
      console.log('⏭️ Skipping tab reload - data already refreshed after activation');
      this.skipTabReload = false;
      return;
    }
    
    // Always load data, even if clicking the same tab (to refresh)
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
    
    // Pass appropriate date to API based on filter to ensure HD Cycle is calculated correctly
    const today = new Date();
    let targetDate = new Date(today);
    
    // For 'tomorrow' filter, pass tomorrow's date so backend calculates HD Cycle for tomorrow
    if (dateFilter === 'tomorrow') {
      targetDate.setDate(targetDate.getDate() + 1);
    }
    
    const dateStr = targetDate.toISOString().split('T')[0]; // Format: YYYY-MM-DD
    
    console.log(`🔄 Loading reserved patients with dateFilter="${dateFilter}", API date="${dateStr}"`);
    
    this.reservationService.getPatientsWithReservationStatus(dateStr).subscribe({
      next: (response) => {
        console.log('📦 Reserved patients API response:', response);
        if (response.success && response.data) {
          // Filter for patients with "Reserved" status
          // This now includes BOTH:
          // 1. Patients with real pre-scheduled database records
          // 2. Patients who SHOULD be scheduled today based on HD Cycle (auto-suggested)
          let allReserved = response.data.patients.filter((p: any) => p.status === 'Reserved');
          
          console.log(`✅ Total reserved patients from API: ${allReserved.length}`);
          console.log('📋 Reserved patient details:', allReserved.map((p: any) => ({
            name: p.name,
            status: p.status,
            isAutoSuggested: p.isAutoSuggested,
            shouldBeScheduledToday: p.shouldBeScheduledToday,
            hdCycle: p.hdCycle,
            todaySession: p.todaySession,
            nextScheduledDate: p.nextScheduledDate
          })));
          
          // Apply date filter
          this.reservedPatients = this.filterReservedByDate(allReserved, dateFilter);
          this.filteredReservedPatients = this.reservedPatients;
          console.log(`🎯 After date filter (${dateFilter}): ${this.reservedPatients.length} patients`);
          
          // Apply time slot grouping for 'today' and 'tomorrow' filters
          if (dateFilter === 'today' || dateFilter === 'tomorrow') {
            this.groupPatientsByTimeSlot(this.filteredReservedPatients);
            // Check for possible no-shows when viewing today
            if (dateFilter === 'today') {
              this.checkPossibleNoShows();
            }
          }
        }
        this.loadingReserved = false;
      },
      error: (error) => {
        console.error('❌ Error loading reserved patients:', error);
        this.loadingReserved = false;
      }
    });
  }

  filterReservedByDate(patients: any[], filterType: string): any[] {
    const now = new Date();
    const today = new Date(now.getFullYear(), now.getMonth(), now.getDate());
    
    console.log(`Filtering for: ${filterType}, Today's date: ${today.toISOString().split('T')[0]}`);
    
    return patients.filter((patient: any) => {
      // For patients who SHOULD be scheduled (based on HD Cycle for the requested date)
      // shouldBeScheduledToday flag is set by backend based on the date we passed to the API
      if (patient.shouldBeScheduledToday && patient.todaySession) {
        console.log(`✓ Patient ${patient.name} should be scheduled for selected date (HD Cycle)`);
        // For 'today' and 'tomorrow' filters, include HD Cycle based patients
        if (filterType === 'today' || filterType === 'tomorrow') {
          return true;
        }
        // For 'all' filter, include them
        if (filterType === 'all') {
          return true;
        }
      }
      
      // Check for both possible field names: nextScheduledDate and nextScheduledDay
      const nextSessionField = patient.nextScheduledDate || patient.nextScheduledDay;
      if (!nextSessionField) {
        // If no future session date but should be scheduled, handle above
        return false;
      }
      
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
    console.log('🎯 ========== ACTIVATE BUTTON CLICKED ==========');
    console.log('🔵 Full patient data:', JSON.stringify(patient, null, 2));
    console.log('🔵 Patient object keys:', Object.keys(patient));
    
    if (!patient) {
      console.error('❌ No patient data provided');
      alert('Error: No patient data provided');
      return;
    }
    
    // Handle all possible patient ID field name variations from API
    const patientId = patient.patientId || patient.PatientID || patient.patientID || patient.PatientId;
    const patientName = patient.name || patient.Name || 'Unknown Patient';
    const nextSession = patient.nextScheduledDay || patient.nextScheduledDate || 'Unknown';
    
    console.log('🔵 Extracted patient ID:', patientId);
    console.log('🔵 Extracted patient name:', patientName);
    console.log('🔵 Next scheduled session:', nextSession);
    
    if (!patientId) {
      console.error('❌ Invalid patient data - missing ID');
      console.error('❌ Available keys:', Object.keys(patient));
      alert(`Error: Patient ID not found!\n\nAvailable fields: ${Object.keys(patient).join(', ')}`);
      return;
    }
    
    // Check if patient has a session scheduled for TODAY
    const today = new Date().toISOString().split('T')[0]; // YYYY-MM-DD format
    const nextSessionDate = nextSession ? new Date(nextSession).toISOString().split('T')[0] : null;
    
    console.log('🔵 Today:', today);
    console.log('🔵 Next session date:', nextSessionDate);
    
    if (nextSessionDate && nextSessionDate !== today) {
      const message = `⚠️ Cannot Activate ${patientName}\n\n` +
                     `This patient's next session is scheduled for:\n` +
                     `${nextSession}\n\n` +
                     `You can only activate patients who have sessions scheduled for TODAY.\n\n` +
                     `What would you like to do?\n\n` +
                     `• Click OK to schedule a session for TODAY\n` +
                     `• Click Cancel to do nothing`;
      
      if (confirm(message)) {
        // Redirect to schedule page to create today's session
        console.log('🔵 Redirecting to schedule HD session for patient:', patientId);
        this.router.navigate(['/schedule/hd-session/new', patientId]);
      }
      return;
    }
    
    // Confirmation prompt for patients with today's session
    const confirmMessage = `Activate ${patientName} (ID: ${patientId}) for today's dialysis session?\n\nThis will move the patient to Active Patients tab.`;
    console.log('🔵 Showing confirmation dialog:', confirmMessage);
    
    if (!confirm(confirmMessage)) {
      console.log('🚫 Activation cancelled by user');
      return;
    }
    
    console.log('✅ User confirmed activation');
    console.log('🔵 Calling activation API for patient:', { id: patientId, name: patientName });
    this.loading = true;
    
    this.reservationService.activateReservedPatient(patientId).subscribe({
      next: (response) => {
        console.log('✅ Patient activated successfully:', response);
        
        // Update local patient data to trigger row color change
        patient.sessionStatus = 'Active';
        
        // Manually add CSS class to the row immediately
        this.updateRowColor(patientId, 'row-active');
        
        // Force grid refresh to show color change
        this.refreshGrids();
        
        this.showToast(`${patientName} activated successfully!`, 'Success');
        
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
        console.error('❌ Full error object:', JSON.stringify(error, null, 2));
        
        // Extract error message from various possible locations
        let errorMsg = 'Failed to activate patient';
        
        if (error.error?.message) {
          errorMsg = error.error.message;
        } else if (error.error?.errors) {
          errorMsg = Object.values(error.error.errors).flat().join(', ');
        } else if (error.message) {
          errorMsg = error.message;
        } else if (error.statusText) {
          errorMsg = error.statusText;
        }
        
        // Show detailed error message to user
        const fullErrorMsg = `❌ Cannot Activate ${patientName}\n\n` +
                            `Error: ${errorMsg}\n\n` +
                            `This usually means:\n` +
                            `• No session scheduled for TODAY\n` +
                            `• Session is already active\n` +
                            `• Session was cancelled or completed\n\n` +
                            `Would you like to schedule a new session for TODAY?`;
        
        if (confirm(fullErrorMsg)) {
          console.log('🔵 User wants to schedule new session - redirecting...');
          this.router.navigate(['/schedule/hd-session/new', patientId]);
        }
        
        this.showToast(errorMsg, 'Error');
        this.loading = false;
      }
    });
  }

  // View history for a reserved patient
  onViewReservedPatientHistory(patient: any): void {
    console.log('🔄 VIEW HISTORY BUTTON CLICKED:', patient);
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

  // ==================== TIME SLOT GROUPING METHODS ====================

  /**
   * Group patients by their actual scheduled time slot
   * Priority: todaySession.slotId > nextSession.slotId > preferredSlotID
   */
  groupPatientsByTimeSlot(patients: any[]): void {
    this.groupedReservedPatients = {
      morning: [],
      afternoon: [],
      evening: [],
      night: [],
      unassigned: []
    };

    patients.forEach((patient: any) => {
      // Try to get actual scheduled slot (prioritize todaySession, then nextSession, then preference)
      let slotId = patient.todaySession?.slotId || patient.nextSession?.slotId || patient.preferredSlotID;
      
      // Debug logging
      console.log(`🔍 Patient ${patient.name}:`, {
        todaySession: patient.todaySession,
        nextSession: patient.nextSession,
        preferredSlotID: patient.preferredSlotID,
        determinedSlotId: slotId,
        bedNumber: patient.bedNumber
      });
      
      // If no slot information at all, patient is unassigned
      if (!slotId) {
        console.log(`⚠️ ${patient.name} → UNASSIGNED (no slot info)`);
        this.groupedReservedPatients.unassigned.push(patient);
        return;
      }
      
      if (slotId === 1) {
        console.log(`✅ ${patient.name} → MORNING`);
        this.groupedReservedPatients.morning.push(patient);
      } else if (slotId === 2) {
        console.log(`✅ ${patient.name} → AFTERNOON`);
        this.groupedReservedPatients.afternoon.push(patient);
      } else if (slotId === 3) {
        console.log(`✅ ${patient.name} → EVENING`);
        this.groupedReservedPatients.evening.push(patient);
      } else if (slotId === 4) {
        console.log(`✅ ${patient.name} → NIGHT`);
        this.groupedReservedPatients.night.push(patient);
      } else {
        console.log(`⚠️ ${patient.name} → UNASSIGNED (invalid slotId: ${slotId})`);
        this.groupedReservedPatients.unassigned.push(patient);
      }
    });

    // Calculate statistics for each slot
    this.calculateSlotStatistics();
  }

  /**
   * Calculate bed usage statistics for each time slot
   */
  calculateSlotStatistics(): void {
    this.slotStatistics = {
      morning: {
        total: 10,
        used: this.groupedReservedPatients.morning.filter((p: any) => p.bedNumber).length,
        patients: this.groupedReservedPatients.morning.length
      },
      afternoon: {
        total: 10,
        used: this.groupedReservedPatients.afternoon.filter((p: any) => p.bedNumber).length,
        patients: this.groupedReservedPatients.afternoon.length
      },
      evening: {
        total: 10,
        used: this.groupedReservedPatients.evening.filter((p: any) => p.bedNumber).length,
        patients: this.groupedReservedPatients.evening.length
      },
      night: {
        total: 10,
        used: this.groupedReservedPatients.night.filter((p: any) => p.bedNumber).length,
        patients: this.groupedReservedPatients.night.length
      }
    };
  }

  /**
   * Get time slot name by ID
   */
  getTimeSlotName(slotId: number): string {
    const slots: { [key: number]: string } = {
      1: 'Morning (06:00 - 10:00)',
      2: 'Afternoon (11:00 - 15:00)',
      3: 'Evening (16:00 - 20:00)',
      4: 'Night (21:00 - 01:00)'
    };
    return slots[slotId] || 'Not Assigned';
  }

  /**
   * Get time slot icon
   */
  getTimeSlotIcon(slotName: string): string {
    if (slotName.includes('Morning')) return '☀️';
    if (slotName.includes('Afternoon')) return '🌤️';
    if (slotName.includes('Evening')) return '🌆';
    if (slotName.includes('Night')) return '🌙';
    return '❓';
  }

  // ==================== MISSED APPOINTMENT METHODS ====================

  /**
   * Check for possible no-shows (auto-detection)
   */
  checkPossibleNoShows(): void {
    this.loadingNoShows = true;
    
    console.log('🔍 Checking for possible no-shows...');
    
    this.scheduleService.checkPossibleNoShows().subscribe({
      next: (response) => {
        console.log('📦 No-show check response:', response);
        if (response.success && response.data) {
          this.possibleNoShows = response.data;
          console.log(`⚠️ Found ${this.possibleNoShows.length} possible no-shows:`, this.possibleNoShows);
          
          // Log each no-show patient (use patientID with uppercase ID)
          this.possibleNoShows.forEach((ns: any) => {
            console.log(`  - Patient ${ns.patientID} (${ns.patientName}): ${ns.minutesLate} minutes late`);
          });
        } else {
          console.log('✅ No late patients detected');
        }
        this.loadingNoShows = false;
      },
      error: (error) => {
        console.error('❌ Error checking for no-shows:', error);
        this.loadingNoShows = false;
      }
    });
  }

  /**
   * Check if a patient should show the "Late/No-Show" warning badge
   * Show only if detected as possible no-show (passed start time without activation)
   */
  isPossibleNoShow(patientId: number): boolean {
    console.log(`🔎 Checking if patient ${patientId} is a no-show...`);
    console.log('   Current possibleNoShows array:', this.possibleNoShows);
    
    // Backend returns 'patientID' (capital ID), not 'patientId'
    const isNoShow = this.possibleNoShows.some(ns => {
      console.log(`   Comparing: ${ns.patientID} === ${patientId} ?`, ns.patientID === patientId);
      return ns.patientID === patientId;
    });
    
    if (isNoShow) {
      console.log(`🚨 Patient ${patientId} IS marked as no-show`);
    } else {
      console.log(`✅ Patient ${patientId} is NOT a no-show`);
    }
    return isNoShow;
  }

  /**
   * Reschedule a late patient to a different time slot
   */
  onRescheduleOrMarkMissed(patient: any): void {
    const patientId = patient.patientId || patient.patientID;
    const patientName = patient.name;
    const scheduleId = patient.scheduleId || patient.ScheduleID;
    
    const choice = prompt(
      `Patient: ${patientName} is late!\n\n` +
      `Choose an option:\n` +
      `1. Reschedule to Morning (06:00 - 10:00)\n` +
      `2. Reschedule to Afternoon (11:00 - 15:00)\n` +
      `3. Reschedule to Evening (16:00 - 20:00)\n` +
      `4. Reschedule to Night (21:00 - 01:00)\n` +
      `5. Mark as Missed\n\n` +
      `Enter number (1-5):`
    );
    
    if (!choice) return;
    
    const slotMap: { [key: string]: number } = {
      '1': 1, // Morning
      '2': 2, // Afternoon
      '3': 3, // Evening
      '4': 4  // Night
    };
    
    if (choice === '5') {
      // Mark as missed
      this.onMarkAsMissed(patient);
      return;
    }
    
    const newSlotId = slotMap[choice];
    if (!newSlotId) {
      this.showToast('Invalid choice', 'Error');
      return;
    }
    
    // Update the session's slot
    this.scheduleService.updateSessionSlot(scheduleId, newSlotId).subscribe({
      next: (response) => {
        if (response.success) {
          const slotNames: { [key: number]: string } = { 1: 'Morning', 2: 'Afternoon', 3: 'Evening', 4: 'Night' };
          this.showToast(`${patientName} rescheduled to ${slotNames[newSlotId]}`, 'Success');
          this.loadReservedPatients(this.selectedPreScheduleDateFilter);
          this.checkPossibleNoShows(); // Refresh no-shows list
        }
      },
      error: (error) => {
        console.error('Error rescheduling:', error);
        this.showToast('Failed to reschedule patient', 'Error');
      }
    });
  }

  /**
   * Mark a session as missed
   */
  onMarkAsMissed(patient: any): void {
    // Extract schedule ID from patient object (check multiple possible locations)
    const scheduleId = patient.todaySession?.scheduleId || 
                       patient.todaySession?.ScheduleID || 
                       patient.nextSession?.scheduleId || 
                       patient.nextSession?.ScheduleID || 
                       patient.scheduleId || 
                       patient.ScheduleID;
    
    console.log('🔵 Marking as missed - Patient data:', patient);
    console.log('🔵 Extracted scheduleId:', scheduleId);
    
    if (!scheduleId) {
      console.error('❌ Cannot mark as missed - Schedule ID not found in patient data');
      this.showToast('Cannot find schedule ID for this patient', 'Error');
      return;
    }
    
    const reason = prompt('Reason for missed appointment?\n\n1. Sick\n2. Emergency\n3. Transportation\n4. Unknown\n5. Other\n\nEnter number (1-5):');
    
    if (!reason) return;
    
    const reasonMap: { [key: string]: string } = {
      '1': 'Sick',
      '2': 'Emergency',
      '3': 'Transportation',
      '4': 'Unknown',
      '5': 'Other'
    };
    
    const missedReason = reasonMap[reason] || 'Unknown';
    const notes = prompt('Additional notes (optional):');
    
    console.log('🔵 Calling markSessionAsMissed API with:', { scheduleId, missedReason, notes });
    
    this.scheduleService.markSessionAsMissed(scheduleId, missedReason, notes || '').subscribe({
      next: (response) => {
        console.log('✅ Mark as missed response:', response);
        if (response.success) {
          this.showToast(`${patient.name} marked as missed: ${missedReason}`, 'Success');
          this.loadReservedPatients(this.selectedPreScheduleDateFilter);
          this.checkPossibleNoShows(); // Refresh no-shows list
        } else {
          this.showToast('Failed to mark session as missed', 'Error');
        }
      },
      error: (error) => {
        console.error('❌ Error marking as missed:', error);
        this.showToast(`Failed to mark session as missed: ${error.message}`, 'Error');
      }
    });
  }

  // ==================== SESSION TRACKING METHODS ====================

  /**
   * Check if this is the patient's last scheduled session
   * Returns true if patient has 1 or fewer pre-scheduled sessions remaining
   */
  isLastSession(patient: any): boolean {
    const sessionInfo = this.getSessionInfo(patient);
    return sessionInfo.remaining <= 1;
  }

  /**
   * Get session information for a patient
   */
  getSessionInfo(patient: any): { remaining: number, completed: number, total: number } {
    // Get patient's pre-scheduled sessions from reservedPatients list
    const patientSessions = this.reservedPatients.filter((p: any) => 
      (p.patientId || p.patientID || p.PatientID) === patient.patientID
    );

    const remaining = patientSessions.length;
    const completed = patient.totalDialysisCompleted || 0;
    const total = remaining + completed;

    return { remaining, completed, total };
  }

  /**
   * Get complete session button tooltip with session info
   */
  getCompleteSessionButtonTitle(patient: any): string {
    const sessionInfo = this.getSessionInfo(patient);
    
    if (sessionInfo.remaining <= 1) {
      return `⚠️ FINAL SESSION - Complete this session (${sessionInfo.completed} completed)`;
    } else if (sessionInfo.remaining > 1) {
      return `Complete today's session (${sessionInfo.remaining} remaining)`;
    } else {
      return `Complete dialysis session (${sessionInfo.completed} sessions completed)`;
    }
  }

  /**
   * Legacy function name - kept for backward compatibility
   */
  getDischargeButtonTitle(patient: any): string {
    return this.getCompleteSessionButtonTitle(patient);
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

// Helper function to get row CSS class based on patient status
export function getPatientRowClass(patient: any): string {
  if (!patient) return '';
  
  // Check status fields in order of priority
  if (patient.sessionStatus === 'Active') return 'row-active';
  if (patient.sessionStatus === 'Missed' || patient.isMissed) return 'row-missed';
  if (patient.isLate) return 'row-late';
  if (patient.sessionStatus === 'Rescheduled' || patient.isRescheduled) return 'row-rescheduled';
  if (patient.isDischarged) return 'row-discharged';
  
  return ''; // Default/Reserved
}
