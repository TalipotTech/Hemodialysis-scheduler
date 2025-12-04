import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment.development';

export interface SlotConfiguration {
  slotID: number;
  slotName: string;
  startTime: string;
  endTime: string;
  maxBeds: number;
  isActive: boolean;
}

export interface CreateSlotRequest {
  slotName: string;
  startTime: string;
  endTime: string;
  maxBeds: number;
}

export interface UpdateSlotRequest {
  slotName: string;
  startTime: string;
  endTime: string;
  maxBeds: number;
  isActive: boolean;
}

export interface BedCapacity {
  slotID: number;
  slotName: string;
  maxBeds: number;
  usedBeds: number;
  reservedBeds: number;
  availableBeds: number;
  occupancyRate: number;
}

export interface UpdateBedCapacityRequest {
  maxBeds: number;
}

export interface SystemParameters {
  totalActiveSlots: number;
  totalBedCapacity: number;
  totalActivePatients: number;
  totalActiveStaff: number;
  databaseVersion: string;
  lastBackup?: Date;
}

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
}

@Injectable({
  providedIn: 'root'
})
export class SystemSettingsService {
  private apiUrl = `${environment.apiUrl}/systemsettings`;

  constructor(private http: HttpClient) {}

  private getHeaders(): HttpHeaders {
    const token = localStorage.getItem('token');
    return new HttpHeaders({
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    });
  }

  // Slot Management
  getSlots(): Observable<ApiResponse<SlotConfiguration[]>> {
    return this.http.get<ApiResponse<SlotConfiguration[]>>(`${this.apiUrl}/slots`, { headers: this.getHeaders() });
  }

  getSlotById(id: number): Observable<ApiResponse<SlotConfiguration>> {
    return this.http.get<ApiResponse<SlotConfiguration>>(`${this.apiUrl}/slots/${id}`, { headers: this.getHeaders() });
  }

  createSlot(request: CreateSlotRequest): Observable<ApiResponse<number>> {
    return this.http.post<ApiResponse<number>>(`${this.apiUrl}/slots`, request, { headers: this.getHeaders() });
  }

  updateSlot(id: number, request: UpdateSlotRequest): Observable<ApiResponse<boolean>> {
    return this.http.put<ApiResponse<boolean>>(`${this.apiUrl}/slots/${id}`, request, { headers: this.getHeaders() });
  }

  deleteSlot(id: number): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.apiUrl}/slots/${id}`, { headers: this.getHeaders() });
  }

  // Bed Configuration
  getBedCapacity(): Observable<ApiResponse<BedCapacity[]>> {
    return this.http.get<ApiResponse<BedCapacity[]>>(`${this.apiUrl}/beds/capacity`, { headers: this.getHeaders() });
  }

  updateBedCapacity(slotId: number, request: UpdateBedCapacityRequest): Observable<ApiResponse<boolean>> {
    return this.http.put<ApiResponse<boolean>>(`${this.apiUrl}/beds/capacity/${slotId}`, request, { headers: this.getHeaders() });
  }

  // System Parameters
  getSystemParameters(): Observable<ApiResponse<SystemParameters>> {
    return this.http.get<ApiResponse<SystemParameters>>(`${this.apiUrl}/parameters`, { headers: this.getHeaders() });
  }
}
