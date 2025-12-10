-- Add missing DialyserModel column to HDSchedule table
IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[HDSchedule]') 
    AND name = 'DialyserModel'
)
BEGIN
    ALTER TABLE [dbo].[HDSchedule]
    ADD DialyserModel NVARCHAR(100) NULL;
    
    SELECT 'SUCCESS: Column DialyserModel added to HDSchedule table.' as Result;
END
ELSE
BEGIN
    SELECT 'INFO: Column DialyserModel already exists in HDSchedule table.' as Result;
END
