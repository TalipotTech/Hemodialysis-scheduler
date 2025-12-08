import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment.development';

export interface DailyVolume {
  date: Date;
  patientCount: number;
}

export interface SlotVolume {
  slotName: string;
  totalAssignments: number;
  uniquePatients: number;
}

export interface PatientVolumeReport {
  startDate: Date;
  endDate: Date;
  totalPatients: number;
  totalSessions: number;
  dailyVolume: DailyVolume[];
  volumeBySlot: SlotVolume[];
}

export interface SlotOccupancy {
  slotID: number;
  slotName: string;
  maxBeds: number;
  occupiedBeds: number;
  occupancyRate: number;
}

export interface OccupancyReport {
  reportDate: Date;
  totalBeds: number;
  occupiedBeds: number;
  availableBeds: number;
  overallOccupancyRate: number;
  slotOccupancy: SlotOccupancy[];
}

export interface PatientCompletion {
  patientID: number;
  patientName: string;
  totalSessions: number;
  completedSessions: number;
  completionRate: number;
}

export interface TreatmentCompletionReport {
  startDate: Date;
  endDate: Date;
  totalSessions: number;
  completedSessions: number;
  completionRate: number;
  patientCompletion: PatientCompletion[];
}

export interface StaffPerformance {
  staffID: number;
  staffName: string;
  role: string;
  sessionsHandled: number;
  avgSessionDuration?: number;
  completedSessions: number;
}

export interface MonthlySummary {
  year: number;
  month: number;
  totalPatients: number;
  newPatients: number;
  totalSessions: number;
  completedSessions: number;
  completionRate: number;
  averageOccupancyRate: number;
}

export interface MonthlyBreakdown {
  month: number;
  totalSessions: number;
  uniquePatients: number;
  completedSessions: number;
}

export interface YearlySummary {
  year: number;
  totalPatients: number;
  totalSessions: number;
  monthlyBreakdown: MonthlyBreakdown[];
}

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
}

@Injectable({
  providedIn: 'root'
})
export class ReportsService {
  private apiUrl = `${environment.apiUrl}/api/reports`;

  constructor(private http: HttpClient) {}

  private getHeaders(): HttpHeaders {
    const token = localStorage.getItem('token');
    return new HttpHeaders({
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    });
  }

  getPatientVolume(startDate?: Date, endDate?: Date): Observable<ApiResponse<PatientVolumeReport>> {
    let params = new HttpParams();
    if (startDate) {
      params = params.set('startDate', startDate.toISOString());
    }
    if (endDate) {
      params = params.set('endDate', endDate.toISOString());
    }
    return this.http.get<ApiResponse<PatientVolumeReport>>(`${this.apiUrl}/patient-volume`, { 
      headers: this.getHeaders(), 
      params 
    });
  }

  getOccupancyRates(date?: Date): Observable<ApiResponse<OccupancyReport>> {
    let params = new HttpParams();
    if (date) {
      params = params.set('date', date.toISOString());
    }
    return this.http.get<ApiResponse<OccupancyReport>>(`${this.apiUrl}/occupancy-rates`, { 
      headers: this.getHeaders(), 
      params 
    });
  }

  getTreatmentCompletion(startDate?: Date, endDate?: Date): Observable<ApiResponse<TreatmentCompletionReport>> {
    let params = new HttpParams();
    if (startDate) {
      params = params.set('startDate', startDate.toISOString());
    }
    if (endDate) {
      params = params.set('endDate', endDate.toISOString());
    }
    return this.http.get<ApiResponse<TreatmentCompletionReport>>(`${this.apiUrl}/treatment-completion`, { 
      headers: this.getHeaders(), 
      params 
    });
  }

  getStaffPerformance(startDate?: Date, endDate?: Date): Observable<ApiResponse<StaffPerformance[]>> {
    let params = new HttpParams();
    if (startDate) {
      params = params.set('startDate', startDate.toISOString());
    }
    if (endDate) {
      params = params.set('endDate', endDate.toISOString());
    }
    return this.http.get<ApiResponse<StaffPerformance[]>>(`${this.apiUrl}/staff-performance`, { 
      headers: this.getHeaders(), 
      params 
    });
  }

  getMonthlySummary(year?: number, month?: number): Observable<ApiResponse<MonthlySummary>> {
    let params = new HttpParams();
    if (year) {
      params = params.set('year', year.toString());
    }
    if (month) {
      params = params.set('month', month.toString());
    }
    return this.http.get<ApiResponse<MonthlySummary>>(`${this.apiUrl}/monthly-summary`, { 
      headers: this.getHeaders(), 
      params 
    });
  }

  getYearlySummary(year?: number): Observable<ApiResponse<YearlySummary>> {
    let params = new HttpParams();
    if (year) {
      params = params.set('year', year.toString());
    }
    return this.http.get<ApiResponse<YearlySummary>>(`${this.apiUrl}/yearly-summary`, { 
      headers: this.getHeaders(), 
      params 
    });
  }
}
