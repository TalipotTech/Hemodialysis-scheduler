import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatCardModule } from '@angular/material/card';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Location } from '@angular/common';
import { UserManagementService, User, CreateUserRequest, UpdateUserRequest } from '../../services/user-management.service';
import { UserDialogComponent } from './user-dialog.component';
import { PasswordResetDialogComponent } from './password-reset-dialog.component';

@Component({
  selector: 'app-user-management',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatDialogModule,
    MatSnackBarModule,
    MatInputModule,
    MatSelectModule,
    MatSlideToggleModule,
    MatCardModule,
    MatTooltipModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './user-management.component.html',
  styleUrls: ['./user-management.component.scss']
})
export class UserManagementComponent implements OnInit {
  users: User[] = [];
  filteredUsers: User[] = [];
  displayedColumns: string[] = ['username', 'role', 'isActive', 'createdAt', 'lastLogin', 'actions'];
  isLoading = false;
  searchTerm = '';
  selectedRole = 'All';
  roles = ['All', 'Admin', 'HOD', 'Doctor', 'Nurse', 'Technician'];

  constructor(
    private userService: UserManagementService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar,
    private location: Location
  ) {}

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.isLoading = true;
    this.userService.getAllUsers().subscribe({
      next: (response) => {
        if (response.success) {
          this.users = response.data;
          this.applyFilters();
        }
        this.isLoading = false;
      },
      error: (error) => {
        this.snackBar.open('Error loading users', 'Close', { duration: 3000 });
        this.isLoading = false;
        console.error('Error loading users:', error);
      }
    });
  }

  applyFilters(): void {
    this.filteredUsers = this.users.filter(user => {
      const matchesSearch = user.username.toLowerCase().includes(this.searchTerm.toLowerCase());
      const matchesRole = this.selectedRole === 'All' || user.role === this.selectedRole;
      return matchesSearch && matchesRole;
    });
  }

  onSearchChange(): void {
    this.applyFilters();
  }

  onRoleFilterChange(): void {
    this.applyFilters();
  }

  openCreateDialog(): void {
    const dialogRef = this.dialog.open(UserDialogComponent, {
      width: '500px',
      data: { mode: 'create' }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.createUser(result);
      }
    });
  }

  openEditDialog(user: User): void {
    const dialogRef = this.dialog.open(UserDialogComponent, {
      width: '500px',
      data: { mode: 'edit', user: { ...user } }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.updateUser(user.userID, result);
      }
    });
  }

  openPasswordResetDialog(user: User): void {
    const dialogRef = this.dialog.open(PasswordResetDialogComponent, {
      width: '400px',
      data: { user }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.resetPassword(user.userID, result);
      }
    });
  }

  createUser(request: CreateUserRequest): void {
    this.userService.createUser(request).subscribe({
      next: (response) => {
        if (response.success) {
          this.snackBar.open('User created successfully', 'Close', { duration: 3000 });
          this.loadUsers();
        }
      },
      error: (error) => {
        this.snackBar.open(error.error?.message || 'Error creating user', 'Close', { duration: 3000 });
        console.error('Error creating user:', error);
      }
    });
  }

  updateUser(id: number, request: UpdateUserRequest): void {
    this.userService.updateUser(id, request).subscribe({
      next: (response) => {
        if (response.success) {
          this.snackBar.open('User updated successfully', 'Close', { duration: 3000 });
          this.loadUsers();
        }
      },
      error: (error) => {
        this.snackBar.open(error.error?.message || 'Error updating user', 'Close', { duration: 3000 });
        console.error('Error updating user:', error);
      }
    });
  }

  deleteUser(user: User): void {
    if (confirm(`Are you sure you want to delete user "${user.username}"? This action cannot be undone.`)) {
      this.userService.deleteUser(user.userID).subscribe({
        next: (response) => {
          if (response.success) {
            this.snackBar.open('User deleted successfully', 'Close', { duration: 3000 });
            this.loadUsers();
          }
        },
        error: (error) => {
          this.snackBar.open(error.error?.message || 'Error deleting user', 'Close', { duration: 3000 });
          console.error('Error deleting user:', error);
        }
      });
    }
  }

  toggleUserStatus(user: User): void {
    const action = user.isActive ? 'disable' : 'enable';
    if (confirm(`Are you sure you want to ${action} user "${user.username}"?`)) {
      this.userService.toggleUserStatus(user.userID).subscribe({
        next: (response) => {
          if (response.success) {
            this.snackBar.open(`User ${action}d successfully`, 'Close', { duration: 3000 });
            this.loadUsers();
          }
        },
        error: (error) => {
          this.snackBar.open(error.error?.message || `Error ${action}ing user`, 'Close', { duration: 3000 });
          console.error(`Error ${action}ing user:`, error);
        }
      });
    }
  }

  resetPassword(id: number, newPassword: string): void {
    this.userService.resetPassword(id, { newPassword }).subscribe({
      next: (response) => {
        if (response.success) {
          this.snackBar.open('Password reset successfully', 'Close', { duration: 3000 });
        }
      },
      error: (error) => {
        this.snackBar.open(error.error?.message || 'Error resetting password', 'Close', { duration: 3000 });
        console.error('Error resetting password:', error);
      }
    });
  }

  goBack(): void {
    this.location.back();
  }
}
