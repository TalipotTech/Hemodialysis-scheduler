import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
// import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ReservationService {
  // Temporarily hardcoded for local development
  private apiUrl = 'http://localhost:5000/api/reservation';

  constructor(private http: HttpClient) { }

  /**
   * Get reservation statistics showing reserved vs active patients based on HD cycles
   */
  getReservationStatistics(date?: string): Observable<any> {
    const params: any = date ? { date } : {};
    return this.http.get(`${this.apiUrl}/statistics`, { params });
  }

  /**
   * Auto-schedule next session for a patient based on their HD cycle
   */
  autoScheduleNextSession(scheduleId: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/auto-schedule-next/${scheduleId}`, {});
  }

  /**
   * Generate multiple future sessions for a patient based on their HD cycle
   */
  generateScheduleForPatient(
    patientId: number, 
    daysAhead: number = 30,
    slotId?: number,
    bedNumber?: number
  ): Observable<any> {
    const params: any = { daysAhead };
    if (slotId) params.slotId = slotId;
    if (bedNumber) params.bedNumber = bedNumber;
    return this.http.post(`${this.apiUrl}/generate-schedule/${patientId}`, {}, { params });
  }

  /**
   * Get all patients with their reservation status (Active or Reserved)
   */
  getPatientsWithReservationStatus(date?: string): Observable<any> {
    const params: any = date ? { date } : {};
    return this.http.get(`${this.apiUrl}/patients-status`, { params });
  }

  /**
   * Activate a reserved patient for TODAY
   * Changes session status from "Pre-Scheduled" to "Active"
   * Bed color changes from purple to red in schedule grid
   */
  activateReservedPatient(patientId: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/activate/${patientId}`, {});
  }

  /**
   * Mark a pre-scheduled session as "Missed" when patient doesn't arrive
   */
  markSessionAsMissed(patientId: number, date?: string): Observable<any> {
    const params: any = date ? { date } : {};
    return this.http.post(`${this.apiUrl}/mark-missed/${patientId}`, {}, { params });
  }
}
