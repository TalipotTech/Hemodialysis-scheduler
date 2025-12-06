import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule, Location } from '@angular/common';
import { Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTableModule } from '@angular/material/table';
import { MatChipsModule } from '@angular/material/chips';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { FormsModule } from '@angular/forms';
import { ScheduleService } from '../../../core/services/schedule.service';
import { DailyScheduleResponse, SlotSchedule, BedStatus } from '../../../core/models/schedule.model';

@Component({
  selector: 'app-schedule-grid',
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatFormFieldModule,
    MatInputModule,
    MatTooltipModule,
    MatCheckboxModule,
    MatSnackBarModule,
    MatTabsModule,
    MatTableModule,
    MatChipsModule,
    MatSlideToggleModule
  ],
  templateUrl: './schedule-grid.html',
  styleUrl: './schedule-grid.scss',
})
export class ScheduleGrid implements OnInit, OnDestroy {
  selectedDate: Date = new Date();
  schedule: DailyScheduleResponse | null = null;
  loading = false;
  errorMessage = '';

  // Filter options
  showAvailable = true;
  showOccupied = true;
  showReserved = true;

  // Auto-refresh settings
  autoRefreshEnabled = false;
  private refreshInterval: any;
  private readonly REFRESH_INTERVAL_MS = 30000; // 30 seconds

  // Future scheduled sessions (Bed Schedule)
  futureScheduledSessions: any[] = [];
  loadingFuture = false;
  selectedTab = 0;

  // Columns for future schedule table
  futureScheduleColumns: string[] = ['sessionDate', 'patientName', 'age', 'slotName', 'bedNumber', 'hdCycle', 'status', 'actions'];

  constructor(
    private scheduleService: ScheduleService,
    private location: Location,
    private router: Router,
    private snackBar: MatSnackBar
  ) {}

  goBack(): void {
    this.location.back();
  }

  ngOnInit(): void {
    this.loadSchedule();
    this.loadFutureScheduledSessions();
    // Auto-refresh when page becomes visible again (e.g., after returning from discharge)
    document.addEventListener('visibilitychange', this.handleVisibilityChange.bind(this));
  }

  ngOnDestroy(): void {
    document.removeEventListener('visibilitychange', this.handleVisibilityChange.bind(this));
    this.stopAutoRefresh();
  }

  toggleAutoRefresh(): void {
    if (this.autoRefreshEnabled) {
      this.startAutoRefresh();
      this.snackBar.open('Auto-refresh enabled (every 30 seconds)', 'Close', { duration: 3000 });
    } else {
      this.stopAutoRefresh();
      this.snackBar.open('Auto-refresh disabled', 'Close', { duration: 3000 });
    }
  }

  startAutoRefresh(): void {
    this.stopAutoRefresh(); // Clear any existing interval
    this.refreshInterval = setInterval(() => {
      if (!document.hidden) {
        this.loadSchedule();
        this.loadFutureScheduledSessions();
      }
    }, this.REFRESH_INTERVAL_MS);
  }

  stopAutoRefresh(): void {
    if (this.refreshInterval) {
      clearInterval(this.refreshInterval);
      this.refreshInterval = null;
    }
  }

  handleVisibilityChange(): void {
    if (!document.hidden) {
      // Page is visible again, refresh the schedule
      this.loadSchedule();
      this.loadFutureScheduledSessions();
    }
  }

  loadSchedule(): void {
    this.loading = true;
    this.errorMessage = '';
    
    console.log('Loading schedule for date:', this.selectedDate);
    
    this.scheduleService.getDailySchedule(this.selectedDate).subscribe({
      next: (response: any) => {
        console.log('Schedule API response:', response);
        if (response.success && response.data) {
          this.schedule = response.data;
          console.log('Schedule data loaded:', this.schedule);
          
          // Debug: Log all beds with their statuses
          if (this.schedule && this.schedule.slots) {
            this.schedule.slots.forEach((slot: any) => {
              slot.beds.forEach((bed: any) => {
                if (bed.status !== 'available') {
                  console.log(`Slot ${slot.slotName}, Bed ${bed.bedNumber}: ${bed.status}, Patient: ${bed.patient?.name}`);
                }
              });
            });
          }
        } else {
          this.errorMessage = response.message || 'Failed to load schedule';
        }
        this.loading = false;
      },
      error: (error: any) => {
        console.error('Error loading schedule:', error);
        this.errorMessage = 'Failed to load schedule. Please try again.';
        this.loading = false;
      }
    });
  }

  onDateChange(): void {
    console.log('Date changed to:', this.selectedDate);
    this.loadSchedule();
  }

  goToPreviousDay(): void {
    const currentDate = new Date(this.selectedDate);
    currentDate.setDate(currentDate.getDate() - 1);
    this.selectedDate = currentDate;
    this.loadSchedule();
  }

  goToNextDay(): void {
    const currentDate = new Date(this.selectedDate);
    currentDate.setDate(currentDate.getDate() + 1);
    this.selectedDate = currentDate;
    this.loadSchedule();
  }

  getSlot(slotId: number): SlotSchedule | undefined {
    return this.schedule?.slots.find((s: SlotSchedule) => s.slotID === slotId);
  }

  getBed(slotId: number, bedNumber: number): BedStatus | undefined {
    const slot = this.getSlot(slotId);
    return slot?.beds.find((b: BedStatus) => b.bedNumber === bedNumber);
  }

  getBedsForSlot(slot: SlotSchedule): number[] {
    const maxBeds = slot.maxBeds || 10; // Default to 10 if not provided
    return Array.from({ length: maxBeds }, (_, i) => i + 1);
  }

  getSlotOccupied(slot: SlotSchedule): number {
    return slot.beds.filter(b => b.status === 'occupied' || b.status === 'pre-scheduled').length;
  }

  getSlotAvailable(slot: SlotSchedule): number {
    const maxBeds = slot.maxBeds || 10;
    return maxBeds - this.getSlotOccupied(slot);
  }

  getBedClass(slotId: number, bedNumber: number): string {
    const bed = this.getBed(slotId, bedNumber);
    if (!bed) return 'bed-empty';
    
    let className = '';
    switch (bed.status) {
      case 'occupied':
        className = 'bed-occupied';
        break;
      case 'pre-scheduled':
        className = 'bed-pre-scheduled';
        break;
      case 'completed':
        className = 'bed-completed';
        break;
      case 'reserved':
        className = 'bed-reserved';
        break;
      default:
        className = 'bed-empty';
    }
    
    // Apply filter - hide beds that don't match selected filters
    if (!this.shouldShowBed(bed.status)) {
      className += ' bed-filtered';
    }
    
    return className;
  }

  shouldShowBed(status: string): boolean {
    switch (status) {
      case 'available':
        return this.showAvailable;
      case 'occupied':
        return this.showOccupied;
      case 'pre-scheduled':
        return this.showOccupied; // Use same filter as occupied
      case 'completed':
        return this.showOccupied; // Show completed sessions with occupied filter
      case 'reserved':
        return this.showReserved;
      default:
        return true;
    }
  }

  onFilterChange(): void {
    // Filters are applied automatically through getBedClass method
    // This method can be used for additional actions if needed
  }

  getBedTooltip(slotId: number, bedNumber: number): string {
    const bed = this.getBed(slotId, bedNumber);
    if (!bed || bed.status === 'available') {
      return 'Available';
    }
    if (bed.patient) {
      const sessionInfo = this.getSessionInfo(bed);
      return `${bed.patient.name} (${bed.patient.age})\n${sessionInfo}`;
    }
    return bed.status;
  }

  getSessionInfo(bed: any): string {
    if (!bed.sessionDate) return 'N/A';
    
    const date = new Date(bed.sessionDate);
    const formattedDate = date.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
    
    if (bed.sessionNumber && bed.totalWeeklySessions) {
      return `Session ${bed.sessionNumber}/${bed.totalWeeklySessions} - ${formattedDate}`;
    }
    
    return formattedDate;
  }

  loadFutureScheduledSessions(): void {
    this.loadingFuture = true;
    this.scheduleService.getFutureScheduledSessions().subscribe({
      next: (response: any) => {
        if (response.success && response.data) {
          this.futureScheduledSessions = response.data;
          console.log('Future scheduled sessions:', this.futureScheduledSessions);
        }
        this.loadingFuture = false;
      },
      error: (error: any) => {
        console.error('Error loading future scheduled sessions:', error);
        this.loadingFuture = false;
      }
    });
  }

  onRefresh(): void {
    if (this.selectedTab === 0) {
      this.loadSchedule();
    } else {
      this.loadFutureScheduledSessions();
    }
  }

  onTabChange(index: number): void {
    this.selectedTab = index;
  }

  getSlotName(slotId: number): string {
    const slotNames: { [key: number]: string } = {
      1: 'Morning (6:00 AM - 10:00 AM)',
      2: 'Afternoon (11:00 AM - 3:00 PM)',
      3: 'Evening (4:00 PM - 8:00 PM)',
      4: 'Night (9:00 PM - 1:00 AM)'
    };
    return slotNames[slotId] || 'Unknown Slot';
  }

  formatDate(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', { weekday: 'short', year: 'numeric', month: 'short', day: 'numeric' });
  }

  viewSessionDetails(scheduleId: number): void {
    this.router.navigate(['/schedule/hd-session/edit', scheduleId]);
  }

  getTotalOccupied(): number {
    if (!this.schedule) return 0;
    return this.schedule.slots.reduce((total: number, slot: SlotSchedule) => {
      return total + slot.beds.filter((b: BedStatus) => b.status === 'occupied' || b.status === 'pre-scheduled').length;
    }, 0);
  }

  getTotalPreScheduled(): number {
    if (!this.schedule) return 0;
    return this.schedule.slots.reduce((total: number, slot: SlotSchedule) => {
      return total + slot.beds.filter((b: BedStatus) => b.status === 'pre-scheduled').length;
    }, 0);
  }

  getTotalCapacity(): number {
    if (!this.schedule) return 0;
    return this.schedule.slots.length * 10;
  }

  getOccupancyRate(): number {
    const total = this.getTotalCapacity();
    const occupied = this.getTotalOccupied();
    return total > 0 ? Math.round((occupied / total) * 100) : 0;
  }

  onBedClick(slotId: number, bedNumber: number): void {
    const bed = this.getBed(slotId, bedNumber);
    
    console.log('Bed clicked:', { slotId, bedNumber, bed });
    
    if (!bed) {
      this.snackBar.open('Bed information not available', 'Close', { duration: 3000 });
      return;
    }

    if ((bed.status === 'occupied' || bed.status === 'pre-scheduled' || bed.status === 'completed') && bed.patient && bed.scheduleId) {
      // Navigate to HD session form in EDIT mode - shows all fields (filled and empty)
      // For completed sessions, staff can view the historical data
      console.log('Navigating to edit session:', bed.scheduleId);
      this.router.navigate(['/schedule/hd-session/edit', bed.scheduleId]);
    } else if (bed.status === 'available') {
      this.snackBar.open('This bed is available. Please select a patient first.', 'Close', { duration: 3000 });
    } else {
      // For any other status, show a generic message
      this.snackBar.open(`Bed is ${bed.status}. No action available.`, 'Close', { duration: 3000 });
    }
  }

  navigateToWorkflow(patientId: number, scheduleId: number): void {
    if (!patientId || !scheduleId) {
      this.snackBar.open('Session information incomplete', 'Close', { duration: 3000 });
      return;
    }
    this.router.navigate(['/patients', patientId, 'workflow', scheduleId]);
  }
}
