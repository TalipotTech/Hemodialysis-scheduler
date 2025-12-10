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
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { PatientService } from '../../../core/services/patient.service';
import { StaffManagementService } from '../../../services/staff-management.service';
import { SystemSettingsService } from '../../../services/system-settings.service';
import { AIService } from '../../../services/ai.service';

@Component({
  selector: 'app-hd-session-form',
  standalone: true,
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
    MatSnackBarModule,
    MatTooltipModule
  ],
  templateUrl: './hd-session-form.component.html',
  styleUrls: ['./hd-session-form.component.scss']
})
export class HdSessionFormComponent implements OnInit {
  hdSessionForm: FormGroup;
  loading = false;
  errorMessage = '';
  patientId: number | null = null;
  patientName = '';

  dialyserTypes = ['HI', 'LO'];
  accessTypes = ['AVF', 'AVG', 'CVC'];
  dialysatePrescriptions = ['Normal', 'K+ Free', 'Ca++', 'Dextrose'];
  anticoagulationTypes = ['Heparin', 'Without Heparin'];
  syringeTypes = ['10 ml', '20 ml'];
  
  slots: any[] = [];
  beds = Array.from({ length: 10 }, (_, i) => i + 1);
  
  hdCycles = [
    { value: 'MWF', label: 'Monday, Wednesday, Friday (3x/week)', frequency: 3 },
    { value: 'TTS', label: 'Tuesday, Thursday, Saturday (3x/week)', frequency: 3 },
    { value: 'MTW', label: 'Monday, Tuesday, Wednesday (3x/week)', frequency: 3 },
    { value: 'MTWTHF', label: 'Monday to Friday (5x/week)', frequency: 5 },
    { value: 'MTWTHFS', label: 'Monday to Saturday (6x/week)', frequency: 6 },
    { value: 'Daily', label: 'Daily (7x/week)', frequency: 7 },
    { value: 'Custom', label: 'Custom Schedule', frequency: null }
  ];
  
  allDoctors: any[] = [];
  allNurses: any[] = [];
  filteredDoctors: any[] = [];
  filteredNurses: any[] = [];
  
  // AI Recommendation
  loadingAIRecommendation = false;
  aiRecommendation: any = null;
  showAICard = false;
  
  // AI Autocomplete
  loadingAutocomplete = false;
  autocompleteData: any = null;
  showAutocompleteCard = false;

  constructor(
    private fb: FormBuilder,
    private patientService: PatientService,
    private staffService: StaffManagementService,
    private systemSettingsService: SystemSettingsService,
    private aiService: AIService,
    private snackBar: MatSnackBar,
    private router: Router,
    private route: ActivatedRoute,
    private location: Location
  ) {
    this.hdSessionForm = this.fb.group({
      // Session Info
      sessionDate: [new Date(), Validators.required],
      
      // Basic HD Info
      dryWeight: [''],
      hdStartDate: [''],
      prescribedDuration: [''],
      hdCycle: [''],
      hdFrequency: [''],
      
      // Equipment & Supplies
      dialyserType: [''],
      dialyserReuseCount: [0, [Validators.min(0)]],
      bloodTubingReuse: [0, [Validators.min(0)]],
      hdUnitNumber: [''],
      
      // Prescription
      ufGoal: [''],
      dialysatePrescription: [''],
      prescribedBFR: [''],
      
      // Anticoagulation
      anticoagulationType: [''],
      heparinDose: [''],
      syringeType: [''],
      bolusDose: [''],
      heparinInfusionRate: [''],
      
      // Access & Vitals
      accessType: [''],
      bloodPressure: [''],
      symptoms: [''],
      bloodTestDone: [false],
      
      // Bed Assignment
      slotID: [null, Validators.required],
      bedNumber: [null, Validators.required],
      
      // Staff Assignment
      assignedDoctor: [null],
      assignedNurse: [null]
    });
  }

  ngOnInit(): void {
    // Get patient ID from route
    this.route.params.subscribe(params => {
      this.patientId = params['id'] ? +params['id'] : null;
      if (this.patientId) {
        this.loadPatientInfo(this.patientId);
      }
    });

    // Load slots from backend
    this.loadSlots();
    
    // Load staff
    this.loadStaff();
  }

  loadSlots(): void {
    this.systemSettingsService.getSlots().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.slots = response.data.map((slot: any) => ({
            id: slot.slotID,
            name: slot.slotName,
            startTime: slot.startTime,
            endTime: slot.endTime,
            bedCapacity: slot.maxBeds
          }));
        }
      },
      error: (error) => {
        console.error('Error loading slots:', error);
        this.snackBar.open('Failed to load slots', 'Close', { duration: 3000 });
      }
    });
  }

  loadPatientInfo(id: number): void {
    this.patientService.getPatient(id).subscribe({
      next: (response: any) => {
        if (response.success && response.data) {
          this.patientName = response.data.name;
        }
      },
      error: (error: any) => {
        console.error('Error loading patient:', error);
        this.errorMessage = 'Failed to load patient information';
      }
    });
  }

  loadStaff(): void {
    this.staffService.getAllStaff().subscribe({
      next: (response: any) => {
        if (response.success && response.data) {
          this.allDoctors = response.data.filter((s: any) => s.role === 'Doctor');
          this.allNurses = response.data.filter((s: any) => s.role === 'Nurse');
          this.filteredDoctors = this.allDoctors;
          this.filteredNurses = this.allNurses;
        }
      },
      error: (error: any) => {
        console.error('Error loading staff:', error);
      }
    });
  }

  filterStaffBySlot(): void {
    const selectedSlotId = this.hdSessionForm.get('slotID')?.value;
    
    if (selectedSlotId) {
      this.filteredDoctors = this.allDoctors.filter(d => 
        d.assignedSlot === selectedSlotId || d.assignedSlot === null
      );
      this.filteredNurses = this.allNurses.filter(n => 
        n.assignedSlot === selectedSlotId || n.assignedSlot === null
      );

      // Auto-assign if only one option available
      if (this.filteredDoctors.length === 1) {
        this.hdSessionForm.patchValue({ assignedDoctor: this.filteredDoctors[0].staffID });
      }
      if (this.filteredNurses.length === 1) {
        this.hdSessionForm.patchValue({ assignedNurse: this.filteredNurses[0].staffID });
      }
    } else {
      this.filteredDoctors = this.allDoctors;
      this.filteredNurses = this.allNurses;
    }
  }

  onSlotChange(): void {
    // Clear current assignments when slot changes
    this.hdSessionForm.patchValue({
      assignedDoctor: null,
      assignedNurse: null
    });
    
    this.filterStaffBySlot();
  }

  onHDCycleChange(): void {
    const selectedCycle = this.hdSessionForm.get('hdCycle')?.value;
    const cycleOption = this.hdCycles.find(c => c.value === selectedCycle);
    if (cycleOption && cycleOption.frequency) {
      this.hdSessionForm.patchValue({ hdFrequency: cycleOption.frequency });
    }
  }

  onSubmit(): void {
    if (this.hdSessionForm.invalid) {
      this.hdSessionForm.markAllAsTouched();
      return;
    }

    if (!this.patientId) {
      this.errorMessage = 'Patient ID is missing';
      return;
    }

    this.loading = true;
    this.errorMessage = '';

    const formValue = this.hdSessionForm.value;
    const hdScheduleData = {
      patientID: this.patientId,
      sessionDate: formValue.sessionDate,
      dryWeight: formValue.dryWeight ? Number(formValue.dryWeight) : null,
      hdStartDate: formValue.hdStartDate || null,
      hdCycle: formValue.hdCycle || null,
      hdFrequency: formValue.hdFrequency ? Number(formValue.hdFrequency) : null,
      dialyserType: formValue.dialyserType || null,
      dialyserReuseCount: Number(formValue.dialyserReuseCount) || 0,
      bloodTubingReuse: Number(formValue.bloodTubingReuse) || 0,
      hdUnitNumber: formValue.hdUnitNumber || null,
      prescribedDuration: formValue.prescribedDuration ? Number(formValue.prescribedDuration) : null,
      ufGoal: formValue.ufGoal ? Number(formValue.ufGoal) : null,
      dialysatePrescription: formValue.dialysatePrescription || null,
      prescribedBFR: formValue.prescribedBFR ? Number(formValue.prescribedBFR) : null,
      anticoagulationType: formValue.anticoagulationType || null,
      heparinDose: formValue.heparinDose ? Number(formValue.heparinDose) : null,
      syringeType: formValue.syringeType || null,
      bolusDose: formValue.bolusDose ? Number(formValue.bolusDose) : null,
      heparinInfusionRate: formValue.heparinInfusionRate ? Number(formValue.heparinInfusionRate) : null,
      accessType: formValue.accessType || null,
      bloodPressure: formValue.bloodPressure || null,
      symptoms: formValue.symptoms || null,
      bloodTestDone: formValue.bloodTestDone || false,
      slotID: Number(formValue.slotID),
      bedNumber: Number(formValue.bedNumber),
      assignedDoctor: formValue.assignedDoctor ? Number(formValue.assignedDoctor) : null,
      assignedNurse: formValue.assignedNurse ? Number(formValue.assignedNurse) : null
    };

    // Call the new HD Schedule API endpoint
    const apiUrl = '/api/hdschedule';
    
    fetch(`http://localhost:5001${apiUrl}`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${localStorage.getItem('token')}`
      },
      body: JSON.stringify(hdScheduleData)
    })
    .then(response => response.json())
    .then(data => {
      if (data.success) {
        // Navigate to schedule page after successful save
        this.router.navigate(['/schedule']);
      } else {
        this.errorMessage = data.message || 'Failed to create HD session';
        this.loading = false;
      }
    })
    .catch(error => {
      console.error('Error creating HD session:', error);
      this.errorMessage = 'Failed to create HD session. Please try again.';
      this.loading = false;
    });
  }

  onCancel(): void {
    this.router.navigate(['/patients']);
  }

  goBack(): void {
    this.location.back();
  }

  // AI Recommendation Methods
  getAIRecommendation(): void {
    if (!this.patientId) {
      this.snackBar.open('Please select a patient first', 'Close', { duration: 3000 });
      return;
    }

    this.loadingAIRecommendation = true;
    this.showAICard = false;

    const sessionDate = this.hdSessionForm.get('sessionDate')?.value || new Date();
    
    this.aiService.getSchedulingRecommendation({
      patientId: this.patientId,
      preferredDate: sessionDate.toISOString()
    }).subscribe({
      next: (recommendation) => {
        this.aiRecommendation = recommendation;
        this.showAICard = true;
        this.loadingAIRecommendation = false;
        
        this.snackBar.open(
          `AI Recommendation Ready! Confidence: ${(recommendation.confidence * 100).toFixed(0)}%`,
          'Close',
          { duration: 5000 }
        );
      },
      error: (error) => {
        console.error('AI recommendation error:', error);
        const errorMsg = error.error?.error || error.error?.message || 'Failed to get AI recommendation';
        this.snackBar.open(errorMsg, 'Close', { duration: 5000 });
        this.loadingAIRecommendation = false;
      }
    });
  }

  applyAIRecommendation(): void {
    if (!this.aiRecommendation) return;

    // Auto-fill form with AI suggestion
    this.hdSessionForm.patchValue({
      slotID: this.aiRecommendation.recommendedSlotId,
      bedNumber: this.aiRecommendation.recommendedBedNumber
    });

    // Find slot name for better message
    const selectedSlot = this.slots.find(s => s.id === this.aiRecommendation!.recommendedSlotId);
    const slotName = selectedSlot ? selectedSlot.name : `Slot ${this.aiRecommendation.recommendedSlotId}`;

    this.snackBar.open(
      `Applied: ${slotName}, Bed ${this.aiRecommendation.recommendedBedNumber}`,
      'Close',
      { duration: 3000 }
    );
  }

  dismissAIRecommendation(): void {
    this.showAICard = false;
    this.aiRecommendation = null;
  }

  getConfidenceLevel(confidence: number): string {
    if (confidence >= 0.8) return 'high';
    if (confidence >= 0.6) return 'medium';
    return 'low';
  }

  // AI Autocomplete Methods
  loadAutocomplete(): void {
    if (!this.patientId) return;

    this.loadingAutocomplete = true;
    this.aiService.getSessionAutocomplete(
      this.patientId,
      this.hdSessionForm.get('sessionDate')?.value || new Date()
    ).subscribe({
      next: (data) => {
        this.autocompleteData = data;
        this.showAutocompleteCard = true;
        this.loadingAutocomplete = false;

        const avgConfidence = data.predictions.length > 0
          ? (data.predictions.reduce((sum: number, p: any) => sum + p.confidence, 0) / data.predictions.length * 100).toFixed(0)
          : 0;

        this.snackBar.open(
          `AI found ${data.predictions.length} predictions with ${avgConfidence}% avg confidence`,
          'Apply All',
          { duration: 8000 }
        ).onAction().subscribe(() => {
          this.applyAllAutocomplete();
        });
      },
      error: (err) => {
        console.error('Autocomplete error:', err);
        this.loadingAutocomplete = false;
        this.snackBar.open('Failed to load predictions. Please try again.', 'Close', { duration: 3000 });
      }
    });
  }

  applyAllAutocomplete(): void {
    if (!this.autocompleteData) return;

    let appliedCount = 0;
    this.autocompleteData.predictions.forEach((pred: any) => {
      if (pred.confidence > 0.7) {
        const control = this.hdSessionForm.get(pred.fieldName);
        if (control && !control.value) {
          control.setValue(pred.predictedValue);
          control.markAsTouched();
          appliedCount++;
        }
      }
    });

    this.showAutocompleteCard = false;
    this.snackBar.open(
      `Applied ${appliedCount} high-confidence predictions`,
      'Close',
      { duration: 3000 }
    );
  }

  applyPrediction(prediction: any): void {
    const control = this.hdSessionForm.get(prediction.fieldName);
    if (control) {
      control.setValue(prediction.predictedValue);
      control.markAsTouched();
      this.snackBar.open(
        `Applied: ${prediction.fieldName} = ${prediction.predictedValue}`,
        'Close',
        { duration: 2000 }
      );
    }
  }

  dismissAutocomplete(): void {
    this.showAutocompleteCard = false;
    this.autocompleteData = null;
  }
}
