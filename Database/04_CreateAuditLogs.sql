-- Create AuditLogs table for tracking all user actions
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AuditLogs')
BEGIN
    CREATE TABLE AuditLogs (
        LogID INT PRIMARY KEY IDENTITY(1,1),
        UserID INT NULL,
        Username NVARCHAR(100) NOT NULL,
        Action NVARCHAR(50) NOT NULL,
        EntityType NVARCHAR(50) NULL,
        EntityID INT NULL,
        OldValues NVARCHAR(MAX) NULL,
        NewValues NVARCHAR(MAX) NULL,
        IPAddress NVARCHAR(50) NULL,
        CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
        
        FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE SET NULL
    );

    -- Create indexes for performance
    CREATE INDEX IX_AuditLogs_UserID ON AuditLogs(UserID);
    CREATE INDEX IX_AuditLogs_EntityType ON AuditLogs(EntityType);
    CREATE INDEX IX_AuditLogs_EntityID ON AuditLogs(EntityID);
    CREATE INDEX IX_AuditLogs_CreatedAt ON AuditLogs(CreatedAt DESC);
    CREATE INDEX IX_AuditLogs_Action ON AuditLogs(Action);

    PRINT 'AuditLogs table created successfully with indexes';
END
ELSE
BEGIN
    PRINT 'AuditLogs table already exists';
END
GO

-- Insert sample audit log entry
INSERT INTO AuditLogs (UserID, Username, Action, EntityType, EntityID, OldValues, NewValues, IPAddress, CreatedAt)
VALUES 
    (1, 'admin', 'LOGIN', NULL, NULL, NULL, 'Logged in successfully', '127.0.0.1', GETDATE()),
    (1, 'admin', 'CREATE', 'Patient', 1, NULL, 'Created patient: John Doe', '127.0.0.1', GETDATE());

PRINT 'Sample audit logs inserted';
GO
