import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment.development';

export interface AuditLog {
  logID: number;
  userID?: number;
  username: string;
  action: string;
  entityType?: string;
  entityID?: number;
  oldValues?: string;
  newValues?: string;
  ipAddress?: string;
  createdAt: Date;
}

export interface UserActivity {
  username: string;
  role: string;
  lastAction: string;
  lastActivityTime: Date;
  actionCount: number;
}

export interface DailyActionCount {
  date: Date;
  count: number;
}

export interface AuditStatistics {
  totalActions: number;
  uniqueUsers: number;
  loginCount: number;
  createCount: number;
  updateCount: number;
  deleteCount: number;
  mostActiveUser: string;
  mostCommonAction: string;
  actionsByDay: DailyActionCount[];
}

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
}

@Injectable({
  providedIn: 'root'
})
export class AuditLogsService {
  private apiUrl = `${environment.apiUrl}/api/AuditLogs`;

  constructor(private http: HttpClient) {}

  private getHeaders(): HttpHeaders {
    const token = localStorage.getItem('token');
    return new HttpHeaders({
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    });
  }

  getAllLogs(page: number = 1, pageSize: number = 50): Observable<ApiResponse<AuditLog[]>> {
    const params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());
    return this.http.get<ApiResponse<AuditLog[]>>(this.apiUrl, { 
      headers: this.getHeaders(), 
      params 
    });
  }

  getLogsByUser(userId: number): Observable<ApiResponse<AuditLog[]>> {
    return this.http.get<ApiResponse<AuditLog[]>>(`${this.apiUrl}/user/${userId}`, { 
      headers: this.getHeaders() 
    });
  }

  getLogsByEntity(entityType: string, entityId: number): Observable<ApiResponse<AuditLog[]>> {
    return this.http.get<ApiResponse<AuditLog[]>>(`${this.apiUrl}/entity/${entityType}/${entityId}`, { 
      headers: this.getHeaders() 
    });
  }

  getLogsByDateRange(startDate: Date, endDate: Date): Observable<ApiResponse<AuditLog[]>> {
    const params = new HttpParams()
      .set('startDate', startDate.toISOString())
      .set('endDate', endDate.toISOString());
    return this.http.get<ApiResponse<AuditLog[]>>(`${this.apiUrl}/daterange`, { 
      headers: this.getHeaders(), 
      params 
    });
  }

  getActivitySummary(startDate?: Date, endDate?: Date): Observable<ApiResponse<UserActivity[]>> {
    let params = new HttpParams();
    if (startDate) {
      params = params.set('startDate', startDate.toISOString());
    }
    if (endDate) {
      params = params.set('endDate', endDate.toISOString());
    }
    return this.http.get<ApiResponse<UserActivity[]>>(`${this.apiUrl}/activity-summary`, { 
      headers: this.getHeaders(), 
      params 
    });
  }

  getLoginHistory(userId?: number, days: number = 30): Observable<ApiResponse<AuditLog[]>> {
    let params = new HttpParams().set('days', days.toString());
    if (userId) {
      params = params.set('userId', userId.toString());
    }
    return this.http.get<ApiResponse<AuditLog[]>>(`${this.apiUrl}/login-history`, { 
      headers: this.getHeaders(), 
      params 
    });
  }

  getLogsByAction(action: string): Observable<ApiResponse<AuditLog[]>> {
    return this.http.get<ApiResponse<AuditLog[]>>(`${this.apiUrl}/actions/${action}`, { 
      headers: this.getHeaders() 
    });
  }

  getStatistics(days: number = 30): Observable<ApiResponse<AuditStatistics>> {
    const params = new HttpParams().set('days', days.toString());
    return this.http.get<ApiResponse<AuditStatistics>>(`${this.apiUrl}/statistics`, { 
      headers: this.getHeaders(), 
      params 
    });
  }
}
