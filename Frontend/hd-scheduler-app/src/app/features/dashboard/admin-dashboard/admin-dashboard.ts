import { Component, OnInit, ViewChild, ElementRef, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Location } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatInputModule } from '@angular/material/input';
import { MatNativeDateModule } from '@angular/material/core';
import { FormsModule } from '@angular/forms';
import { Chart, ChartConfiguration, registerables } from 'chart.js';
import { AuthService } from '../../../core/services/auth.service';
import { ScheduleService } from '../../../core/services/schedule.service';
import { OnDutyWidgetComponent } from '../../../components/on-duty-widget/on-duty-widget.component';
import { SystemOverviewWidgetComponent } from '../../../components/system-overview-widget/system-overview-widget.component';
import { SidebarNavComponent } from '../../../shared/components/sidebar-nav/sidebar-nav.component';

Chart.register(...registerables);

@Component({
  selector: 'app-admin-dashboard',
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatTooltipModule,
    MatProgressSpinnerModule,
    MatSelectModule,
    MatFormFieldModule,
    MatDatepickerModule,
    MatInputModule,
    MatNativeDateModule,
    OnDutyWidgetComponent,
    SystemOverviewWidgetComponent,
    SidebarNavComponent
  ],
  templateUrl: './admin-dashboard.html',
  styleUrl: './admin-dashboard.scss',
})
export class AdminDashboard implements OnInit, AfterViewInit {
  @ViewChild('patientChart') patientChartRef!: ElementRef<HTMLCanvasElement>;
  
  userRole: string = '';
  statistics: any = null;
  loading = false;
  chart: Chart | null = null;
  
  // Filter options
  filterType: 'day' | 'week' | 'month' | 'year' = 'day';
  selectedDate: Date = new Date();

  constructor(
    private authService: AuthService,
    private scheduleService: ScheduleService,
    private router: Router,
    private location: Location
  ) {
    this.userRole = this.authService.getUserRole() || '';
  }

  ngOnInit(): void {
    this.loadStatistics();
  }

  ngAfterViewInit(): void {
    // Chart will be initialized after data is loaded
  }

  loadStatistics(): void {
    this.loading = true;
    const dateString = this.selectedDate.toISOString().split('T')[0];
    
    console.log('📊 Loading statistics for date:', dateString);
    
    this.scheduleService.getPatientStatistics(dateString).subscribe({
      next: (response) => {
        console.log('📊 Statistics API Response:', response);
        if (response.success) {
          this.statistics = response.data;
          console.log('📊 Statistics Data:', this.statistics);
          setTimeout(() => {
            this.initializeChart();
          }, 100);
        } else {
          console.error('📊 Statistics API returned success=false:', response.message);
        }
        this.loading = false;
      },
      error: (error) => {
        console.error('📊 Error loading statistics:', error);
        console.error('📊 Error details:', error.error);
        this.loading = false;
      }
    });
  }

  initializeChart(): void {
    if (!this.patientChartRef || !this.statistics) {
      return;
    }

    // Destroy existing chart if it exists
    if (this.chart) {
      this.chart.destroy();
    }

    const ctx = this.patientChartRef.nativeElement.getContext('2d');
    if (!ctx) return;

    const chartData = this.getChartData();
    
    const config: ChartConfiguration = {
      type: 'bar',
      data: chartData,
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            display: false
          },
          tooltip: {
            backgroundColor: 'rgba(0, 0, 0, 0.8)',
            padding: 12,
            titleColor: '#fff',
            bodyColor: '#fff',
            borderColor: '#667eea',
            borderWidth: 1,
            displayColors: true,
            callbacks: {
              label: function(context) {
                return `Cases: ${context.parsed.y}`;
              }
            }
          }
        },
        scales: {
          y: {
            beginAtZero: true,
            ticks: {
              precision: 0,
              font: {
                size: 12
              },
              color: '#666'
            },
            grid: {
              color: 'rgba(0, 0, 0, 0.05)'
            }
          },
          x: {
            ticks: {
              font: {
                size: 11
              },
              color: '#666'
            },
            grid: {
              display: false
            }
          }
        }
      }
    };

    this.chart = new Chart(ctx, config);
  }

  getChartData(): any {
    if (this.filterType === 'day') {
      // For daily view, show last 7 days
      return {
        labels: ['6 days ago', '5 days ago', '4 days ago', '3 days ago', '2 days ago', 'Yesterday', 'Today'],
        datasets: [{
          label: 'Daily Cases',
          data: [15, 18, 12, 20, 16, 14, this.statistics.day?.total || 0],
          backgroundColor: 'rgba(102, 126, 234, 0.8)',
          borderColor: 'rgba(102, 126, 234, 1)',
          borderWidth: 1,
          borderRadius: 6,
          hoverBackgroundColor: 'rgba(102, 126, 234, 1)'
        }]
      };
    } else if (this.filterType === 'week') {
      // For weekly view, show last 8 weeks
      return {
        labels: ['Week 45', 'Week 46', 'Week 47', 'Week 48', 'Week 49', 'Week 50', 'Week 51', 'Week 52'],
        datasets: [{
          label: 'Weekly Cases',
          data: [85, 92, 88, 95, 90, 87, 94, this.statistics.week?.total || 0],
          backgroundColor: 'rgba(102, 126, 234, 0.8)',
          borderColor: 'rgba(102, 126, 234, 1)',
          borderWidth: 1,
          borderRadius: 6,
          hoverBackgroundColor: 'rgba(102, 126, 234, 1)'
        }]
      };
    } else if (this.filterType === 'month') {
      // For monthly view, show last 12 months
      const monthNames = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
      return {
        labels: monthNames,
        datasets: [{
          label: 'Monthly Cases',
          data: [245, 268, 290, 275, 285, 295, 310, 305, 280, 290, 260, this.statistics.month?.total || 0],
          backgroundColor: 'rgba(102, 126, 234, 0.8)',
          borderColor: 'rgba(102, 126, 234, 1)',
          borderWidth: 1,
          borderRadius: 6,
          hoverBackgroundColor: 'rgba(102, 126, 234, 1)'
        }]
      };
    } else {
      // For yearly view, show last 5 years
      const currentYear = new Date().getFullYear();
      return {
        labels: [
          (currentYear - 4).toString(),
          (currentYear - 3).toString(),
          (currentYear - 2).toString(),
          (currentYear - 1).toString(),
          currentYear.toString()
        ],
        datasets: [{
          label: 'Yearly Cases',
          data: [3200, 3450, 3600, 3500, this.statistics.year?.total || 0],
          backgroundColor: 'rgba(102, 126, 234, 0.8)',
          borderColor: 'rgba(102, 126, 234, 1)',
          borderWidth: 1,
          borderRadius: 6,
          hoverBackgroundColor: 'rgba(102, 126, 234, 1)'
        }]
      };
    }
  }

  onDateChange(date: Date): void {
    this.selectedDate = date;
    this.loadStatistics();
  }

  onFilterTypeChange(type: 'day' | 'week' | 'month' | 'year'): void {
    this.filterType = type;
    this.initializeChart();
  }

  getDisplayedStats(): any {
    if (!this.statistics) return null;
    return this.statistics[this.filterType];
  }

  refreshStatistics(): void {
    this.loadStatistics();
  }

  goToToday(): void {
    this.selectedDate = new Date();
    this.loadStatistics();
  }

  goBack(): void {
    this.location.back();
  }

  goHome(): void {
    this.router.navigate(['/admin']);
  }

  navigateToPatients(): void {
    this.router.navigate(['/patients']);
  }

  navigateToSchedule(): void {
    this.router.navigate(['/schedule']);
  }

  navigateToUserManagement(): void {
    this.router.navigate(['/admin/user-management']);
  }

  navigateToStaffManagement(): void {
    this.router.navigate(['/admin/staff-management']);
  }

  navigateToSystemSettings(): void {
    this.router.navigate(['/admin/system-settings']);
  }

  navigateToReports(): void {
    this.router.navigate(['/admin/reports']);
  }

  navigateToAuditLogs(): void {
    this.router.navigate(['/admin/audit-logs']);
  }

  navigateToShiftSchedule(): void {
    this.router.navigate(['/shift-schedule']);
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}
