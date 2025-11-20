-- Migration Script: Add IsMovedToHistory column to HDSchedule table
-- This enables automatic session management based on slot completion times

-- Add IsMovedToHistory column to HDSchedule table
ALTER TABLE HDSchedule 
ADD COLUMN IsMovedToHistory INTEGER DEFAULT 0 NOT NULL;

-- Create index for better query performance
CREATE INDEX IF NOT EXISTS idx_hdschedule_history 
ON HDSchedule(IsMovedToHistory, SessionDate, SlotID);

-- Update existing records: set old sessions to history
UPDATE HDSchedule
SET IsMovedToHistory = 1
WHERE date(SessionDate) < date('now', '-1 day')
  AND IsDischarged = 0;

-- Verify the changes
SELECT 'IsMovedToHistory column added successfully' as Status;
SELECT COUNT(*) as TotalRecords, 
       SUM(CASE WHEN IsMovedToHistory = 1 THEN 1 ELSE 0 END) as HistoryRecords,
       SUM(CASE WHEN IsMovedToHistory = 0 THEN 1 ELSE 0 END) as ActiveRecords
FROM HDSchedule;
