// Patient Model - Basic Patient Information with Latest Session Data
export interface Patient {
  patientID: number;
  mrn?: string; // Medical Record Number
  name: string; // Patient full name
  age: number; // Patient age in years
  gender?: 'Male' | 'Female' | 'Other'; // Patient gender
  contactNumber: string; // Patient contact information
  emergencyContact?: string; // Emergency contact details
  address?: string; // Patient address
  guardianName?: string; // Guardian name
  hdCycle?: string; // HD Cycle pattern (e.g., "MWF", "TTS", "Daily")
  hdFrequency?: number; // Number of sessions per week
  isActive: boolean;
  createdAt: Date;
  updatedAt: Date;
  
  // Latest Session Data
  scheduleID?: number | null;
  slotID?: number | null;
  bedNumber?: number | null;
  dialyserType?: string | null;
  assignedDoctor?: number | null;
  assignedNurse?: number | null;
  assignedDoctorName?: string | null;
  assignedNurseName?: string | null;
  sessionStartTime?: string | null; // ISO date string for session start time
  isDischarged?: boolean;
}

export interface CreatePatientRequest {
  mrn?: string | null;
  name: string;
  age: number;
  gender?: string;
  contactNumber: string;
  emergencyContact?: string | null;
  address?: string | null;
  guardianName?: string | null;
  hdCycle?: string | null;
  hdFrequency?: number | null;
}

export interface UpdatePatientRequest extends CreatePatientRequest {
  patientID: number;
}
