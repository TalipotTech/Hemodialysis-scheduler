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
  selector: 'app-technician-view',
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
  templateUrl: './technician-view.html',
  styleUrl: './technician-view.scss',
})
export class TechnicianView implements OnInit {
  userName: string = '';
  loading = true;
  
  // Statistics (READ ONLY)
  totalPatients = 0;
  activePatients = 0;
  todaySessions = 0;
  
  // Today's schedule (READ ONLY)
  todaySchedule: any[] = [];

  constructor(
    private authService: AuthService,
    private router: Router,
    private patientService: PatientService,
    private scheduleService: ScheduleService
  ) {}

  ngOnInit(): void {
    const user = this.authService.getCurrentUser();
    this.userName = user?.username || 'Technician';
    this.loadDashboardData();
  }

  loadDashboardData(): void {
    this.loading = true;
    
    // Load patients (READ ONLY)
    this.patientService.getAllPatients().subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.totalPatients = response.data.length;
          this.activePatients = response.data.filter((p: any) => p.isActive).length;
        }
      },
      error: (error) => console.error('Error loading patients:', error)
    });

    // Load today's schedule (READ ONLY)
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

  navigateToSchedule(): void {
    this.router.navigate(['/schedule']);
  }

  navigateToPatients(): void {
    this.router.navigate(['/patients']);
  }

  navigateToPatientHistory(patientId: number): void {
    this.router.navigate([`/patients/${patientId}/history`]);
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
