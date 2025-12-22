import { Component, OnInit } from '@angular/core';
import { CommonModule, Location } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatChipsModule } from '@angular/material/chips';
import { MatTableModule } from '@angular/material/table';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { environment } from '../../../../environments/environment.development';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';

@Component({
  selector: 'app-patient-history',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatExpansionModule,
    MatChipsModule,
    MatTableModule,
    MatTabsModule,
    MatSnackBarModule,
    MatTooltipModule,
    MatProgressBarModule
  ],
  templateUrl: './patient-history.component.html',
  styleUrl: './patient-history.component.scss'
})
export class PatientHistoryComponent implements OnInit {
  patientId: number | null = null;
  loading = false;
  errorMessage = '';
  
  // Patient History Data
  patientInfo: any = null;
  sessions: any[] = [];
  statistics: any = null;
  vitalTrends: any = null;
  medications: any[] = [];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private location: Location,
    private http: HttpClient,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      if (params['id']) {
        this.patientId = +params['id'];
        this.loadPatientHistory();
      }
    });
  }

  loadPatientHistory(): void {
    if (!this.patientId) return;
    
    this.loading = true;
    this.errorMessage = '';
    
    const token = localStorage.getItem('token');
    const headers = new HttpHeaders({
      'Authorization': `Bearer ${token}`
    });
    
    // Fetch patient history from API
    this.http.get<any>(`${environment.apiUrl}/api/PatientHistory/${this.patientId}`, { headers })
      .subscribe({
        next: (response) => {
          console.log('Patient History API Response:', response);
          if (response.success && response.data) {
            const data = response.data;
            console.log('Sessions data:', data.sessions || data.Sessions);
            if ((data.sessions || data.Sessions) && (data.sessions || data.Sessions).length > 0) {
              console.log('First session details:', (data.sessions || data.Sessions)[0]);
            }
            
            // Set patient info - handle both PascalCase (from API) and camelCase
            const pInfo = data.patientInfo || data.PatientInfo;
            if (pInfo) {
              this.patientInfo = {
                patientID: pInfo.patientID || pInfo.PatientID,
                name: pInfo.name || pInfo.Name,
                mrn: pInfo.mrn || pInfo.MRN,
                age: pInfo.age || pInfo.Age,
                gender: pInfo.gender || pInfo.Gender,
                dryWeight: pInfo.dryWeight || pInfo.DryWeight,
                hdCycle: pInfo.hdCycle || pInfo.HDCycle,
                hdStartDate: pInfo.hdStartDate || pInfo.HDStartDate,
                dialyserType: pInfo.dialyserType || pInfo.DialyserType,
                dialyserCount: pInfo.dialyserCount || pInfo.DialyserCount,
                bloodTubingCount: pInfo.bloodTubingCount || pInfo.BloodTubingCount,
                totalDialysisCompleted: pInfo.totalDialysisCompleted || pInfo.TotalDialysisCompleted,
                dialysersPurchased: pInfo.dialysersPurchased || pInfo.DialysersPurchased,
                bloodTubingPurchased: pInfo.bloodTubingPurchased || pInfo.BloodTubingPurchased
              };
            }
            
            // Set sessions - handle both cases
            const rawSessions = data.sessions || data.Sessions || [];
            
            // Filter to only show past sessions (actual history)
            const today = new Date();
            today.setHours(0, 0, 0, 0); // Reset to start of day for accurate comparison
            
            this.sessions = rawSessions.filter((session: any) => {
              const sessionDate = new Date(session.sessionDate || session.SessionDate);
              sessionDate.setHours(0, 0, 0, 0);
              return sessionDate <= today; // Only include sessions on or before today
            });
            
            // Set statistics - handle both cases
            const stats = data.statistics || data.Statistics;
            if (stats) {
              this.statistics = {
                totalSessions: stats.totalSessions || stats.TotalSessions || 0,
                averageWeightLoss: stats.averageWeightLoss || stats.AverageWeightLoss || 0,
                averagePreWeight: stats.averagePreWeight || stats.AveragePreWeight || 0,
                firstSessionDate: stats.firstSessionDate || stats.FirstSessionDate
              };
            }
            
            // Load vital trends
            this.loadVitalTrends();
            
            // Extract medications from sessions
            this.extractMedications();
            
            this.loading = false;
          } else {
            this.errorMessage = 'No history data found';
            this.loading = false;
          }
        },
        error: (error) => {
          console.error('Error loading patient history:', error);
          
          if (error.status === 401) {
            this.errorMessage = 'Authentication required. Please log in again.';
            this.snackBar.open('Session expired. Please log in again.', 'Close', { duration: 5000 });
          } else if (error.status === 404) {
            this.errorMessage = 'Patient not found';
            this.snackBar.open('Patient not found', 'Close', { duration: 5000 });
          } else {
            this.errorMessage = error.error?.message || 'Failed to load patient history';
            this.snackBar.open(this.errorMessage, 'Close', { duration: 5000 });
          }
          
          this.loading = false;
        }
      });
  }

  loadVitalTrends(): void {
    if (!this.patientId) return;
    
    const token = localStorage.getItem('token');
    const headers = new HttpHeaders({
      'Authorization': `Bearer ${token}`
    });
    
    // Fetch vital trends from API
    this.http.get<any>(`${environment.apiUrl}/api/PatientHistory/${this.patientId}/trends`, { headers })
      .subscribe({
        next: (response) => {
          if (response.success && response.data) {
            this.vitalTrends = response.data;
          }
        },
        error: (error) => {
          console.warn('Could not load vital trends:', error);
        }
      });
  }

  extractMedications(): void {
    // Extract unique medications from sessions
    const medMap = new Map();
    
    this.sessions.forEach(session => {
      if (session.medicationsCount && session.medicationsCount > 0) {
        // Mark that this session has medications
        medMap.set(session.scheduleID, {
          sessionDate: session.sessionDate,
          count: session.medicationsCount
        });
      }
    });
    
    this.medications = Array.from(medMap.values());
  }

  goBack(): void {
    this.location.back();
  }

  goHome(): void {
    this.router.navigate(['/']);
  }

  editSession(scheduleId: number): void {
    this.router.navigate(['/schedule/hd-session/edit', scheduleId]);
  }

  viewSessionDetails(session: any): void {
    const scheduleId = session.scheduleID || session.ScheduleID;
    console.log('viewSessionDetails called with session:', session);
    console.log('scheduleId:', scheduleId);
    console.log('patientId:', this.patientId);
    console.log('Navigating to:', ['/patients', this.patientId, 'session', scheduleId]);
    this.router.navigate(['/patients', this.patientId, 'session', scheduleId]);
  }

  getStatusClass(isDischarged: boolean): string {
    return isDischarged ? 'status-discharged' : 'status-active';
  }

  getUsagePercentage(current: number, max: number): number {
    if (!max || max === 0) return 0;
    return (current / max) * 100;
  }
}
