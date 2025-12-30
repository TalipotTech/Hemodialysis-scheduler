import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { tap } from 'rxjs/operators';
import { environment } from '../../environments/environment.development';
import { ApiResponse } from '../core/models/user.model';

export interface BedNamingConfig {
  pattern: string;
  prefix: string;
  bedsPerGroup: number;
  customFormat: string;
  availablePatterns?: BedPatternOption[];
  previewSamples?: string[];
}

export interface BedPatternOption {
  value: string;
  label: string;
  description: string;
  example: string;
}

export interface UpdateBedNamingConfigRequest {
  pattern: string;
  prefix: string;
  bedsPerGroup: number;
  customFormat: string;
}

@Injectable({
  providedIn: 'root'
})
export class BedFormatterService {
  private apiUrl = `${environment.apiUrl}/api/configuration`;
  private configSubject = new BehaviorSubject<BedNamingConfig | null>(null);
  public config$ = this.configSubject.asObservable();

  constructor(private http: HttpClient) {
    this.loadConfiguration();
  }

  private getHeaders(): HttpHeaders {
    const token = localStorage.getItem('token');
    return new HttpHeaders({
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    });
  }

  loadConfiguration(): void {
    this.http.get<ApiResponse<BedNamingConfig>>(`${this.apiUrl}/bed-naming`, { headers: this.getHeaders() })
      .subscribe({
        next: (response) => {
          if (response.success && response.data) {
            this.configSubject.next(response.data);
          }
        },
        error: (error) => console.error('Error loading bed naming config:', error)
      });
  }

  getBedNamingConfiguration(): Observable<ApiResponse<BedNamingConfig>> {
    return this.http.get<ApiResponse<BedNamingConfig>>(`${this.apiUrl}/bed-naming`, { headers: this.getHeaders() })
      .pipe(tap(response => {
        if (response.success && response.data) {
          this.configSubject.next(response.data);
        }
      }));
  }

  updateBedNamingConfiguration(request: UpdateBedNamingConfigRequest): Observable<ApiResponse<boolean>> {
    return this.http.put<ApiResponse<boolean>>(`${this.apiUrl}/bed-naming`, request, { headers: this.getHeaders() })
      .pipe(tap(() => this.loadConfiguration()));
  }

  previewBedNames(config: BedNamingConfig): Observable<ApiResponse<string[]>> {
    return this.http.post<ApiResponse<string[]>>(`${this.apiUrl}/bed-naming/preview`, config, { headers: this.getHeaders() });
  }

  // Format bed number using current configuration
  formatBedNumber(bedNumber: number): string {
    const config = this.configSubject.value;
    if (!config) {
      return `Bed ${bedNumber}`;
    }
    return this.formatBed(bedNumber, config);
  }

  // Format bed number with specific configuration
  formatBed(bedNumber: number, config: BedNamingConfig): string {
    switch (config.pattern) {
      case 'NUMERIC':
        return bedNumber.toString();
      
      case 'PREFIXED_NUMERIC':
        return `${config.prefix} ${bedNumber}`;
      
      case 'ALPHA_NUMERIC':
        return this.formatAlphaNumeric(bedNumber, config.bedsPerGroup);
      
      case 'ALPHABETIC':
        return this.formatAlphabetic(bedNumber);
      
      case 'CUSTOM':
        return this.applyCustomFormat(bedNumber, config);
      
      default:
        return `Bed ${bedNumber}`;
    }
  }

  private formatAlphaNumeric(bedNumber: number, bedsPerGroup: number): string {
    const letter = String.fromCharCode(65 + Math.floor((bedNumber - 1) / bedsPerGroup));
    const number = ((bedNumber - 1) % bedsPerGroup) + 1;
    return `${letter}${number}`;
  }

  private formatAlphabetic(bedNumber: number): string {
    if (bedNumber <= 26) {
      return String.fromCharCode(64 + bedNumber);
    }
    const first = Math.floor((bedNumber - 1) / 26);
    const second = (bedNumber - 1) % 26;
    return `${String.fromCharCode(65 + first)}${String.fromCharCode(65 + second)}`;
  }

  private applyCustomFormat(bedNumber: number, config: BedNamingConfig): string {
    let format = config.customFormat;
    
    // If format is just a single letter or ends with a letter, auto-append bed number
    // Examples: "W" -> "W1", "ICU-D" -> "ICU-D1", "A" -> "A1", "D" -> "D1"
    if (format && !format.includes('{')) {
      // Check if format ends with a letter (case-insensitive)
      const lastChar = format[format.length - 1];
      if (/[a-zA-Z]/.test(lastChar)) {
        return `${format}${bedNumber}`;
      }
    }
    
    // Replace placeholders
    format = format.replace('{n}', bedNumber.toString());
    format = format.replace('{N}', bedNumber.toString().padStart(2, '0'));
    
    // Letter placeholder
    const letter = String.fromCharCode(65 + Math.floor((bedNumber - 1) / config.bedsPerGroup));
    format = format.replace('{a}', letter);
    format = format.replace('{A}', letter);
    
    // Group number
    const groupNum = ((bedNumber - 1) % config.bedsPerGroup) + 1;
    format = format.replace('{g}', groupNum.toString());
    
    return format;
  }

  // Generate preview for multiple beds
  generatePreview(config: BedNamingConfig, count: number = 10): string[] {
    const preview: string[] = [];
    for (let i = 1; i <= count; i++) {
      preview.push(this.formatBed(i, config));
    }
    return preview;
  }
}
