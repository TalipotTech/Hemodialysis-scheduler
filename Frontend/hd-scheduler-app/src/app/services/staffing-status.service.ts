import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment.development';

export interface ShiftStaffingStatus {
  slotID: number;
  slotName: string;
  slotTime: string;
  bedCapacity: number;
  doctorCount: number;
  nurseCount: number;
  technicianCount: number;
  totalStaff: number;
  recommendedDoctors: number;
  recommendedNurses: number;
  recommendedTechnicians: number;
  recommendedTotal: number;
  staffingPercentage: number;
  status: 'Critical' | 'Understaffed' | 'Adequate';
  statusColor: 'red' | 'yellow' | 'green';
}

export interface ApiResponse<T> {
  success: boolean;
  data: T;
  message?: string;
}

@Injectable({
  providedIn: 'root'
})
export class StaffingStatusService {
  private apiUrl = `${environment.apiUrl}/staffingstatus`;

  constructor(private http: HttpClient) {}

  private getHeaders(): HttpHeaders {
    const token = localStorage.getItem('token');
    return new HttpHeaders({
      'Authorization': `Bearer ${token}`
    });
  }

  getAllShiftStatuses(): Observable<ApiResponse<ShiftStaffingStatus[]>> {
    return this.http.get<ApiResponse<ShiftStaffingStatus[]>>(
      this.apiUrl,
      { headers: this.getHeaders() }
    );
  }

  getShiftStatus(slotId: number): Observable<ApiResponse<ShiftStaffingStatus>> {
    return this.http.get<ApiResponse<ShiftStaffingStatus>>(
      `${this.apiUrl}/${slotId}`,
      { headers: this.getHeaders() }
    );
  }
}
