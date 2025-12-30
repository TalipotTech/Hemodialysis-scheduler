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
import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';

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
    if (!this.sessionData || !this.sessionData.sessionInfo) {
      this.snackBar.open('No session data available to print', 'Close', { duration: 3000 });
      return;
    }

    try {
      this.generateSessionPDF();
      this.snackBar.open('PDF generated successfully!', 'Close', { duration: 3000 });
    } catch (error) {
      console.error('Error generating PDF:', error);
      this.snackBar.open('Failed to generate PDF report', 'Close', { duration: 3000 });
    }
  }

  private generateSessionPDF(): void {
    const doc = new jsPDF();
    const session = this.sessionData.sessionInfo;
    let yPos = 15;

    // Helper function to get value or 'N/A'
    const getValue = (value: any, suffix: string = ''): string => {
      if (value === null || value === undefined || value === '') return 'N/A';
      return value + suffix;
    };

    // Title
    doc.setFontSize(18);
    doc.setFont('helvetica', 'bold');
    doc.text('HEMODIALYSIS SESSION REPORT', 105, yPos, { align: 'center' });
    
    yPos += 10;
    doc.setFontSize(10);
    doc.setFont('helvetica', 'normal');
    doc.text(`Generated: ${new Date().toLocaleDateString()} ${new Date().toLocaleTimeString()}`, 105, yPos, { align: 'center' });
    
    yPos += 10;
    doc.setLineWidth(0.5);
    doc.line(15, yPos, 195, yPos);
    yPos += 8;

    // Patient Information Section
    doc.setFontSize(12);
    doc.setFont('helvetica', 'bold');
    doc.text('PATIENT INFORMATION', 15, yPos);
    yPos += 5;

    autoTable(doc, {
      startY: yPos,
      head: [],
      body: [
        ['Patient Name', getValue(session.patientName || session.PatientName), 'Patient ID', getValue(session.patientID || session.PatientID)],
        ['MRN', getValue(session.mrn || session.MRN), 'Age', getValue(session.age || session.Age, ' years')],
        ['Gender', getValue(session.gender || session.Gender), 'Contact Number', getValue(session.contactNumber || session.ContactNumber)],
        ['Dry Weight', getValue(session.patientDryWeight || session.PatientDryWeight || session.dryWeight || session.DryWeight, ' kg'), 'HD Cycle', getValue(session.patientHDCycle || session.PatientHDCycle || session.hdCycle || session.HDCycle)],
        ['Sessions Per Week', getValue(session.hdFrequency || session.HDFrequency || session.sessionsPerWeek || session.SessionsPerWeek), 'HD Start Date', getValue(session.patientHDStartDate || session.PatientHDStartDate || session.hdStartDate || session.HDStartDate)],
        ['Guardian', getValue(session.guardianName || session.GuardianName), 'Emergency Contact', getValue(session.emergencyContact || session.EmergencyContact)]
      ],
      theme: 'grid',
      headStyles: { fillColor: [41, 128, 185], textColor: 255, fontStyle: 'bold' },
      styles: { fontSize: 9, cellPadding: 3 },
      columnStyles: {
        0: { fontStyle: 'bold', fillColor: [240, 240, 240], cellWidth: 45 },
        1: { cellWidth: 50 },
        2: { fontStyle: 'bold', fillColor: [240, 240, 240], cellWidth: 45 },
        3: { cellWidth: 50 }
      }
    });

    yPos = (doc as any).lastAutoTable.finalY + 10;

    // HD Prescription & Treatment Plan
    doc.setFontSize(12);
    doc.setFont('helvetica', 'bold');
    doc.text('HD PRESCRIPTION & TREATMENT PLAN', 15, yPos);
    yPos += 5;

    autoTable(doc, {
      startY: yPos,
      head: [],
      body: [
        ['Treatment Date', getValue(session.sessionDate || session.SessionDate), 'Time Slot', getValue(session.slotName || session.SlotName)],
        ['Bed Number', getValue(session.bedNumber || session.BedNumber), 'Dialyser Type', getValue(session.patientDialyserType || session.PatientDialyserType || session.dialyserType || session.DialyserType)],
        ['Dialyser Model', getValue(session.patientDialyserModel || session.PatientDialyserModel || session.dialyserModel || session.DialyserModel), 'Prescribed BFR', getValue(session.patientPrescribedBFR || session.PatientPrescribedBFR || session.prescribedBFR || session.PrescribedBFR, ' mL/min')],
        ['Prescribed Duration', getValue(session.patientPrescribedDuration || session.PatientPrescribedDuration || session.prescribedDuration || session.PrescribedDuration, ' hours'), 'Dialysate', getValue(session.patientDialysatePrescription || session.PatientDialysatePrescription || session.dialysatePrescription || session.DialysatePrescription || session.dialysate || session.Dialysate)]
      ],
      theme: 'grid',
      styles: { fontSize: 9, cellPadding: 3 },
      columnStyles: {
        0: { fontStyle: 'bold', fillColor: [240, 240, 240], cellWidth: 45 },
        1: { cellWidth: 50 },
        2: { fontStyle: 'bold', fillColor: [240, 240, 240], cellWidth: 45 },
        3: { cellWidth: 50 }
      }
    });

    yPos = (doc as any).lastAutoTable.finalY + 10;

    // Treatment Session Details (if available)
    if (session.startTime || session.preWeight || session.ufGoal || session.bloodPressure || session.preBPSitting) {
      doc.setFontSize(12);
      doc.setFont('helvetica', 'bold');
      doc.text('TREATMENT SESSION DETAILS', 15, yPos);
      yPos += 5;

      const treatmentDetails: any[] = [];
      if (session.startTime || session.preWeight) {
        treatmentDetails.push(['Start Time', getValue(session.startTime), 'Pre-Weight', getValue(session.preWeight, ' kg')]);
      }
      if (session.ufGoal || session.bloodPressure || session.preBPSitting) {
        treatmentDetails.push(['UF Goal', getValue(session.ufGoal, ' L'), 'Pre-BP Sitting', getValue(session.bloodPressure || session.preBPSitting || session.PreBPSitting)]);
      }
      if (session.preTemperature || session.bloodTestDone !== undefined) {
        treatmentDetails.push(['Pre-Temperature', getValue(session.preTemperature || session.PreTemperature, '°C'), 'Blood Test Done', session.bloodTestDone || session.BloodTestDone ? 'Yes' : 'No']);
      }
      if (session.symptoms) {
        treatmentDetails.push([{content: 'Symptoms', colSpan: 1, styles: {fontStyle: 'bold'}}, {content: getValue(session.symptoms || session.Symptoms), colSpan: 3}]);
      }
      if (session.complications) {
        treatmentDetails.push([{content: 'Complications', colSpan: 1, styles: {fontStyle: 'bold'}}, {content: getValue(session.complications || session.Complications), colSpan: 3}]);
      }

      if (treatmentDetails.length > 0) {
        autoTable(doc, {
          startY: yPos,
          head: [],
          body: treatmentDetails,
          theme: 'grid',
          styles: { fontSize: 9, cellPadding: 3 },
          columnStyles: {
            0: { fontStyle: 'bold', fillColor: [240, 240, 240], cellWidth: 45 },
            1: { cellWidth: 50 },
            2: { fontStyle: 'bold', fillColor: [240, 240, 240], cellWidth: 45 },
            3: { cellWidth: 50 }
          }
        });

        yPos = (doc as any).lastAutoTable.finalY + 10;
      }
    }

    // Vascular Access Information (if available)
    if (session.accessType || session.accessLocation || session.accessBleedingTime || session.accessStatus || session.postAccessStatus) {
      if (yPos > 250) {
        doc.addPage();
        yPos = 15;
      }

      doc.setFontSize(12);
      doc.setFont('helvetica', 'bold');
      doc.text('VASCULAR ACCESS INFORMATION', 15, yPos);
      yPos += 5;

      const accessDetails: any[] = [];
      if (session.accessType || session.accessLocation) {
        accessDetails.push(['Access Type', getValue(session.accessType), 'Access Location', getValue(session.accessLocation)]);
      }
      if (session.accessBleedingTime || session.accessStatus) {
        accessDetails.push(['Access Bleeding Time', getValue(session.accessBleedingTime || session.AccessBleedingTime, ' min'), 'Pre-Treatment Status', getValue(session.accessStatus || session.AccessStatus)]);
      }
      if (session.postAccessStatus) {
        accessDetails.push([{content: 'Post-Treatment Status', colSpan: 1, styles: {fontStyle: 'bold'}}, {content: getValue(session.postAccessStatus || session.PostAccessStatus), colSpan: 3}]);
      }

      if (accessDetails.length > 0) {
        autoTable(doc, {
          startY: yPos,
          head: [],
          body: accessDetails,
          theme: 'grid',
          styles: { fontSize: 9, cellPadding: 3 },
          columnStyles: {
            0: { fontStyle: 'bold', fillColor: [240, 240, 240], cellWidth: 45 },
            1: { cellWidth: 50 },
            2: { fontStyle: 'bold', fillColor: [240, 240, 240], cellWidth: 45 },
            3: { cellWidth: 50 }
          }
        });

        yPos = (doc as any).lastAutoTable.finalY + 10;
      }
    }

    // Anticoagulation (if available)
    if (session.anticoagulationType || session.syringeType || session.bolusDose || session.heparinDose || session.heparinBolusDose || session.heparinInfusionRate) {
      if (yPos > 250) {
        doc.addPage();
        yPos = 15;
      }

      doc.setFontSize(12);
      doc.setFont('helvetica', 'bold');
      doc.text('ANTICOAGULATION', 15, yPos);
      yPos += 5;

      const anticoagDetails: any[] = [];
      if (session.anticoagulationType || session.syringeType) {
        anticoagDetails.push(['Anticoagulation Type', getValue(session.anticoagulationType), 'Syringe Type', getValue(session.syringeType)]);
      }
      if (session.heparinDose || session.bolusDose || session.heparinBolusDose) {
        anticoagDetails.push(['Heparin Dose', getValue(session.heparinDose || session.HeparinDose, ' units'), 'Bolus Dose', getValue(session.bolusDose || session.heparinBolusDose || session.HeparinBolusDose, ' mL')]);
      }
      if (session.heparinInfusionRate) {
        anticoagDetails.push([{content: 'Heparin Infusion Rate', colSpan: 1, styles: {fontStyle: 'bold'}}, {content: getValue(session.heparinInfusionRate || session.HeparinInfusionRate, ' mL/hr'), colSpan: 3}]);
      }

      if (anticoagDetails.length > 0) {
        autoTable(doc, {
          startY: yPos,
          head: [],
          body: anticoagDetails,
          theme: 'grid',
          styles: { fontSize: 9, cellPadding: 3 },
          columnStyles: {
            0: { fontStyle: 'bold', fillColor: [240, 240, 240], cellWidth: 45 },
            1: { cellWidth: 50 },
            2: { fontStyle: 'bold', fillColor: [240, 240, 240], cellWidth: 45 },
            3: { cellWidth: 50 }
          }
        });

        yPos = (doc as any).lastAutoTable.finalY + 10;
      }
    }

    // Intra-Dialytic Monitoring Records
    if (this.sessionData.intraDialyticRecords && this.sessionData.intraDialyticRecords.length > 0) {
      if (yPos > 200) {
        doc.addPage();
        yPos = 15;
      }

      doc.setFontSize(12);
      doc.setFont('helvetica', 'bold');
      doc.text('INTRA-DIALYTIC MONITORING RECORDS', 15, yPos);
      yPos += 5;

      const monitoringData: any[] = [];
      this.sessionData.intraDialyticRecords.forEach((record: any, index: number) => {
        monitoringData.push([
          `#${index + 1}`,
          getValue(record.timeRecorded || record.TimeRecorded),
          getValue(record.bloodPressure || record.BloodPressure),
          getValue(record.pulseRate || record.PulseRate || record.heartRate || record.HeartRate, ' bpm'),
          getValue(record.temperature || record.Temperature, '°C'),
          getValue(record.ufVolume || record.UFVolume, ' L')
        ]);
      });

      autoTable(doc, {
        startY: yPos,
        head: [['#', 'Time', 'BP', 'HR', 'Temp', 'UF Vol']],
        body: monitoringData,
        theme: 'striped',
        headStyles: { fillColor: [52, 152, 219], textColor: 255, fontStyle: 'bold', fontSize: 9 },
        styles: { fontSize: 8, cellPadding: 2 },
        columnStyles: {
          0: { cellWidth: 15 },
          1: { cellWidth: 35 },
          2: { cellWidth: 30 },
          3: { cellWidth: 25 },
          4: { cellWidth: 25 },
          5: { cellWidth: 25 }
        }
      });

      yPos = (doc as any).lastAutoTable.finalY + 5;

      // Detailed monitoring records
      this.sessionData.intraDialyticRecords.forEach((record: any, index: number) => {
        if (yPos > 240) {
          doc.addPage();
          yPos = 15;
        }

        doc.setFontSize(10);
        doc.setFont('helvetica', 'bold');
        doc.text(`Reading #${index + 1} Details`, 15, yPos);
        yPos += 3;

        const recordDetails: any[] = [
          ['Venous Pressure', getValue(record.venousPressure || record.VenousPressure, ' mmHg'), 'Arterial Pressure', getValue(record.arterialPressure || record.ArterialPressure, ' mmHg')],
          ['Blood Flow Rate', getValue(record.bloodFlowRate || record.BloodFlowRate, ' mL/min'), 'Current UFR', getValue(record.currentUFR || record.CurrentUFR, ' mL/hr')],
          ['TMP Pressure', getValue(record.tmpPressure || record.TMPPressure, ' mmHg'), 'Staff Initials', getValue(record.staffInitials || record.StaffInitials)]
        ];

        if (record.symptoms || record.Symptoms) {
          recordDetails.push([{content: 'Symptoms', colSpan: 1, styles: {fontStyle: 'bold'}}, {content: getValue(record.symptoms || record.Symptoms), colSpan: 3}]);
        }
        if (record.interventions || record.Interventions) {
          recordDetails.push([{content: 'Interventions', colSpan: 1, styles: {fontStyle: 'bold'}}, {content: getValue(record.interventions || record.Interventions), colSpan: 3}]);
        }
        if (record.notes || record.Notes) {
          recordDetails.push([{content: 'Notes', colSpan: 1, styles: {fontStyle: 'bold'}}, {content: getValue(record.notes || record.Notes), colSpan: 3}]);
        }
        if (record.recordedBy || record.RecordedBy) {
          recordDetails.push([{content: 'Recorded By', colSpan: 1, styles: {fontStyle: 'bold'}}, {content: getValue(record.recordedBy || record.RecordedBy), colSpan: 3}]);
        }

        autoTable(doc, {
          startY: yPos,
          head: [],
          body: recordDetails,
          theme: 'grid',
          styles: { fontSize: 8, cellPadding: 2 },
          columnStyles: {
            0: { fontStyle: 'bold', fillColor: [240, 240, 240], cellWidth: 45 },
            1: { cellWidth: 50 },
            2: { fontStyle: 'bold', fillColor: [240, 240, 240], cellWidth: 45 },
            3: { cellWidth: 50 }
          }
        });

        yPos = (doc as any).lastAutoTable.finalY + 8;
      });
    }

    // Post-Dialysis Medications
    if (session.medicationName || session.medicationType) {
      if (yPos > 250) {
        doc.addPage();
        yPos = 15;
      }

      doc.setFontSize(12);
      doc.setFont('helvetica', 'bold');
      doc.text('POST-DIALYSIS MEDICATIONS', 15, yPos);
      yPos += 5;

      autoTable(doc, {
        startY: yPos,
        head: [],
        body: [
          ['Medication Type', getValue(session.medicationType), 'Medication Name', getValue(session.medicationName)],
          ['Dose', getValue(session.dose), 'Route', getValue(session.route)],
          [{content: 'Administered At', colSpan: 1, styles: {fontStyle: 'bold'}}, {content: getValue(session.administeredAt), colSpan: 3}]
        ],
        theme: 'grid',
        headStyles: { fillColor: [46, 204, 113] },
        styles: { fontSize: 9, cellPadding: 3 },
        columnStyles: {
          0: { fontStyle: 'bold', fillColor: [240, 240, 240], cellWidth: 45 },
          1: { cellWidth: 50 },
          2: { fontStyle: 'bold', fillColor: [240, 240, 240], cellWidth: 45 },
          3: { cellWidth: 50 }
        }
      });

      yPos = (doc as any).lastAutoTable.finalY + 10;
    }

    // Treatment Alerts
    if (session.alertType || session.alertMessage) {
      if (yPos > 250) {
        doc.addPage();
        yPos = 15;
      }

      doc.setFontSize(12);
      doc.setFont('helvetica', 'bold');
      doc.text('TREATMENT ALERTS', 15, yPos);
      yPos += 5;

      const alertDetails: any[] = [
        ['Alert Type', getValue(session.alertType), 'Severity', getValue(session.severity)]
      ];
      if (session.alertMessage) {
        alertDetails.push([{content: 'Alert Message', colSpan: 1, styles: {fontStyle: 'bold'}}, {content: getValue(session.alertMessage), colSpan: 3}]);
      }
      if (session.resolution) {
        alertDetails.push([{content: 'Resolution', colSpan: 1, styles: {fontStyle: 'bold'}}, {content: getValue(session.resolution), colSpan: 3}]);
      }

      autoTable(doc, {
        startY: yPos,
        head: [],
        body: alertDetails,
        theme: 'grid',
        headStyles: { fillColor: [231, 76, 60] },
        styles: { fontSize: 9, cellPadding: 3 },
        columnStyles: {
          0: { fontStyle: 'bold', fillColor: [255, 243, 224], cellWidth: 45 },
          1: { cellWidth: 50 },
          2: { fontStyle: 'bold', fillColor: [255, 243, 224], cellWidth: 45 },
          3: { cellWidth: 50 }
        }
      });

      yPos = (doc as any).lastAutoTable.finalY + 10;
    }

    // Staff Assignment
    if (yPos > 260) {
      doc.addPage();
      yPos = 15;
    }

    doc.setFontSize(12);
    doc.setFont('helvetica', 'bold');
    doc.text('ASSIGNED STAFF', 15, yPos);
    yPos += 5;

    autoTable(doc, {
      startY: yPos,
      head: [],
      body: [
        ['Doctor', getValue(session.assignedDoctorName || 'Not Assigned'), 'Nurse', getValue(session.assignedNurseName || 'Not Assigned')]
      ],
      theme: 'grid',
      styles: { fontSize: 9, cellPadding: 3 },
      columnStyles: {
        0: { fontStyle: 'bold', fillColor: [240, 240, 240], cellWidth: 45 },
        1: { cellWidth: 50 },
        2: { fontStyle: 'bold', fillColor: [240, 240, 240], cellWidth: 45 },
        3: { cellWidth: 50 }
      }
    });

    yPos = (doc as any).lastAutoTable.finalY + 10;

    // Post-Dialysis Assessment
    if (session.postWeight || session.postSBP || session.postDBP || session.postHR || session.totalFluidRemoved || session.postAccessStatus) {
      if (yPos > 250) {
        doc.addPage();
        yPos = 15;
      }

      doc.setFontSize(12);
      doc.setFont('helvetica', 'bold');
      doc.text('POST-DIALYSIS ASSESSMENT', 15, yPos);
      yPos += 5;

      const postDialysisDetails: any[] = [];
      if (session.postWeight || (session.preWeight && session.postWeight)) {
        const weightLoss = (session.preWeight && session.postWeight) ? (session.preWeight - session.postWeight).toFixed(1) : 'N/A';
        postDialysisDetails.push(['Post-Weight', getValue(session.postWeight, ' kg'), 'Weight Loss', weightLoss + (weightLoss !== 'N/A' ? ' kg' : '')]);
      }
      if (session.postSBP || session.postDBP) {
        postDialysisDetails.push(['Post-SBP', getValue(session.postSBP, ' mmHg'), 'Post-DBP', getValue(session.postDBP, ' mmHg')]);
      }
      if (session.postHR || session.totalFluidRemoved) {
        postDialysisDetails.push(['Post-HR', getValue(session.postHR, ' bpm'), 'Total Fluid Removed', getValue(session.totalFluidRemoved || session.TotalFluidRemoved, ' L')]);
      }
      if (session.postAccessStatus) {
        postDialysisDetails.push([{content: 'Post-Access Status', colSpan: 1, styles: {fontStyle: 'bold'}}, {content: getValue(session.postAccessStatus || session.PostAccessStatus), colSpan: 3}]);
      }
      if (session.notes) {
        postDialysisDetails.push([{content: 'Notes', colSpan: 1, styles: {fontStyle: 'bold'}}, {content: getValue(session.notes), colSpan: 3}]);
      }

      if (postDialysisDetails.length > 0) {
        autoTable(doc, {
          startY: yPos,
          head: [],
          body: postDialysisDetails,
          theme: 'grid',
          styles: { fontSize: 9, cellPadding: 3 },
          columnStyles: {
            0: { fontStyle: 'bold', fillColor: [240, 240, 240], cellWidth: 45 },
            1: { cellWidth: 50 },
            2: { fontStyle: 'bold', fillColor: [240, 240, 240], cellWidth: 45 },
            3: { cellWidth: 50 }
          }
        });

        yPos = (doc as any).lastAutoTable.finalY + 10;
      }
    }

    // Footer on last page
    const pageCount = doc.getNumberOfPages();
    for (let i = 1; i <= pageCount; i++) {
      doc.setPage(i);
      doc.setFontSize(8);
      doc.setFont('helvetica', 'italic');
      doc.text(`HD Scheduler System | Page ${i} of ${pageCount}`, 105, 285, { align: 'center' });
      doc.line(15, 280, 195, 280);
    }

    // Save PDF
    const patientName = (session.patientName || session.PatientName || 'Patient').replace(/\s+/g, '_');
    const sessionDate = (session.sessionDate || session.SessionDate || new Date().toISOString().split('T')[0]).split('T')[0];
    doc.save(`HD_Session_${patientName}_${sessionDate}.pdf`);
  }
}
