import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatInputModule } from '@angular/material/input';
import { MatTabsModule } from '@angular/material/tabs';
import { FormsModule } from '@angular/forms';
import { AIService } from '../../services/ai.service';
import { PatientService } from '../../core/services/patient.service';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';

@Component({
  selector: 'app-report-generation',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSelectModule,
    MatFormFieldModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatInputModule,
    MatTabsModule,
    FormsModule
  ],
  templateUrl: './report-generation.component.html',
  styleUrls: ['./report-generation.component.scss']
})
export class ReportGenerationComponent implements OnInit {
  loading = false;
  generatedReport: string | null = null;
  reportTitle = '';
  reportType = 'daily';
  selectedTabIndex = 0;
  selectedDate: Date = new Date();
  selectedStartDate: Date = new Date();
  selectedEndDate: Date = new Date();
  selectedPatientId: number | null = null;
  patients: any[] = [];
  templates: any[] = [];

  constructor(
    private aiService: AIService,
    private patientService: PatientService,
    private sanitizer: DomSanitizer
  ) {}

  ngOnInit() {
    this.loadPatients();
    this.loadTemplates();
    this.selectedStartDate.setDate(this.selectedStartDate.getDate() - 7);
  }

  onTabChange(index: number) {
    this.selectedTabIndex = index;
    const tabTypes = ['daily', 'weekly', 'patient'];
    this.reportType = tabTypes[index];
    this.generatedReport = null; // Clear previous report when switching tabs
  }

  loadPatients() {
    this.patientService.getActivePatients().subscribe({
      next: (response: any) => {
        this.patients = response.data || response || [];
      },
      error: (err: any) => console.error('Error loading patients:', err)
    });
  }

  loadTemplates() {
    this.aiService.getReportTemplates().subscribe({
      next: (templates: any) => {
        this.templates = templates;
      },
      error: (err: any) => console.error('Error loading templates:', err)
    });
  }

  generateReport() {
    this.loading = true;
    this.generatedReport = null;

    let reportObservable;

    switch (this.reportType) {
      case 'daily':
        this.reportTitle = `Daily Report - ${this.selectedDate.toLocaleDateString()}`;
        reportObservable = this.aiService.generateDailyReport(this.selectedDate);
        break;

      case 'weekly':
        this.reportTitle = `Weekly Report - ${this.selectedStartDate.toLocaleDateString()}`;
        reportObservable = this.aiService.generateWeeklyReport(this.selectedStartDate);
        break;

      case 'patient':
        if (!this.selectedPatientId) {
          alert('Please select a patient');
          this.loading = false;
          return;
        }
        const patient = this.patients.find((p: any) => p.patientID === this.selectedPatientId);
        this.reportTitle = `Patient Report - ${patient?.name || 'Unknown'}`;
        reportObservable = this.aiService.generatePatientReport(
          this.selectedPatientId,
          this.selectedStartDate,
          this.selectedEndDate
        );
        break;

      default:
        alert('Invalid report type');
        this.loading = false;
        return;
    }

    reportObservable.subscribe({
      next: (response: any) => {
        // Handle both old format (direct content) and new format (wrapped in content property)
        const reportContent = response.content || response;
        this.generatedReport = typeof reportContent === 'string' 
          ? this.convertMarkdownToHtml(reportContent)
          : this.formatReport(reportContent);
        this.loading = false;
      },
      error: (err: any) => {
        console.error('Error generating report:', err);
        alert('Failed to generate report. Please try again.');
        this.loading = false;
      }
    });
  }

  formatReport(report: any): string {
    let formatted = '';

    if (report.summary || report.insights || report.clinicalSummary) {
      formatted += `<h3>Summary</h3><p>${report.summary || report.insights || report.clinicalSummary}</p>`;
    }

    if (report.totalSessions !== undefined) {
      formatted += `<h3>Statistics</h3><ul>`;
      formatted += `<li><strong>Total Sessions:</strong> ${report.totalSessions}</li>`;
      formatted += `<li><strong>Completed:</strong> ${report.completedSessions || 0}</li>`;
      formatted += `<li><strong>Missed:</strong> ${report.missedSessions || 0}</li>`;
      if (report.patientsServed) formatted += `<li><strong>Patients Served:</strong> ${report.patientsServed}</li>`;
      if (report.uniquePatientsServed) formatted += `<li><strong>Unique Patients:</strong> ${report.uniquePatientsServed}</li>`;
      formatted += `</ul>`;
    }

    if (report.treatments && report.treatments.length > 0) {
      formatted += `<h3>Treatments</h3><p>Total treatments in period: ${report.treatments.length}</p>`;
    }

    if (!formatted) {
      formatted = `<p>${JSON.stringify(report, null, 2)}</p>`;
    }

    return formatted;
  }

  getSafeHtml(html: string): SafeHtml {
    return this.sanitizer.sanitize(1, html) || '';
  }

  convertMarkdownToHtml(markdown: string): string {
    let html = markdown;
    
    // Convert headers
    html = html.replace(/^### (.*$)/gim, '<h3>$1</h3>');
    html = html.replace(/^## (.*$)/gim, '<h2>$1</h2>');
    html = html.replace(/^# (.*$)/gim, '<h1>$1</h1>');
    
    // Convert bold
    html = html.replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>');
    
    // Convert bullet points
    html = html.replace(/^\* (.*$)/gim, '<li>$1</li>');
    html = html.replace(/^- (.*$)/gim, '<li>$1</li>');
    
    // Wrap lists
    html = html.replace(/(<li>.*<\/li>)/s, '<ul>$1</ul>');
    
    // Convert line breaks
    html = html.replace(/\n\n/g, '</p><p>');
    html = html.replace(/\n/g, '<br>');
    
    // Wrap in paragraph if not already wrapped
    if (!html.startsWith('<')) {
      html = '<p>' + html + '</p>';
    }
    
    return html;
  }

  exportToPdf() {
    if (!this.generatedReport) return;

    this.loading = true;
    this.aiService.exportReportToPdf(this.generatedReport, this.reportTitle).subscribe({
      next: (blob: Blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `${this.reportTitle.replace(/\s+/g, '_')}.pdf`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
        this.loading = false;
      },
      error: (err: any) => {
        console.error('Error exporting PDF:', err);
        alert('Failed to export PDF');
        this.loading = false;
      }
    });
  }

  printReport() {
    if (!this.generatedReport) return;
    window.print();
  }
}
