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

export interface FormPrediction {
  fieldName: string;
  predictedValue: any;
  confidence: number;
  reasoning: string;
  dataSources: string[];
}

export interface SessionAutocomplete {
  patientId: number;
  patientName: string;
  predictions: FormPrediction[];
  warnings: string[];
  summary: string;
  generatedAt: Date;
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
   * Process natural language query
   */
  processNaturalQuery(query: string): Observable<any> {
    return this.http.post<any>(`${environment.apiUrl}/api/aiquery/natural-query`, { query });
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

  /**
   * Get saved prompts
   */
  getSavedPrompts(): Observable<any[]> {
    return this.http.get<any[]>(`${environment.apiUrl}/api/aiquery/saved-prompts`);
  }

  /**
   * Save a new prompt
   */
  savePrompt(promptText: string, category: string | null): Observable<any> {
    return this.http.post<any>(`${environment.apiUrl}/api/aiquery/saved-prompts`, {
      promptText,
      category
    });
  }

  /**
   * Record prompt usage
   */
  recordPromptUsage(promptId: number): Observable<any> {
    return this.http.put<any>(`${environment.apiUrl}/api/aiquery/saved-prompts/${promptId}/use`, {});
  }

  /**
   * Delete a saved prompt
   */
  deletePrompt(promptId: number): Observable<any> {
    return this.http.delete<any>(`${environment.apiUrl}/api/aiquery/saved-prompts/${promptId}`);
  }

  // Risk Assessment APIs
  getPatientRiskAssessment(patientId: number): Observable<any> {
    return this.http.get<any>(`${environment.apiUrl}/api/riskassessment/patient/${patientId}`);
  }

  getBatchRiskAssessment(patientIds: number[]): Observable<any[]> {
    return this.http.post<any[]>(`${environment.apiUrl}/api/riskassessment/batch`, { patientIds });
  }

  getHighRiskPatients(threshold: number = 60): Observable<any> {
    return this.http.get<any>(`${environment.apiUrl}/api/riskassessment/high-risk?threshold=${threshold}`);
  }

  analyzeRiskFactors(patientId: number, customFactors?: any): Observable<any> {
    return this.http.post<any>(`${environment.apiUrl}/api/riskassessment/factors/${patientId}`, { customFactors });
  }

  // Report Generation APIs
  generateDailyReport(date?: Date): Observable<any> {
    const dateParam = date ? `?date=${date.toISOString()}` : '';
    return this.http.get<any>(`${environment.apiUrl}/api/reportgeneration/daily${dateParam}`);
  }

  generateWeeklyReport(startDate?: Date): Observable<any> {
    const dateParam = startDate ? `?startDate=${startDate.toISOString()}` : '';
    return this.http.get<any>(`${environment.apiUrl}/api/reportgeneration/weekly${dateParam}`);
  }

  generatePatientReport(patientId: number, startDate?: Date, endDate?: Date): Observable<any> {
    let params = '';
    if (startDate) params += `?startDate=${startDate.toISOString()}`;
    if (endDate) params += `${params ? '&' : '?'}endDate=${endDate.toISOString()}`;
    return this.http.get<any>(`${environment.apiUrl}/api/reportgeneration/patient/${patientId}${params}`);
  }

  generateCustomReport(reportType: string, parameters?: any): Observable<any> {
    return this.http.post<any>(`${environment.apiUrl}/api/reportgeneration/custom`, { reportType, parameters });
  }

  getReportTemplates(): Observable<any[]> {
    return this.http.get<any[]>(`${environment.apiUrl}/api/reportgeneration/templates`);
  }

  exportReportToPdf(reportContent: string, title: string): Observable<Blob> {
    return this.http.post(`${environment.apiUrl}/api/reportgeneration/export/pdf`, 
      { reportContent, title }, 
      { responseType: 'blob' });
  }

  // Analytics Dashboard APIs
  getUsageMetrics(startDate?: Date, endDate?: Date): Observable<any> {
    let params = '';
    if (startDate) params += `?startDate=${startDate.toISOString()}`;
    if (endDate) params += `${params ? '&' : '?'}endDate=${endDate.toISOString()}`;
    return this.http.get<any>(`${environment.apiUrl}/api/analyticsdashboard/usage-metrics${params}`);
  }

  getCostAnalytics(startDate?: Date, endDate?: Date): Observable<any> {
    let params = '';
    if (startDate) params += `?startDate=${startDate.toISOString()}`;
    if (endDate) params += `${params ? '&' : '?'}endDate=${endDate.toISOString()}`;
    return this.http.get<any>(`${environment.apiUrl}/api/analyticsdashboard/cost-analytics${params}`);
  }

  getPerformanceMetrics(): Observable<any> {
    return this.http.get<any>(`${environment.apiUrl}/api/analyticsdashboard/performance`);
  }

  getUsageTrends(days: number = 30): Observable<any> {
    return this.http.get<any>(`${environment.apiUrl}/api/analyticsdashboard/trends?days=${days}`);
  }

  getSystemHealth(): Observable<any> {
    return this.http.get<any>(`${environment.apiUrl}/api/analyticsdashboard/system-health`);
  }

  getFeatureUsage(): Observable<any> {
    return this.http.get<any>(`${environment.apiUrl}/api/analyticsdashboard/feature-usage`);
  }

  getCostProjection(days: number = 30): Observable<any> {
    return this.http.get<any>(`${environment.apiUrl}/api/analyticsdashboard/cost-projection?days=${days}`);
  }

  // Form Autocomplete APIs
  getSessionAutocomplete(patientId: number, sessionDate: Date, slotId?: number): Observable<SessionAutocomplete> {
    // Validate inputs
    if (!patientId || patientId <= 0) {
      throw new Error('Invalid patientId: must be a positive number');
    }
    if (!(sessionDate instanceof Date) || isNaN(sessionDate.getTime())) {
      throw new Error('Invalid sessionDate: must be a valid Date object');
    }

    const requestBody = {
      patientId,
      sessionDate: sessionDate.toISOString(),
      slotId: slotId || null,
      partialData: {}
    };

    console.log('AI Autocomplete Request:', requestBody);

    return this.http.post<SessionAutocomplete>(
      `${environment.apiUrl}/api/formautocomplete/predict-session`,
      requestBody
    );
  }

  getPredictedFieldValue(patientId: number, fieldName: string): Observable<FormPrediction> {
    return this.http.get<FormPrediction>(
      `${environment.apiUrl}/api/formautocomplete/predict-field/${patientId}/${fieldName}`
    );
  }

  getCachedFieldValue(patientId: number, fieldName: string): Observable<any> {
    return this.http.get<any>(
      `${environment.apiUrl}/api/formautocomplete/cache/${patientId}/${fieldName}`
    );
  }

  // Feature Suggestion APIs
  analyzeAndSuggestFeatures(): Observable<any> {
    return this.http.post<any>(
      `${environment.apiUrl}/api/featuresuggestion/analyze`,
      {}
    );
  }

  getPendingFeatureSuggestions(category?: string): Observable<any> {
    const params = category ? `?category=${category}` : '';
    return this.http.get<any>(
      `${environment.apiUrl}/api/featuresuggestion/pending${params}`
    );
  }

  updateFeatureSuggestionStatus(id: number, status: string, notes: string): Observable<any> {
    return this.http.put<any>(
      `${environment.apiUrl}/api/featuresuggestion/${id}/status`,
      { status, developerNotes: notes }
    );
  }

  getFeatureSuggestionStats(): Observable<any> {
    return this.http.get<any>(
      `${environment.apiUrl}/api/featuresuggestion/stats`
    );
  }
}
