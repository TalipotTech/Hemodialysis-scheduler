import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule, Location } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatStepperModule } from '@angular/material/stepper';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatSelectModule } from '@angular/material/select';
import { ScheduleService } from '../../../core/services/schedule.service';

@Component({
  selector: 'app-dialysis-workflow',
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
    MatProgressSpinnerModule,
    MatStepperModule,
    MatSnackBarModule,
    MatTooltipModule,
    MatChipsModule,
    MatDividerModule,
    MatProgressBarModule,
    MatSelectModule
  ],
  templateUrl: './dialysis-workflow.component.html',
  styleUrls: ['./dialysis-workflow.component.scss']
})
export class DialysisWorkflowComponent implements OnInit, OnDestroy {
  patientId: number | null = null;
  scheduleId: number | null = null;
  hdLogId: number | null = null;
  
  loading = false;
  saving = false;
  sessionData: any = null;
  phaseStatus: any = null;
  
  preDialysisForm: FormGroup;
  postDialysisForm: FormGroup;
  
  autoSaveTimer: any = null;
  preSaveStatus: 'idle' | 'saving' | 'saved' = 'idle';
  postSaveStatus: 'idle' | 'saving' | 'saved' = 'idle';
  preLastSaved: Date | null = null;
  postLastSaved: Date | null = null;

  // Post-Dialysis Medications
  medications: Array<{
    medicationType: string;
    medicationName: string;
    dose: string;
    route: string;
    administeredAt: string;
  }> = [];
  
  medicationTypes = ['Erythropoietin', 'Iron Supplement', 'Vitamin D', 'Antibiotic', 'Analgesic', 'Other'];
  medicationRoutes = ['IV', 'SC', 'IM', 'PO', 'Other'];

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private location: Location,
    private scheduleService: ScheduleService,
    private snackBar: MatSnackBar
  ) {
    // Pre-Dialysis Form
    this.preDialysisForm = this.fb.group({
      preWeight: ['', [Validators.required, Validators.min(20), Validators.max(250)]],
      preSBP: ['', [Validators.required, Validators.min(60), Validators.max(250)]],
      preDBP: ['', [Validators.required, Validators.min(40), Validators.max(150)]],
      preHR: ['', [Validators.min(40), Validators.max(200)]],
      preTemp: ['', [Validators.min(35), Validators.max(42)]],
      accessSite: [''],
      preAssessmentNotes: ['']
    });

    // Post-Dialysis Form
    this.postDialysisForm = this.fb.group({
      postDialysisWeight: ['', [Validators.required, Validators.min(20), Validators.max(250)]],
      postDialysisSBP: ['', [Validators.required, Validators.min(60), Validators.max(250)]],
      postDialysisDBP: ['', [Validators.required, Validators.min(40), Validators.max(150)]],
      postDialysisHR: ['', [Validators.required, Validators.min(40), Validators.max(200)]],
      accessBleedingTime: ['', [Validators.required, Validators.min(0), Validators.max(60)]],
      totalFluidRemoved: ['', [Validators.required, Validators.min(0)]],
      postAccessStatus: ['', Validators.required],
      medicationsAdministered: [''],
      dischargeNotes: ['']
    });
  }

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      console.log('🚀 Dialysis Workflow initialized with params:', params);
      
      if (params['id']) {
        this.patientId = +params['id'];
        console.log('👤 Patient ID:', this.patientId);
      }
      if (params['scheduleId']) {
        this.scheduleId = +params['scheduleId'];
        console.log('📅 Schedule ID:', this.scheduleId);
        this.loadSessionData();
      }
    });

    // Auto-save pre-dialysis form
    this.preDialysisForm.valueChanges.subscribe(() => {
      this.scheduleAutoSave('pre');
    });

    // Auto-save post-dialysis form
    this.postDialysisForm.valueChanges.subscribe(() => {
      this.scheduleAutoSave('post');
    });
  }

  ngOnDestroy(): void {
    if (this.autoSaveTimer) {
      clearTimeout(this.autoSaveTimer);
    }
  }

  loadSessionData(): void {
    if (!this.scheduleId) return;
    
    this.loading = true;
    
    // First, get the HD Log ID for this schedule
    this.scheduleService.getActiveSessions().subscribe({
      next: (response: any) => {
        if (response.success && response.data) {
          const schedule = response.data.find((s: any) => s.scheduleID === this.scheduleId);
          
          if (schedule && schedule.hdLogID) {
            this.hdLogId = schedule.hdLogID;
            this.loadPhaseStatus();
            this.loadHDLogData();
          } else {
            this.snackBar.open('Session not found or not yet initialized', 'Close', { duration: 5000 });
            this.loading = false;
          }
        }
      },
      error: (error) => {
        console.error('Error loading schedule:', error);
        this.snackBar.open('Failed to load session data', 'Close', { duration: 5000 });
        this.loading = false;
      }
    });
  }

  loadPhaseStatus(): void {
    if (!this.hdLogId) return;
    
    this.scheduleService.getPhaseStatus(this.hdLogId).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.phaseStatus = response.data;
          console.log('Phase status:', this.phaseStatus);
        }
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading phase status:', error);
        this.loading = false;
      }
    });
  }

  loadHDLogData(): void {
    if (!this.hdLogId) return;
    
    console.log('📋 Loading HD Log data for hdLogId:', this.hdLogId);
    
    this.scheduleService.getHDLogById(this.hdLogId).subscribe({
      next: (response) => {
        console.log('📋 HD Log response:', response);
        
        if (response.success && response.data) {
          this.sessionData = response.data;
          
          // Populate pre-dialysis form if data exists
          if (this.sessionData.preWeight) {
            console.log('✅ Loading pre-dialysis data into form');
            this.preDialysisForm.patchValue({
              preWeight: this.sessionData.preWeight,
              preSBP: this.sessionData.preSBP,
              preDBP: this.sessionData.preDBP,
              preHR: this.sessionData.preHR,
              preTemp: this.sessionData.preTemp,
              accessSite: this.sessionData.accessSite,
              preAssessmentNotes: this.sessionData.preAssessmentNotes
            }, { emitEvent: false });
          }
          
          // Populate post-dialysis form if data exists
          if (this.sessionData.postDialysisWeight) {
            console.log('✅ Loading post-dialysis data into form');
            this.postDialysisForm.patchValue({
              postDialysisWeight: this.sessionData.postDialysisWeight,
              postDialysisSBP: this.sessionData.postDialysisSBP,
              postDialysisDBP: this.sessionData.postDialysisDBP,
              postDialysisHR: this.sessionData.postDialysisHR,
              accessBleedingTime: this.sessionData.accessBleedingTime,
              totalFluidRemoved: this.sessionData.totalFluidRemoved,
              postAccessStatus: this.sessionData.postAccessStatus,
              medicationsAdministered: this.sessionData.medicationsAdministered,
              dischargeNotes: this.sessionData.dischargeNotes
            }, { emitEvent: false });
          }
        }
        
        // Always load medications from database, regardless of whether other data exists
        console.log('🔄 Now loading medications...');
        this.loadExistingMedications();
      },
      error: (error) => {
        console.error('❌ Error loading HD Log:', error);
        // Still try to load medications even if HD Log data fails
        this.loadExistingMedications();
      }
    });
  }

  // Load existing medications from database
  loadExistingMedications(): void {
    if (!this.scheduleId) {
      console.warn('⚠️ Cannot load medications: scheduleId is missing');
      return;
    }
    
    console.log('🔍 Loading medications for scheduleId:', this.scheduleId);
    
    this.scheduleService.getMedications(this.scheduleId).subscribe({
      next: (response) => {
        console.log('📦 Medication API response:', response);
        
        if (response.success && response.data && Array.isArray(response.data)) {
          console.log('✅ Found', response.data.length, 'medications in database');
          
          // Convert database medications to component format
          this.medications = response.data.map((med: any) => ({
            medicationType: med.medicationType || '',
            medicationName: med.medicationName || '',
            dose: med.dosage || med.dose || '',
            route: med.route || '',
            administeredAt: med.givenTime ? this.formatTimeFromDatabase(med.givenTime) : ''
          }));
          
          console.log('✅ Medications loaded into form:', this.medications);
        } else {
          console.log('ℹ️ No medications found or invalid response format');
        }
      },
      error: (error) => {
        console.error('❌ Error loading medications:', error);
      }
    });
  }

  // Convert database time format (HH:mm:ss or TimeSpan) to HH:mm for input[type="time"]
  formatTimeFromDatabase(timeValue: any): string {
    if (!timeValue) return '';
    
    // If it's already a string in HH:mm:ss format
    if (typeof timeValue === 'string') {
      const parts = timeValue.split(':');
      if (parts.length >= 2) {
        return `${parts[0]}:${parts[1]}`; // Return HH:mm
      }
    }
    
    return '';
  }

  // Auto-save functionality
  scheduleAutoSave(phase: 'pre' | 'post'): void {
    if (this.autoSaveTimer) {
      clearTimeout(this.autoSaveTimer);
    }
    
    if (phase === 'pre') {
      this.preSaveStatus = 'idle';
    } else {
      this.postSaveStatus = 'idle';
    }
    
    this.autoSaveTimer = setTimeout(() => {
      if (phase === 'pre' && !this.phaseStatus?.isPreDialysisLocked) {
        this.autoSavePreDialysis();
      } else if (phase === 'post' && this.phaseStatus?.sessionPhase === 'POST_DIALYSIS') {
        this.autoSavePostDialysis();
      }
    }, 2000); // Auto-save after 2 seconds of no typing
  }

  autoSavePreDialysis(): void {
    if (!this.hdLogId || this.preDialysisForm.invalid) return;
    
    this.preSaveStatus = 'saving';
    
    this.scheduleService.savePreDialysis(this.hdLogId, this.preDialysisForm.value).subscribe({
      next: (response) => {
        if (response.success) {
          this.preSaveStatus = 'saved';
          this.preLastSaved = new Date();
          setTimeout(() => this.preSaveStatus = 'idle', 3000);
        } else {
          this.preSaveStatus = 'idle';
        }
      },
      error: () => {
        this.preSaveStatus = 'idle';
      }
    });
  }

  autoSavePostDialysis(): void {
    if (!this.hdLogId || this.postDialysisForm.invalid) return;
    
    this.postSaveStatus = 'saving';
    
    this.scheduleService.savePostDialysis(this.hdLogId, this.postDialysisForm.value).subscribe({
      next: (response) => {
        if (response.success) {
          this.postSaveStatus = 'saved';
          this.postLastSaved = new Date();
          setTimeout(() => this.postSaveStatus = 'idle', 3000);
        } else {
          this.postSaveStatus = 'idle';
        }
      },
      error: () => {
        this.postSaveStatus = 'idle';
      }
    });
  }

  // Manual save buttons
  savePreDialysis(): void {
    if (this.preDialysisForm.invalid) {
      this.preDialysisForm.markAllAsTouched();
      this.snackBar.open('Please fill in all required fields correctly', 'Close', { duration: 3000 });
      return;
    }

    if (!this.hdLogId) {
      this.snackBar.open('Session not initialized', 'Close', { duration: 3000 });
      return;
    }

    this.saving = true;
    
    this.scheduleService.savePreDialysis(this.hdLogId, this.preDialysisForm.value).subscribe({
      next: (response) => {
        if (response.success) {
          this.snackBar.open('✓ Pre-Dialysis data saved successfully!', 'Close', { duration: 3000 });
          this.preLastSaved = new Date();
          this.loadPhaseStatus();
        } else {
          this.snackBar.open(response.message || 'Failed to save', 'Close', { duration: 3000 });
        }
        this.saving = false;
      },
      error: (error) => {
        console.error('Error saving pre-dialysis:', error);
        this.snackBar.open('Failed to save pre-dialysis data', 'Close', { duration: 3000 });
        this.saving = false;
      }
    });
  }

  completePreDialysis(): void {
    if (this.preDialysisForm.invalid) {
      this.preDialysisForm.markAllAsTouched();
      this.snackBar.open('Please complete all required fields before proceeding', 'Close', { duration: 3000 });
      return;
    }

    if (!this.hdLogId) return;

    if (!confirm('Complete Pre-Dialysis phase and start treatment? You will not be able to edit pre-dialysis data after this.')) {
      return;
    }

    this.saving = true;

    // First save the data, then complete the phase
    this.scheduleService.savePreDialysis(this.hdLogId, this.preDialysisForm.value).subscribe({
      next: (saveResponse) => {
        if (saveResponse.success) {
          // Now complete the phase
          this.scheduleService.completePreDialysis(this.hdLogId!).subscribe({
            next: (completeResponse) => {
              if (completeResponse.success) {
                this.snackBar.open('✓ Pre-Dialysis completed! Starting treatment phase...', 'Close', { duration: 3000 });
                this.loadPhaseStatus();
                this.loadHDLogData();
              } else {
                this.snackBar.open(completeResponse.message || 'Failed to complete phase', 'Close', { duration: 3000 });
              }
              this.saving = false;
            },
            error: (error) => {
              console.error('Error completing pre-dialysis:', error);
              this.snackBar.open('Failed to complete pre-dialysis phase', 'Close', { duration: 3000 });
              this.saving = false;
            }
          });
        } else {
          this.snackBar.open('Failed to save data', 'Close', { duration: 3000 });
          this.saving = false;
        }
      },
      error: (error) => {
        console.error('Error saving pre-dialysis:', error);
        this.snackBar.open('Failed to save pre-dialysis data', 'Close', { duration: 3000 });
        this.saving = false;
      }
    });
  }

  startPostDialysis(): void {
    if (!this.hdLogId) return;

    if (!confirm('End treatment and start Post-Dialysis phase?')) {
      return;
    }

    this.saving = true;

    this.scheduleService.startPostDialysis(this.hdLogId).subscribe({
      next: (response) => {
        if (response.success) {
          this.snackBar.open('✓ Treatment ended. Starting Post-Dialysis phase...', 'Close', { duration: 3000 });
          this.loadPhaseStatus();
          this.loadHDLogData();
          // Ensure medications are loaded when entering POST_DIALYSIS phase
          this.loadExistingMedications();
        } else {
          this.snackBar.open(response.message || 'Failed to start post-dialysis', 'Close', { duration: 3000 });
        }
        this.saving = false;
      },
      error: (error) => {
        console.error('Error starting post-dialysis:', error);
        this.snackBar.open('Failed to start post-dialysis phase', 'Close', { duration: 3000 });
        this.saving = false;
      }
    });
  }

  savePostDialysis(): void {
    if (this.postDialysisForm.invalid) {
      this.postDialysisForm.markAllAsTouched();
      this.snackBar.open('Please fill in all required fields correctly', 'Close', { duration: 3000 });
      return;
    }

    if (!this.hdLogId) return;

    this.saving = true;

    // Combine form data with medications
    const postDialysisData = {
      ...this.postDialysisForm.value,
      medicationsAdministered: this.getMedicationsString()
    };

    this.scheduleService.savePostDialysis(this.hdLogId, postDialysisData).subscribe({
      next: (response) => {
        if (response.success) {
          // Save medications to database
          this.saveMedicationsToDatabase().then(() => {
            this.snackBar.open('✓ Post-Dialysis data and medications saved successfully!', 'Close', { duration: 3000 });
            this.postLastSaved = new Date();
            this.loadPhaseStatus();
            // DON'T reload medications - keep them visible in the form
            // this.loadExistingMedications(); // Removed to keep form data
            this.saving = false;
          }).catch((error) => {
            console.error('Error saving medications:', error);
            this.snackBar.open('Post-dialysis saved, but some medications failed to save', 'Close', { duration: 3000 });
            this.saving = false;
          });
        } else {
          this.snackBar.open(response.message || 'Failed to save', 'Close', { duration: 3000 });
          this.saving = false;
        }
      },
      error: (error) => {
        console.error('Error saving post-dialysis:', error);
        this.snackBar.open('Failed to save post-dialysis data', 'Close', { duration: 3000 });
        this.saving = false;
      }
    });
  }

  completePostDialysis(): void {
    if (this.postDialysisForm.invalid) {
      this.postDialysisForm.markAllAsTouched();
      this.snackBar.open('Please complete all required fields before discharging patient', 'Close', { duration: 3000 });
      return;
    }

    if (!this.hdLogId || !this.scheduleId) return;

    if (!confirm('Complete Post-Dialysis and discharge patient? This will finalize the session.')) {
      return;
    }

    this.saving = true;

    // First save the post-dialysis data
    this.scheduleService.savePostDialysis(this.hdLogId, this.postDialysisForm.value).subscribe({
      next: (saveResponse) => {
        if (saveResponse.success) {
          // Save each medication individually to the database
          this.saveMedicationsToDatabase().then(() => {
            // Now complete the phase
            this.scheduleService.completePostDialysis(this.hdLogId!).subscribe({
              next: (completeResponse) => {
                if (completeResponse.success) {
                  this.snackBar.open('✓ Session completed! Patient discharged successfully.', 'Close', { duration: 5000 });
                  
                  // Navigate to patient history or schedule view after 2 seconds
                  setTimeout(() => {
                    if (this.patientId) {
                      this.router.navigate(['/patients/history', this.patientId]);
                    } else {
                      this.router.navigate(['/schedule']);
                    }
                  }, 2000);
                } else {
                  this.snackBar.open(completeResponse.message || 'Failed to complete session', 'Close', { duration: 3000 });
                }
                this.saving = false;
              },
              error: (error) => {
                console.error('Error completing post-dialysis:', error);
                this.snackBar.open('Failed to complete session', 'Close', { duration: 3000 });
                this.saving = false;
              }
            });
          }).catch((error) => {
            console.error('Error saving medications:', error);
            this.snackBar.open('Failed to save medications', 'Close', { duration: 3000 });
            this.saving = false;
          });
        } else {
          this.snackBar.open('Failed to save data', 'Close', { duration: 3000 });
          this.saving = false;
        }
      },
      error: (error) => {
        console.error('Error saving post-dialysis:', error);
        this.snackBar.open('Failed to save post-dialysis data', 'Close', { duration: 3000 });
        this.saving = false;
      }
    });
  }

  goToMonitoring(): void {
    if (this.patientId && this.scheduleId) {
      this.router.navigate(['/patients', this.patientId, 'monitoring', this.scheduleId]);
    }
  }

  goBack(): void {
    this.location.back();
  }

  goHome(): void {
    this.router.navigate(['/']);
  }

  // Helper method to get current step index
  getStepIndex(): number {
    if (!this.phaseStatus) return 0;
    
    switch (this.phaseStatus.sessionPhase) {
      case 'PRE_DIALYSIS':
        return 0;
      case 'INTRA_DIALYSIS':
        return 1;
      case 'POST_DIALYSIS':
        return 2;
      case 'COMPLETED':
        return 3;
      default:
        return 0;
    }
  }

  // Helper method to format date/time
  formatDateTime(date: Date | string | null): string {
    if (!date) return 'Not set';
    return new Date(date).toLocaleString();
  }

  // Helper method to get phase chip color
  getPhaseChipColor(phase: string): string {
    switch (phase) {
      case 'PRE_DIALYSIS':
        return 'primary';
      case 'INTRA_DIALYSIS':
        return 'accent';
      case 'POST_DIALYSIS':
        return 'warn';
      case 'COMPLETED':
        return 'default';
      default:
        return 'default';
    }
  }

  // Medication management methods
  addMedication(): void {
    this.medications.push({
      medicationType: '',
      medicationName: '',
      dose: '',
      route: '',
      administeredAt: ''
    });
  }

  removeMedication(index: number): void {
    this.medications.splice(index, 1);
  }

  getMedicationsString(): string {
    return this.medications
      .filter(m => m.medicationName && m.dose)
      .map(m => `${m.medicationName} ${m.dose} ${m.route} at ${m.administeredAt}`)
      .join('; ');
  }

  // Save medications individually to PostDialysisMedications table
  async saveMedicationsToDatabase(): Promise<void> {
    if (!this.scheduleId || !this.patientId) {
      console.warn('Cannot save medications: missing scheduleId or patientId');
      return;
    }

    // Filter valid medications (must have name, dose, route, and time)
    const validMedications = this.medications.filter(
      med => med.medicationName && 
             med.dose && 
             med.route && 
             med.administeredAt && 
             med.administeredAt !== ''
    );

    if (validMedications.length === 0) {
      console.log('No valid medications to save');
      return;
    }

    // Get session date from form or session data
    const sessionDate = this.sessionData?.sessionDate || new Date().toISOString().split('T')[0];

    // Save each medication
    const savePromises = validMedications.map(med => {
      // Format time as HH:mm:ss for SQL TIME column
      const givenTime = med.administeredAt.includes(':') ? `${med.administeredAt}:00` : null;

      const medicationData = {
        scheduleID: this.scheduleId,
        patientID: this.patientId,
        sessionDate: sessionDate,
        medicationName: med.medicationName,
        dosage: med.dose,
        route: med.route,
        givenTime: givenTime,
        givenBy: null
      };

      console.log('💊 Saving medication:', medicationData);

      return new Promise<void>((resolve, reject) => {
        this.scheduleService.addMedication(this.scheduleId!, medicationData).subscribe({
          next: (response) => {
            if (response.success) {
              console.log('✅ Medication saved:', med.medicationName);
              resolve();
            } else {
              console.error('❌ Failed to save medication:', response.message);
              reject(new Error(response.message));
            }
          },
          error: (err) => {
            console.error('❌ Error saving medication:', err);
            reject(err);
          }
        });
      });
    });

    // Wait for all medications to be saved
    await Promise.all(savePromises);
    console.log('✅ All medications saved successfully');
  }
}
