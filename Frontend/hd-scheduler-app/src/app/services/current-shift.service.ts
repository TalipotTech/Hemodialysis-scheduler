import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../environments/environment.development';
import { Staff } from './staff-management.service';

export interface CurrentShiftStaff {
  slotName: string;
  slotTime: string;
  doctors: Staff[];
  nurses: Staff[];
  technicians: Staff[];
  totalStaff: number;
}

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
}

@Injectable({
  providedIn: 'root'
})
export class CurrentShiftService {
  private apiUrl = `${environment.apiUrl}/api/staffmanagement`;

  constructor(private http: HttpClient) {}

  private getHeaders(): HttpHeaders {
    const token = localStorage.getItem('token');
    return new HttpHeaders({
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    });
  }

  getCurrentShiftStaff(): Observable<CurrentShiftStaff> {
    const currentHour = new Date().getHours();
    let currentSlotId: number;
    let slotName: string;
    let slotTime: string;

    // Determine current shift based on time
    if (currentHour >= 6 && currentHour < 11) {
      currentSlotId = 1;
      slotName = 'Morning';
      slotTime = '6:00 AM - 10:00 AM';
    } else if (currentHour >= 11 && currentHour < 16) {
      currentSlotId = 2;
      slotName = 'Afternoon';
      slotTime = '11:00 AM - 3:00 PM';
    } else if (currentHour >= 16 && currentHour < 21) {
      currentSlotId = 3;
      slotName = 'Evening';
      slotTime = '4:00 PM - 8:00 PM';
    } else {
      currentSlotId = 4;
      slotName = 'Night';
      slotTime = '9:00 PM - 1:00 AM';
    }

    return this.http.get<ApiResponse<Staff[]>>(`${this.apiUrl}/slot/${currentSlotId}`, { 
      headers: this.getHeaders() 
    }).pipe(
      map(response => {
        const staff = response.data || [];
        return {
          slotName,
          slotTime,
          doctors: staff.filter(s => s.role === 'Doctor'),
          nurses: staff.filter(s => s.role === 'Nurse'),
          technicians: staff.filter(s => s.role === 'Technician'),
          totalStaff: staff.length
        };
      })
    );
  }

  getAllShiftsStaff(): Observable<{ [key: string]: CurrentShiftStaff }> {
    const slots = [
      { id: 1, name: 'Morning', time: '6:00 AM - 10:00 AM' },
      { id: 2, name: 'Afternoon', time: '11:00 AM - 3:00 PM' },
      { id: 3, name: 'Evening', time: '4:00 PM - 8:00 PM' },
      { id: 4, name: 'Night', time: '9:00 PM - 1:00 AM' }
    ];

    return new Observable(observer => {
      const shifts: { [key: string]: CurrentShiftStaff } = {};
      let completed = 0;

      slots.forEach(slot => {
        this.http.get<ApiResponse<Staff[]>>(`${this.apiUrl}/slot/${slot.id}`, { 
          headers: this.getHeaders() 
        }).subscribe({
          next: (response) => {
            const staff = response.data || [];
            shifts[slot.name] = {
              slotName: slot.name,
              slotTime: slot.time,
              doctors: staff.filter(s => s.role === 'Doctor'),
              nurses: staff.filter(s => s.role === 'Nurse'),
              technicians: staff.filter(s => s.role === 'Technician'),
              totalStaff: staff.length
            };
            completed++;
            if (completed === slots.length) {
              observer.next(shifts);
              observer.complete();
            }
          },
          error: (err) => {
            completed++;
            if (completed === slots.length) {
              observer.next(shifts);
              observer.complete();
            }
          }
        });
      });
    });
  }
}
