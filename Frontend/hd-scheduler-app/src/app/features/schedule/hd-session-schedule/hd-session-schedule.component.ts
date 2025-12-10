import { Component, OnInit } from '@angular/core';
import { CommonModule, Location } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatTableModule } from '@angular/material/table';
import { MatNativeDateModule } from '@angular/material/core';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatGridListModule } from '@angular/material/grid-list';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatStepperModule } from '@angular/material/stepper';
import { MatTabsModule } from '@angular/material/tabs';
import { PatientService } from '../../../core/services/patient.service';
import { ScheduleService } from '../../../core/services/schedule.service';
import { AuthService } from '../../../core/services/auth.service';
import { AIService } from '../../../services/ai.service';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatRadioModule } from '@angular/material/radio';
import { EquipmentUsageAlertComponent } from '../../../shared/components/equipment-usage-alert/equipment-usage-alert.component';

@Component({
  selector: 'app-hd-session-schedule',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatDatepickerModule,
    MatExpansionModule,
    MatTableModule,
    MatNativeDateModule,
    MatProgressSpinnerModule,
    MatGridListModule,
    MatTooltipModule,
    MatSnackBarModule,
    MatStepperModule,
    MatTabsModule,
    MatRadioModule,
    EquipmentUsageAlertComponent
  ],
  templateUrl: './hd-session-schedule.component.html',
  styleUrl: './hd-session-schedule.component.scss'
})
export class HdSessionScheduleComponent implements OnInit {
  patientId: number | null = null;
  scheduleId: number | null = null;
  isEditMode: boolean = false;
  sessionForm: FormGroup;
  loading = false;
  errorMessage = '';
  currentStep = 0;
  autoSaveTimer: any = null;
  saveStatus: 'idle' | 'saving' | 'saved' = 'idle';
  lastSavedTime: Date | null = null;
  
  patient: any = null;
  patientHistory: any[] = [];
  showPatientHistory: boolean = false;
  selectedSlot: number | null = null;
  selectedBed: number | null = null;
  sessionNumber: number | null = null;
  totalWeeklySessions: number | null = null;
  bedAssignmentMode: 'auto' | 'manual' = 'auto'; // Default to auto-assign
  autoAssignedBed: number | null = null;
  showBedChangeOption: boolean = false;
  isAuthenticated: boolean = false;

  slots = [
    { id: 1, name: 'Slot 1 - Morning', time: '06:00 AM - 10:00 AM', beds: 10 },
    { id: 2, name: 'Slot 2 - Afternoon', time: '11:00 AM - 03:00 PM', beds: 10 },
    { id: 3, name: 'Slot 3 - Evening', time: '04:00 PM - 08:00 PM', beds: 10 },
    { id: 4, name: 'Slot 4 - Night', time: '09:00 PM - 01:00 AM', beds: 10 }
  ];

  beds: Array<{ number: number; isOccupied: boolean; patientName: string | null }> = Array.from({ length: 10 }, (_, i) => ({
    number: i + 1,
    isOccupied: false,
    patientName: null
  }));

  dialyserTypes = ['HI', 'LO'];
  accessTypes = ['AVF', 'AVG', 'CVC'];
  dialysatePrescriptions = ['Normal', 'K+ Free', 'Ca++', 'Dextrose'];
  anticoagulationTypes = ['Heparin', 'Without Heparin'];
  medicationTypes = ['Erythropoietin', 'Iron Supplement', 'Vitamin D', 'Antibiotic', 'Analgesic', 'Other'];
  medicationRoutes = ['IV', 'SC', 'IM', 'PO', 'Other'];
  alertTypes = ['Hypotension', 'Hypertension', 'Cramping', 'Chest Pain', 'Access Issue', 'Machine Alarm', 'Other'];
  severityLevels = ['Low', 'Medium', 'High', 'Critical'];
  
  // Monitoring Records for multiple entries
  monitoringRecords: any[] = [];
  currentTime: Date = new Date();
  monitoringTimeInterval: any = null;
  
  // AI Autocomplete
  loadingAutocomplete = false;
  autocompleteData: any = null;
  showAutocompleteCard = false;
  
  // Auto-filled fields tracking
  autoFilledFields: Array<{label: string, value: any}> = [];
  
  // Max date for date pickers (today)
  today: Date = new Date();
  
  hdCycles = [
    { value: 'Daily', label: 'Every Day (Daily)', frequency: 7 },
    { value: 'Every 2 days', label: 'Every 2 Days (Alternate Days)', frequency: 3 },
    { value: 'Every 3 days', label: 'Every 3 Days (Twice Weekly)', frequency: 2 },
    { value: 'Every 4 days', label: 'Every 4 Days', frequency: 2 },
    { value: 'Every 5 days', label: 'Every 5 Days', frequency: 1 },
    { value: 'Every 7 days', label: 'Every 7 Days (Weekly)', frequency: 1 },
    { value: '3x/week', label: '3 Times Per Week', frequency: 3 },
    { value: '2x/week', label: '2 Times Per Week', frequency: 2 },
    { value: 'Custom', label: 'Custom Schedule', frequency: null }
  ];

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private location: Location,
    private patientService: PatientService,
    private scheduleService: ScheduleService,
    private authService: AuthService,
    private aiService: AIService,
    private snackBar: MatSnackBar
  ) {
    this.sessionForm = this.fb.group({
      treatmentDate: [new Date()],
      slotID: [''],
      bedNumber: [''],
      dryWeight: ['', [Validators.min(1)]],
      hdStartDate: [new Date()],
      hdCycle: [''],
      hdFrequency: [''],
      hdCustomDays: [''],
      accessType: [''],
      accessLocation: [''],
      prescribedDuration: [4.0],  // Removed validators
      dialyserType: [''],
      dialyserModel: [''],
      prescribedBFR: [400],  // Removed validators
      dialysatePrescription: [''],
      anticoagulationType: [''],
      heparinBolusDose: [''],
      heparinInfusionRate: [''],
      // HDTreatmentSession fields
      startTime: [''],
      preWeight: [''],  // Removed validators
      ufGoal: [''],
      preBPSitting: [''],
      preTemperature: [''],
      accessBleedingTime: [''],
      accessStatus: [''],
      complications: [''],
      // IntraDialyticMonitoring fields
      monitoringTime: [''],
      bloodPressure: [''],
      heartRate: [''],
      temperature: [''],
      actualBFR: [''],
      venousPressure: [''],
      arterialPressure: [''],
      currentUFR: [''],
      totalUFAchieved: [''],
      tmpPressure: [''],
      symptoms: [''],
      interventions: [''],
      staffInitials: [''],
      monitoringNotes: [''],
      // Post-Dialysis Vital Signs
      postWeight: [''],
      postSBP: [''],
      postDBP: [''],
      postHR: [''],
      totalFluidRemoved: [''],
      postAccessStatus: [''],
      // PostDialysisMedications fields
      medicationType: [''],
      medicationName: [''],
      dose: [''],
      route: [''],
      administeredAt: [''],
      // TreatmentAlerts fields
      alertType: [''],
      alertMessage: [''],
      severity: [''],
      resolution: [''],
      // Additional notes field
      notes: ['']
    });
  }

  ngOnInit(): void {
    // Check authentication first
    this.isAuthenticated = this.authService.isAuthenticated();
    if (!this.isAuthenticated) {
      console.warn('⚠️ User not authenticated. Showing warning banner.');
      this.snackBar.open('⚠️ Please log in to save HD session data', 'Login', { 
        duration: 8000,
        horizontalPosition: 'center',
        verticalPosition: 'top'
      }).onAction().subscribe(() => {
        this.router.navigate(['/login'], { queryParams: { returnUrl: this.router.url } });
      });
    }
    
    // Start time updater for monitoring entries
    this.monitoringTimeInterval = setInterval(() => {
      this.updateCurrentTime();
    }, 1000); // Update every second

    this.route.params.subscribe(params => {
      if (params['patientId']) {
        this.patientId = +params['patientId'];
        this.loadPatientInfo();
        
        // Check for saved draft in localStorage
        setTimeout(() => {
          this.checkForSavedDraft();
        }, 500);
      }
      if (params['scheduleId']) {
        this.scheduleId = +params['scheduleId'];
        this.isEditMode = true;
        this.loadExistingSession();
        
        // Load monitoring records
        setTimeout(() => {
          this.loadMonitoringRecords();
        }, 1000);
        
        // Setup auto-save on form changes (debounced) - only in edit mode
        setTimeout(() => {
          this.sessionForm.valueChanges.subscribe(() => {
            if (this.autoSaveTimer) {
              clearTimeout(this.autoSaveTimer);
            }
            this.autoSaveTimer = setTimeout(() => {
              this.autoSaveData();
            }, 2000); // Auto-save after 2 seconds of no typing
          });
        }, 1000); // Wait for form to load before watching changes
      } else {
        // Setup auto-save for new sessions using localStorage
        setTimeout(() => {
          this.sessionForm.valueChanges.subscribe(() => {
            if (this.autoSaveTimer) {
              clearTimeout(this.autoSaveTimer);
            }
            this.autoSaveTimer = setTimeout(() => {
              this.saveDraftToLocalStorage();
            }, 2000); // Auto-save draft after 2 seconds of no typing
          });
        }, 1000);
      }
    });
  }

  loadPatientInfo(): void {
    if (!this.patientId) {
      this.errorMessage = 'Invalid patient ID';
      return;
    }

    this.loading = true;
    this.patientService.getPatient(this.patientId).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.patient = response.data;
          console.log('✅ Patient loaded:', this.patient);
          console.log('📋 Patient prescription fields:', {
            dialyserModel: this.patient.dialyserModel,
            prescribedDuration: this.patient.prescribedDuration,
            prescribedBFR: this.patient.prescribedBFR,
            dialysatePrescription: this.patient.dialysatePrescription,
            hdCycle: this.patient.hdCycle,
            hdFrequency: this.patient.hdFrequency
          });
          
          // Pre-fill form with patient data for new sessions
          if (!this.isEditMode && !this.scheduleId) {
            this.autoFilledFields = []; // Reset array
            
            // Track and display auto-filled fields
            if (this.patient.dryWeight) {
              this.sessionForm.patchValue({ dryWeight: this.patient.dryWeight });
              this.autoFilledFields.push({ label: 'Dry Weight', value: `${this.patient.dryWeight} kg` });
            }
            
            if (this.patient.hdStartDate) {
              const hdStartDate = new Date(this.patient.hdStartDate);
              this.sessionForm.patchValue({ 
                hdStartDate: hdStartDate,
                treatmentDate: hdStartDate // Set treatment date to HD start date
              });
              this.autoFilledFields.push({ label: 'HD Start Date', value: hdStartDate.toLocaleDateString() });
              this.autoFilledFields.push({ label: 'Treatment Date', value: hdStartDate.toLocaleDateString() });
            }
            
            if (this.patient.hdCycle) {
              this.sessionForm.patchValue({ hdCycle: this.patient.hdCycle });
              const cycleLabel = this.hdCycles.find(c => c.value === this.patient.hdCycle)?.label || this.patient.hdCycle;
              this.autoFilledFields.push({ label: 'HD Cycle', value: cycleLabel });
              
              // Auto-populate hdFrequency based on hdCycle
              const cycleOption = this.hdCycles.find(c => c.value === this.patient.hdCycle);
              if (cycleOption && cycleOption.frequency !== null) {
                this.sessionForm.patchValue({ hdFrequency: cycleOption.frequency });
              } else if (this.patient.hdFrequency) {
                this.sessionForm.patchValue({ hdFrequency: this.patient.hdFrequency });
              }
            } else if (this.patient.hdFrequency) {
              this.sessionForm.patchValue({ hdFrequency: this.patient.hdFrequency });
            }
            
            if (this.patient.dialyserType) {
              this.sessionForm.patchValue({ dialyserType: this.patient.dialyserType });
              this.autoFilledFields.push({ label: 'Dialyser Type', value: this.patient.dialyserType });
            }
            
            if (this.patient.dialyserModel) {
              this.sessionForm.patchValue({ dialyserModel: this.patient.dialyserModel });
              this.autoFilledFields.push({ label: 'Dialyser Model', value: this.patient.dialyserModel });
            }
            
            if (this.patient.prescribedDuration) {
              this.sessionForm.patchValue({ prescribedDuration: this.patient.prescribedDuration });
              this.autoFilledFields.push({ label: 'Prescribed Duration', value: `${this.patient.prescribedDuration} hours` });
            } else {
              this.sessionForm.patchValue({ prescribedDuration: 4.0 });
              this.autoFilledFields.push({ label: 'Prescribed Duration', value: '4 hours' });
            }
            
            if (this.patient.prescribedBFR) {
              this.sessionForm.patchValue({ prescribedBFR: this.patient.prescribedBFR });
              this.autoFilledFields.push({ label: 'Prescribed BFR', value: `${this.patient.prescribedBFR} mL/min` });
            } else {
              this.sessionForm.patchValue({ prescribedBFR: 400 });
              this.autoFilledFields.push({ label: 'Prescribed BFR', value: '400 mL/min' });
            }
            
            if (this.patient.dialysatePrescription) {
              this.sessionForm.patchValue({ dialysatePrescription: this.patient.dialysatePrescription });
              this.autoFilledFields.push({ label: 'Dialysate Prescription', value: this.patient.dialysatePrescription });
            }
          }
          
          // Auto-load suggested equipment counts from previous session
          this.loadSuggestedEquipmentCounts();
          
          // Load patient history
          this.loadPatientHistory();
          
          this.loading = false;
        } else {
          this.errorMessage = 'Failed to load patient information';
          this.loading = false;
        }
      },
      error: (error) => {
        console.error('Error loading patient:', error);
        this.errorMessage = 'Error loading patient information';
        this.loading = false;
        this.snackBar.open('Failed to load patient information', 'Close', { duration: 3000 });
      }
    });
  }

  loadPatientHistory(): void {
    if (!this.patientId) return;
    
    this.patientService.getPatientHistory(this.patientId).subscribe({
      next: (response: any) => {
        if (response.success && response.data?.sessions) {
          this.patientHistory = response.data.sessions
            .filter((s: any) => s.scheduleID !== this.scheduleId) // Exclude current session
            .sort((a: any, b: any) => new Date(b.sessionDate).getTime() - new Date(a.sessionDate).getTime());
        }
      },
      error: (error) => {
        console.error('Error loading patient history:', error);
      }
    });
  }

  togglePatientHistory(): void {
    this.showPatientHistory = !this.showPatientHistory;
  }

  loadPatientDetails(patientId: number): void {
    this.patientId = patientId;
    this.patientService.getPatient(patientId).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.patient = response.data;
          console.log('✅ Patient loaded in edit mode:', this.patient);
          console.log('📋 Prescription fields:', {
            dialyserModel: this.patient.dialyserModel,
            prescribedDuration: this.patient.prescribedDuration,
            prescribedBFR: this.patient.prescribedBFR,
            dialysatePrescription: this.patient.dialysatePrescription,
            hdCycle: this.patient.hdCycle,
            hdFrequency: this.patient.hdFrequency,
            dryWeight: this.patient.dryWeight,
            hdStartDate: this.patient.hdStartDate,
            dialyserType: this.patient.dialyserType
          });
          console.log('🔍 isEditMode:', this.isEditMode, '| patient exists:', !!this.patient);
          
          // Update treatment date to match HD Start Date if available
          if (this.patient.hdStartDate && this.isEditMode) {
            const hdStartDate = new Date(this.patient.hdStartDate);
            this.sessionForm.patchValue({ 
              treatmentDate: hdStartDate,
              hdStartDate: hdStartDate
            });
            console.log('📅 Updated treatment date to HD Start Date:', hdStartDate);
          }
        }
      },
      error: (error) => {
        console.error('Error loading patient details:', error);
      }
    });
  }

  viewSessionDetails(scheduleId: number): void {
    // Navigate to view/edit the selected session
    this.router.navigate(['/schedule/hd-session/edit', scheduleId]);
  }

  getPatientHDFrequency(): string {
    if (!this.patient) {
      return 'Loading...';
    }
    
    // Return HD Frequency if available
    if (this.patient.hdFrequency) {
      return this.patient.hdFrequency + 'x/week';
    }
    
    // Otherwise, try to calculate from HD Cycle
    if (this.patient.hdCycle) {
      // Try to match both the value (e.g., "MWF") and the full label
      const cycleOption = this.hdCycles.find(c => 
        c.value === this.patient.hdCycle || 
        c.value.toLowerCase() === this.patient.hdCycle.toLowerCase() ||
        c.label === this.patient.hdCycle
      );
      
      if (cycleOption && cycleOption.frequency !== null) {
        return cycleOption.frequency + 'x/week';
      }
    }
    
    // If we have the form loaded, try to get from the form
    const formCycle = this.sessionForm?.get('hdCycle')?.value;
    const formFrequency = this.sessionForm?.get('hdFrequency')?.value;
    
    if (formFrequency) {
      return formFrequency + 'x/week';
    }
    
    if (formCycle) {
      const cycleOption = this.hdCycles.find(c => c.value === formCycle);
      if (cycleOption && cycleOption.frequency !== null) {
        return cycleOption.frequency + 'x/week';
      }
    }
    
    return 'N/A';
  }

  loadSuggestedEquipmentCounts(): void {
    // This method has been deprecated - equipment counts are now managed at the patient level
    // Kept for backward compatibility but does nothing
    console.log('loadSuggestedEquipmentCounts called but equipment tracking moved to patient level');
  }

  loadExistingSession(): void {
    if (!this.scheduleId) {
      return;
    }

    this.loading = true;
    this.scheduleService.getScheduleById(this.scheduleId).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          const session = response.data;
          this.patientId = session.patientID;
          
          // Store session information
          this.sessionNumber = session.sessionNumber || session.SessionNumber || null;
          this.totalWeeklySessions = session.totalWeeklySessions || session.TotalWeeklySessions || null;
          
          // Load full patient details
          this.loadPatientDetails(session.patientID);
          
          // Pre-fill ALL form fields with existing data (or leave empty if null)
          // Log the data to see what we're receiving
          console.log('Loading session data:', session);
          
          // Backend returns PascalCase from database, so use PascalCase property names
          // Use HD Start Date as treatment date if available, otherwise use session date
          const hdStartDate = session.hdStartDate || session.HDStartDate;
          const treatmentDate = hdStartDate ? new Date(hdStartDate) : (session.sessionDate ? new Date(session.sessionDate) : new Date());
          
          this.sessionForm.patchValue({
            // Core fields
            treatmentDate: treatmentDate,
            slotID: session.slotID || session.SlotID || '',
            bedNumber: session.bedNumber || session.BedNumber || '',
            
            // Basic HD Info
            dryWeight: session.dryWeight || session.DryWeight || '',
            hdStartDate: hdStartDate ? new Date(hdStartDate) : null,
            hdCycle: session.hdCycle || session.HDCycle || '',
            hdFrequency: session.hdFrequency || session.HDFrequency || session.hdFrequency || '',
            hdCustomDays: session.hdCustomDays || session.HDCustomDays || '',
            weightGain: session.weightGain || session.WeightGain || '',
            
            // Equipment
            dialyserType: session.dialyserType || session.DialyserType || '',
            dialyserModel: session.dialyserModel || session.DialyserModel || '',
            hdUnitNumber: session.hdUnitNumber || session.HDUnitNumber || '',
            
            // Prescription
            prescribedDuration: session.prescribedDuration || session.PrescribedDuration || 4.0,
            prescribedBFR: session.prescribedBFR || session.PrescribedBFR || 400,
            ufGoal: session.ufGoal || session.UFGoal || '',
            dialysatePrescription: session.dialysatePrescription || session.DialysatePrescription || '',
            
            // Anticoagulation
            anticoagulationType: session.anticoagulationType || session.AnticoagulationType || '',
            heparinDose: session.heparinDose || session.HeparinDose || '',
            syringeType: session.syringeType || session.SyringeType || '',
            heparinBolusDose: session.bolusDose || session.BolusDose || '',
            heparinInfusionRate: session.heparinInfusionRate || session.HeparinInfusionRate || '',
            
            // Access
            accessType: session.accessType || session.AccessType || '',
            accessLocation: session.accessLocation || session.AccessLocation || '',
            
            // Vitals & Symptoms
            bloodPressure: session.bloodPressure || session.BloodPressure || '',
            symptoms: session.symptoms || session.Symptoms || '',
            bloodTestDone: session.bloodTestDone || session.BloodTestDone || false,
            
            // Staff Assignment
            assignedDoctor: session.assignedDoctor || session.AssignedDoctor || null,
            assignedNurse: session.assignedNurse || session.AssignedNurse || null,
            
            // HDTreatmentSession fields
            startTime: session.startTime || session.StartTime || '',
            preWeight: session.preWeight || session.PreWeight || '',
            preBPSitting: session.preBPSitting || session.PreBPSitting || '',
            preTemperature: session.preTemperature || session.PreTemperature || '',
            accessStatus: session.accessStatus || session.AccessStatus || '',
            complications: session.complications || session.Complications || '',
            
            // IntraDialyticMonitoring fields
            monitoringTime: session.monitoringTime || session.MonitoringTime || '',
            heartRate: session.heartRate || session.HeartRate || '',
            actualBFR: session.actualBFR || session.ActualBFR || '',
            venousPressure: session.venousPressure || session.VenousPressure || '',
            arterialPressure: session.arterialPressure || session.ArterialPressure || '',
            currentUFR: session.currentUFR || session.CurrentUFR || '',
            totalUFAchieved: session.totalUFAchieved || session.TotalUFAchieved || '',
            tmpPressure: session.tmpPressure || session.TmpPressure || '',
            interventions: session.interventions || session.Interventions || '',
            staffInitials: session.staffInitials || session.StaffInitials || '',
            
            // Post-Dialysis Vital Signs
            postWeight: session.postWeight || session.PostWeight || '',
            postSBP: session.postSBP || session.PostSBP || '',
            postDBP: session.postDBP || session.PostDBP || '',
            postHR: session.postHR || session.PostHR || '',
            accessBleedingTime: session.accessBleedingTime || session.AccessBleedingTime || '',
            totalFluidRemoved: session.totalFluidRemoved || session.TotalFluidRemoved || '',
            postAccessStatus: session.postAccessStatus || session.PostAccessStatus || '',
            
            // PostDialysisMedications fields
            medicationType: session.medicationType || session.MedicationType || '',
            medicationName: session.medicationName || session.MedicationName || '',
            dose: session.dose || session.Dose || '',
            route: session.route || session.Route || '',
            administeredAt: session.administeredAt || session.AdministeredAt || '',
            
            // TreatmentAlerts fields
            alertType: session.alertType || session.AlertType || '',
            alertMessage: session.alertMessage || session.AlertMessage || '',
            severity: session.severity || session.Severity || '',
            resolution: session.resolution || session.Resolution || ''
          });
          
          this.selectedSlot = session.slotID;
          this.selectedBed = session.bedNumber;
          
          // Auto-populate HD Frequency based on HD Cycle if not already set
          if ((session.hdCycle || session.HDCycle) && !(session.hdFrequency || session.HDFrequency)) {
            this.onHDCycleChange();
          }
          
          // DISABLE only the core registration fields ONLY if they have values (i.e., session is already activated)
          // For pre-scheduled sessions (no slot/bed assigned), keep these fields enabled
          if (session.slotID || session.SlotID) {
            this.sessionForm.get('treatmentDate')?.disable();
            this.sessionForm.get('slotID')?.disable();
            this.sessionForm.get('bedNumber')?.disable();
          }
          if (session.accessType || session.AccessType) {
            this.sessionForm.get('accessType')?.disable();
          }
          if (session.dialyserType || session.DialyserType) {
            this.sessionForm.get('dialyserType')?.disable();
          }
          
          // Keep all treatment parameters EDITABLE so staff can update them during treatment
          // This includes: dryWeight, hdStartDate, hdCycle, prescribedDuration, prescribedBFR, 
          // ufGoal, dialysatePrescription, weightGain, bloodPressure, symptoms, etc.
          
          this.loading = false;
        }
      },
      error: (error) => {
        console.error('Error loading session:', error);
        this.snackBar.open('Failed to load session data', 'Close', { duration: 3000 });
        this.loading = false;
      }
    });
  }

  autoSaveData(): void {
    if (!this.isEditMode || !this.scheduleId) {
      return;
    }

    const formValue = this.sessionForm.getRawValue(); // Get all values including disabled fields
    const updates: any = {};

    // Include ALL editable treatment fields that have values
    // Basic HD Info
    if (formValue.dryWeight != null && formValue.dryWeight !== '') updates.DryWeight = formValue.dryWeight;
    if (formValue.hdStartDate) updates.HDStartDate = formValue.hdStartDate;
    if (formValue.hdCycle != null && formValue.hdCycle !== '') updates.HDCycle = formValue.hdCycle;
    if (formValue.hdFrequency != null && formValue.hdFrequency !== '') updates.HDFrequency = formValue.hdFrequency;
    if (formValue.weightGain != null && formValue.weightGain !== '') updates.WeightGain = formValue.weightGain;
    
    // Equipment
    if (formValue.dialyserType) updates.DialyserType = formValue.dialyserType;
    if (formValue.dialyserModel) updates.DialyserModel = formValue.dialyserModel;
    if (formValue.hdUnitNumber) updates.HDUnitNumber = formValue.hdUnitNumber;
    
    // Prescription
    if (formValue.prescribedDuration != null && formValue.prescribedDuration !== '') updates.PrescribedDuration = formValue.prescribedDuration;
    if (formValue.ufGoal != null && formValue.ufGoal !== '') updates.UFGoal = formValue.ufGoal;
    // Note: dialysatePrescription is excluded from auto-save (user request)
    if (formValue.prescribedBFR != null && formValue.prescribedBFR !== '') updates.PrescribedBFR = formValue.prescribedBFR;
    
    // Anticoagulation
    if (formValue.anticoagulationType) updates.AnticoagulationType = formValue.anticoagulationType;
    if (formValue.heparinDose != null && formValue.heparinDose !== '') updates.HeparinDose = formValue.heparinDose;
    if (formValue.syringeType) updates.SyringeType = formValue.syringeType;
    if (formValue.heparinBolusDose != null && formValue.heparinBolusDose !== '') updates.BolusDose = formValue.heparinBolusDose;
    if (formValue.heparinInfusionRate != null && formValue.heparinInfusionRate !== '') updates.HeparinInfusionRate = formValue.heparinInfusionRate;
    
    // Access
    if (formValue.accessType) updates.AccessType = formValue.accessType;
    if (formValue.accessLocation) updates.AccessLocation = formValue.accessLocation;
    
    // Vitals & Symptoms
    if (formValue.bloodPressure) updates.BloodPressure = formValue.bloodPressure;
    if (formValue.preBPSitting) updates.PreBPSitting = formValue.preBPSitting;
    if (formValue.symptoms) updates.Symptoms = formValue.symptoms;
    
    // Staff Assignment
    if (formValue.assignedDoctor != null) updates.AssignedDoctor = formValue.assignedDoctor;
    if (formValue.assignedNurse != null) updates.AssignedNurse = formValue.assignedNurse;
    
    // Test Status
    if (formValue.bloodTestDone != null) updates.BloodTestDone = formValue.bloodTestDone;
    
    // HDTreatmentSession fields
    if (formValue.startTime) updates.StartTime = formValue.startTime;
    if (formValue.preWeight != null && formValue.preWeight !== '') updates.PreWeight = formValue.preWeight;
    if (formValue.preTemperature != null && formValue.preTemperature !== '') updates.PreTemperature = formValue.preTemperature;
    if (formValue.accessBleedingTime) updates.AccessBleedingTime = formValue.accessBleedingTime;
    if (formValue.accessStatus) updates.AccessStatus = formValue.accessStatus;
    if (formValue.complications) updates.Complications = formValue.complications;
    
    // IntraDialyticMonitoring fields
    if (formValue.monitoringTime) updates.MonitoringTime = formValue.monitoringTime;
    if (formValue.heartRate != null && formValue.heartRate !== '') updates.HeartRate = formValue.heartRate;
    if (formValue.actualBFR != null && formValue.actualBFR !== '') updates.ActualBFR = formValue.actualBFR;
    if (formValue.venousPressure != null && formValue.venousPressure !== '') updates.VenousPressure = formValue.venousPressure;
    if (formValue.arterialPressure != null && formValue.arterialPressure !== '') updates.ArterialPressure = formValue.arterialPressure;
    if (formValue.currentUFR != null && formValue.currentUFR !== '') updates.CurrentUFR = formValue.currentUFR;
    if (formValue.totalUFAchieved != null && formValue.totalUFAchieved !== '') updates.TotalUFAchieved = formValue.totalUFAchieved;
    if (formValue.tmpPressure != null && formValue.tmpPressure !== '') updates.TmpPressure = formValue.tmpPressure;
    if (formValue.interventions) updates.Interventions = formValue.interventions;
    if (formValue.staffInitials) updates.StaffInitials = formValue.staffInitials;
    
    // PostDialysisMedications fields
    if (formValue.medicationType) updates.MedicationType = formValue.medicationType;
    if (formValue.medicationName) updates.MedicationName = formValue.medicationName;
    if (formValue.dose) updates.Dose = formValue.dose;
    if (formValue.route) updates.Route = formValue.route;
    if (formValue.administeredAt) updates.AdministeredAt = formValue.administeredAt;
    
    // TreatmentAlerts fields
    if (formValue.alertType) updates.AlertType = formValue.alertType;
    if (formValue.alertMessage) updates.AlertMessage = formValue.alertMessage;
    if (formValue.severity) updates.Severity = formValue.severity;
    if (formValue.resolution) updates.Resolution = formValue.resolution;
    
    // Additional Notes
    if (formValue.notes) updates.Notes = formValue.notes;
    
    // Debug: Log treatment alerts fields
    if (formValue.alertType || formValue.alertMessage || formValue.severity || formValue.resolution || formValue.notes) {
      console.log('🚨 Treatment Alerts & Notes auto-save:', {
        alertType: formValue.alertType,
        alertMessage: formValue.alertMessage,
        severity: formValue.severity,
        resolution: formValue.resolution,
        notes: formValue.notes
      });
    }
    
    // Post-Dialysis Vital Signs
    if (formValue.postWeight != null && formValue.postWeight !== '') updates.PostWeight = formValue.postWeight;
    if (formValue.postSBP != null && formValue.postSBP !== '') updates.PostSBP = formValue.postSBP;
    if (formValue.postDBP != null && formValue.postDBP !== '') updates.PostDBP = formValue.postDBP;
    if (formValue.postHR != null && formValue.postHR !== '') updates.PostHR = formValue.postHR;
    if (formValue.totalFluidRemoved != null && formValue.totalFluidRemoved !== '') updates.TotalFluidRemoved = formValue.totalFluidRemoved;
    if (formValue.postAccessStatus) updates.PostAccessStatus = formValue.postAccessStatus;
    
    // Debug: Log post-dialysis fields
    if (formValue.postWeight || formValue.postSBP || formValue.postDBP) {
      console.log('📊 Post-Dialysis fields:', {
        postWeight: formValue.postWeight,
        postSBP: formValue.postSBP,
        postDBP: formValue.postDBP,
        postHR: formValue.postHR,
        PostWeight: updates.PostWeight,
        PostSBP: updates.PostSBP,
        PostDBP: updates.PostDBP
      });
    }
    
    // Additional notes field - IMPORTANT for medical records
    if (formValue.notes) updates.Notes = formValue.notes;

    if (Object.keys(updates).length === 0) {
      return;
    }

    this.scheduleService.autoSaveSchedule(this.scheduleId, updates).subscribe({
      next: (response) => {
        if (response.success) {
          console.log('✓ Auto-saved successfully at', new Date().toLocaleTimeString());
          this.snackBar.open('✓ Saved', '', { duration: 1000, horizontalPosition: 'end', verticalPosition: 'top' });
        }
      },
      error: (error) => {
        console.error('Auto-save failed:', error);
        console.error('Error details:', error.error);
        
        // Extract error message from backend
        let errorMessage = 'Failed to save changes';
        if (error.error?.message) {
          errorMessage = error.error.message;
        } else if (error.error?.errors) {
          errorMessage = Object.values(error.error.errors).join(', ');
        } else if (typeof error.error === 'string') {
          errorMessage = error.error;
        }
        
        console.error('Backend error message:', errorMessage);
        
        // Handle 401 Unauthorized specifically
        if (error.status === 401) {
          this.snackBar.open('⚠️ Session expired. Please log in again.', 'Login', { 
            duration: 5000,
            horizontalPosition: 'center',
            verticalPosition: 'top',
            panelClass: ['error-snackbar']
          }).onAction().subscribe(() => {
            this.router.navigate(['/login'], { queryParams: { returnUrl: this.router.url } });
          });
        } else if (error.status === 400) {
          // Show specific error message for 400 Bad Request
          this.snackBar.open(`❌ ${errorMessage}`, 'OK', { 
            duration: 6000,
            horizontalPosition: 'center',
            verticalPosition: 'top'
          });
        } else {
          this.snackBar.open(errorMessage, 'Retry', { duration: 3000 }).onAction().subscribe(() => {
            this.autoSaveData();
          });
        }
      }
    });
  }

  onHDCycleChange(): void {
    const selectedCycle = this.sessionForm.get('hdCycle')?.value;
    const cycleOption = this.hdCycles.find(c => c.value === selectedCycle);
    
    if (cycleOption) {
      if (cycleOption.frequency !== null) {
        // Auto-fill for predefined cycles
        this.sessionForm.patchValue({ hdFrequency: cycleOption.frequency, hdCustomDays: '' });
        console.log(`Auto-filled hdFrequency to ${cycleOption.frequency} based on cycle ${selectedCycle}`);
      } else {
        // Clear the fields for Custom cycle - let user enter manually
        this.sessionForm.patchValue({ hdFrequency: '', hdCustomDays: '' });
      }
    }
    
    // Trigger auto-save if in edit mode
    if (this.isEditMode) {
      this.autoSaveData();
    }
  }

  onSlotChange(): void {
    this.selectedSlot = this.sessionForm.get('slotID')?.value;
    this.selectedBed = null;
    this.sessionForm.patchValue({ bedNumber: '' });
    this.loadOccupiedBeds();
    
    // Auto-assign bed if in auto mode
    if (this.bedAssignmentMode === 'auto' && this.selectedSlot) {
      setTimeout(() => {
        this.autoAssignBed();
      }, 500); // Wait for beds to load
    }
  }

  loadOccupiedBeds(): void {
    if (!this.selectedSlot) {
      return;
    }

    const treatmentDate = this.sessionForm.get('treatmentDate')?.value;
    
    this.scheduleService.getSlotSchedule(this.selectedSlot, treatmentDate).subscribe({
      next: (response) => {
        if (response.success && response.data?.beds) {
          // Map the bed status from API to our bed display format
          this.beds = Array.from({ length: 10 }, (_, i) => {
            const bedNum = i + 1;
            const bedStatus = response.data?.beds.find(b => b.bedNumber === bedNum);
            
            return {
              number: bedNum,
              isOccupied: bedStatus?.status === 'occupied',
              patientName: bedStatus?.patient?.name || null
            };
          });
        }
      },
      error: (error) => {
        console.error('Error loading bed availability:', error);
        // Initialize all beds as available on error
        this.beds = Array.from({ length: 10 }, (_, i) => ({
          number: i + 1,
          isOccupied: false,
          patientName: null
        }));
      }
    });
  }

  onBedModeChange(): void {
    if (this.bedAssignmentMode === 'auto' && this.selectedSlot) {
      this.autoAssignBed();
    } else {
      this.autoAssignedBed = null;
      this.showBedChangeOption = false;
    }
  }

  autoAssignBed(): void {
    if (!this.beds || this.beds.length === 0) {
      return;
    }

    const nextBed = this.getNextAvailableBed();
    if (nextBed) {
      this.autoAssignedBed = nextBed.number;
      this.selectedBed = nextBed.number;
      this.sessionForm.patchValue({ bedNumber: nextBed.number });
      this.showBedChangeOption = true;
      this.snackBar.open(`✓ Auto-assigned to Bed ${nextBed.number}`, 'Close', { duration: 3000 });
    } else {
      this.snackBar.open('⚠️ No available beds. Please try another slot or switch to manual mode.', 'Close', { duration: 4000 });
    }
  }

  getNextAvailableBed(): { number: number } | null {
    // Smart bed assignment algorithm
    const availableBeds = this.beds.filter(bed => !bed.isOccupied);
    
    if (availableBeds.length === 0) {
      return null;
    }

    // Strategy: Infection control - prefer beds with spacing
    const bedsWithSpacing = availableBeds.filter(bed => {
      const bedNum = bed.number;
      const adjacentLeft = this.beds.find(b => b.number === bedNum - 1);
      const adjacentRight = this.beds.find(b => b.number === bedNum + 1);
      
      // Prefer beds with at least one empty neighbor
      return !adjacentLeft?.isOccupied || !adjacentRight?.isOccupied;
    });

    const targetBeds = bedsWithSpacing.length > 0 ? bedsWithSpacing : availableBeds;

    // Pick the first available bed with spacing preference
    return targetBeds[0];
  }

  changeBedManually(): void {
    this.bedAssignmentMode = 'manual';
    this.showBedChangeOption = false;
    this.autoAssignedBed = null;
  }

  selectBed(bedNumber: number, isOccupied: boolean): void {
    if (!isOccupied) {
      this.selectedBed = bedNumber;
      this.sessionForm.patchValue({ bedNumber });
    }
  }

  saveCurrentStep(): void {
    console.log('💾 Save Progress clicked | isEditMode:', this.isEditMode, '| scheduleId:', this.scheduleId);
    
    // If we're not in edit mode yet, we need to validate and create the initial session
    if (!this.isEditMode && !this.scheduleId) {
      // Check if slot is selected
      if (!this.selectedSlot) {
        this.snackBar.open('Please select a time slot before saving', 'Close', { duration: 3000 });
        return;
      }

      // Check if bed is selected
      if (!this.selectedBed) {
        this.snackBar.open('Please select a bed before saving', 'Close', { duration: 3000 });
        return;
      }

      // Validate only the essential required fields for Step 1
      const formValue = this.sessionForm.value;
      let missingFields: string[] = [];
      
      // Check treatment date
      if (!formValue.treatmentDate) {
        missingFields.push('Treatment Date');
      }
      
      // Check dry weight (must be a number greater than 0)
      if (!formValue.dryWeight || formValue.dryWeight <= 0) {
        missingFields.push('Dry Weight');
      }
      
      // Check prescribed duration
      if (!formValue.prescribedDuration || formValue.prescribedDuration <= 0) {
        missingFields.push('Prescribed Duration');
      }
      
      // Check access type
      if (!formValue.accessType || formValue.accessType === '') {
        missingFields.push('Access Type');
      }

      if (missingFields.length > 0) {
        const fieldNames = missingFields.join(', ');
        this.snackBar.open(`Please fill in required fields: ${fieldNames}`, 'Close', { duration: 4000 });
        return;
      }

      // Ensure slotID and bedNumber are set in the form
      this.sessionForm.patchValue({
        slotID: this.selectedSlot,
        bedNumber: this.selectedBed
      });

      // If all required fields are filled, create the initial session
      this.createInitialSession();
    } else if (this.isEditMode && this.scheduleId) {
      // We're in edit mode, just auto-save current changes
      console.log('💾 Calling autoSaveData in edit mode...');
      this.autoSaveData();
      this.snackBar.open('✓ Progress saved successfully!', 'Close', { duration: 2000 });
    } else {
      console.warn('⚠️ Cannot save: isEditMode =', this.isEditMode, ', scheduleId =', this.scheduleId);
      this.snackBar.open('Unable to save. Please refresh the page.', 'Close', { duration: 3000 });
    }
  }

  private createInitialSession(): void {
    this.loading = true;
    const formValue = this.sessionForm.value;
    
    // Helper function to convert empty strings to null
    const toNumber = (value: any): number | null => {
      if (value === '' || value === null || value === undefined) return null;
      const num = Number(value);
      return isNaN(num) ? null : num;
    };

    const toString = (value: any): string | null => {
      if (value === '' || value === null || value === undefined) return null;
      return String(value);
    };
    
    const sessionData = {
      patientID: this.patientId,
      sessionDate: formValue.treatmentDate,
      dryWeight: toNumber(formValue.dryWeight),
      hdStartDate: formValue.hdStartDate,
      hdCycle: toString(formValue.hdCycle),
      hdFrequency: toNumber(formValue.hdFrequency),
      dialyserType: formValue.dialyserType,
      prescribedDuration: toNumber(formValue.prescribedDuration),
      ufGoal: toNumber(formValue.ufGoal),
      dialysatePrescription: toString(formValue.dialysatePrescription),
      prescribedBFR: toNumber(formValue.prescribedBFR),
      anticoagulationType: toString(formValue.anticoagulationType),
      heparinDose: toNumber(formValue.heparinBolusDose),
      bolusDose: toNumber(formValue.heparinBolusDose),
      heparinInfusionRate: toNumber(formValue.heparinInfusionRate),
      accessType: formValue.accessType,
      accessLocation: toString(formValue.accessLocation),
      slotID: formValue.slotID,
      bedNumber: formValue.bedNumber,
      dialyserModel: toString(formValue.dialyserModel),
      sessionsPerWeek: toNumber(formValue.sessionsPerWeek)
    };

    console.log('🚀 Creating HD session with data:', JSON.stringify(sessionData, null, 2));

    this.scheduleService.createHDSession(sessionData).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          // The API returns the scheduleID as a number directly in response.data
          this.scheduleId = response.data;
          this.isEditMode = true;
          
          console.log('✓ Session created with ID:', this.scheduleId, '| Edit mode:', this.isEditMode);
          
          this.snackBar.open('✓ HD Session saved! You can now proceed to Monitoring step.', 'Close', { 
            duration: 4000 
          });
          
          // Clear the auto-filled fields indicator since session is now saved
          this.autoFilledFields = [];
          
          this.loading = false;
        } else {
          this.snackBar.open('Failed to create initial session', 'Close', { duration: 3000 });
          this.loading = false;
        }
      },
      error: (error) => {
        console.error('❌ Error creating initial session:', error);
        console.error('Error details:', error.error);
        const errorMessage = error.error?.message || error.message || 'Error creating initial session';
        this.snackBar.open(errorMessage, 'Close', { duration: 5000 });
        this.loading = false;
      }
    });
  }

  onSubmit(): void {
    // Removed form validation check - allow submission with any data entered
    // if (this.sessionForm.invalid) {
    //   this.sessionForm.markAllAsTouched();
    //   this.snackBar.open('Please fill in all required fields', 'Close', { duration: 3000 });
    //   return;
    // }

    // Only check bed selection in create mode, not in edit mode
    if (!this.isEditMode && !this.selectedBed) {
      this.snackBar.open('Please select a bed', 'Close', { duration: 3000 });
      return;
    }

    // Check if patient already has a session on this date
    const treatmentDateValue = this.sessionForm.value.treatmentDate || this.sessionForm.get('treatmentDate')?.value;
    if (treatmentDateValue) {
      const selectedDate = new Date(treatmentDateValue);
      if (!isNaN(selectedDate.getTime())) {
        this.checkExistingSession(selectedDate);
      }
    }
  }

  private checkExistingSession(selectedDate: Date): void {
    // Validate the date before converting to ISO string
    if (!selectedDate || isNaN(selectedDate.getTime())) {
      console.error('Invalid date provided to checkExistingSession');
      return;
    }
    
    const dateString = selectedDate.toISOString().split('T')[0];
    
    this.patientService.getPatientHistory(this.patientId!).subscribe({
      next: (response: any) => {
        if (response.success && response.data?.sessions) {
          const existingSession = response.data.sessions.find((session: any) => {
            const sessionDate = new Date(session.sessionDate).toISOString().split('T')[0];
            // Skip the current session when in edit mode
            const isDifferentSession = this.isEditMode ? session.scheduleID !== this.scheduleId : true;
            return sessionDate === dateString && isDifferentSession;
          });

          if (existingSession) {
            this.snackBar.open(
              `⚠️ Patient already has an HD session scheduled on ${selectedDate.toLocaleDateString()}. Each patient can only have ONE session per day.`,
              'Close',
              { duration: 5000 }
            );
            return;
          }
        }
        
        // No existing session found, proceed with save
        this.proceedWithSave();
      },
      error: (error) => {
        console.error('Error checking existing sessions:', error);
        // If check fails, ask user to confirm
        if (confirm('Unable to verify if patient already has a session today. Do you want to proceed anyway?')) {
          this.proceedWithSave();
        }
      }
    });
  }

  private proceedWithSave(): void {
    this.loading = true;
    this.errorMessage = '';

    // In edit mode, use UPDATE instead of CREATE
    if (this.isEditMode && this.scheduleId) {
      this.updateExistingSession();
      return;
    }

    const formValue = this.sessionForm.value;
    
    // Helper function to convert empty strings to null for numeric fields
    const toNumber = (value: any): number | null => {
      if (value === '' || value === null || value === undefined) return null;
      const num = Number(value);
      return isNaN(num) ? null : num;
    };

    const toString = (value: any): string | null => {
      if (value === '' || value === null || value === undefined) return null;
      return String(value);
    };
    
    const sessionData = {
      patientID: this.patientId,
      sessionDate: formValue.treatmentDate,
      dryWeight: toNumber(formValue.dryWeight),
      hdStartDate: formValue.hdStartDate,
      hdCycle: toString(formValue.hdCycle),
      hdFrequency: toNumber(formValue.hdFrequency),
      dialyserType: formValue.dialyserType,
      prescribedDuration: toNumber(formValue.prescribedDuration),
      ufGoal: toNumber(formValue.ufGoal),
      dialysatePrescription: toString(formValue.dialysatePrescription),
      prescribedBFR: toNumber(formValue.prescribedBFR),
      anticoagulationType: toString(formValue.anticoagulationType),
      heparinDose: toNumber(formValue.heparinBolusDose),
      bolusDose: toNumber(formValue.heparinBolusDose),
      heparinInfusionRate: toNumber(formValue.heparinInfusionRate),
      accessType: formValue.accessType,
      accessLocation: toString(formValue.accessLocation),
      slotID: formValue.slotID,
      bedNumber: formValue.bedNumber,
      assignedDoctor: null,  // Will be assigned by staff later
      assignedNurse: null,   // Will be assigned by staff later
      dialyserModel: toString(formValue.dialyserModel),
      bloodTestDone: false,
      sessionsPerWeek: toNumber(formValue.sessionsPerWeek),
      // HDTreatmentSession fields
      startTime: toString(formValue.startTime),
      preWeight: toNumber(formValue.preWeight),
      preBPSitting: toString(formValue.preBPSitting),
      preTemperature: toNumber(formValue.preTemperature),
      accessStatus: toString(formValue.accessStatus),
      complications: toString(formValue.complications),
      // IntraDialyticMonitoring fields
      monitoringTime: toString(formValue.monitoringTime),
      bloodPressure: toString(formValue.bloodPressure),
      heartRate: toNumber(formValue.heartRate),
      actualBFR: toNumber(formValue.actualBFR),
      venousPressure: toNumber(formValue.venousPressure),
      arterialPressure: toNumber(formValue.arterialPressure),
      currentUFR: toNumber(formValue.currentUFR),
      totalUFAchieved: toNumber(formValue.totalUFAchieved),
      tmpPressure: toNumber(formValue.tmpPressure),
      symptoms: toString(formValue.symptoms),
      interventions: toString(formValue.interventions),
      staffInitials: toString(formValue.staffInitials),
      // Post-Dialysis Vital Signs
      postWeight: toNumber(formValue.postWeight),
      postSBP: toNumber(formValue.postSBP),
      postDBP: toNumber(formValue.postDBP),
      postHR: toNumber(formValue.postHR),
      accessBleedingTime: toString(formValue.accessBleedingTime),
      totalFluidRemoved: toNumber(formValue.totalFluidRemoved),
      postAccessStatus: toString(formValue.postAccessStatus),
      // PostDialysisMedications fields
      medicationType: toString(formValue.medicationType),
      medicationName: toString(formValue.medicationName),
      dose: toString(formValue.dose),
      route: toString(formValue.route),
      administeredAt: toString(formValue.administeredAt),
      // TreatmentAlerts fields
      alertType: toString(formValue.alertType),
      alertMessage: toString(formValue.alertMessage),
      severity: toString(formValue.severity),
      resolution: toString(formValue.resolution),
      // Additional notes
      notes: toString(formValue.notes)
    };

    console.log('🔍 Full Form Values:', formValue);
    console.log('📤 Submitting HD session data:', sessionData);
    console.log('📋 Pre-Dialysis Fields:', {
      preWeight: formValue.preWeight,
      preBPSitting: formValue.preBPSitting,
      preTemperature: formValue.preTemperature
    });
    console.log('📊 Intra-Dialytic Fields:', {
      monitoringTime: formValue.monitoringTime,
      bloodPressure: formValue.bloodPressure,
      heartRate: formValue.heartRate
    });
    console.log('✅ Post-Dialysis Fields:', {
      postWeight: formValue.postWeight,
      postSBP: formValue.postSBP,
      postDBP: formValue.postDBP
    });
    
    this.scheduleService.createHDSession(sessionData).subscribe({
      next: (response) => {
        console.log('HD session response:', response);
        if (response.success) {
          // Clear the draft after successful save
          this.clearDraft();
          
          this.snackBar.open('HD session scheduled successfully!', 'Close', { duration: 3000 });
          this.router.navigate(['/schedule']);
        } else {
          this.errorMessage = response.message || 'Failed to create HD session';
          this.snackBar.open(this.errorMessage, 'Close', { duration: 5000 });
          this.loading = false;
        }
      },
      error: (error) => {
        console.error('Error creating HD session:', error);
        console.error('Error details:', JSON.stringify(error, null, 2));
        
        let errorMsg = 'An error occurred while creating the HD session';
        if (error.error?.message) {
          errorMsg = error.error.message;
        } else if (error.error?.errors) {
          errorMsg = Object.values(error.error.errors).flat().join(', ');
        } else if (error.message) {
          errorMsg = error.message;
        }
        
        this.errorMessage = errorMsg;
        this.snackBar.open(errorMsg, 'Close', { duration: 5000 });
        this.loading = false;
      }
    });
  }

  private updateExistingSession(): void {
    const formValue = this.sessionForm.getRawValue(); // Get all values including disabled
    
    // Helper function to convert empty strings to null for numeric fields
    const toNumber = (value: any): number | null => {
      if (value === '' || value === null || value === undefined) return null;
      const num = Number(value);
      return isNaN(num) ? null : num;
    };

    const toString = (value: any): string | null => {
      if (value === '' || value === null || value === undefined) return null;
      return String(value);
    };
    
    const updateData = {
      scheduleID: this.scheduleId,
      patientID: this.patientId,
      sessionDate: formValue.treatmentDate ? new Date(formValue.treatmentDate).toISOString() : new Date().toISOString(),
      dryWeight: toNumber(formValue.dryWeight),
      hdStartDate: formValue.hdStartDate ? new Date(formValue.hdStartDate).toISOString() : null,
      hdCycle: toString(formValue.hdCycle),
      hdFrequency: toNumber(formValue.hdFrequency),
      weightGain: toNumber(formValue.weightGain),
      dialyserType: formValue.dialyserType,
      dialyserModel: toString(formValue.dialyserModel),
      prescribedDuration: toNumber(formValue.prescribedDuration),
      ufGoal: toNumber(formValue.ufGoal),
      dialysatePrescription: toString(formValue.dialysatePrescription),
      prescribedBFR: toNumber(formValue.prescribedBFR),
      anticoagulationType: toString(formValue.anticoagulationType),
      heparinDose: toNumber(formValue.heparinBolusDose),
      bolusDose: toNumber(formValue.heparinBolusDose),
      heparinInfusionRate: toNumber(formValue.heparinInfusionRate),
      accessType: formValue.accessType,
      accessLocation: toString(formValue.accessLocation),
      slotID: toNumber(formValue.slotID),
      bedNumber: toNumber(formValue.bedNumber),
      bloodPressure: toString(formValue.bloodPressure),
      symptoms: toString(formValue.symptoms),
      bloodTestDone: false,
      // HDTreatmentSession fields
      startTime: toString(formValue.startTime),
      preWeight: toNumber(formValue.preWeight),
      preBPSitting: toString(formValue.preBPSitting),
      preTemperature: toNumber(formValue.preTemperature),
      accessBleedingTime: toString(formValue.accessBleedingTime),
      accessStatus: toString(formValue.accessStatus),
      complications: toString(formValue.complications),
      // IntraDialyticMonitoring fields
      monitoringTime: toString(formValue.monitoringTime),
      heartRate: toNumber(formValue.heartRate),
      actualBFR: toNumber(formValue.actualBFR),
      venousPressure: toNumber(formValue.venousPressure),
      arterialPressure: toNumber(formValue.arterialPressure),
      currentUFR: toNumber(formValue.currentUFR),
      totalUFAchieved: toNumber(formValue.totalUFAchieved),
      tmpPressure: toNumber(formValue.tmpPressure),
      interventions: toString(formValue.interventions),
      staffInitials: toString(formValue.staffInitials),
      // Post-Dialysis Vital Signs
      postWeight: toNumber(formValue.postWeight),
      postSBP: toNumber(formValue.postSBP),
      postDBP: toNumber(formValue.postDBP),
      postHR: toNumber(formValue.postHR),
      totalFluidRemoved: toNumber(formValue.totalFluidRemoved),
      postAccessStatus: toString(formValue.postAccessStatus),
      // PostDialysisMedications fields
      medicationType: toString(formValue.medicationType),
      medicationName: toString(formValue.medicationName),
      dose: toString(formValue.dose),
      route: toString(formValue.route),
      administeredAt: toString(formValue.administeredAt),
      // TreatmentAlerts fields
      alertType: toString(formValue.alertType),
      alertMessage: toString(formValue.alertMessage),
      severity: toString(formValue.severity),
      resolution: toString(formValue.resolution),
      // Additional notes
      notes: toString(formValue.notes),
      isDischarged: false
    };

    console.log('Updating HD session with scheduleId:', this.scheduleId);
    console.log('Update data:', updateData);
    
    // Backend expects the data directly (not wrapped in a 'request' object)
    this.scheduleService.updateSchedule(this.scheduleId!, updateData).subscribe({
      next: (response) => {
        console.log('Update response:', response);
        if (response.success) {
          this.snackBar.open('HD session updated successfully!', 'Close', { duration: 3000 });
          // Stay on the same page instead of navigating away
          this.loading = false;
          // Reload the session data to show updated values
          this.loadExistingSession();
        } else {
          this.errorMessage = response.message || 'Failed to update HD session';
          this.snackBar.open(this.errorMessage, 'Close', { duration: 5000 });
          this.loading = false;
        }
      },
      error: (error) => {
        console.error('❌ Error updating HD session:', error);
        console.error('❌ Error response:', error.error);
        console.error('❌ Error status:', error.status);
        if (error.error?.errors) {
          console.error('❌ Validation errors details:', JSON.stringify(error.error.errors, null, 2));
        }
        let errorMsg = 'An error occurred while updating the HD session';
        if (error.error?.message) {
          errorMsg = error.error.message;
        } else if (error.error?.errors) {
          const errors = Object.values(error.error.errors).flat().join(', ');
          errorMsg = `Validation errors: ${errors}`;
        } else if (error.message) {
          errorMsg = error.message;
        }
        this.errorMessage = errorMsg;
        this.snackBar.open(errorMsg, 'Close', { duration: 5000 });
        this.loading = false;
      }
    });
  }

  goBack(): void {
    this.location.back();
  }

  goHome(): void {
    this.router.navigate(['/']);
  }

  goToLogin(): void {
    this.router.navigate(['/login'], { queryParams: { returnUrl: this.router.url } });
  }

  getSlotName(slotId: number): string {
    const slot = this.slots.find(s => s.id === slotId);
    return slot ? `${slot.name} (${slot.time})` : 'Not selected';
  }

  // Auto-save draft methods for new sessions
  private getDraftKey(): string {
    return `hd_session_draft_${this.patientId}`;
  }

  private saveDraftToLocalStorage(): void {
    if (!this.patientId || this.isEditMode) return;

    this.saveStatus = 'saving';
    const formValue = this.sessionForm.getRawValue();
    const draft = {
      formData: formValue,
      timestamp: new Date().toISOString(),
      patientId: this.patientId,
      bedAssignmentMode: this.bedAssignmentMode,
      autoAssignedBed: this.autoAssignedBed,
      selectedBed: this.selectedBed,
      selectedSlot: this.selectedSlot
    };

    try {
      localStorage.setItem(this.getDraftKey(), JSON.stringify(draft));
      this.saveStatus = 'saved';
      this.lastSavedTime = new Date();
      
      // Reset to idle after 3 seconds
      setTimeout(() => {
        if (this.saveStatus === 'saved') {
          this.saveStatus = 'idle';
        }
      }, 3000);
    } catch (error) {
      console.error('Error saving draft to localStorage:', error);
      this.saveStatus = 'idle';
      this.snackBar.open('Failed to auto-save draft', 'Close', { duration: 2000 });
    }
  }

  private checkForSavedDraft(): void {
    if (!this.patientId || this.isEditMode) return;

    const draftKey = this.getDraftKey();
    const draftJson = localStorage.getItem(draftKey);

    if (!draftJson) return;

    try {
      const draft = JSON.parse(draftJson);
      
      // Check if draft is less than 24 hours old
      const draftDate = new Date(draft.timestamp);
      const hoursDiff = (new Date().getTime() - draftDate.getTime()) / (1000 * 60 * 60);
      
      if (hoursDiff > 24) {
        // Draft is too old, clear it
        localStorage.removeItem(draftKey);
        return;
      }

      // Show restoration prompt
      const draftTime = draftDate.toLocaleString();
      const message = `Found unsaved draft from ${draftTime}. Restore it?`;
      
      this.snackBar.open(message, 'Restore', { 
        duration: 10000,
        horizontalPosition: 'center',
        verticalPosition: 'top'
      }).onAction().subscribe(() => {
        this.restoreDraftFromLocalStorage(draft);
      });

      // Add a dismiss action to clear the draft
      setTimeout(() => {
        const clearMessage = 'Draft not restored. Clear it?';
        this.snackBar.open(clearMessage, 'Clear', { 
          duration: 5000 
        }).onAction().subscribe(() => {
          this.clearDraft();
          this.snackBar.open('Draft cleared', 'OK', { duration: 2000 });
        });
      }, 10500);

    } catch (error) {
      console.error('Error parsing saved draft:', error);
      localStorage.removeItem(draftKey);
    }
  }

  private restoreDraftFromLocalStorage(draft: any): void {
    if (!draft || !draft.formData) return;

    // Convert date strings back to Date objects
    if (draft.formData.treatmentDate) {
      draft.formData.treatmentDate = new Date(draft.formData.treatmentDate);
    }
    if (draft.formData.sessionStartTime) {
      draft.formData.sessionStartTime = new Date(draft.formData.sessionStartTime);
    }

    // Restore form data
    this.sessionForm.patchValue(draft.formData);

    // Restore bed assignment state
    if (draft.bedAssignmentMode) {
      this.bedAssignmentMode = draft.bedAssignmentMode;
    }
    if (draft.autoAssignedBed) {
      this.autoAssignedBed = draft.autoAssignedBed;
      this.showBedChangeOption = true;
    }
    if (draft.selectedBed) {
      this.selectedBed = draft.selectedBed;
    }
    if (draft.selectedSlot) {
      this.selectedSlot = draft.selectedSlot;
      // Reload bed availability for the selected slot
      this.loadOccupiedBeds();
    }

    this.snackBar.open('Draft restored successfully', 'OK', { duration: 3000 });
  }

  private clearDraft(): void {
    if (!this.patientId) return;
    localStorage.removeItem(this.getDraftKey());
  }

  startWorkflow(): void {
    if (!this.patientId || !this.scheduleId) {
      this.snackBar.open('Session information is not available', 'Close', { duration: 3000 });
      return;
    }
    
    // Navigate to the 3-phase workflow
    this.router.navigate(['/patients', this.patientId, 'workflow', this.scheduleId]);
  }

  saveDraftAndExit(): void {
    if (this.sessionForm.invalid) {
      this.snackBar.open('Please fill in all required fields before saving', 'Close', { duration: 3000 });
      return;
    }

    // Trigger auto-save
    this.saveDraftToLocalStorage();
    
    this.snackBar.open('✓ Draft saved! You can continue later.', 'Close', { duration: 3000 });
    
    // Navigate back after brief delay
    setTimeout(() => {
      this.router.navigate(['/schedule']);
    }, 1500);
  }

  // ===== INTRA-DIALYTIC MONITORING METHODS =====
  
  loadMonitoringRecords(): void {
    if (!this.scheduleId) return;

    this.scheduleService.getIntraDialyticRecords(this.scheduleId).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.monitoringRecords = response.data;
          console.log('Loaded monitoring records:', this.monitoringRecords);
        }
      },
      error: (error) => {
        console.error('Error loading monitoring records:', error);
      }
    });
  }

  addMonitoringRecord(): void {
    if (!this.scheduleId) {
      this.snackBar.open('⚠ Please save the HD session first (click "Save HD Session" button) before adding monitoring records', 'Close', { duration: 6000 });
      // Scroll to top to show the save button
      window.scrollTo({ top: 0, behavior: 'smooth' });
      return;
    }

    const formValues = this.sessionForm.value;
    
    // Format date properly for backend
    const sessionDate = formValues.treatmentDate instanceof Date 
      ? formValues.treatmentDate.toISOString().split('T')[0]
      : formValues.treatmentDate;
    
    // Build comprehensive monitoring record object with ALL fields
    const monitoringRecord = {
      patientID: this.patientId,
      scheduleID: this.scheduleId,
      sessionDate: sessionDate,
      timeRecorded: new Date().toISOString(),
      bloodPressure: formValues.bloodPressure || '',
      pulseRate: formValues.heartRate || null,
      temperature: formValues.temperature || null,
      ufVolume: formValues.totalUFAchieved || null,
      venousPressure: formValues.venousPressure || null,
      arterialPressure: formValues.arterialPressure || null,
      bloodFlowRate: formValues.actualBFR || null,
      dialysateFlowRate: formValues.dialysateFlowRate || null,
      currentUFR: formValues.currentUFR || null,
      tmpPressure: formValues.tmpPressure || null,
      symptoms: formValues.symptoms || '',
      interventions: formValues.interventions || '',
      staffInitials: formValues.staffInitials || '',
      recordedBy: formValues.staffInitials || null,
      notes: formValues.monitoringNotes || ''
    };

    console.log('Sending monitoring record:', monitoringRecord);
    
    this.loading = true;
    this.scheduleService.addIntraDialyticRecord(monitoringRecord).subscribe({
      next: (response) => {
        if (response.success) {
          const entryNumber = this.monitoringRecords.length + 1;
          this.snackBar.open(`✓ Monitoring Entry #${entryNumber} saved successfully!`, 'Close', { duration: 3000 });
          
          // Reload monitoring records from backend to get the complete list
          this.loadMonitoringRecords();

          // Clear monitoring fields for next entry
          this.sessionForm.patchValue({
            bloodPressure: '',
            heartRate: null,
            temperature: null,
            actualBFR: null,
            venousPressure: null,
            arterialPressure: null,
            currentUFR: null,
            totalUFAchieved: null,
            tmpPressure: null,
            symptoms: '',
            interventions: '',
            staffInitials: '',
            monitoringNotes: ''
          });

          console.log(`✅ Cleared fields for new entry. Next entry will be #${entryNumber + 1}`);
          this.loading = false;
        } else {
          this.snackBar.open('Failed to save monitoring record', 'Close', { duration: 3000 });
          this.loading = false;
        }
      },
      error: (error) => {
        console.error('Error saving monitoring record:', error);
        console.error('Error details:', error.error);
        
        // Handle authentication error specifically
        if (error.status === 401) {
          this.snackBar.open('⚠️ Session expired. Please log in again.', 'Login', { 
            duration: 5000,
            horizontalPosition: 'center',
            verticalPosition: 'top'
          }).onAction().subscribe(() => {
            this.router.navigate(['/login'], { queryParams: { returnUrl: this.router.url } });
          });
        } else {
          const errorMsg = error.error?.message || error.message || 'Unknown error';
          this.snackBar.open(`Error: ${errorMsg}`, 'Close', { duration: 5000 });
        }
        this.loading = false;
      }
    });
  }

  updateCurrentTime(): void {
    this.currentTime = new Date();
  }
  
  // AI Autocomplete Methods
  loadAutocomplete(): void {
    if (!this.patientId) {
      this.snackBar.open('⚠️ Patient information not loaded', 'Close', { duration: 3000 });
      return;
    }

    const treatmentDate = this.sessionForm.get('treatmentDate')?.value;
    const slotId = this.sessionForm.get('slotID')?.value;
    
    if (!treatmentDate) {
      this.snackBar.open('⚠️ Please select a treatment date first', 'Close', { duration: 3000 });
      return;
    }

    // Ensure treatmentDate is a valid Date object
    let sessionDate: Date;
    if (treatmentDate instanceof Date) {
      sessionDate = treatmentDate;
    } else if (typeof treatmentDate === 'string') {
      sessionDate = new Date(treatmentDate);
    } else {
      console.error('Invalid treatment date format:', treatmentDate);
      this.snackBar.open('⚠️ Invalid treatment date format', 'Close', { duration: 3000 });
      return;
    }

    // Validate patientId
    if (!this.patientId || this.patientId <= 0) {
      console.error('Invalid patientId:', this.patientId);
      this.snackBar.open('⚠️ Invalid patient ID', 'Close', { duration: 3000 });
      return;
    }

    this.loadingAutocomplete = true;
    this.aiService.getSessionAutocomplete(this.patientId, sessionDate, slotId).subscribe({
      next: (response: any) => {
        this.autocompleteData = response;
        this.showAutocompleteCard = true;
        this.loadingAutocomplete = false;
        
        // Calculate average confidence
        const avgConfidence = response.predictions.reduce((sum: number, p: any) => sum + p.confidence, 0) / response.predictions.length;
        this.snackBar.open(`✨ AI predictions ready! Average confidence: ${(avgConfidence * 100).toFixed(0)}%`, 'Close', { duration: 4000 });
      },
      error: (error) => {
        console.error('Error loading autocomplete:', error);
        if (error.error?.details) {
          console.error('Validation errors:', error.error.details);
        }
        this.loadingAutocomplete = false;
        const errorMsg = error.error?.error || error.message || 'Could not load AI predictions';
        this.snackBar.open(`⚠️ ${errorMsg}`, 'Close', { duration: 5000 });
      }
    });
  }

  applyAllAutocomplete(): void {
    if (!this.autocompleteData?.predictions) return;

    let appliedCount = 0;
    this.autocompleteData.predictions.forEach((prediction: any) => {
      if (prediction.confidence > 0.7 && !this.sessionForm.get(prediction.fieldName)?.value) {
        this.sessionForm.patchValue({ [prediction.fieldName]: prediction.predictedValue });
        this.sessionForm.get(prediction.fieldName)?.markAsTouched();
        appliedCount++;
      }
    });

    this.snackBar.open(`✅ Applied ${appliedCount} high-confidence predictions`, 'Close', { duration: 3000 });
  }

  applyPrediction(prediction: any): void {
    this.sessionForm.patchValue({ [prediction.fieldName]: prediction.predictedValue });
    this.sessionForm.get(prediction.fieldName)?.markAsTouched();
    this.snackBar.open(`✅ Applied: ${prediction.fieldName}`, 'Close', { duration: 2000 });
  }

  dismissAutocomplete(): void {
    this.showAutocompleteCard = false;
    this.autocompleteData = null;
  }

  getConfidenceLevel(confidence: number): string {
    if (confidence >= 0.8) return 'high';
    if (confidence >= 0.6) return 'medium';
    return 'low';
  }

  ngOnDestroy(): void {
    if (this.autoSaveTimer) {
      clearTimeout(this.autoSaveTimer);
    }
    if (this.monitoringTimeInterval) {
      clearInterval(this.monitoringTimeInterval);
    }
  }
}
