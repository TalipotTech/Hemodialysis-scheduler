-- Add AccessLocation column to HDSchedule table if it doesn't exist
-- Run this against your Azure SQL Database

IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[HDSchedule]') 
    AND name = 'AccessLocation'
)
BEGIN
    ALTER TABLE [dbo].[HDSchedule]
    ADD AccessLocation NVARCHAR(200) NULL;
    
    PRINT 'Column AccessLocation added to HDSchedule table successfully.';
END
ELSE
BEGIN
    PRINT 'Column AccessLocation already exists in HDSchedule table.';
END
GO
