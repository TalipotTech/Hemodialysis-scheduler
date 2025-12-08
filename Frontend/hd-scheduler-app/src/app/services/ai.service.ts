import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment.development';

export interface AIScheduleRecommendationRequest {
  patientId: number;
  preferredSlotId?: number;
  preferredDate?: string;
}

export interface AIScheduleRecommendation {
  recommendedSlotId: number;
  recommendedBedNumber: number;
  recommendedDate: string;
  confidence: number;
  reasoning: string;
  factors: string[];
  alternatives?: AlternativeRecommendation[];
  modelUsed: string;
  processingTimeMs: number;
  cost: number;
}

export interface AlternativeRecommendation {
  slotId: number;
  bedNumber: number;
  confidence: number;
  reason: string;
}

export interface AISettingsDto {
  aiEnabled: boolean;
  aiProvider: string;
  dailyCostLimit: number;
  monthlyCostLimit: number;
  enableSchedulingRecommendations: boolean;
  enableNaturalLanguageQueries: boolean;
  enablePredictiveAnalytics: boolean;
  currentDailyCost: number;
  currentMonthlyCost: number;
  todayRequestCount: number;
  monthRequestCount: number;
  hasApiKey: boolean;
}

export interface UpdateAISettingsDto {
  aiEnabled?: boolean;
  aiProvider?: string;
  apiKey?: string;
  dailyCostLimit?: number;
  monthlyCostLimit?: number;
  enableSchedulingRecommendations?: boolean;
  enableNaturalLanguageQueries?: boolean;
  enablePredictiveAnalytics?: boolean;
}

export interface AIUsageStatsDto {
  todayCost: number;
  monthCost: number;
  todayRequests: number;
  monthRequests: number;
  dailyCostLimit: number;
  monthlyCostLimit: number;
  dailyUsagePercentage: number;
  monthlyUsagePercentage: number;
  usageBreakdown: UsageByType[];
  costTrend: DailyCostTrend[];
}

export interface UsageByType {
  requestType: string;
  count: number;
  totalCost: number;
  avgProcessingTimeMs: number;
}

export interface DailyCostTrend {
  date: string;
  cost: number;
  requestCount: number;
}

export interface AIStatusDto {
  enabled: boolean;
  provider: string;
  underBudget: boolean;
  dailyUsage: string;
  monthlyUsage: string;
}

@Injectable({
  providedIn: 'root'
})
export class AIService {
  private apiUrl = `${environment.apiUrl}/api/ai`;

  constructor(private http: HttpClient) { }

  /**
   * Get scheduling recommendation from AI
   */
  getSchedulingRecommendation(request: AIScheduleRecommendationRequest): Observable<AIScheduleRecommendation> {
    return this.http.post<AIScheduleRecommendation>(`${this.apiUrl}/schedule/recommend`, request);
  }

  /**
   * Get AI settings (Admin only)
   */
  getSettings(): Observable<AISettingsDto> {
    return this.http.get<AISettingsDto>(`${this.apiUrl}/settings`);
  }

  /**
   * Update AI settings (Admin only)
   */
  updateSettings(settings: UpdateAISettingsDto): Observable<AISettingsDto> {
    return this.http.put<AISettingsDto>(`${this.apiUrl}/settings`, settings);
  }

  /**
   * Get usage statistics (Admin only)
   */
  getUsageStats(): Observable<AIUsageStatsDto> {
    return this.http.get<AIUsageStatsDto>(`${this.apiUrl}/usage/stats`);
  }

  /**
   * Get AI status (all authenticated users)
   */
  getStatus(): Observable<AIStatusDto> {
    return this.http.get<AIStatusDto>(`${this.apiUrl}/status`);
  }

  /**
   * Check if AI is enabled and available
   */
  isAIAvailable(): Observable<boolean> {
    return new Observable(observer => {
      this.getStatus().subscribe({
        next: (status) => {
          observer.next(status.enabled && status.underBudget);
          observer.complete();
        },
        error: () => {
          observer.next(false);
          observer.complete();
        }
      });
    });
  }
}
