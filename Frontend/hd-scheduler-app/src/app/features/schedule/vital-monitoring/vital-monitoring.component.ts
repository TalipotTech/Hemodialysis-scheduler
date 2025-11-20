import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule, Location } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatChipsModule } from '@angular/material/chips';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { environment } from '../../../../environments/environment.development';

@Component({
  selector: 'app-vital-monitoring',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatTableModule,
    MatSnackBarModule,
    MatTooltipModule,
    MatChipsModule
  ],
  templateUrl: './vital-monitoring.component.html',
  styleUrl: './vital-monitoring.component.scss'
})
export class VitalMonitoringComponent implements OnInit, OnDestroy {
  patientId: number | null = null;
  scheduleId: number | null = null;
  hdLogId: number | null = null;
  loading = false;
  saving = false;
  errorMessage = '';
  
  sessionData: any = null;
  monitoringRecords: any[] = [];
  vitalForm: FormGroup;
  autoSaveTimer: any = null;
  saveStatus: 'idle' | 'saving' | 'saved' = 'idle';
  lastSavedTime: Date | null = null;

  displayedColumns: string[] = ['time', 'bp', 'pulse', 'temp', 'uf', 'bfr', 'actions'];

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private location: Location,
    private http: HttpClient,
    private snackBar: MatSnackBar
  ) {
    this.vitalForm = this.fb.group({
      bloodPressure: ['', Validators.required],
      pulseRate: ['', [Validators.required, Validators.min(40), Validators.max(200)]],
      temperature: ['', [Validators.required, Validators.min(35), Validators.max(42)]],
      ufVolume: ['', [Validators.required, Validators.min(0)]],
      actualBFR: ['', [Validators.min(50), Validators.max(600)]],
      venousPressure: [''],
      arterialPressure: [''],
      tmpPressure: [''],
      symptoms: [''],
      interventions: ['']
    });
  }

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      if (params['id']) {
        this.patientId = +params['id'];
      }
      if (params['scheduleId']) {
        this.scheduleId = +params['scheduleId'];
        this.loadSessionAndMonitoring();
        
        // Check for saved draft
        setTimeout(() => {
          this.checkForSavedDraft();
        }, 1000);
      }
    });

    // Setup auto-save on form changes (debounced)
    this.vitalForm.valueChanges.subscribe(() => {
      if (this.autoSaveTimer) {
        clearTimeout(this.autoSaveTimer);
      }
      this.saveStatus = 'idle';
      this.autoSaveTimer = setTimeout(() => {
        this.autoSaveDraft();
      }, 2000); // Auto-save after 2 seconds of no typing
    });
  }

  ngOnDestroy(): void {
    if (this.autoSaveTimer) {
      clearTimeout(this.autoSaveTimer);
    }
  }

  loadSessionAndMonitoring(): void {
    if (!this.scheduleId) return;
    
    this.loading = true;
    this.errorMessage = '';
    
    const token = localStorage.getItem('token');
    const headers = new HttpHeaders({
      'Authorization': `Bearer ${token}`
    });
    
    // Load session details first
    this.http.get<any>(`${environment.apiUrl}/PatientHistory/session/${this.scheduleId}`, { headers })
      .subscribe({
        next: (response) => {
          console.log('Session response:', response);
          
          if (response.success && response.data) {
            this.sessionData = response.data;
            
            // Get HDLogID from session log
            if (response.data.sessionLog?.logID) {
              this.hdLogId = response.data.sessionLog.logID;
              this.loadMonitoringRecords();
            } else {
              // Session exists but no HDLog yet - we'll create monitoring records directly
              this.monitoringRecords = response.data.intraDialyticRecords || [];
              this.loading = false;
            }
          } else {
            this.errorMessage = 'Session not found';
            this.loading = false;
          }
        },
        error: (error) => {
          console.error('Error loading session:', error);
          this.errorMessage = error.error?.message || 'Failed to load session';
          this.snackBar.open(this.errorMessage, 'Close', { duration: 5000 });
          this.loading = false;
        }
      });
  }

  loadMonitoringRecords(): void {
    if (!this.hdLogId) {
      this.loading = false;
      return;
    }
    
    const token = localStorage.getItem('token');
    const headers = new HttpHeaders({
      'Authorization': `Bearer ${token}`
    });
    
    this.http.get<any>(`${environment.apiUrl}/HDLog/${this.hdLogId}/monitoring`, { headers })
      .subscribe({
        next: (response) => {
          console.log('Monitoring records:', response);
          
          if (response.success && response.data) {
            this.monitoringRecords = response.data;
          }
          this.loading = false;
        },
        error: (error) => {
          console.error('Error loading monitoring records:', error);
          this.monitoringRecords = [];
          this.loading = false;
        }
      });
  }

  recordVitals(): void {
    if (this.vitalForm.invalid) {
      this.vitalForm.markAllAsTouched();
      this.snackBar.open('Please fill in all required fields correctly', 'Close', { duration: 3000 });
      return;
    }

    if (!this.scheduleId) {
      this.snackBar.open('Session not found', 'Close', { duration: 3000 });
      return;
    }

    this.saving = true;
    const formValue = this.vitalForm.value;
    
    // Merge with existing session data and update HDSchedule
    const updateData = {
      ...this.sessionData,
      ScheduleID: this.scheduleId,
      MonitoringTime: new Date().toLocaleTimeString('en-US', { hour12: false }),
      BloodPressure: formValue.bloodPressure || null,
      HeartRate: formValue.pulseRate ? parseInt(formValue.pulseRate) : null,
      PreTemperature: formValue.temperature ? parseFloat(formValue.temperature) : null,
      CurrentUFR: formValue.ufVolume ? parseFloat(formValue.ufVolume) : null,
      ActualBFR: formValue.actualBFR ? parseInt(formValue.actualBFR) : null,
      VenousPressure: formValue.venousPressure ? parseInt(formValue.venousPressure) : null,
      ArterialPressure: formValue.arterialPressure ? parseInt(formValue.arterialPressure) : null,
      TmpPressure: formValue.tmpPressure ? parseInt(formValue.tmpPressure) : null,
      Symptoms: formValue.symptoms || null,
      Interventions: formValue.interventions || null
    };

    console.log('Updating schedule with vitals:', updateData);
    
    const token = localStorage.getItem('token');
    const headers = new HttpHeaders({
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    });
    
    this.http.put<any>(
      `${environment.apiUrl}/HDSchedule/${this.scheduleId}`,
      updateData,
      { headers }
    ).subscribe({
      next: (response) => {
        console.log('Vitals saved:', response);
        
        if (response.success) {
          this.snackBar.open('✓ Vitals recorded successfully and saved to patient history!', 'Close', { duration: 4000 });
          
          // Clear the draft after successful save
          this.clearDraft();
          
          // Reset form
          this.vitalForm.reset();
          
          // Reload session data
          this.loadSessionAndMonitoring();
        } else {
          this.snackBar.open(response.message || 'Failed to record vitals', 'Close', { duration: 5000 });
        }
        this.saving = false;
      },
      error: (error) => {
        console.error('Error recording vitals:', error);
        const errorMsg = error.error?.message || 'Failed to record vitals';
        this.snackBar.open(errorMsg, 'Close', { duration: 5000 });
        this.saving = false;
      }
    });
  }

  deleteRecord(monitoringId: number): void {
    if (!confirm('Are you sure you want to delete this monitoring record?')) {
      return;
    }

    const token = localStorage.getItem('token');
    const headers = new HttpHeaders({
      'Authorization': `Bearer ${token}`
    });
    
    this.http.delete<any>(
      `${environment.apiUrl}/HDLog/monitoring/${monitoringId}`,
      { headers }
    ).subscribe({
      next: (response) => {
        if (response.success) {
          this.snackBar.open('Record deleted successfully', 'Close', { duration: 3000 });
          this.loadMonitoringRecords();
        } else {
          this.snackBar.open('Failed to delete record', 'Close', { duration: 3000 });
        }
      },
      error: (error) => {
        console.error('Error deleting record:', error);
        this.snackBar.open('Failed to delete record', 'Close', { duration: 3000 });
      }
    });
  }

  goBack(): void {
    this.location.back();
  }

  goHome(): void {
    this.router.navigate(['/']);
  }

  viewFullHistory(): void {
    if (this.patientId) {
      this.router.navigate(['/patients/history', this.patientId]);
    }
  }

  // Auto-save draft methods
  private getDraftKey(): string {
    return `vital_monitoring_draft_${this.scheduleId}`;
  }

  private autoSaveDraft(): void {
    if (!this.scheduleId) return;

    this.saveStatus = 'saving';
    const formValue = this.vitalForm.value;
    
    // Only save if there's actual data
    const hasData = Object.values(formValue).some(val => val !== null && val !== '');
    if (!hasData) {
      this.saveStatus = 'idle';
      return;
    }

    const draft = {
      formData: formValue,
      timestamp: new Date().toISOString(),
      scheduleId: this.scheduleId,
      patientId: this.patientId
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
      console.error('Error auto-saving draft:', error);
      this.saveStatus = 'idle';
    }
  }

  private checkForSavedDraft(): void {
    if (!this.scheduleId) return;

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
      const message = `Found unsaved vitals from ${draftTime}. Restore them?`;
      
      this.snackBar.open(message, 'Restore', { 
        duration: 10000,
        horizontalPosition: 'center',
        verticalPosition: 'top'
      }).onAction().subscribe(() => {
        this.restoreDraft(draft);
      });

    } catch (error) {
      console.error('Error parsing saved draft:', error);
      localStorage.removeItem(draftKey);
    }
  }

  private restoreDraft(draft: any): void {
    if (!draft || !draft.formData) return;

    // Restore form data
    this.vitalForm.patchValue(draft.formData);
    this.snackBar.open('✓ Draft restored successfully', 'Close', { duration: 3000 });
  }

  private clearDraft(): void {
    if (!this.scheduleId) return;
    localStorage.removeItem(this.getDraftKey());
  }
}
