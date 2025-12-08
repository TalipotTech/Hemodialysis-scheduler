import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTabsModule } from '@angular/material/tabs';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatTableModule } from '@angular/material/table';
import { Location } from '@angular/common';
import { AIService, AISettingsDto, UpdateAISettingsDto, AIUsageStatsDto } from '../../services/ai.service';

@Component({
  selector: 'app-ai-settings',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatTabsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSlideToggleModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatTooltipModule,
    MatProgressBarModule,
    MatTableModule
  ],
  templateUrl: './ai-settings.component.html',
  styleUrls: ['./ai-settings.component.scss']
})
export class AISettingsComponent implements OnInit {
  loading = false;
  savingSettings = false;
  settings: AISettingsDto | null = null;
  usageStats: AIUsageStatsDto | null = null;
  
  // Form fields
  apiKey: string = '';
  showApiKey = false;
  
  // Display columns for usage breakdown table
  usageColumns: string[] = ['requestType', 'count', 'totalCost', 'avgTime'];
  costTrendColumns: string[] = ['date', 'cost', 'requestCount'];

  constructor(
    private aiService: AIService,
    private location: Location,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    this.loadSettings();
    this.loadUsageStats();
  }

  loadSettings(): void {
    this.loading = true;
    this.aiService.getSettings().subscribe({
      next: (settings) => {
        this.settings = settings;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading AI settings:', error);
        if (error.status === 401) {
          this.snackBar.open('Authentication required. Please login as Admin.', 'Close', { duration: 5000 });
        } else if (error.status === 403) {
          this.snackBar.open('Access denied. Admin role required.', 'Close', { duration: 5000 });
        } else {
          this.snackBar.open('Failed to load AI settings', 'Close', { duration: 3000 });
        }
        this.loading = false;
      }
    });
  }

  loadUsageStats(): void {
    this.aiService.getUsageStats().subscribe({
      next: (stats) => {
        this.usageStats = stats;
      },
      error: (error) => {
        console.error('Error loading usage stats:', error);
      }
    });
  }

  saveSettings(): void {
    if (!this.settings) return;

    this.savingSettings = true;
    const updateDto: UpdateAISettingsDto = {
      aiEnabled: this.settings.aiEnabled,
      aiProvider: this.settings.aiProvider,
      dailyCostLimit: this.settings.dailyCostLimit,
      monthlyCostLimit: this.settings.monthlyCostLimit,
      enableSchedulingRecommendations: this.settings.enableSchedulingRecommendations,
      enableNaturalLanguageQueries: this.settings.enableNaturalLanguageQueries,
      enablePredictiveAnalytics: this.settings.enablePredictiveAnalytics
    };

    // Add API key if provided
    if (this.apiKey && this.apiKey.trim()) {
      updateDto.apiKey = this.apiKey.trim();
    }

    this.aiService.updateSettings(updateDto).subscribe({
      next: (updatedSettings) => {
        this.settings = updatedSettings;
        this.apiKey = ''; // Clear API key after saving
        this.savingSettings = false;
        this.snackBar.open('AI settings saved successfully', 'Close', { duration: 3000 });
        this.loadUsageStats(); // Refresh stats
      },
      error: (error) => {
        console.error('Error saving AI settings:', error);
        const errorMsg = error.error?.message || 'Failed to save AI settings';
        this.snackBar.open(errorMsg, 'Close', { duration: 5000 });
        this.savingSettings = false;
      }
    });
  }

  toggleApiKeyVisibility(): void {
    this.showApiKey = !this.showApiKey;
  }

  goBack(): void {
    this.location.back();
  }

  refreshStats(): void {
    this.loadUsageStats();
  }

  getUsagePercentageColor(percentage: number): string {
    if (percentage >= 90) return 'warn';
    if (percentage >= 75) return 'accent';
    return 'primary';
  }

  formatCurrency(amount: number): string {
    return `$${amount.toFixed(2)}`;
  }

  formatDate(dateString: string): string {
    const date = new Date(dateString);
    return date.toLocaleDateString();
  }

  get dailyUsageColor(): string {
    if (!this.usageStats) return 'primary';
    return this.getUsagePercentageColor(this.usageStats.dailyUsagePercentage);
  }

  get monthlyUsageColor(): string {
    if (!this.usageStats) return 'primary';
    return this.getUsagePercentageColor(this.usageStats.monthlyUsagePercentage);
  }

  get isOverBudget(): boolean {
    if (!this.usageStats) return false;
    return this.usageStats.dailyUsagePercentage >= 100 || this.usageStats.monthlyUsagePercentage >= 100;
  }

  get budgetWarning(): boolean {
    if (!this.usageStats) return false;
    return (this.usageStats.dailyUsagePercentage >= 80 && this.usageStats.dailyUsagePercentage < 100) ||
           (this.usageStats.monthlyUsagePercentage >= 80 && this.usageStats.monthlyUsagePercentage < 100);
  }
}
