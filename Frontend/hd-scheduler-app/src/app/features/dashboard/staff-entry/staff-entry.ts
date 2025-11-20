import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Location } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-staff-entry',
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule
  ],
  templateUrl: './staff-entry.html',
  styleUrl: './staff-entry.scss',
})
export class StaffEntry {
  userName: string = '';
  userRole: string = '';

  constructor(
    private authService: AuthService,
    private router: Router,
    private location: Location
  ) {
    this.userName = 'Staff Member';
    this.userRole = this.authService.getUserRole() || '';
  }

  goBack(): void {
    this.location.back();
  }

  navigateToPatients(): void {
    this.router.navigate(['/patients']);
  }

  navigateToSchedule(): void {
    this.router.navigate(['/schedule']);
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
