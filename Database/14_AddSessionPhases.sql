-- Migration 14: Add Session Phase Tracking and Post-Dialysis Fields
-- Purpose: Implement 3-phase dialysis workflow (Pre → Intra → Post)

-- Add session phase tracking columns
ALTER TABLE HDLogs ADD COLUMN SessionPhase TEXT DEFAULT 'PRE_DIALYSIS' 
  CHECK(SessionPhase IN ('PRE_DIALYSIS', 'INTRA_DIALYSIS', 'POST_DIALYSIS', 'COMPLETED'));

-- Timestamps for phase transitions
ALTER TABLE HDLogs ADD COLUMN PreDialysisCompletedAt DATETIME NULL;
ALTER TABLE HDLogs ADD COLUMN IntraDialysisStartedAt DATETIME NULL;
ALTER TABLE HDLogs ADD COLUMN PostDialysisStartedAt DATETIME NULL;

-- Lock flags to prevent editing completed phases
ALTER TABLE HDLogs ADD COLUMN IsPreDialysisLocked BOOLEAN DEFAULT 0;
ALTER TABLE HDLogs ADD COLUMN IsIntraDialysisLocked BOOLEAN DEFAULT 0;

-- Add new Post-Dialysis fields
ALTER TABLE HDLogs ADD COLUMN PostDialysisWeight DECIMAL(5,2) NULL;
ALTER TABLE HDLogs ADD COLUMN PostDialysisSBP DECIMAL(5,2) NULL;
ALTER TABLE HDLogs ADD COLUMN PostDialysisDBP DECIMAL(5,2) NULL;
ALTER TABLE HDLogs ADD COLUMN PostDialysisHR DECIMAL(5,2) NULL;
ALTER TABLE HDLogs ADD COLUMN AccessBleedingTime INT NULL; -- in minutes
ALTER TABLE HDLogs ADD COLUMN TotalFluidRemoved DECIMAL(5,2) NULL; -- in liters
ALTER TABLE HDLogs ADD COLUMN PostAccessStatus TEXT NULL;
ALTER TABLE HDLogs ADD COLUMN DischargeNotes TEXT NULL;
ALTER TABLE HDLogs ADD COLUMN MedicationsAdministered TEXT NULL;

-- Add pre-assessment notes field
ALTER TABLE HDLogs ADD COLUMN PreAssessmentNotes TEXT NULL;

-- Initialize existing active sessions to INTRA_DIALYSIS phase
UPDATE HDLogs 
SET SessionPhase = 'INTRA_DIALYSIS', 
    IsPreDialysisLocked = 1,
    PreDialysisCompletedAt = SessionStartTime,
    IntraDialysisStartedAt = SessionStartTime
WHERE Status = 'Active' AND IsDischarged = 0;

-- Initialize existing completed sessions
UPDATE HDLogs 
SET SessionPhase = 'COMPLETED',
    IsPreDialysisLocked = 1,
    IsIntraDialysisLocked = 1
WHERE IsDischarged = 1 OR Status = 'Completed';

-- Set new scheduled sessions to PRE_DIALYSIS
UPDATE HDLogs 
SET SessionPhase = 'PRE_DIALYSIS',
    IsPreDialysisLocked = 0,
    IsIntraDialysisLocked = 0
WHERE Status = 'Scheduled';

-- Create index for faster phase queries
CREATE INDEX IF NOT EXISTS idx_hdlogs_sessionphase ON HDLogs(SessionPhase);
