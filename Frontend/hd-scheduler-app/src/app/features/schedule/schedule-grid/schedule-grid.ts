import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule, Location } from '@angular/common';
import { Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';
import { MatIconModule } from '@angular/material/icon';
import { FormsModule } from '@angular/forms';
// Syncfusion imports
import { GridModule, PageService, SortService, FilterService, ToolbarService } from '@syncfusion/ej2-angular-grids';
import { DatePickerModule } from '@syncfusion/ej2-angular-calendars';
import { ButtonModule, ChipListModule, SwitchModule } from '@syncfusion/ej2-angular-buttons';
import { ToastModule, ToastUtility } from '@syncfusion/ej2-angular-notifications';
import { TabModule } from '@syncfusion/ej2-angular-navigations';
import { TooltipModule } from '@syncfusion/ej2-angular-popups';
import { ScheduleService } from '../../../core/services/schedule.service';
import { ReservationService } from '../../../core/services/reservation.service';
import { DailyScheduleResponse, SlotSchedule, BedStatus } from '../../../core/models/schedule.model';
import { ApiResponse } from '../../../core/models/user.model';
import { BedFormatterService } from '../../../services/bed-formatter.service';

@Component({
  selector: 'app-schedule-grid',
  imports: [
    CommonModule,
    FormsModule,
    MatIconModule, // Keep Material icons for now
    GridModule,
    DatePickerModule,
    ButtonModule,
    ChipListModule,
    SwitchModule,
    ToastModule,
    TabModule,
    TooltipModule
  ],
  providers: [PageService, SortService, FilterService, ToolbarService],
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
  showCompleted = true;
  showDischarged = false;

  // Search and filter
  searchText = '';
  filterHDCycle = '';
  currentTime = new Date();

  // Auto-refresh settings
  autoRefreshEnabled = false;
  private refreshInterval: any;
  private readonly REFRESH_INTERVAL_MS = 300000; // 5 minutes (300 seconds)
  private timeUpdateInterval: any;

  // Future scheduled sessions (Bed Schedule)
  futureSessions: any[] = [];
  loadingFuture = false;
  selectedTab = 0;
  private routerSubscription: any;

  constructor(
    private scheduleService: ScheduleService,
    private reservationService: ReservationService,
    private bedFormatter: BedFormatterService,
    private location: Location,
    private router: Router
  ) {
    // Listen for navigation events to refresh when returning from edit page
    this.routerSubscription = this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe((event: NavigationEnd) => {
        // If navigating to /schedule (this component), refresh the data
        if (event.url === '/schedule' || event.url.startsWith('/schedule?')) {
          setTimeout(() => {
            this.loadSchedule();
            this.loadFutureScheduledSessions();
          }, 100);
        }
      });
  }

  goBack(): void {
    this.location.back();
  }

  ngOnInit(): void {
    this.loadSchedule();
    this.loadFutureScheduledSessions();
    // Auto-refresh when page becomes visible again (e.g., after returning from discharge)
    document.addEventListener('visibilitychange', this.handleVisibilityChange.bind(this));
    // Update current time every minute
    this.startTimeUpdate();
  }

  ngOnDestroy(): void {
    document.removeEventListener('visibilitychange', this.handleVisibilityChange.bind(this));
    this.stopAutoRefresh();
    this.stopTimeUpdate();
    if (this.routerSubscription) {
      this.routerSubscription.unsubscribe();
    }
  }

  startTimeUpdate(): void {
    this.timeUpdateInterval = setInterval(() => {
      this.currentTime = new Date();
    }, 60000); // Update every minute
  }

  stopTimeUpdate(): void {
    if (this.timeUpdateInterval) {
      clearInterval(this.timeUpdateInterval);
      this.timeUpdateInterval = null;
    }
  }

  toggleAutoRefresh(): void {
    if (this.autoRefreshEnabled) {
      this.startAutoRefresh();
      this.showToast('Auto-refresh enabled (every 5 minutes)', 'Information');
    } else {
      this.stopAutoRefresh();
      this.showToast('Auto-refresh disabled', 'Information');
    }
  }
  
  private showToast(message: string, title: string = 'Notification'): void {
    ToastUtility.show({
      title: title,
      content: message,
      position: { X: 'Right', Y: 'Top' },
      showCloseButton: true,
      timeOut: 3000
    });
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
    
    this.scheduleService.getDailySchedule(this.selectedDate).subscribe({
      next: (response: any) => {
        if (response.success && response.data) {
          this.schedule = response.data;
        } else {
          this.errorMessage = response.message || 'Failed to load schedule';
          console.error('❌ Schedule load failed:', this.errorMessage);
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
    
    // Check if this is an auto-generated suggested session (scheduleId === 0)
    // Treat it as pre-scheduled (purple) since it's already scheduled, just needs activation
    if (bed.scheduleId === 0) {
      let className = 'bed-pre-scheduled'; // Purple - auto-generated, ready to activate
      if (!this.shouldShowBed('pre-scheduled')) {
        className += ' bed-filtered';
      }
      return className;
    }
    
    let className = '';
    switch (bed.status) {
      case 'occupied':
        className = 'bed-occupied';
        break;
      case 'pre-scheduled':
        className = 'bed-pre-scheduled'; // Purple - confirmed future session
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

  // Format bed number using the configured naming pattern
  formatBedNumber(bedNumber: number): string {
    return this.bedFormatter.formatBedNumber(bedNumber);
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
          this.futureSessions = response.data;
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

  formatDate(dateString: string | Date): string {
    const date = typeof dateString === 'string' ? new Date(dateString) : dateString;
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
    // Count ALL pre-scheduled sessions including auto-suggested (scheduleId === 0)
    return this.schedule.slots.reduce((total: number, slot: SlotSchedule) => {
      return total + slot.beds.filter((b: BedStatus) => 
        b.status === 'pre-scheduled'
      ).length;
    }, 0);
  }

  getTotalCompleted(): number {
    if (!this.schedule) return 0;
    return this.schedule.slots.reduce((total: number, slot: SlotSchedule) => {
      return total + slot.beds.filter((b: BedStatus) => b.status === 'completed').length;
    }, 0);
  }

  getTotalBedRelease(): number {
    if (!this.schedule) return 0;
    // Beds that need to be released (completed but not yet moved to history)
    return this.schedule.slots.reduce((total: number, slot: SlotSchedule) => {
      return total + slot.beds.filter((b: BedStatus) => 
        b.status === 'completed' && !b.patient?.isDischarged
      ).length;
    }, 0);
  }

  getTotalAvailable(): number {
    if (!this.schedule) return 0;
    return this.getTotalCapacity() - this.getTotalOccupied();
  }

  getTotalDischarged(): number {
    if (!this.schedule) return 0;
    return this.schedule.slots.reduce((total: number, slot: SlotSchedule) => {
      return total + slot.beds.filter((b: BedStatus) => 
        b.patient?.isDischarged === true
      ).length;
    }, 0);
  }

  getSessionDuration(bed: any): string {
    if (!bed.sessionStartTime) return '';
    const start = new Date(bed.sessionStartTime);
    const now = this.currentTime;
    const diffMs = now.getTime() - start.getTime();
    const hours = Math.floor(diffMs / (1000 * 60 * 60));
    const minutes = Math.floor((diffMs % (1000 * 60 * 60)) / (1000 * 60));
    return `${hours}h ${minutes}m`;
  }

  matchesSearch(bed: BedStatus): boolean {
    if (!this.searchText) return true;
    const search = this.searchText.toLowerCase();
    return bed.patient?.name?.toLowerCase().includes(search) || false;
  }

  matchesHDCycleFilter(bed: BedStatus): boolean {
    if (!this.filterHDCycle) return true;
    return bed.patient?.hdCycle === this.filterHDCycle;
  }

  shouldShowBedWithFilters(bed: BedStatus): boolean {
    return this.shouldShowBed(bed.status) && 
           this.matchesSearch(bed) && 
           this.matchesHDCycleFilter(bed);
  }

  getUniqueHDCycles(): string[] {
    if (!this.schedule) return [];
    const cycles = new Set<string>();
    this.schedule.slots.forEach(slot => {
      slot.beds.forEach(bed => {
        if (bed.patient?.hdCycle) {
          cycles.add(bed.patient.hdCycle);
        }
      });
    });
    return Array.from(cycles).sort();
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
    
    if (!bed) {
      this.showToast('Bed information not available', 'Warning');
      return;
    }

    if ((bed.status === 'occupied' || bed.status === 'pre-scheduled' || bed.status === 'completed') && bed.patient && bed.scheduleId) {
      // Navigate to HD session form in EDIT mode - shows all fields (filled and empty)
      // For completed sessions, staff can view the historical data
      this.router.navigate(['/schedule/hd-session/edit', bed.scheduleId]);
    } else if (bed.status === 'available') {
      this.showToast('This bed is available. Please select a patient first.', 'Information');
    } else {
      // For any other status, show a generic message
      this.showToast(`Bed is ${bed.status}. No action available.`, 'Information');
    }
  }

  navigateToWorkflow(patientId: number, scheduleId: number): void {
    if (!patientId || !scheduleId) {
      this.showToast('Session information incomplete', 'Warning');
      return;
    }
    this.router.navigate(['/patients', patientId, 'workflow', scheduleId]);
  }
  
  onQuickAssign(slotId: number, bedNumber: number): void {
    this.showToast('Quick assign feature - Coming soon! Navigate to Patient Management to assign a patient.', 'Information');
  }
  
  confirmSuggestedSession(slotId: number, bedNumber: number): void {
    const bed = this.getBed(slotId, bedNumber);
    
    if (!bed || !bed.patient) {
      this.showToast('Invalid bed data', 'Error');
      return;
    }
    
    const patientId = bed.patient.patientId;
    const patientName = bed.patient?.name || 'Unknown';
    const scheduleId = bed.scheduleId;
    
    // Confirm activation
    if (!confirm(`Activate ${patientName} and start dialysis treatment?`)) {
      return;
    }
    
    // For real scheduleId (not auto-generated 0), activate using ReservationService
    if (scheduleId && scheduleId > 0) {
      // Use the same activation method as Patient List for consistency
      this.reservationService.activateReservedPatient(patientId).subscribe({
        next: (response) => {
          if (response.success) {
            this.showToast(`${patientName} activated - Treatment started`, 'Success');
            
            // Reload schedule to show updated status (occupied/red)
            setTimeout(() => {
              this.loadSchedule();
            }, 300);
          } else {
            this.showToast('Failed to activate patient', 'Error');
          }
        },
        error: (error) => {
          const errorMsg = error.error?.message || error.message || 'Failed to activate patient';
          this.showToast(errorMsg, 'Error');
        }
      });
    } else {
      // Auto-generated (scheduleId === 0): Create the session first, then activate it
      const scheduleData = {
        patientId: patientId,
        sessionDate: this.selectedDate.toISOString().split('T')[0],
        slotId: slotId,
        bedNumber: bedNumber,
        sessionStatus: 'In Progress' // Set to In Progress directly (occupied/red)
      };
      
      // Create the session using createHDSession
      this.scheduleService.createHDSession(scheduleData).subscribe({
        next: (response: ApiResponse<number>) => {
          if (response.success && response.data) {
            this.showToast(`${patientName} activated - Treatment started`, 'Success');
            
            // Reload schedule to show the new session with red (occupied) status
            setTimeout(() => {
              this.loadSchedule();
            }, 300);
          } else {
            this.showToast('Failed to create session', 'Error');
          }
        },
        error: (error: any) => {
          this.showToast('Error creating session', 'Error');
        }
      });
    }
  }
  
  completeActiveSession(bed: any): void {
    if (!bed || !bed.scheduleId) {
      this.showToast('Invalid bed data', 'Error');
      return;
    }
    
    const scheduleId = bed.scheduleId;
    const patientName = bed.patient?.name || 'Unknown';
    
    // Confirm with user
    if (!confirm(`Mark treatment as completed for ${patientName}?`)) {
      return;
    }
    
    // Call the force-discharge endpoint to mark as completed
    this.scheduleService.forceDischargeSession(scheduleId).subscribe({
      next: (response) => {
        this.showToast(`Treatment completed for ${patientName}`, 'Success');
        // Reload the schedule to show updated status
        this.loadSchedule();
      },
      error: (error) => {
        this.showToast('Failed to complete session. Please try again.', 'Error');
      }
    });
  }
  
  editSuggestedSession(slotId: number, bedNumber: number): void {
    const bed = this.getBed(slotId, bedNumber);
    
    if (!bed || !bed.patient) {
      this.showToast('Invalid bed data', 'Error');
      return;
    }
    
    const patientId = bed.patient.patientId;
    
    // Navigate to HD Schedule form for editing
    this.router.navigate(['/schedule/hd-session/new', patientId], {
      queryParams: {
        date: this.formatDateForUrl(this.selectedDate),
        slotId: slotId,
        bedNumber: bedNumber
      }
    });
  }
  
  private formatDateForUrl(date: Date): string {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }
}
