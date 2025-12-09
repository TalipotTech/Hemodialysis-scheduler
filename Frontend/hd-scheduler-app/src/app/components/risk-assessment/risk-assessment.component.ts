import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { FormsModule } from '@angular/forms';
import { AIService } from '../../services/ai.service';
import { PatientService } from '../../core/services/patient.service';

@Component({
  selector: 'app-risk-assessment',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    MatProgressSpinnerModule,
    MatChipsModule,
    MatSelectModule,
    MatFormFieldModule,
    FormsModule
  ],
  templateUrl: './risk-assessment.component.html',
  styleUrls: ['./risk-assessment.component.scss']
})
export class RiskAssessmentComponent implements OnInit {
  loading = false;
  patients: any[] = [];
  selectedPatientId: number | null = null;
  riskAssessment: any = null;
  highRiskPatients: any[] = [];
  riskThreshold = 60;
  displayedColumns = ['patientName', 'riskScore', 'riskLevel', 'missedSessions', 'actions'];

  constructor(
    private aiService: AIService,
    private patientService: PatientService
  ) {}

  ngOnInit() {
    this.loadPatients();
    this.loadHighRiskPatients();
  }

  loadPatients() {
    this.patientService.getActivePatients().subscribe({
      next: (response: any) => {
        this.patients = response.data || response || [];
      },
      error: (err: any) => console.error('Error loading patients:', err)
    });
  }

  loadHighRiskPatients() {
    this.loading = true;
    this.aiService.getHighRiskPatients(this.riskThreshold).subscribe({
      next: (response) => {
        this.highRiskPatients = response.patients || [];
        this.loading = false;
      },
      error: (err) => {
        console.error('Error loading high-risk patients:', err);
        this.loading = false;
      }
    });
  }

  assessPatient() {
    if (!this.selectedPatientId) return;

    this.loading = true;
    this.aiService.getPatientRiskAssessment(this.selectedPatientId).subscribe({
      next: (assessment) => {
        this.riskAssessment = assessment;
        this.loading = false;
      },
      error: (err) => {
        console.error('Error assessing patient:', err);
        this.loading = false;
      }
    });
  }

  getRiskLevelColor(level: string): string {
    const colors: any = {
      'Minimal': 'accent',
      'Low': 'primary',
      'Medium': 'warn',
      'High': 'warn',
      'Critical': 'warn'
    };
    return colors[level] || 'primary';
  }

  getRiskScoreColor(score: number): string {
    if (score >= 80) return '#dc2626';
    if (score >= 60) return '#ea580c';
    if (score >= 40) return '#f59e0b';
    if (score >= 20) return '#3b82f6';
    return '#10b981';
  }

  viewDetails(patient: any) {
    this.selectedPatientId = patient.patientId;
    this.assessPatient();
  }
}
