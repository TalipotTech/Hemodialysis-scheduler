import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment.development';
import { 
  DailyScheduleResponse, 
  SlotSchedule, 
  AssignBedRequest, 
  BedAvailability
} from '../models/schedule.model';
import { ApiResponse } from '../models/user.model';

@Injectable({
  providedIn: 'root'
})
export class ScheduleService {
  private apiUrl = `${environment.apiUrl}/schedule`;

  constructor(private http: HttpClient) { }

  getDailySchedule(date?: Date): Observable<ApiResponse<DailyScheduleResponse>> {
    let params = new HttpParams();
    if (date) {
      params = params.set('date', date.toISOString());
    }
    return this.http.get<ApiResponse<DailyScheduleResponse>>(`${this.apiUrl}/daily`, { params });
  }

  getSlotSchedule(slotId: number, date?: Date): Observable<ApiResponse<SlotSchedule>> {
    let params = new HttpParams();
    if (date) {
      params = params.set('date', date.toISOString());
    }
    return this.http.get<ApiResponse<SlotSchedule>>(`${environment.apiUrl}/hdschedule/slot/${slotId}`, { params });
  }

  assignBed(request: AssignBedRequest): Observable<ApiResponse<number>> {
    return this.http.post<ApiResponse<number>>(`${this.apiUrl}/assign`, request);
  }

  dischargePatient(scheduleId: number): Observable<ApiResponse<boolean>> {
    return this.http.put<ApiResponse<boolean>>(`${environment.apiUrl}/hdschedule/${scheduleId}/discharge`, {});
  }

  forceDischargeSession(scheduleId: number): Observable<ApiResponse<any>> {
    return this.http.post<ApiResponse<any>>(`${this.apiUrl}/force-discharge/${scheduleId}`, {});
  }

  getBedAvailability(date?: Date): Observable<ApiResponse<BedAvailability[]>> {
    let params = new HttpParams();
    if (date) {
      params = params.set('date', date.toISOString());
    }
    return this.http.get<ApiResponse<BedAvailability[]>>(`${this.apiUrl}/availability`, { params });
  }

  createHDSession(sessionData: any): Observable<ApiResponse<number>> {
    return this.http.post<ApiResponse<number>>(`${environment.apiUrl}/hdschedule`, sessionData);
  }

  getHistorySessions(): Observable<ApiResponse<any[]>> {
    return this.http.get<ApiResponse<any[]>>(`${environment.apiUrl}/hdschedule/history`);
  }

  getFutureScheduledSessions(): Observable<ApiResponse<any[]>> {
    return this.http.get<ApiResponse<any[]>>(`${environment.apiUrl}/hdschedule/future-scheduled`);
  }

  getTodaySchedules(): Observable<ApiResponse<any[]>> {
    return this.http.get<ApiResponse<any[]>>(`${environment.apiUrl}/hdschedule/today`);
  }

  getActiveSessions(): Observable<ApiResponse<any[]>> {
    return this.http.get<ApiResponse<any[]>>(`${environment.apiUrl}/hdschedule/active`);
  }

  getScheduleById(scheduleId: number): Observable<ApiResponse<any>> {
    return this.http.get<ApiResponse<any>>(`${environment.apiUrl}/hdschedule/${scheduleId}`);
  }

  autoSaveSchedule(scheduleId: number, updates: any): Observable<ApiResponse<boolean>> {
    return this.http.patch<ApiResponse<boolean>>(`${environment.apiUrl}/hdschedule/${scheduleId}/auto-save`, updates);
  }

  updateSchedule(scheduleId: number, scheduleData: any): Observable<ApiResponse<boolean>> {
    // Backend expects data wrapped in 'request' object
    return this.http.put<ApiResponse<boolean>>(`${environment.apiUrl}/hdschedule/${scheduleId}`, scheduleData);
  }

  getSuggestedEquipmentCounts(patientId: number): Observable<ApiResponse<any>> {
    return this.http.get<ApiResponse<any>>(`${environment.apiUrl}/hdschedule/patient/${patientId}/suggested-equipment-counts`);
  }

  // ==================== SESSION PHASE MANAGEMENT ====================

  getPhaseStatus(hdLogId: number): Observable<ApiResponse<any>> {
    return this.http.get<ApiResponse<any>>(`${environment.apiUrl}/hdlog/${hdLogId}/phase-status`);
  }

  // Phase 1: Pre-Dialysis
  savePreDialysis(hdLogId: number, data: any): Observable<ApiResponse<any>> {
    return this.http.put<ApiResponse<any>>(`${environment.apiUrl}/hdlog/${hdLogId}/save-pre-dialysis`, data);
  }

  completePreDialysis(hdLogId: number): Observable<ApiResponse<string>> {
    return this.http.post<ApiResponse<string>>(`${environment.apiUrl}/hdlog/${hdLogId}/complete-pre-dialysis`, {});
  }

  // Phase 2: Intra-Dialysis
  startPostDialysis(hdLogId: number): Observable<ApiResponse<string>> {
    return this.http.post<ApiResponse<string>>(`${environment.apiUrl}/hdlog/${hdLogId}/start-post-dialysis`, {});
  }

  // Phase 3: Post-Dialysis
  savePostDialysis(hdLogId: number, data: any): Observable<ApiResponse<any>> {
    return this.http.put<ApiResponse<any>>(`${environment.apiUrl}/hdlog/${hdLogId}/save-post-dialysis`, data);
  }

  completePostDialysis(hdLogId: number): Observable<ApiResponse<string>> {
    return this.http.post<ApiResponse<string>>(`${environment.apiUrl}/hdlog/${hdLogId}/complete-post-dialysis`, {});
  }

  // Get HD Log by ID
  getHDLogById(hdLogId: number): Observable<ApiResponse<any>> {
    return this.http.get<ApiResponse<any>>(`${environment.apiUrl}/hdlog/${hdLogId}`);
  }

  // ==================== INTRA-DIALYTIC MONITORING ====================
  
  getIntraDialyticRecords(scheduleId: number): Observable<ApiResponse<any[]>> {
    return this.http.get<ApiResponse<any[]>>(`${environment.apiUrl}/hdschedule/${scheduleId}/monitoring`);
  }

  addIntraDialyticRecord(record: any): Observable<ApiResponse<number>> {
    return this.http.post<ApiResponse<number>>(`${environment.apiUrl}/hdschedule/monitoring`, record);
  }

  deleteIntraDialyticRecord(recordId: number): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${environment.apiUrl}/hdschedule/monitoring/${recordId}`);
  }

  // ==================== PATIENT STATISTICS ====================
  
  getPatientStatistics(date?: string): Observable<ApiResponse<any>> {
    let params = new HttpParams();
    if (date) {
      params = params.set('date', date);
    }
    return this.http.get<ApiResponse<any>>(`${this.apiUrl}/patient-statistics`, { params });
  }

  getAutoDischargeInfo(): Observable<ApiResponse<any>> {
    return this.http.get<ApiResponse<any>>(`${this.apiUrl}/auto-discharge-info`);
  }
}
