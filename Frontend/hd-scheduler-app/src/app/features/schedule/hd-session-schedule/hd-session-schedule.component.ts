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
  bedAssignmentMode: 'auto' | 'manual' = 'auto'; // Default to auto-assign
  autoAssignedBed: number | null = null;
  showBedChangeOption: boolean = false;

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
  
  hdCycles = [
    { value: 'MWF', label: 'Monday, Wednesday, Friday (3x/week)', frequency: 3 },
    { value: 'TTS', label: 'Tuesday, Thursday, Saturday (3x/week)', frequency: 3 },
    { value: 'MTW', label: 'Monday, Tuesday, Wednesday (3x/week)', frequency: 3 },
    { value: 'MTWTHF', label: 'Monday to Friday (5x/week)', frequency: 5 },
    { value: 'MTWTHFS', label: 'Monday to Saturday (6x/week)', frequency: 6 },
    { value: 'Daily', label: 'Daily (7x/week)', frequency: 7 },
    { value: 'Custom', label: 'Custom Schedule', frequency: null }
  ];

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private location: Location,
    private patientService: PatientService,
    private scheduleService: ScheduleService,
    private snackBar: MatSnackBar
  ) {
    this.sessionForm = this.fb.group({
      treatmentDate: [new Date(), Validators.required],
      slotID: ['', Validators.required],
      bedNumber: ['', Validators.required],
      dryWeight: ['', [Validators.required, Validators.min(1)]],
      hdStartDate: [new Date(), Validators.required],
      hdCycle: [''],
      hdFrequency: [''],
      hdCustomDays: [''],
      accessType: ['', Validators.required],
      accessLocation: [''],
      prescribedDuration: [4.0, [Validators.required, Validators.min(1), Validators.max(8)]],
      dialyserType: ['', Validators.required],
      dialyserModel: [''],
      prescribedBFR: [400, [Validators.min(100), Validators.max(600)]],
      dialysatePrescription: [''],
      anticoagulationType: [''],
      heparinBolusDose: [''],
      heparinInfusionRate: [''],
      // HDTreatmentSession fields
      startTime: [''],
      preWeight: ['', [Validators.required, Validators.min(1)]],
      ufGoal: [''],
      preBPSitting: [''],
      preTemperature: [''],
      dialyserReuseNumber: [0],
      bloodTubingReuse: [0],
      accessBleedingTime: [''],
      accessStatus: [''],
      complications: [''],
      // IntraDialyticMonitoring fields
      monitoringTime: [''],
      bloodPressure: [''],
      heartRate: [''],
      actualBFR: [''],
      venousPressure: [''],
      arterialPressure: [''],
      currentUFR: [''],
      totalUFAchieved: [''],
      tmpPressure: [''],
      symptoms: [''],
      interventions: [''],
      staffInitials: [''],
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
      resolution: ['']
    });
  }

  ngOnInit(): void {
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

  viewSessionDetails(scheduleId: number): void {
    // Navigate to view/edit the selected session
    this.router.navigate(['/schedule/hd-session/edit', scheduleId]);
  }

  loadSuggestedEquipmentCounts(): void {
    if (!this.patientId) return;

    this.scheduleService.getSuggestedEquipmentCounts(this.patientId).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          console.log('Auto-loaded equipment counts:', response.data);
          
          // Automatically populate the form fields
          this.sessionForm.patchValue({
            dialyserReuseNumber: response.data.dialyserReuseCount,
            bloodTubingReuse: response.data.bloodTubingReuse
          });
          
          // Show info message to user
          this.snackBar.open(
            response.data.message || 'Equipment counts loaded automatically',
            'OK',
            { duration: 5000 }
          );
        }
      },
      error: (error) => {
        console.warn('Could not load equipment counts:', error);
        // Silently fail - not critical, defaults to 0
      }
    });
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
          this.patient = { name: session.patientName, mrn: session.patientMRN, age: session.patientAge };
          
          // Pre-fill ALL form fields with existing data (or leave empty if null)
          // Log the data to see what we're receiving
          console.log('Loading session data:', session);
          
          // Backend returns PascalCase from database, so use PascalCase property names
          this.sessionForm.patchValue({
            // Core fields
            treatmentDate: session.sessionDate ? new Date(session.sessionDate) : new Date(),
            slotID: session.slotID || session.SlotID || '',
            bedNumber: session.bedNumber || session.BedNumber || '',
            
            // Basic HD Info
            dryWeight: session.dryWeight || session.DryWeight || '',
            hdStartDate: session.hdStartDate || session.HDStartDate ? new Date(session.hdStartDate || session.HDStartDate) : null,
            hdCycle: session.hdCycle || session.HDCycle || '',
            weightGain: session.weightGain || session.WeightGain || '',
            
            // Equipment
            dialyserType: session.dialyserType || session.DialyserType || '',
            dialyserModel: session.dialyserModel || session.DialyserModel || '',
            dialyserReuseNumber: session.dialyserReuseCount || session.DialyserReuseCount || 0,
            bloodTubingReuse: session.bloodTubingReuse || session.BloodTubingReuse || 0,
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
          
          // DISABLE only the core registration fields that cannot be changed after initial bed assignment
          // These are the mandatory setup fields that define the schedule slot/location
          this.sessionForm.get('treatmentDate')?.disable();
          this.sessionForm.get('slotID')?.disable();
          this.sessionForm.get('bedNumber')?.disable();
          this.sessionForm.get('accessType')?.disable();
          this.sessionForm.get('dialyserType')?.disable();
          
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
    if (formValue.weightGain != null && formValue.weightGain !== '') updates.WeightGain = formValue.weightGain;
    
    // Equipment
    if (formValue.dialyserModel) updates.DialyserModel = formValue.dialyserModel;
    if (formValue.dialyserReuseNumber != null && formValue.dialyserReuseNumber !== '') updates.DialyserReuseCount = formValue.dialyserReuseNumber;
    if (formValue.bloodTubingReuse != null && formValue.bloodTubingReuse !== '') updates.BloodTubingReuse = formValue.bloodTubingReuse;
    if (formValue.hdUnitNumber) updates.HDUnitNumber = formValue.hdUnitNumber;
    
    // Prescription
    if (formValue.prescribedDuration != null && formValue.prescribedDuration !== '') updates.PrescribedDuration = formValue.prescribedDuration;
    if (formValue.ufGoal != null && formValue.ufGoal !== '') updates.UFGoal = formValue.ufGoal;
    if (formValue.dialysatePrescription) updates.DialysatePrescription = formValue.dialysatePrescription;
    if (formValue.prescribedBFR != null && formValue.prescribedBFR !== '') updates.PrescribedBFR = formValue.prescribedBFR;
    
    // Anticoagulation
    if (formValue.anticoagulationType) updates.AnticoagulationType = formValue.anticoagulationType;
    if (formValue.heparinDose != null && formValue.heparinDose !== '') updates.HeparinDose = formValue.heparinDose;
    if (formValue.syringeType) updates.SyringeType = formValue.syringeType;
    if (formValue.heparinBolusDose != null && formValue.heparinBolusDose !== '') updates.BolusDose = formValue.heparinBolusDose;
    if (formValue.heparinInfusionRate != null && formValue.heparinInfusionRate !== '') updates.HeparinInfusionRate = formValue.heparinInfusionRate;
    
    // Access
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
        this.snackBar.open('Failed to save changes', 'Retry', { duration: 3000 }).onAction().subscribe(() => {
          this.autoSaveData();
        });
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
      } else {
        // Clear the fields for Custom cycle - let user enter manually
        this.sessionForm.patchValue({ hdFrequency: '', hdCustomDays: '' });
      }
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
    // Save current form data without validation (allows partial saves)
    if (this.isEditMode && this.scheduleId) {
      this.autoSaveData();
      this.snackBar.open('✓ Progress saved successfully!', 'Close', { duration: 2000 });
    } else {
      this.snackBar.open('Please complete Step 1 first to enable auto-save', 'Close', { duration: 3000 });
    }
  }

  onSubmit(): void {
    if (this.sessionForm.invalid) {
      this.sessionForm.markAllAsTouched();
      this.snackBar.open('Please fill in all required fields', 'Close', { duration: 3000 });
      return;
    }

    if (!this.selectedBed) {
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
      dialyserReuseCount: toNumber(formValue.dialyserReuseNumber) || 0,
      bloodTubingReuse: 0,
      bloodTestDone: false,
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
      resolution: toString(formValue.resolution)
    };

    console.log('Submitting HD session data:', sessionData);
    
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
      sessionDate: formValue.treatmentDate,
      dryWeight: toNumber(formValue.dryWeight),
      hdStartDate: formValue.hdStartDate,
      hdCycle: toString(formValue.hdCycle),
      weightGain: toNumber(formValue.weightGain),
      dialyserType: formValue.dialyserType,
      dialyserModel: toString(formValue.dialyserModel),
      dialyserReuseCount: toNumber(formValue.dialyserReuseNumber) || 0,
      bloodTubingReuse: 0,
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
      isDischarged: false
    };

    console.log('Updating HD session:', updateData);
    
    this.scheduleService.updateSchedule(this.scheduleId!, updateData).subscribe({
      next: (response) => {
        if (response.success) {
          this.snackBar.open('HD session updated successfully!', 'Close', { duration: 3000 });
          this.router.navigate(['/schedule']);
        } else {
          this.errorMessage = response.message || 'Failed to update HD session';
          this.snackBar.open(this.errorMessage, 'Close', { duration: 5000 });
          this.loading = false;
        }
      },
      error: (error) => {
        console.error('Error updating HD session:', error);
        let errorMsg = 'An error occurred while updating the HD session';
        if (error.error?.message) {
          errorMsg = error.error.message;
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
}
