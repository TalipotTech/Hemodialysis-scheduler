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
  hdCycle?: string; // HD Cycle pattern (e.g., "3x/week", "2x/week")
  hdFrequency?: number; // Number of sessions per week
  preferredSlotID?: number | null; // Preferred time slot (1=Morning, 2=Afternoon, 3=Evening, 4=Night)
  isActive: boolean;
  createdAt: Date;
  updatedAt: Date;
  
  // HD Treatment Information
  dryWeight?: number; // Dry weight in kg
  hdStartDate?: Date | string; // Date when HD treatment started
  dialyserType?: string | null; // Hi Flux or Lo Flux
  dialyserModel?: string | null; // Specific dialyser model
  prescribedDuration?: number | null; // Prescribed treatment duration in hours
  prescribedBFR?: number | null; // Prescribed Blood Flow Rate in mL/min
  dialysatePrescription?: string | null; // Dialysate prescription details
  dialyserCount?: number; // Current dialyser usage count
  bloodTubingCount?: number; // Current blood tubing usage count
  totalDialysisCompleted?: number; // Total number of dialysis sessions completed
  dialysersPurchased?: number; // Total number of dialysers purchased (lifetime)
  bloodTubingPurchased?: number; // Total number of blood tubing sets purchased (lifetime)
  
  // Latest Session Data
  scheduleID?: number | null;
  slotID?: number | null;
  bedNumber?: number | null;
  sessionDate?: Date | string | null; // Date of the session
  sessionStatus?: string | null; // Session status (Pre-Scheduled, Active, In Progress, Completed)
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
  
  // HD Treatment Information
  dryWeight?: number | null;
  hdStartDate?: Date | string | null;
  dialyserType?: string | null;
  dialyserCount?: number | null;
  bloodTubingCount?: number | null;
  totalDialysisCompleted?: number | null;
  dialysersPurchased?: number | null;
  bloodTubingPurchased?: number | null;
  preferredSlotID?: number | null;
  prescribedDuration?: number | null;
  dialyserModel?: string | null;
  prescribedBFR?: number | null;
  dialysatePrescription?: string | null;
}

export interface UpdatePatientRequest extends CreatePatientRequest {
  patientID: number;
}
