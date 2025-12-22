import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule, Location } from '@angular/common';
import { Router } from '@angular/router';
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
  private readonly REFRESH_INTERVAL_MS = 30000; // 30 seconds
  private timeUpdateInterval: any;

  // Future scheduled sessions (Bed Schedule)
  futureSessions: any[] = [];
  loadingFuture = false;
  selectedTab = 0;

  constructor(
    private scheduleService: ScheduleService,
    private reservationService: ReservationService,
    private location: Location,
    private router: Router
  ) {}

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
      this.showToast('Auto-refresh enabled (every 30 seconds)', 'Information');
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
    
    console.log('Loading schedule for date:', this.selectedDate);
    
    this.scheduleService.getDailySchedule(this.selectedDate).subscribe({
      next: (response: any) => {
        console.log('Schedule API response:', response);
        if (response.success && response.data) {
          this.schedule = response.data;
          console.log('Schedule data loaded:', this.schedule);
          
          // Debug: Log all beds with their statuses and HD Cycles
          if (this.schedule && this.schedule.slots) {
            this.schedule.slots.forEach((slot: any) => {
              slot.beds.forEach((bed: any) => {
                if (bed.status !== 'available' && bed.patient) {
                  console.log(`🛏️ Slot ${slot.slotName}, Bed ${bed.bedNumber}: STATUS="${bed.status}", SessionStatus="${bed.sessionStatus}", Patient: ${bed.patient?.name}, HD Cycle: ${bed.patient?.hdCycle || 'NOT PROVIDED'}, ScheduleID: ${bed.scheduleId}`);
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
    
    // Check if this is an auto-generated suggested session (scheduleId === 0)
    // Treat it as pre-scheduled (purple) since it's already scheduled, just needs activation
    if (bed.scheduleId === 0) {
      let className = 'bed-pre-scheduled'; // Purple - auto-generated, ready to activate
      if (!this.shouldShowBed('pre-scheduled')) {
        className += ' bed-filtered';
      }
      console.log(`🎨 Bed ${bedNumber} in slot ${slotId}: scheduleId=0 → ${className}`);
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
    
    console.log(`🎨 Bed ${bedNumber} in slot ${slotId}: status="${bed.status}", sessionStatus="${bed.sessionStatus}" → ${className}`);
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
          this.futureSessions = response.data;
          console.log('Future scheduled sessions:', this.futureSessions);
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
    return this.schedule.slots.reduce((total: number, slot: SlotSchedule) => {
      return total + slot.beds.filter((b: BedStatus) => b.status === 'pre-scheduled').length;
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
    const uniqueCycles = Array.from(cycles).sort();
    console.log('Unique HD Cycles found:', uniqueCycles);
    return uniqueCycles;
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
      this.showToast('Bed information not available', 'Warning');
      return;
    }

    if ((bed.status === 'occupied' || bed.status === 'pre-scheduled' || bed.status === 'completed') && bed.patient && bed.scheduleId) {
      // Navigate to HD session form in EDIT mode - shows all fields (filled and empty)
      // For completed sessions, staff can view the historical data
      console.log('Navigating to edit session:', bed.scheduleId);
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
    console.log('Quick assign clicked for slot:', slotId, 'bed:', bedNumber);
    this.showToast('Quick assign feature - Coming soon! Navigate to Patient Management to assign a patient.', 'Information');
    // TODO: Implement quick assign modal or navigate to patient assignment
  }
  
  confirmSuggestedSession(slotId: number, bedNumber: number): void {
    console.log('🟢 Activate button clicked for slot:', slotId, 'bed:', bedNumber);
    
    const bed = this.getBed(slotId, bedNumber);
    
    if (!bed || !bed.patient) {
      console.error('❌ No bed or patient data', bed);
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
    
    console.log('📋 Activating patient:', { patientName, patientId, scheduleId, slotId, bedNumber });
    
    // For real scheduleId (not auto-generated 0), activate using ReservationService
    if (scheduleId && scheduleId > 0) {
      // Use the same activation method as Patient List for consistency
      this.reservationService.activateReservedPatient(patientId).subscribe({
        next: (response) => {
          if (response.success) {
            console.log('✅ Patient activated successfully:', response);
            this.showToast(`${patientName} activated - Treatment started`, 'Success');
            
            // Reload schedule to show updated status (occupied/red)
            setTimeout(() => {
              console.log('🔄 Reloading schedule to show activated patient...');
              this.loadSchedule();
            }, 300);
          } else {
            console.error('❌ Failed to activate patient:', response);
            this.showToast('Failed to activate patient', 'Error');
          }
        },
        error: (error) => {
          console.error('❌ Error activating patient:', error);
          const errorMsg = error.error?.message || error.message || 'Failed to activate patient';
          this.showToast(errorMsg, 'Error');
        }
      });
    } else {
      // Auto-generated (scheduleId === 0): Create the session first, then activate it
      console.log('📝 Creating auto-suggested session before activation...');
      
      // Create a schedule object for the auto-suggested session
      const scheduleData = {
        patientId: patientId,
        sessionDate: this.selectedDate.toISOString().split('T')[0],
        slotId: slotId,
        bedNumber: bedNumber,
        sessionStatus: 'In Progress' // Set to In Progress directly (occupied/red)
      };
      
      console.log('Creating schedule with data:', scheduleData);
      
      // Create the session using createHDSession
      this.scheduleService.createHDSession(scheduleData).subscribe({
        next: (response: ApiResponse<number>) => {
          if (response.success && response.data) {
            const newScheduleId = response.data;
            console.log('✅ Session created with ID:', newScheduleId);
            this.showToast(`${patientName} activated - Treatment started`, 'Success');
            
            // Reload schedule to show the new session with red (occupied) status
            // DO NOT navigate to workflow - just update the color
            setTimeout(() => {
              console.log('🔄 Reloading schedule to show red (occupied) status...');
              this.loadSchedule();
            }, 300);
          } else {
            console.error('❌ Failed to create session:', response);
            this.showToast('Failed to create session', 'Error');
          }
        },
        error: (error: any) => {
          console.error('❌ Error creating session:', error);
          this.showToast('Error creating session', 'Error');
        }
      });
    }
  }
  
  completeActiveSession(bed: any): void {
    console.log('✅ Complete button clicked for active session!', bed);
    
    if (!bed || !bed.scheduleId) {
      console.error('❌ No bed or schedule data', bed);
      this.showToast('Invalid bed data', 'Error');
      return;
    }
    
    const scheduleId = bed.scheduleId;
    const patientName = bed.patient?.name || 'Unknown';
    
    // Confirm with user
    if (!confirm(`Mark treatment as completed for ${patientName}?`)) {
      return;
    }
    
    console.log('📋 Marking session as completed:', scheduleId);
    
    // Call the force-discharge endpoint to mark as completed
    this.scheduleService.forceDischargeSession(scheduleId).subscribe({
      next: (response) => {
        console.log('✅ Session marked as completed:', response);
        this.showToast(`Treatment completed for ${patientName}`, 'Success');
        // Reload the schedule to show updated status
        this.loadSchedule();
      },
      error: (error) => {
        console.error('❌ Error completing session:', error);
        this.showToast('Failed to complete session. Please try again.', 'Error');
      }
    });
  }
  
  editSuggestedSession(slotId: number, bedNumber: number): void {
    console.log('🟡 Edit button clicked for slot:', slotId, 'bed:', bedNumber);
    
    const bed = this.getBed(slotId, bedNumber);
    
    if (!bed || !bed.patient) {
      console.error('❌ No bed or patient data', bed);
      this.showToast('Invalid bed data', 'Error');
      return;
    }
    
    const patientId = bed.patient.patientId;
    
    console.log('✏️ Navigating to edit for patient:', patientId);
    
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
