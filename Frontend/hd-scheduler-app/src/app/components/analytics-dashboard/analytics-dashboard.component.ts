import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';
import { AIService } from '../../services/ai.service';

@Component({
  selector: 'app-analytics-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatChipsModule
  ],
  templateUrl: './analytics-dashboard.component.html',
  styleUrls: ['./analytics-dashboard.component.scss']
})
export class AnalyticsDashboardComponent implements OnInit {
  loading = true;
  usageMetrics: any = null;
  costAnalytics: any = null;
  performanceMetrics: any = null;
  systemHealth: any = null;
  featureUsage: any = null;

  constructor(private aiService: AIService) {}

  ngOnInit() {
    this.loadDashboardData();
  }

  loadDashboardData() {
    this.loading = true;

    Promise.all([
      this.aiService.getUsageMetrics().toPromise(),
      this.aiService.getCostAnalytics().toPromise(),
      this.aiService.getPerformanceMetrics().toPromise(),
      this.aiService.getSystemHealth().toPromise(),
      this.aiService.getFeatureUsage().toPromise()
    ]).then(([usage, cost, performance, health, features]) => {
      this.usageMetrics = usage;
      this.costAnalytics = cost;
      this.performanceMetrics = performance;
      this.systemHealth = health;
      this.featureUsage = features;
      this.loading = false;
    }).catch(err => {
      console.error('Error loading dashboard:', err);
      this.loading = false;
    });
  }

  getHealthStatusColor(): string {
    if (!this.systemHealth) return '#gray';
    if (this.systemHealth.status === 'Healthy') return '#10b981';
    if (this.systemHealth.status === 'Warning') return '#f59e0b';
    return '#dc2626';
  }

  getSuccessRateColor(rate: number): string {
    if (rate >= 95) return '#10b981';
    if (rate >= 85) return '#f59e0b';
    return '#dc2626';
  }

  formatCurrency(value: number): string {
    return `$${value.toFixed(2)}`;
  }

  formatNumber(value: number): string {
    return value.toLocaleString();
  }
}
