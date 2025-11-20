import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment.development';

export interface Staff {
  staffID: number;
  name: string;
  role: string;
  contactNumber?: string;
  staffSpecialization?: string;
  assignedSlot?: number;
  assignedSlotName?: string;
  isActive: boolean;
  createdAt: Date;
}

export interface CreateStaffRequest {
  name: string;
  role: string;
  contactNumber?: string;
  specialization?: string;
}

export interface UpdateStaffRequest {
  name: string;
  role: string;
  contactNumber?: string;
  specialization?: string;
  isActive?: boolean;
}

export interface AssignSlotRequest {
  slotID?: number;
}

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
}

@Injectable({
  providedIn: 'root'
})
export class StaffManagementService {
  private apiUrl = `${environment.apiUrl}/staffmanagement`;

  constructor(private http: HttpClient) {}

  private getHeaders(): HttpHeaders {
    const token = localStorage.getItem('token');
    return new HttpHeaders({
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    });
  }

  getAllStaff(): Observable<ApiResponse<Staff[]>> {
    return this.http.get<ApiResponse<Staff[]>>(this.apiUrl, { headers: this.getHeaders() });
  }

  getActiveStaff(): Observable<ApiResponse<Staff[]>> {
    return this.http.get<ApiResponse<Staff[]>>(`${this.apiUrl}/active`, { headers: this.getHeaders() });
  }

  getStaffById(id: number): Observable<ApiResponse<Staff>> {
    return this.http.get<ApiResponse<Staff>>(`${this.apiUrl}/${id}`, { headers: this.getHeaders() });
  }

  getStaffByRole(role: string): Observable<ApiResponse<Staff[]>> {
    return this.http.get<ApiResponse<Staff[]>>(`${this.apiUrl}/role/${role}`, { headers: this.getHeaders() });
  }

  getStaffBySlot(slotId: number): Observable<ApiResponse<Staff[]>> {
    return this.http.get<ApiResponse<Staff[]>>(`${this.apiUrl}/slot/${slotId}`, { headers: this.getHeaders() });
  }

  createStaff(request: CreateStaffRequest): Observable<ApiResponse<number>> {
    return this.http.post<ApiResponse<number>>(this.apiUrl, request, { headers: this.getHeaders() });
  }

  updateStaff(id: number, request: UpdateStaffRequest): Observable<ApiResponse<boolean>> {
    return this.http.put<ApiResponse<boolean>>(`${this.apiUrl}/${id}`, request, { headers: this.getHeaders() });
  }

  deleteStaff(id: number): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.apiUrl}/${id}`, { headers: this.getHeaders() });
  }

  assignToSlot(id: number, request: AssignSlotRequest): Observable<ApiResponse<boolean>> {
    return this.http.post<ApiResponse<boolean>>(`${this.apiUrl}/${id}/assign-slot`, request, { headers: this.getHeaders() });
  }

  toggleStaffStatus(id: number): Observable<ApiResponse<boolean>> {
    return this.http.post<ApiResponse<boolean>>(`${this.apiUrl}/${id}/toggle-status`, {}, { headers: this.getHeaders() });
  }
}
