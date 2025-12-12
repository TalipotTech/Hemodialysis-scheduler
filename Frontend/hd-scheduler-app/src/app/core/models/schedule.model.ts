export interface Slot {
  slotID: number;
  slotName: string;
  startTime: string;
  endTime: string;
  bedCapacity: number;
}

export interface BedAssignment {
  assignmentID: number;
  patientID: number;
  slotID: number;
  bedNumber: number;
  assignmentDate: Date;
  isActive: boolean;
  createdAt: Date;
  dischargedAt?: Date;
}

export interface DailyScheduleResponse {
  date: Date;
  slots: SlotSchedule[];
  statistics?: {
    totalActivePatients: number;
    totalReservedPatients: number;
    activeSessionsToday: number;
    preScheduledSessionsToday: number;
    futureSessionsCount: number;
  };
}

export interface SlotSchedule {
  slotID: number;
  slotName: string;
  timeRange: string;
  maxBeds?: number;
  beds: BedStatus[];
}

export interface BedStatus {
  bedNumber: number;
  status: 'available' | 'occupied' | 'reserved' | 'pre-scheduled' | 'completed';
  scheduleId?: number;
  patient?: PatientSummary;
  sessionStatus?: string;
  sessionDate?: string;
  sessionNumber?: number;
  totalWeeklySessions?: number;
  needsBedAssignment?: boolean;
  sessionStartTime?: string;
  slotId?: number;
}

export interface HDSchedule {
  scheduleID: number;
  patientID: number;
  sessionDate: Date;
  slotID?: number;
  bedNumber?: number;
  isDischarged: boolean;
  isMovedToHistory: boolean;
  patientName?: string;
  assignedDoctorName?: string;
  assignedNurseName?: string;
  createdAt: Date;
  updatedAt: Date;
}

export interface PatientSummary {
  id: number;
  patientId: number;
  name: string;
  age: number;
  bloodPressure?: string;
  hdCycle?: string;
  isDischarged?: boolean;
}

export interface AssignBedRequest {
  patientID: number;
  slotID: number;
  bedNumber: number;
  assignmentDate: Date;
}

export interface BedAvailability {
  slotID: number;
  slotName: string;
  totalBeds: number;
  occupiedBeds: number;
  availableBeds: number;
  availableBedNumbers: number[];
}
