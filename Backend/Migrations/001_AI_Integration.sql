-- AI Integration Database Migration
-- Creates tables for AI settings and usage tracking

-- AI Settings table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AISettings')
BEGIN
    CREATE TABLE AISettings (
        Id INT PRIMARY KEY IDENTITY(1,1),
        AIEnabled BIT NOT NULL DEFAULT 0,
        AIProvider NVARCHAR(50) NOT NULL DEFAULT 'Gemini',
        DailyCostLimit DECIMAL(10,2) NOT NULL DEFAULT 10.00,
        MonthlyCostLimit DECIMAL(10,2) NOT NULL DEFAULT 250.00,
        EnableSchedulingRecommendations BIT NOT NULL DEFAULT 1,
        EnableNaturalLanguageQueries BIT NOT NULL DEFAULT 0,
        EnablePredictiveAnalytics BIT NOT NULL DEFAULT 0,
        CurrentDailyCost DECIMAL(10,6) NOT NULL DEFAULT 0,
        CurrentMonthlyCost DECIMAL(10,6) NOT NULL DEFAULT 0,
        TodayRequestCount INT NOT NULL DEFAULT 0,
        MonthRequestCount INT NOT NULL DEFAULT 0,
        LastDailyReset DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        LastMonthlyReset DATETIME2 NOT NULL,
        EncryptedApiKey NVARCHAR(500) NULL,
        LastUpdated DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedBy INT NULL
    );
    
    -- Insert default settings
    INSERT INTO AISettings (
        AIEnabled, 
        AIProvider, 
        LastMonthlyReset
    ) VALUES (
        0, 
        'Gemini', 
        DATEFROMPARTS(YEAR(GETUTCDATE()), MONTH(GETUTCDATE()), 1)
    );
    
    PRINT 'AISettings table created successfully';
END
ELSE
BEGIN
    PRINT 'AISettings table already exists';
END
GO

-- AI Usage Logs table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AIUsageLogs')
BEGIN
    CREATE TABLE AIUsageLogs (
        Id INT PRIMARY KEY IDENTITY(1,1),
        Timestamp DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        Provider NVARCHAR(50) NOT NULL,
        RequestType NVARCHAR(100) NOT NULL,
        InputTokens INT NOT NULL DEFAULT 0,
        OutputTokens INT NOT NULL DEFAULT 0,
        TotalTokens INT NOT NULL DEFAULT 0,
        Cost DECIMAL(10,6) NOT NULL DEFAULT 0,
        ProcessingTimeMs INT NOT NULL DEFAULT 0,
        Success BIT NOT NULL DEFAULT 1,
        ErrorMessage NVARCHAR(MAX) NULL,
        UserId INT NULL,
        Metadata NVARCHAR(MAX) NULL,
        
        INDEX IX_AIUsageLogs_Timestamp (Timestamp DESC),
        INDEX IX_AIUsageLogs_UserId (UserId),
        INDEX IX_AIUsageLogs_RequestType (RequestType)
    );
    
    PRINT 'AIUsageLogs table created successfully';
END
ELSE
BEGIN
    PRINT 'AIUsageLogs table already exists';
END
GO

PRINT 'AI Integration database migration completed successfully';
