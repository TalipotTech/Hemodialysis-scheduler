import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { FormsModule } from '@angular/forms';
import { ScheduleService } from '../../../core/services/schedule.service';
import { Router } from '@angular/router';

interface BedConflict {
  scheduleId: number;
  patientId: number;
  patientName: string;
  sessionDate: Date;
  slotId: number;
  slotName: string;
  bedNumber: number | null;
  conflictType: string;
  conflictDetails: string;
}

@Component({
  selector: 'app-bed-conflict-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    MatProgressSpinnerModule,
    MatChipsModule,
    MatTooltipModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatFormFieldModule,
    MatInputModule
  ],
  templateUrl: './bed-conflict-dashboard.component.html',
  styleUrls: ['./bed-conflict-dashboard.component.scss']
})
export class BedConflictDashboardComponent implements OnInit {
  conflicts: BedConflict[] = [];
  loading = false;
  errorMessage = '';
  
  startDate: Date = new Date();
  endDate: Date = new Date();
  
  displayedColumns: string[] = ['conflictType', 'sessionDate', 'patientName', 'slot', 'bed', 'details', 'actions'];

  constructor(
    private scheduleService: ScheduleService,
    private router: Router
  ) {
    // Default: scan last 7 days and next 7 days
    this.startDate.setDate(this.startDate.getDate() - 7);
    this.endDate.setDate(this.endDate.getDate() + 7);
  }

  ngOnInit(): void {
    this.scanConflicts();
  }

  scanConflicts(): void {
    this.loading = true;
    this.errorMessage = '';
    
    this.scheduleService.getBedConflicts(this.startDate, this.endDate).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.conflicts = response.data;
          console.log(`🔍 Found ${this.conflicts.length} bed conflicts`);
        } else {
          this.errorMessage = response.message || 'Failed to load conflicts';
        }
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading bed conflicts:', error);
        this.errorMessage = error.error?.message || 'Error loading conflicts';
        this.loading = false;
      }
    });
  }

  getConflictTypeClass(type: string): string {
    switch (type) {
      case 'DOUBLE_BOOKING':
        return 'conflict-critical';
      case 'MISSING_BED':
        return 'conflict-warning';
      default:
        return 'conflict-info';
    }
  }

  getConflictTypeIcon(type: string): string {
    switch (type) {
      case 'DOUBLE_BOOKING':
        return 'error';
      case 'MISSING_BED':
        return 'warning';
      default:
        return 'info';
    }
  }

  getConflictTypeLabel(type: string): string {
    switch (type) {
      case 'DOUBLE_BOOKING':
        return 'Double Booking';
      case 'MISSING_BED':
        return 'Missing Bed';
      default:
        return type;
    }
  }

  viewSchedule(scheduleId: number): void {
    this.router.navigate(['/schedule/hd-session/edit', scheduleId]);
  }

  formatDate(date: Date | string): string {
    const d = new Date(date);
    return d.toLocaleDateString('en-US', { 
      weekday: 'short', 
      year: 'numeric', 
      month: 'short', 
      day: 'numeric' 
    });
  }

  getBedDisplay(bedNumber: number | null): string {
    return bedNumber ? `Bed ${bedNumber}` : 'Not Assigned';
  }

  refreshConflicts(): void {
    this.scanConflicts();
  }

  exportConflicts(): void {
    // Export to CSV
    const csvContent = this.conflicts.map(c => 
      `${c.conflictType},${this.formatDate(c.sessionDate)},${c.patientName},${c.slotName},${c.bedNumber || 'N/A'},"${c.conflictDetails}"`
    ).join('\n');
    
    const header = 'Conflict Type,Date,Patient,Slot,Bed,Details\n';
    const blob = new Blob([header + csvContent], { type: 'text/csv' });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `bed-conflicts-${new Date().toISOString().split('T')[0]}.csv`;
    a.click();
    window.URL.revokeObjectURL(url);
  }

  getDoubleBookingCount(): number {
    return this.conflicts.filter(c => c.conflictType === 'DOUBLE_BOOKING').length;
  }

  getMissingBedCount(): number {
    return this.conflicts.filter(c => c.conflictType === 'MISSING_BED').length;
  }
}
