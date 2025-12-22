import { Component, OnInit } from '@angular/core';
import { CommonModule, Location } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTableModule } from '@angular/material/table';
import { MatChipsModule } from '@angular/material/chips';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { environment } from '../../../../environments/environment.development';

@Component({
  selector: 'app-session-details',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatTabsModule,
    MatTableModule,
    MatChipsModule,
    MatSnackBarModule,
    MatTooltipModule
  ],
  templateUrl: './session-details.component.html',
  styleUrl: './session-details.component.scss'
})
export class SessionDetailsComponent implements OnInit {
  patientId: number | null = null;
  scheduleId: number | null = null;
  loading = false;
  errorMessage = '';
  
  sessionData: any = null;

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
      }
      if (params['scheduleId']) {
        this.scheduleId = +params['scheduleId'];
        this.loadSessionDetails();
      }
    });
  }

  loadSessionDetails(): void {
    if (!this.scheduleId) {
      this.errorMessage = 'Session ID is missing';
      return;
    }
    
    this.loading = true;
    this.errorMessage = '';
    
    const token = localStorage.getItem('token');
    const headers = new HttpHeaders({
      'Authorization': `Bearer ${token}`
    });
    
    console.log('Loading session details for schedule ID:', this.scheduleId);
    console.log('API URL:', `${environment.apiUrl}/api/PatientHistory/session/${this.scheduleId}`);
    
    this.http.get<any>(`${environment.apiUrl}/api/PatientHistory/session/${this.scheduleId}`, { headers })
      .subscribe({
        next: (response) => {
          console.log('Session details response:', response);
          
          if (response.success && response.data) {
            // Handle both PascalCase and camelCase
            const data = response.data;
            this.sessionData = {
              sessionInfo: data.sessionInfo || data.SessionInfo,
              intraDialyticRecords: data.intraDialyticRecords || data.IntraDialyticRecords || [],
              medications: data.medications || data.Medications || [],
              sessionLog: data.sessionLog || data.SessionLog
            };
            console.log('Session data loaded:', this.sessionData);
            this.loading = false;
          } else {
            this.errorMessage = 'No session data found';
            this.loading = false;
          }
        },
        error: (error) => {
          console.error('Error loading session details:', error);
          console.error('Error status:', error.status);
          console.error('Error details:', error.error);
          
          if (error.status === 404) {
            this.errorMessage = 'Session not found';
          } else if (error.status === 401) {
            this.errorMessage = 'Authentication required. Please log in again.';
          } else {
            this.errorMessage = error.error?.message || 'Failed to load session details';
          }
          
          this.snackBar.open(this.errorMessage, 'Close', { duration: 5000 });
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

  viewPatientHistory(): void {
    if (this.patientId) {
      this.router.navigate(['/patients/history', this.patientId]);
    }
  }

  goToMonitoring(): void {
    if (this.patientId && this.scheduleId) {
      this.router.navigate(['/patients', this.patientId, 'monitoring', this.scheduleId]);
    }
  }

  printSession(): void {
    window.print();
  }
}
