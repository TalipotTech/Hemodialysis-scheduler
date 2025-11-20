import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTabsModule } from '@angular/material/tabs';
import { Location } from '@angular/common';
import { ReportsService } from '../../services/reports.service';

@Component({
  selector: 'app-reports',
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
            <mat-card-title>Reports & Analytics</mat-card-title>
          </div>
        </mat-card-header>
        <mat-card-content>
          <mat-tab-group>
            <mat-tab label="Patient Volume">
              <div class="tab-content">
                <h3>Patient Volume Report</h3>
                <p>Daily patient volume and slot distribution</p>
                <p>Full implementation with charts coming soon!</p>
              </div>
            </mat-tab>
            <mat-tab label="Occupancy Rates">
              <div class="tab-content">
                <h3>Bed Occupancy Report</h3>
                <p>Real-time bed utilization by slot</p>
                <p>Full implementation with charts coming soon!</p>
              </div>
            </mat-tab>
            <mat-tab label="Treatment Completion">
              <div class="tab-content">
                <h3>Treatment Completion Statistics</h3>
                <p>Session completion rates and patient-level statistics</p>
                <p>Full implementation with charts coming soon!</p>
              </div>
            </mat-tab>
            <mat-tab label="Staff Performance">
              <div class="tab-content">
                <h3>Staff Performance Report</h3>
                <p>Sessions handled, average duration, and completion rates</p>
                <p>Full implementation with charts coming soon!</p>
              </div>
            </mat-tab>
            <mat-tab label="Monthly Summary">
              <div class="tab-content">
                <h3>Monthly Summary</h3>
                <p>Comprehensive monthly statistics and trends</p>
                <p>Full implementation with charts coming soon!</p>
              </div>
            </mat-tab>
          </mat-tab-group>
        </mat-card-content>
      </mat-card>
    </div>
  `,
  styles: [`
    .container { padding: 20px; max-width: 1400px; margin: 0 auto; }
    .header-content { display: flex; align-items: center; gap: 16px; width: 100%; }
    mat-card-title { flex: 1; }
    .tab-content { padding: 20px; }
  `]
})
export class ReportsComponent implements OnInit {
  constructor(
    private reportsService: ReportsService,
    private location: Location
  ) {}

  ngOnInit(): void {
    // Load reports data
  }

  goBack(): void {
    this.location.back();
  }
}
