import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { AuthService } from '../../../core/services/auth.service';
import { PatientService } from '../../../core/services/patient.service';
import { ScheduleService } from '../../../core/services/schedule.service';

@Component({
  selector: 'app-doctor-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatChipsModule,
    MatTooltipModule
  ],
  templateUrl: './doctor-dashboard.html',
  styleUrl: './doctor-dashboard.scss',
})
export class DoctorDashboard implements OnInit {
  userName: string = '';
  loading = true;
  
  // Statistics
  totalPatients = 0;
  activePatients = 0;
  todaySessions = 0;
  pendingPrescriptions = 0;
  
  // Today's schedule
  todaySchedule: any[] = [];
  recentPatients: any[] = [];

  constructor(
    private authService: AuthService,
    private router: Router,
    private patientService: PatientService,
    private scheduleService: ScheduleService
  ) {}

  ngOnInit(): void {
    const user = this.authService.getCurrentUser();
    this.userName = user?.username || 'Doctor';
    this.loadDashboardData();
  }

  loadDashboardData(): void {
    this.loading = true;
    
    // Load patients
    this.patientService.getAllPatients().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.totalPatients = response.data.length;
          this.activePatients = response.data.filter((p: any) => p.isActive).length;
          this.recentPatients = response.data.slice(0, 5);
        }
      },
      error: (error) => console.error('Error loading patients:', error)
    });

    // Load today's schedule
    this.scheduleService.getDailySchedule(new Date()).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.todaySchedule = [];
          response.data.slots?.forEach((slot: any) => {
            slot.beds?.forEach((bed: any) => {
              if (bed.status === 'occupied' && bed.patient) {
                this.todaySchedule.push({
                  slotName: slot.slotName,
                  bedNumber: bed.bedNumber,
                  patientName: bed.patient.name,
                  patientId: bed.patient.id,
                  scheduleId: bed.scheduleId
                });
              }
            });
          });
          this.todaySessions = this.todaySchedule.length;
        }
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading schedule:', error);
        this.loading = false;
      }
    });
  }

  navigateToPatients(): void {
    this.router.navigate(['/patients']);
  }

  navigateToSchedule(): void {
    this.router.navigate(['/schedule']);
  }

  navigateToNewPatient(): void {
    this.router.navigate(['/patients/new']);
  }

  navigateToPatientHistory(patientId: number): void {
    this.router.navigate([`/patients/${patientId}/history`]);
  }

  goHome(): void {
    this.router.navigate(['/doctor']);
  }

  goBack(): void {
    this.router.navigate(['/doctor']);
  }

  dischargePatient(scheduleId: number, patientName: string): void {
    if (confirm(`Are you sure you want to discharge ${patientName}? This will mark the session as complete.`)) {
      this.scheduleService.dischargePatient(scheduleId).subscribe({
        next: (response) => {
          if (response.success) {
            alert('Patient discharged successfully');
            this.loadDashboardData();
          } else {
            alert('Failed to discharge patient: ' + response.message);
          }
        },
        error: (error) => {
          console.error('Error discharging patient:', error);
          alert('Error discharging patient. Please try again.');
        }
      });
    }
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
