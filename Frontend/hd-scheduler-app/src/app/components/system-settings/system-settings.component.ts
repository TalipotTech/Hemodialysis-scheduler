import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTabsModule } from '@angular/material/tabs';
import { Location } from '@angular/common';
import { SystemSettingsService } from '../../services/system-settings.service';

@Component({
  selector: 'app-system-settings',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatButtonModule, MatIconModule, MatTabsModule],
  template: `
    <div class="container">
      <mat-card>
        <mat-card-header>
          <div class="header-content">
            <button mat-icon-button (click)="goBack()">
              <mat-icon>arrow_back</mat-icon>
            </button>
            <mat-card-title>System Settings</mat-card-title>
          </div>
        </mat-card-header>
        <mat-card-content>
          <mat-tab-group>
            <mat-tab label="Slot Configuration">
              <div class="tab-content">
                <h3>Time Slot Management</h3>
                <p>Configure dialysis time slots (Morning, Afternoon, Evening, Night)</p>
                <p>Full implementation coming soon!</p>
              </div>
            </mat-tab>
            <mat-tab label="Bed Capacity">
              <div class="tab-content">
                <h3>Bed Capacity Management</h3>
                <p>Set maximum bed capacity per slot</p>
                <p>Full implementation coming soon!</p>
              </div>
            </mat-tab>
            <mat-tab label="System Parameters">
              <div class="tab-content">
                <h3>System Overview</h3>
                <p>View system configuration and statistics</p>
                <p>Full implementation coming soon!</p>
              </div>
            </mat-tab>
          </mat-tab-group>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    .container { padding: 20px; max-width: 1200px; margin: 0 auto; }
    .header-content { display: flex; align-items: center; gap: 16px; width: 100%; }
    mat-card-title { flex: 1; }
    .tab-content { padding: 20px; }
  `]
})
export class SystemSettingsComponent implements OnInit {
  constructor(
    private settingsService: SystemSettingsService,
    private location: Location
  ) {}

  ngOnInit(): void {
    // Load settings
  }

  goBack(): void {
    this.location.back();
  }
}
