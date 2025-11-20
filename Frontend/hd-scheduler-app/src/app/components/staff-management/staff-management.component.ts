import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { Location } from '@angular/common';
import { StaffManagementService, Staff, CreateStaffRequest, UpdateStaffRequest } from '../../services/staff-management.service';

@Component({
  selector: 'app-staff-management',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatInputModule,
    MatFormFieldModule,
    MatSelectModule,
    MatChipsModule,
    MatTooltipModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatDialogModule
  ],
  templateUrl: './staff-management.component.html',
  styleUrls: ['./staff-management.component.scss']
})
export class StaffManagementComponent implements OnInit {
  staff: Staff[] = [];
  filteredStaff: Staff[] = [];
  displayedColumns: string[] = ['name', 'role', 'contact', 'specialization', 'assignedSlot', 'status', 'actions'];
  
  searchTerm: string = '';
  selectedRole: string = 'All';
  selectedStatus: string = 'All';
  isLoading: boolean = false;

  roles: string[] = ['All', 'Doctor', 'Nurse', 'Technician'];
  statusOptions: string[] = ['All', 'Active', 'Inactive'];

  constructor(
    private staffService: StaffManagementService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar,
    private location: Location
  ) {}

  ngOnInit(): void {
    this.loadStaff();
  }

  loadStaff(): void {
    this.isLoading = true;
    this.staffService.getAllStaff().subscribe({
      next: (response) => {
        if (response.success) {
          this.staff = response.data;
          this.applyFilters();
        }
        this.isLoading = false;
      },
      error: (error) => {
        this.snackBar.open('Error loading staff', 'Close', { duration: 3000 });
        this.isLoading = false;
        console.error('Error loading staff:', error);
      }
    });
  }

  applyFilters(): void {
    this.filteredStaff = this.staff.filter(staff => {
      const matchesSearch = staff.name.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
                           (staff.staffSpecialization?.toLowerCase().includes(this.searchTerm.toLowerCase()) ?? false);
      const matchesRole = this.selectedRole === 'All' || staff.role === this.selectedRole;
      const matchesStatus = this.selectedStatus === 'All' || 
                           (this.selectedStatus === 'Active' ? staff.isActive : !staff.isActive);
      return matchesSearch && matchesRole && matchesStatus;
    });
  }

  onSearchChange(): void {
    this.applyFilters();
  }

  onRoleFilterChange(): void {
    this.applyFilters();
  }

  onStatusFilterChange(): void {
    this.applyFilters();
  }

  async openCreateDialog(): Promise<void> {
    const { StaffDialogComponent } = await import('./staff-dialog.component');
    const dialogRef = this.dialog.open(StaffDialogComponent, {
      width: '600px',
      data: { mode: 'create' }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.createStaff(result);
      }
    });
  }

  async openEditDialog(staff: Staff): Promise<void> {
    const { StaffDialogComponent } = await import('./staff-dialog.component');
    const dialogRef = this.dialog.open(StaffDialogComponent, {
      width: '600px',
      data: { mode: 'edit', staff: { ...staff } }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.updateStaff(staff.staffID, result);
      }
    });
  }

  async openSlotAssignmentDialog(staff: Staff): Promise<void> {
    const { SlotAssignmentDialogComponent } = await import('./slot-assignment-dialog.component');
    const dialogRef = this.dialog.open(SlotAssignmentDialogComponent, {
      width: '500px',
      data: { staff }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.assignToSlot(staff.staffID, result.slotId);
      }
    });
  }

  createStaff(request: CreateStaffRequest): void {
    this.staffService.createStaff(request).subscribe({
      next: (response) => {
        if (response.success) {
          this.snackBar.open('Staff member created successfully', 'Close', { duration: 3000 });
          this.loadStaff();
        }
      },
      error: (error) => {
        this.snackBar.open(error.error?.message || 'Error creating staff', 'Close', { duration: 3000 });
        console.error('Error creating staff:', error);
      }
    });
  }

  updateStaff(id: number, request: UpdateStaffRequest): void {
    this.staffService.updateStaff(id, request).subscribe({
      next: (response) => {
        if (response.success) {
          this.snackBar.open('Staff member updated successfully', 'Close', { duration: 3000 });
          this.loadStaff();
        }
      },
      error: (error) => {
        this.snackBar.open(error.error?.message || 'Error updating staff', 'Close', { duration: 3000 });
        console.error('Error updating staff:', error);
      }
    });
  }

  assignToSlot(staffId: number, slotId: number): void {
    this.staffService.assignToSlot(staffId, { slotID: slotId }).subscribe({
      next: (response) => {
        if (response.success) {
          this.snackBar.open('Staff assigned to slot successfully', 'Close', { duration: 3000 });
          this.loadStaff();
        }
      },
      error: (error) => {
        this.snackBar.open(error.error?.message || 'Error assigning staff to slot', 'Close', { duration: 3000 });
        console.error('Error assigning staff:', error);
      }
    });
  }

  toggleStatus(staff: Staff): void {
    this.staffService.toggleStaffStatus(staff.staffID).subscribe({
      next: (response: any) => {
        if (response.success) {
          const newStatus = !staff.isActive ? 'activated' : 'deactivated';
          this.snackBar.open(`Staff ${newStatus} successfully`, 'Close', { duration: 3000 });
          this.loadStaff();
        }
      },
      error: (error: any) => {
        this.snackBar.open(error.error?.message || 'Error updating staff status', 'Close', { duration: 3000 });
        console.error('Error toggling status:', error);
      }
    });
  }

  deleteStaff(staff: Staff): void {
    if (confirm(`Are you sure you want to delete ${staff.name}?`)) {
      this.staffService.deleteStaff(staff.staffID).subscribe({
        next: (response) => {
          if (response.success) {
            this.snackBar.open('Staff member deleted successfully', 'Close', { duration: 3000 });
            this.loadStaff();
          }
        },
        error: (error) => {
          this.snackBar.open(error.error?.message || 'Error deleting staff', 'Close', { duration: 3000 });
          console.error('Error deleting staff:', error);
        }
      });
    }
  }

  goBack(): void {
    this.location.back();
  }
}
