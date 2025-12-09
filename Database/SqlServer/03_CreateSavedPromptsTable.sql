-- Create SavedPrompts table for storing reusable AI prompts
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'SavedPrompts')
BEGIN
    CREATE TABLE SavedPrompts (
        Id INT PRIMARY KEY IDENTITY(1,1),
        PromptText NVARCHAR(MAX) NOT NULL,
        Category NVARCHAR(100) NULL,
        UsageCount INT NOT NULL DEFAULT 0,
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
        LastUsedAt DATETIME NULL,
        IsDeleted BIT NOT NULL DEFAULT 0
    );
    
    CREATE INDEX IX_SavedPrompts_UsageCount ON SavedPrompts(UsageCount DESC);
    CREATE INDEX IX_SavedPrompts_LastUsedAt ON SavedPrompts(LastUsedAt DESC);
    CREATE INDEX IX_SavedPrompts_Category ON SavedPrompts(Category);
    
    PRINT 'SavedPrompts table created successfully';
END
ELSE
BEGIN
    PRINT 'SavedPrompts table already exists';
END
GO

-- Insert default prompts
IF NOT EXISTS (SELECT * FROM SavedPrompts WHERE PromptText = 'Show me available beds for tomorrow morning')
BEGIN
    INSERT INTO SavedPrompts (PromptText, Category, UsageCount, CreatedAt, IsDeleted)
    VALUES 
        ('Show me available beds for tomorrow morning', 'Bed Availability', 0, GETDATE(), 0),
        ('Which patients are scheduled for slot 2 today?', 'Patient Schedule', 0, GETDATE(), 0),
        ('How many beds are occupied this afternoon?', 'Bed Availability', 0, GETDATE(), 0),
        ('Show me missed appointments this week', 'Missed Appointments', 0, GETDATE(), 0),
        ('What is the utilization rate for all slots today?', 'Slot Utilization', 0, GETDATE(), 0),
        ('Show available beds for next Monday', 'Bed Availability', 0, GETDATE(), 0),
        ('Find all patients scheduled tomorrow', 'Patient Schedule', 0, GETDATE(), 0),
        ('Show me the busiest time slots this week', 'Slot Utilization', 0, GETDATE(), 0);
    
    PRINT 'Default prompts inserted successfully';
END
GO
